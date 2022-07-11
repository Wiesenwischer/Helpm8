using System;
using System.IO;
using Xunit;

namespace Helpm8.Json.Tests
{
    public class HelpBuilderExtensionsTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void AddJsonFile_ThrowsIfFilePathIsNullOrEmpty(string path)
        {
            // Arrange
            var helpBuilder = new HelpBuilder();

            // Act and Assert
            var ex = Assert.Throws<ArgumentException>(() => helpBuilder.AddJsonFile(path));
            Assert.Equal("path", ex.ParamName);
            Assert.StartsWith("File path must be a non-empty string.", ex.Message);
        }

        [Fact]
        public void AddJsonFile_ThrowsIfFileDoesNotExistAtPath()
        {
            // Arrange
            var path = "file-does-not-exist.json";

            // Act and Assert
            var ex = Assert.Throws<FileNotFoundException>(() => new HelpBuilder().AddJsonFile(path).Build());
            Assert.StartsWith($"The help file '{path}' was not found and is not optional. The expected physical path was '", ex.Message);
        }
    }
}
