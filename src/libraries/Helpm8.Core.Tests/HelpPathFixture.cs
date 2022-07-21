using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Helpm8.Core.Tests
{
    [ExcludeFromCodeCoverage]
    public class HelpPathFixture
    {
        [Fact]
        public void CombineGivenParamsWithEmptySegmentLeavesDelimiter()
        {
            Assert.Equal("parent:", HelpPath.Combine("parent", ""));
            Assert.Equal("parent::", HelpPath.Combine("parent", "", ""));
            Assert.Equal("parent:::key", HelpPath.Combine("parent", "", "", "key"));
        }

        [Fact]
        public void CombineGivenEnumerationWithEmptySegmentLeavesDelimiter()
        {
            Assert.Equal("parent:", HelpPath.Combine(new List<string> { "parent", ""}));
            Assert.Equal("parent::", HelpPath.Combine(new List<string> { "parent", "", ""}));
            Assert.Equal("parent:::key", HelpPath.Combine(new List<string> {"parent", "", "", "key"}));
        }

        [Fact]
        public void GetLastSegmenGetSectionKeyTests()
        {
            Assert.Null(HelpPath.GetSectionKey(null));
            Assert.Equal("", HelpPath.GetSectionKey(""));
            Assert.Equal("", HelpPath.GetSectionKey(":::"));
            Assert.Equal("c", HelpPath.GetSectionKey("a::b:::c"));
            Assert.Equal("", HelpPath.GetSectionKey("a:::b:"));
            Assert.Equal("key", HelpPath.GetSectionKey("key"));
            Assert.Equal("key", HelpPath.GetSectionKey(":key"));
            Assert.Equal("key", HelpPath.GetSectionKey("::key"));
            Assert.Equal("key", HelpPath.GetSectionKey("parent:key"));
        }

        [Fact]
        public void GetParentPathTests()
        {
            Assert.Null(HelpPath.GetParentPath(null));
            Assert.Null(HelpPath.GetParentPath(""));
            Assert.Equal("::", HelpPath.GetParentPath(":::"));
            Assert.Equal("a::b::", HelpPath.GetParentPath("a::b:::c"));
            Assert.Equal("a:::b", HelpPath.GetParentPath("a:::b:"));
            Assert.Null(HelpPath.GetParentPath("key"));
            Assert.Equal("", HelpPath.GetParentPath(":key"));
            Assert.Equal(":", HelpPath.GetParentPath("::key"));
            Assert.Equal("parent", HelpPath.GetParentPath("parent:key"));
        }
    }
}
