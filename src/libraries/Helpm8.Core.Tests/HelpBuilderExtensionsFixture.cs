using Helpm8.InMemory;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Helpm8.Core.Tests
{
    [ExcludeFromCodeCoverage]
    public class HelpBuilderExtensionsFixture
    {
        [Fact]
        public void AddInvokesConfigureSourceCallback()
        {
            bool invoked = false;
            var helpBuilder = new HelpBuilder();
            
            helpBuilder.Add<InMemoryHelpSource>(cfg =>
            {
                invoked = true;
            });

            Assert.True(invoked);
        }

        [Fact]
        public void AddAddsSource()
        {
            var helpBuilder = new TestHelpBuilder();
            
            helpBuilder.Add<InMemoryHelpSource>(cfg => { });
            
            Assert.True(helpBuilder.AddCalled);
        }
        
        [ExcludeFromCodeCoverage]
        private class TestHelpBuilder : IHelpBuilder
        {
            public bool AddCalled { get; private set; } = false;

            public IHelpBuilder Add(IHelpSource source)
            {
                AddCalled = true;
                return this;
            }

            public IList<IHelpSource> Sources { get; }
            public IHelpRoot Build()
            {
                throw new NotImplementedException();
            }
        }
    }
}
