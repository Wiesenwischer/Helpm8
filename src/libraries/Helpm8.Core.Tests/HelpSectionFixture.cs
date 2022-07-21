using System;
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

        [Fact]
        public void CtorThrowsOnNulls()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new HelpSection(null, string.Empty);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                var builder = new HelpBuilder();
                builder.AddInMemoryCollection();
                var helpRoot = builder.Build();
                new HelpSection(helpRoot, null);
            });
        }

        [Fact]
        public void ReturnsEmptyKeyWhenNotSet()
        {
            var builder = new HelpBuilder();
            builder.AddInMemoryCollection();
            var helpRoot = builder.Build();
         
            var section = new HelpSection(helpRoot, string.Empty);
            
            Assert.Empty(section.Key);
        }
    }
}
