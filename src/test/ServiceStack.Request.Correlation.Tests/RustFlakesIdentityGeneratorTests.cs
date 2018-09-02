// This Source Code Form is subject to the terms of the Mozilla Public 
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Request.Correlation.Tests
{
    using System.Linq;
    using FluentAssertions;
    using Xunit;

    public class RustFlakesIdentityGeneratorTests
    {
        private readonly RustFlakesIdentityGenerator generator;

        public RustFlakesIdentityGeneratorTests()
        {
            generator = new RustFlakesIdentityGenerator();
        }

        [Fact]
        public void GenerateIdentity_GeneratesString()
        {
            var str = generator.GenerateIdentity();
            str.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void GenerateIdentity_GeneratesUnique()
        {
            var collection = Enumerable.Range(0, 100).Select(_ => generator.GenerateIdentity()).ToArray();

            collection.Length.Should().Be(collection.Distinct().Count());
        }
    }
}
