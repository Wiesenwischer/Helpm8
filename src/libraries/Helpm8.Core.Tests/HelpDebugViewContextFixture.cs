using System.Diagnostics.CodeAnalysis;
using Helpm8.InMemory;
using Xunit;

namespace Helpm8.Core.Tests
{
    [ExcludeFromCodeCoverage]
    public class HelpDebugViewContextFixture
    {
        [Fact]
        public void GettersReturnGivenValues()
        {
            var provider = new InMemoryHelpProvider(new InMemoryHelpSource());
            var context = new HelpDebugViewContext("TestForm", "TestKey", "TestValue", provider);

            Assert.Equal("TestForm", context.Path);
            Assert.Equal("TestKey", context.Key);
            Assert.Equal("TestValue", context.Value);
            Assert.Same(provider, context.HelpProvider);
        }
    }
}
