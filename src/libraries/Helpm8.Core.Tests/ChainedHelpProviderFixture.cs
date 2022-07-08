using System.Collections.Generic;
using System.Linq;
using Helpm8.InMemory;
using Xunit;

namespace Helpm8.Core.Tests
{
    public class ChainedHelpProviderFixture
    {
        [Fact]
        public void ChainedHelp_UsingMemoryHelpSource_ChainedCouldExposeProvider()
        {
            var chainedHelpProvider = new ChainedHelpSource
            {
                Help = new HelpBuilder()
                            .Add(new InMemoryHelpSource
                            {
                                InitialData = new Dictionary<string, string>() { { "a:b", "c" } }
                            })
                            .Build(),
                ShouldDisposeHelp = false,
            }
                .Build(new HelpBuilder()) as ChainedHelpProvider;

#pragma warning disable CS8632
            // ReSharper disable once PossibleNullReferenceException
            Assert.True(chainedHelpProvider.TryGet("a:b", out string? value));
#pragma warning restore CS8632
            Assert.Equal("c", value);
            Assert.Equal("c", chainedHelpProvider.Help["a:b"]);

            var helpRoot = chainedHelpProvider.Help as IHelpRoot;
            Assert.NotNull(helpRoot);
            Assert.Single(helpRoot.Providers);
            Assert.IsType<InMemoryHelpProvider>(helpRoot.Providers.First());
        }

        [Fact]
        public void ChainedHelp_ExposesProvider()
        {
            var providers = new IHelpProvider[] {
                new TestHelpProvider("foo", "foo-value")
            };
            var chainedHelpSource = new ChainedHelpSource
            {
                Help = new HelpRoot(providers),
                ShouldDisposeHelp = false,
            };

            var chainedHelpProvider = chainedHelpSource
                .Build(new HelpBuilder()) as ChainedHelpProvider;

            // ReSharper disable once PossibleNullReferenceException
            var helpRoot = chainedHelpProvider.Help as IHelpRoot;
            Assert.NotNull(helpRoot);
            Assert.Equal(providers, helpRoot.Providers);
        }

        private class TestHelpProvider : HelpProvider
        {
            public TestHelpProvider(string key, string value)
                => Data.Add(key, value);
        }
    }
}