using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Helpm8.Core.Tests
{
    [ExcludeFromCodeCoverage]
    public class HelpSectionFixture
    {
        [Fact]
        public void SetCombinesOnRoot()
        {
            var builder = new HelpBuilder();
            builder.AddInMemoryCollection();
            var helpRoot = builder.Build();
            var section = new HelpSection(helpRoot, "TestForm");

            section["foo"] = "foo-value";

            Assert.Equal("foo-value", helpRoot["TestForm:foo"]);
        }

        [Fact]
        public void SetValueUpdatedRoot()
        {
            var builder = new HelpBuilder();
            builder.AddInMemoryCollection();
            var helpRoot = builder.Build();
            var section = new HelpSection(helpRoot, "TestForm");

            section.Value = "TestForm-value";

            Assert.Equal("TestForm-value", helpRoot["TestForm"]);
        }
    }
}
