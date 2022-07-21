using Helpm8.InMemory;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Helpm8.Core.Tests
{
    [ExcludeFromCodeCoverage]
    public class InMemoryHelpProviderFixture : HelpProviderFixtureBase
    {
        protected override (IHelpProvider Provider, Action Initializer) LoadThroughProvider(TestSection testHelp) =>
            LoadUsingInMemoryProvider(testHelp);

        [Fact]
        public override void Null_values_are_included_in_the_help()
        {
            AssertHelp(BuildHelpRoot(LoadThroughProvider(TestSection.NullsTestHelp)), expectNulls: true);
        }

        [Fact]
        public void AddsValue()
        {
            var provider = new InMemoryHelpProvider(new InMemoryHelpSource());
            provider.Add("foo", "foo-value");
            Assert.Equal("foo-value", provider.Get("foo"));
        }
    }
}
