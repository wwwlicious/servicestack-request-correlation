// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Request.Correlation.Tests
{
    using System;
    using FakeItEasy;
    using FluentAssertions;
    using Testing;
    using Xunit;

    public class ServiceGatewayFactoryBaseDecoratorTests
    {
        private readonly ServiceGatewayFactoryBaseDecorator gateway;
        private readonly ServiceGatewayFactoryBase decorated;
        private const string HeaderName = "x-correlation-id";
        private readonly string correlationId = Guid.NewGuid().ToString();

        public ServiceGatewayFactoryBaseDecoratorTests()
        {
            decorated = A.Fake<ServiceGatewayFactoryBase>();
            gateway = new ServiceGatewayFactoryBaseDecorator(HeaderName, decorated);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Ctor_Throws_IfHeaderNameNullOrEmpty(string header)
        {
            Assert.Throws<ArgumentNullException>(() => new ServiceGatewayFactoryBaseDecorator(header, decorated));
        }

        [Fact]
        public void Ctor_Throws_IfFactoryBaseNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ServiceGatewayFactoryBaseDecorator(HeaderName, null));
        }

        [Fact]
        public void GetServiceGateway_CallsUnderlyingGetServiceGateway()
        {
            var mockHttpRequest = new MockHttpRequest();
            gateway.GetServiceGateway(mockHttpRequest);

            A.CallTo(() => decorated.GetServiceGateway(mockHttpRequest)).MustHaveHappened();
        }

        [Fact]
        public void GetServiceGateway_ReturnsSelf()
        {
            var result = gateway.GetServiceGateway(new MockHttpRequest());
            result.Should().Be(gateway);
        }

        [Fact]
        public void GetGateway_CallsUnderlyingGetGateway()
        {
            var type = typeof (int);
            gateway.GetGateway(type);

            A.CallTo(() => decorated.GetGateway(type)).MustHaveHappened();
        }

        [Fact]
        public void GetGateway_ReturnsUnderlyingGetGateway_IfNoCorrelationId()
        {
            var type = typeof(int);
            var serviceGateway = A.Fake<IServiceGateway>();
            A.CallTo(() => decorated.GetGateway(type)).Returns(serviceGateway);

            gateway.GetGateway(type).Should().Be(serviceGateway);
        }

        [Fact]
        public void GetGateway_Returns_UnderlyingGetGateway_IfCorrelationIdAndNotServiceClientBase()
        {
            SetCorrelationId();
            var type = typeof(int);
            var serviceGateway = A.Fake<IServiceGateway>();
            A.CallTo(() => decorated.GetGateway(type)).Returns(serviceGateway);

            gateway.GetGateway(type).Should().Be(serviceGateway);
        }

        [Fact]
        public void GetGateway_SetsUnderlyingServiceClientBaseHeaders_IfCorrelationIdAndServiceClientBase()
        {
            SetCorrelationId();
            var type = typeof(int);

            var jsonClient = new JsonServiceClient();
            A.CallTo(() => decorated.GetGateway(type)).Returns(jsonClient);

            gateway.GetGateway(type);
            jsonClient.Headers[HeaderName].Should().Be(correlationId);
        }

        private void SetCorrelationId()
        {
            var mock = new MockHttpRequest();
            mock.Headers[HeaderName] = correlationId;

            gateway.GetServiceGateway(mock);
        }
    }
}
