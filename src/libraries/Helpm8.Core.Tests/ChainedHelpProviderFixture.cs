using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Helpm8.InMemory;
using Xunit;

namespace Helpm8.Core.Tests
{
    [ExcludeFromCodeCoverage]
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

        [Fact]
        public void CanSetValue()
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
            chainedHelpProvider.Set("bar", "bar-value");

            // ReSharper disable once PossibleNullReferenceException
            var helpRoot = chainedHelpProvider.Help as IHelpRoot;
            // ReSharper disable once PossibleNullReferenceException
            Assert.Equal("bar-value", helpRoot["bar"]);
        }

        [Fact]
        public void CtorThrowsOnNulls()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new ChainedHelpProvider(null);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                var source = new ChainedHelpSource();
                new ChainedHelpProvider(source);
            });
        }

        [Fact]
        public void DisposesHelp()
        {
            var help = new TestableDisposeCalledHelp();
            var source = new ChainedHelpSource();
            source.ShouldDisposeHelp = true;
            source.Help = help;
            var chainedProvider = new ChainedHelpProvider(source);
            
            chainedProvider.Dispose();

            Assert.True(help.DisposedCalled);
        }

        private class TestableDisposeCalledHelp : IHelp, IDisposable
        {
#pragma warning disable CS8632
            public string? this[string key]
#pragma warning restore CS8632
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public IHelpSection GetSection(string key)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<IHelpSection> GetChildren()
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                DisposedCalled = true;
            }

            public bool DisposedCalled { get; private set; }
        }

        private class TestHelpProvider : HelpProvider
        {
            public TestHelpProvider(string key, string value)
                => Data.Add(key, value);
        }
    }
}