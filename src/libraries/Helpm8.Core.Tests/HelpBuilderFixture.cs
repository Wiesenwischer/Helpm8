using System;
using Xunit;

namespace Helpm8.Core.Tests
{
    public class HelpBuilderFixture
    {
        [Fact]
        public void ThrowsOnAddWhenHelpSourceIsNull()
        {
            var helpBuilder = new HelpBuilder();

            Assert.Throws<ArgumentNullException>(() => helpBuilder.Add(null));
        }
    }
}
