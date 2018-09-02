// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Request.Correlation.Tests.Extensions
{
    using System;
    using Correlation.Extensions;
    using FluentAssertions;
    using ServiceStack.Configuration.Consul.Tests.Fixtures;
    using Testing;
    using Xunit;

    [Collection("AppHost")]
    public class RequestExtensionsTests
    {
        private ServiceStackHost appHost;
        
        private const string headerName = "x-correlationId";
        private readonly string headerValue = Guid.NewGuid().ToString();
        private readonly string itemValue = Guid.NewGuid().ToString();

        public RequestExtensionsTests(AppHostFixture fixture)
        {
            appHost = fixture.AppHost;
        }
        
        [Fact]
        public void GetCorrelationId_ReturnsNull_IfNotSet()
        {
            var request = new MockHttpRequest();

            request.GetCorrelationId(headerName).Should().BeNull();
        }

        [Fact]
        public void GetCorrelationId_ReturnsHeaderValue_IfPresent()
        {
            var request = new MockHttpRequest();
            request.Headers.Add(headerName, headerValue);

            var correlationId = request.GetCorrelationId(headerName);

            correlationId.Should().Be(headerValue);
        }

        [Fact]
        public void GetCorrelationId_ReturnsHeaderValue_IfBothHeaderAndItemsSet()
        {
            var request = new MockHttpRequest();
            request.Headers.Add(headerName, headerValue);
            request.Items.Add(headerName, itemValue);

            var correlationId = request.GetCorrelationId(headerName);

            correlationId.Should().Be(headerValue);
        }

        [Fact]
        public void GetCorrelationId_ReturnsItemValue_IfNotSet()
        {
            var request = new MockHttpRequest();
            request.Items.Add(headerName, itemValue);

            var correlationId = request.GetCorrelationId(headerName);

            correlationId.Should().Be(itemValue);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GetCorrelationId_ReturnsItemValue_HeaderNullOrEmpty(string header)
        {
            var request = new MockHttpRequest();
            request.Items.Add(headerName, itemValue);
            request.Headers.Add(headerName, header);

            var correlationId = request.GetCorrelationId(headerName);

            correlationId.Should().Be(itemValue);
        }
    }
}
