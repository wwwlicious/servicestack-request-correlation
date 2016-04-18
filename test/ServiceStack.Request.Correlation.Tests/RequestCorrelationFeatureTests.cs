// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Request.Correlation.Tests
{
    using System;
    using FakeItEasy;
    using FluentAssertions;
    using Interfaces;
    using Ploeh.AutoFixture.Xunit2;
    using ServiceStack;
    using Testing;
    using Web;
    using Xunit;

    [Collection("RequestCorrelationTests")]
    public class RequestCorrelationFeatureTests
    {
        private readonly RequestCorrelationFeature feature;
        private readonly IIdentityGenerator generator;
        private readonly string newId = Guid.NewGuid().ToString();

        public RequestCorrelationFeatureTests()
        {
            generator = A.Fake<IIdentityGenerator>();
            A.CallTo(() => generator.GenerateIdentity()).Returns(newId);
            feature = new RequestCorrelationFeature { IdentityGenerator = generator };
        }

        [Fact]
        public void Register_AddsPreRequestFilter()
        {
            var appHost = A.Fake<IAppHost>();
            appHost.PreRequestFilters.Count.Should().Be(0);

            feature.Register(appHost);

            appHost.PreRequestFilters.Count.Should().Be(1);
        }

        [Fact]
        public void Register_AddsPreRequestFilter_AtPosition0()
        {
            var appHost = A.Fake<IAppHost>();
            Action<IRequest, IResponse> myDelegate = (request, response) => { };

            // Add delegate at position 0
            appHost.PreRequestFilters.Insert(0, myDelegate);

            feature.Register(appHost);

            // After registering delegate added at 0 should now be at 1
            appHost.PreRequestFilters[1].Should().Be(myDelegate);
        }

        [Fact]
        public void HeaderName_HasDefaultValue()
        {
            var testFeature = new RequestCorrelationFeature();
            testFeature.HeaderName.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void IdentityGenerator_UsesRustflakesIdentityGeneratorByDefault()
        {
            var testFeature = new RequestCorrelationFeature();
            testFeature.IdentityGenerator.Should().BeOfType<RustflakesIdentityGenerator>();
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
        public void ProcessRequest_SetsNewIdOnRequest_IfNotProvided()
        {
            var mockHttpRequest = new MockHttpRequest();

            feature.ProcessRequest(mockHttpRequest, new MockHttpResponse());

            mockHttpRequest.Headers[feature.HeaderName].Should().Be(newId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ProcessRequest_SetsNewIdOnRequest_IfProvidedButEmpty(string requestId)
        {
            var mockHttpRequest = new MockHttpRequest();
            mockHttpRequest.Headers[feature.HeaderName] = requestId;

            feature.ProcessRequest(mockHttpRequest, new MockHttpResponse());

            mockHttpRequest.Headers[feature.HeaderName].Should().Be(newId);
        }

        [Theory, InlineAutoData]
        public void ProcessRequest_DoesNotChangeHeaderOnRequest_IfProvided(string requestId)
        {
            var mockHttpRequest = new MockHttpRequest();
            mockHttpRequest.Headers[feature.HeaderName] = requestId;

            feature.ProcessRequest(mockHttpRequest, new MockHttpResponse());

            mockHttpRequest.Headers[feature.HeaderName].Should().Be(requestId);
        }

        [Fact]
        public void ProcessRequest_SetsHeaderOnResponse()
        {
            var mockHttpResponse = new MockHttpResponse();

            feature.ProcessRequest(new MockHttpRequest(), mockHttpResponse);

            mockHttpResponse.Headers[feature.HeaderName].Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void ProcessRequest_SetsNewIdOnResponse_IfNotProvidedInRequest()
        {
            var mockHttpResponse = new MockHttpResponse();

            feature.ProcessRequest(new MockHttpRequest(), mockHttpResponse);

            mockHttpResponse.Headers[feature.HeaderName].Should().Be(newId);
        }

        [Theory, InlineAutoData]
        public void ProcessRequest_SetsIdOnResponse_IfNotInRequest(string requestId)
        {
            var mockHttpResponse = new MockHttpResponse();
            var mockHttpRequest = new MockHttpRequest();
            mockHttpRequest.Headers[feature.HeaderName] = requestId;

            feature.ProcessRequest(mockHttpRequest, mockHttpResponse);

            mockHttpResponse.Headers[feature.HeaderName].Should().Be(requestId);
        }
    }
}
