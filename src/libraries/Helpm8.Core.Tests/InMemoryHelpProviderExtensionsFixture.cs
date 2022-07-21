using System;
using System.Collections.Generic;
using Xunit;

namespace Helpm8.Core.Tests
{
    public class InMemoryHelpProviderExtensionsFixture
    {
        [Fact]
        public void ThrowsOnNulls()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                InMemoryHelpBuilderExtensions.AddInMemoryCollection(null);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
#pragma warning disable CS8632 // Die Anmerkung für Nullable-Verweistypen darf nur in Code innerhalb eines #nullable-Anmerkungskontexts verwendet werden.
                InMemoryHelpBuilderExtensions.AddInMemoryCollection(null, new Dictionary<string, string?>());
#pragma warning restore CS8632 // Die Anmerkung für Nullable-Verweistypen darf nur in Code innerhalb eines #nullable-Anmerkungskontexts verwendet werden.
            });

        }
    }
}
