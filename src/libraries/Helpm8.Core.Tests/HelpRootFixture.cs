using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Helpm8.Core.Tests
{
    public class HelpRootFixture
    {
        [Fact]
        public void CtorThrowsOnNulls()
        {
            Assert.Throws<ArgumentNullException>(() => new HelpRoot(null));
        }

        [Fact]
        public void DisposesProvidersOnDispose()
        {
            var provider1 = new DisposableProvider();
            var provider2 = new DisposableProvider();

            var helpRoot = new HelpRoot(new List<IHelpProvider>
            {
                provider1, provider2
            });
            helpRoot.Dispose();

            Assert.True(provider1.DisposedCalled);
            Assert.True(provider2.DisposedCalled);
        }

        [Fact]
        public void DisposeProvidersDoesNotThrowWhenProviderIsNotDisposable()
        {
            var provider1 = new TestableProvider();
            var provider2 = new TestableProvider();

            var helpRoot = new HelpRoot(new List<IHelpProvider>
            {
                provider1, provider2
            });
            helpRoot.Dispose();
        }

        private class DisposableProvider : IHelpProvider, IDisposable
        {
            public void Load()
            {
            }

#pragma warning disable CS8632
            public bool TryGet(string key, out string? value)
#pragma warning restore CS8632
            {
                throw new NotImplementedException();
            }

#pragma warning disable CS8632
            public void Set(string key, string? value)
#pragma warning restore CS8632
            {
                throw new NotImplementedException();
            }

#pragma warning disable CS8632
            public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
#pragma warning restore CS8632
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                DisposedCalled = true;
            }

            public bool DisposedCalled { get; private set; }
        }

        private class TestableProvider : IHelpProvider
        {
            public void Load()
            {
            }

#pragma warning disable CS8632
            public bool TryGet(string key, out string? value)
#pragma warning restore CS8632
            {
                throw new NotImplementedException();
            }

#pragma warning disable CS8632
            public void Set(string key, string? value)
#pragma warning restore CS8632
            {
                throw new NotImplementedException();
            }

#pragma warning disable CS8632
            public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
#pragma warning restore CS8632
            {
                throw new NotImplementedException();
            }
        }
    }
}
