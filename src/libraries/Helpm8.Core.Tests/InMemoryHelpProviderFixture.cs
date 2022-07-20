using System;

namespace Helpm8.Core.Tests
{
    public class InMemoryHelpProviderFixture : HelpProviderFixtureBase
    {
        protected override (IHelpProvider Provider, Action Initializer) LoadThroughProvider(TestSection testHelp) =>
            LoadUsingInMemoryProvider(testHelp);

        public override void Null_values_are_included_in_the_help()
        {
            AssertHelp(BuildHelpRoot(LoadThroughProvider(TestSection.NullsTestHelp)), expectNulls: true);
        }
    }
}
