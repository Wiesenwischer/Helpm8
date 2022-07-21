using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Helpm8.Core.Tests
{
    public class ChainedHelpProviderExtensionsFixture
    {
        [Fact]
        public void ThrowsOnNulls()
        {
            var builder = new HelpBuilder();
            var root = builder.Build();

            Assert.Throws<ArgumentNullException>(() =>
            {
                ChainedBuilderExtensions.AddHelp(null, root, true);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                ChainedBuilderExtensions.AddHelp(builder, null, true);
            });
        }
    }
}
