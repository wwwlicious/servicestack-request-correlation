namespace ServiceStack.Request.Correlation.Tests
{
    using System.Linq;
    using FluentAssertions;
    using Xunit;

    public class RustflakesIdentityGeneratorTests
    {
        private readonly RustflakesIdentityGenerator _generator;

        public RustflakesIdentityGeneratorTests()
        {
            _generator = new RustflakesIdentityGenerator();
        }

        [Fact]
        public void GenerateIdentity_GeneratesString()
        {
            var str = _generator.GenerateIdentity();
            str.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void GenerateIdentity_GeneratesUnique()
        {
            var collection = Enumerable.Range(0, 100).Select(_ => _generator.GenerateIdentity()).ToArray();

            collection.Length.Should().Be(collection.Distinct().Count());
        }
    }
}
