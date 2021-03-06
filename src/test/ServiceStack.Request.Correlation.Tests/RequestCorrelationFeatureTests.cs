﻿// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Request.Correlation.Tests
{
    using System;
    using FakeItEasy;
    using FluentAssertions;
    using Interfaces;
    using ServiceStack;
    using ServiceStack.Configuration.Consul.Tests.Fixtures;
    using Testing;
    using Web;
    using Xunit;
    
    [Collection("AppHost")]
    public class RequestCorrelationFeatureTests
    {
        private readonly RequestCorrelationFeature feature;
        private readonly IIdentityGenerator generator;
        private readonly string newId = Guid.NewGuid().ToString();
        private readonly ServiceStackHost appHost;

        public RequestCorrelationFeatureTests(AppHostFixture fixture)
        {
            appHost = fixture.AppHost;
            generator = A.Fake<IIdentityGenerator>();
            A.CallTo(() => generator.GenerateIdentity()).Returns(newId);
            feature = new RequestCorrelationFeature { IdentityGenerator = generator };
        }

        [Fact]
        public void Register_AddsPreRequestFilter()
        {
            appHost.PreRequestFilters.Count.Should().Be(0);

            feature.Register(appHost);

            appHost.PreRequestFilters.Count.Should().Be(1);
        }

        [Fact]
        public void Register_AddsPreRequestFilter_AtPosition0()
        {
            Action<IRequest, IResponse> myDelegate = (request, response) => { };

            // Add delegate at position 0
            appHost.PreRequestFilters.Insert(0, myDelegate);

            feature.Register(appHost);

            // After registering delegate added at 0 should now be at 1
            appHost.PreRequestFilters[1].Should().Be(myDelegate);
        }

        [Fact]
        public void Register_AddsResponseFilter()
        {
            var filterCount = appHost.GlobalResponseFilters.Count;

            feature.Register(appHost);

            appHost.GlobalResponseFilters.Count.Should().Be(filterCount + 1);
        }

        [Fact]
        public void HeaderName_HasDefaultValue()
        {
            var testFeature = new RequestCorrelationFeature();
            testFeature.HeaderName.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void IdentityGenerator_UsesRustFlakesIdentityGeneratorByDefault()
        {
            var testFeature = new RequestCorrelationFeature();
            testFeature.IdentityGenerator.Should().BeOfType<RustFlakesIdentityGenerator>();
        }

        [Fact]
        public void ProcessRequest_SetsHeaderOnRequest_IfNotProvided()
        {
            var mockHttpRequest = new MockHttpRequest();

            feature.ProcessRequest(mockHttpRequest, new MockHttpResponse());

            mockHttpRequest.Headers[feature.HeaderName].Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void ProcessRequest_UsesCustomHeaderName_IfSet()
        {
            var mockHttpRequest = new MockHttpRequest();

            const string customHeader = "x-my-test-header";
            feature.HeaderName = customHeader;
            feature.ProcessRequest(mockHttpRequest, new MockHttpResponse());

            mockHttpRequest.Headers[customHeader].Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void ProcessRequest_SetsNewIdOnRequestHeader_IfNotProvided()
        {
            var mockHttpRequest = new MockHttpRequest();

            feature.ProcessRequest(mockHttpRequest, new MockHttpResponse());

            mockHttpRequest.Headers[feature.HeaderName].Should().Be(newId);
        }

        [Fact]
        public void ProcessRequest_AddsNewIdOnRequestItems_IfNotProvided()
        {
            var mockHttpRequest = new MockHttpRequest();

            feature.ProcessRequest(mockHttpRequest, new MockHttpResponse());

            mockHttpRequest.Items[feature.HeaderName].ToString().Should().Be(newId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ProcessRequest_SetsNewIdOnRequestHeader_IfProvidedButEmpty(string requestId)
        {
            var mockHttpRequest = new MockHttpRequest();
            mockHttpRequest.Headers[feature.HeaderName] = requestId;

            feature.ProcessRequest(mockHttpRequest, new MockHttpResponse());

            mockHttpRequest.Headers[feature.HeaderName].Should().Be(newId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ProcessRequest_SetsNewIdOnRequestItems_IfProvidedButEmpty(string requestId)
        {
            var mockHttpRequest = new MockHttpRequest();
            mockHttpRequest.Headers[feature.HeaderName] = requestId;

            feature.ProcessRequest(mockHttpRequest, new MockHttpResponse());

            mockHttpRequest.Items[feature.HeaderName].Should().Be(newId);
        }

        [Fact]
        public void ProcessRequest_DoesNotChangeHeaderOnRequest_IfProvided()
        {
            var requestId = Guid.NewGuid().ToString();
            var mockHttpRequest = new MockHttpRequest();
            mockHttpRequest.Headers[feature.HeaderName] = requestId;

            feature.ProcessRequest(mockHttpRequest, new MockHttpResponse());

            mockHttpRequest.Headers[feature.HeaderName].Should().Be(requestId);
        }

        [Fact]
        public void SetResponseCorrelationId_SetsIdOnResponse_IfInRequest()
        {
            var requestId = Guid.NewGuid().ToString();
            var mockHttpResponse = new MockHttpResponse();
            var mockHttpRequest = new MockHttpRequest();
            mockHttpRequest.Headers[feature.HeaderName] = requestId;

            feature.SetResponseCorrelationId(mockHttpRequest, mockHttpResponse, 1);

            mockHttpResponse.Headers[feature.HeaderName].Should().Be(requestId);
        }

        [Fact]
        public void AfterPluginsLoaded_GetsIServiceGatewayFactory_FromContainer()
        {
            var appHost = A.Fake<IAppHost>();

            feature.AfterPluginsLoaded(appHost);

            A.CallTo(() => appHost.TryResolve<IServiceGatewayFactory>()).MustHaveHappened();
        }

        [Fact]
        public void AfterPluginsLoaded_RegistersDecorator_IfIServiceGatewayFactory_IsServiceGatewayFactoryBase()
        {
            var appHost = A.Fake<IAppHost>();
            A.CallTo(() => appHost.TryResolve<IServiceGatewayFactory>()).Returns(new TestServiceGatewayFactory());

            feature.AfterPluginsLoaded(appHost);
            
            A.CallTo(() =>
                    appHost.Register<IServiceGatewayFactory>(
                        A<IServiceGatewayFactory>.That.Matches(g => g.GetType() == typeof(ServiceGatewayFactoryBaseDecorator))))
                .MustHaveHappened();
        }

        [Fact]
        public void AfterPluginsLoaded_DoesNotRegistersDecorator_IfIServiceGatewayFactory_IsNotServiceGatewayFactoryBase()
        {
            var appHost = A.Fake<IAppHost>();
            A.CallTo(() => appHost.TryResolve<IServiceGatewayFactory>()).Returns(new BasicServiceGatewayFactory());

            feature.AfterPluginsLoaded(appHost);

            A.CallTo(() => appHost.Register<IServiceGatewayFactory>(A<IServiceGatewayFactory>.Ignored))
                .MustNotHaveHappened();
        }
    }

    public class TestServiceGatewayFactory : ServiceGatewayFactoryBase
    {
        public override IServiceGateway GetGateway(Type requestType)
        {
            throw new NotImplementedException();
        }
    }

    public class BasicServiceGatewayFactory : IServiceGatewayFactory
    {
        public IServiceGateway GetServiceGateway(IRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
