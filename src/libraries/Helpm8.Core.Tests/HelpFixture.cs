using System;
using System.Collections.Generic;
using System.Linq;
using Helpm8;
using Helpm8.Core.Tests;
using Helpm8.InMemory;
using Xunit;

namespace Microsoft.Extensions.Help.Test
{
    public class HelpFixture
    {
        [Fact]
        public void LoadAndCombineKeyValuePairsFromDifferentHelpProviders()
        {
            // Arrange
            var dic1 = new Dictionary<string, string>()
            {
                {"Mem1:KeyInMem1", "ValueInMem1"}
            };
            var dic2 = new Dictionary<string, string>()
            {
                {"Mem2:KeyInMem2", "ValueInMem2"}
            };
            var dic3 = new Dictionary<string, string>()
            {
                {"Mem3:KeyInMem3", "ValueInMem3"}
            };
            var memConfigSrc1 = new InMemoryHelpSource { InitialData = dic1 };
            var memConfigSrc2 = new InMemoryHelpSource { InitialData = dic2 };
            var memConfigSrc3 = new InMemoryHelpSource { InitialData = dic3 };

            var helpBuilder = new HelpBuilder();

            // Act
            helpBuilder.Add(memConfigSrc1);
            helpBuilder.Add(memConfigSrc2);
            helpBuilder.Add(memConfigSrc3);

            var help = helpBuilder.Build();

            var memVal1 = help["mem1:keyinmem1"];
            var memVal2 = help["Mem2:KeyInMem2"];
            var memVal3 = help["MEM3:KEYINMEM3"];

            // Assert
            Assert.Contains(memConfigSrc1, helpBuilder.Sources);
            Assert.Contains(memConfigSrc2, helpBuilder.Sources);
            Assert.Contains(memConfigSrc3, helpBuilder.Sources);

            Assert.Equal("ValueInMem1", memVal1);
            Assert.Equal("ValueInMem2", memVal2);
            Assert.Equal("ValueInMem3", memVal3);

            Assert.Equal("ValueInMem1", help["mem1:keyinmem1"]);
            Assert.Equal("ValueInMem2", help["Mem2:KeyInMem2"]);
            Assert.Equal("ValueInMem3", help["MEM3:KEYINMEM3"]);
            Assert.Null(help["NotExist"]);
        }

        [Fact]
        private void GetChildKeys_CanChainEmptyKeys()
        {
            var input = new Dictionary<string, string>() { };
            for (int i = 0; i < 1000; i++)
            {
                input.Add(new string(' ', i), string.Empty);
            }

            IHelpRoot helpRoot = new HelpBuilder()
                .Add(new InMemoryHelpSource
                {
                    InitialData = input
                })
                .Build();

            var chainedHelpSource = new ChainedHelpSource
            {
                Help = helpRoot,
                ShouldDisposeHelp = false,
            };

            var chainedHelp = new ChainedHelpProvider(chainedHelpSource);
            IEnumerable<string> childKeys = chainedHelp.GetChildKeys(new string[0], null);
            Assert.Equal(1000, childKeys.Count());
            Assert.Equal(string.Empty, childKeys.First());
            Assert.Equal(999, childKeys.Last().Length);
        }

        [Fact]
        private void GetChildKeys_CanChainKeyWithNoDelimiter()
        {
            var input = new Dictionary<string, string>() { };
            for (int i = 1000; i < 2000; i++)
            {
                input.Add(i.ToString(), string.Empty);
            }

            IHelpRoot helpRoot = new HelpBuilder()
                .Add(new InMemoryHelpSource
                {
                    InitialData = input
                })
                .Build();

            var chainedHelpSource = new ChainedHelpSource
            {
                Help = helpRoot,
                ShouldDisposeHelp = false,
            };

            var chainedHelp = new ChainedHelpProvider(chainedHelpSource);
            IEnumerable<string> childKeys = chainedHelp.GetChildKeys(new string[0], null);
            Assert.Equal(1000, childKeys.Count());
            Assert.Equal("1000", childKeys.First());
            Assert.Equal("1999", childKeys.Last());
        }

        [Fact]
        public void CanChainHelp()
        {
            // Arrange
            var dic1 = new Dictionary<string, string>()
            {
                {"Mem1:KeyInMem1", "ValueInMem1"}
            };
            var dic2 = new Dictionary<string, string>()
            {
                {"Mem2:KeyInMem2", "ValueInMem2"}
            };
            var dic3 = new Dictionary<string, string>()
            {
                {"Mem3:KeyInMem3", "ValueInMem3"}
            };
            var memConfigSrc1 = new InMemoryHelpSource { InitialData = dic1 };
            var memConfigSrc2 = new InMemoryHelpSource { InitialData = dic2 };
            var memConfigSrc3 = new InMemoryHelpSource { InitialData = dic3 };

            var helpBuilder = new HelpBuilder();

            // Act
            helpBuilder.Add(memConfigSrc1);
            helpBuilder.Add(memConfigSrc2);
            helpBuilder.Add(memConfigSrc3);

            var help = helpBuilder.Build();

            var chained = new HelpBuilder().AddHelp(help).Build();
            var memVal1 = chained["mem1:keyinmem1"];
            var memVal2 = chained["Mem2:KeyInMem2"];
            var memVal3 = chained["MEM3:KEYINMEM3"];

            // Assert

            Assert.Equal("ValueInMem1", memVal1);
            Assert.Equal("ValueInMem2", memVal2);
            Assert.Equal("ValueInMem3", memVal3);

            Assert.Null(chained["NotExist"]);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ChainedAsEnumerateFlattensIntoDictionaryTest(bool removePath)
        {
            // Arrange
            var dic1 = new Dictionary<string, string>()
            {
                {"Mem1", "Value1"},
                {"Mem1:", "NoKeyValue1"},
                {"Mem1:KeyInMem1", "ValueInMem1"},
                {"Mem1:KeyInMem1:Deep1", "ValueDeep1"}
            };
            var dic2 = new Dictionary<string, string>()
            {
                {"Mem2", "Value2"},
                {"Mem2:", "NoKeyValue2"},
                {"Mem2:KeyInMem2", "ValueInMem2"},
                {"Mem2:KeyInMem2:Deep2", "ValueDeep2"}
            };
            var dic3 = new Dictionary<string, string>()
            {
                {"Mem3", "Value3"},
                {"Mem3:", "NoKeyValue3"},
                {"Mem3:KeyInMem3", "ValueInMem3"},
                {"Mem3:KeyInMem3:Deep3", "ValueDeep3"}
            };
            var memConfigSrc1 = new InMemoryHelpSource { InitialData = dic1 };
            var memConfigSrc2 = new InMemoryHelpSource { InitialData = dic2 };
            var memConfigSrc3 = new InMemoryHelpSource { InitialData = dic3 };

            var helpBuilder = new HelpBuilder();

            // Act
            helpBuilder.Add(memConfigSrc1);
            helpBuilder.Add(memConfigSrc2);
            var help = new HelpBuilder()
                .AddHelp(helpBuilder.Build())
                .Add(memConfigSrc3)
                .Build();
            var dict = help.AsEnumerable(makePathsRelative: removePath).ToDictionary(k => k.Key, v => v.Value);

            // Assert
            Assert.Equal("Value1", dict["Mem1"]);
            Assert.Equal("NoKeyValue1", dict["Mem1:"]);
            Assert.Equal("ValueDeep1", dict["Mem1:KeyInMem1:Deep1"]);
            Assert.Equal("ValueInMem2", dict["Mem2:KeyInMem2"]);
            Assert.Equal("Value2", dict["Mem2"]);
            Assert.Equal("NoKeyValue2", dict["Mem2:"]);
            Assert.Equal("ValueDeep2", dict["Mem2:KeyInMem2:Deep2"]);
            Assert.Equal("Value3", dict["Mem3"]);
            Assert.Equal("NoKeyValue3", dict["Mem3:"]);
            Assert.Equal("ValueInMem3", dict["Mem3:KeyInMem3"]);
            Assert.Equal("ValueDeep3", dict["Mem3:KeyInMem3:Deep3"]);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AsEnumerateFlattensIntoDictionaryTest(bool removePath)
        {
            // Arrange
            var dic1 = new Dictionary<string, string>()
            {
                {"Mem1", "Value1"},
                {"Mem1:", "NoKeyValue1"},
                {"Mem1:KeyInMem1", "ValueInMem1"},
                {"Mem1:KeyInMem1:Deep1", "ValueDeep1"}
            };
            var dic2 = new Dictionary<string, string>()
            {
                {"Mem2", "Value2"},
                {"Mem2:", "NoKeyValue2"},
                {"Mem2:KeyInMem2", "ValueInMem2"},
                {"Mem2:KeyInMem2:Deep2", "ValueDeep2"}
            };
            var dic3 = new Dictionary<string, string>()
            {
                {"Mem3", "Value3"},
                {"Mem3:", "NoKeyValue3"},
                {"Mem3:KeyInMem3", "ValueInMem3"},
                {"Mem3:KeyInMem3:Deep3", "ValueDeep3"}
            };
            var memConfigSrc1 = new InMemoryHelpSource { InitialData = dic1 };
            var memConfigSrc2 = new InMemoryHelpSource { InitialData = dic2 };
            var memConfigSrc3 = new InMemoryHelpSource { InitialData = dic3 };

            var helpBuilder = new HelpBuilder();

            // Act
            helpBuilder.Add(memConfigSrc1);
            helpBuilder.Add(memConfigSrc2);
            helpBuilder.Add(memConfigSrc3);
            var help = helpBuilder.Build();
            var dict = help.AsEnumerable(makePathsRelative: removePath).ToDictionary(k => k.Key, v => v.Value);

            // Assert
            Assert.Equal("Value1", dict["Mem1"]);
            Assert.Equal("NoKeyValue1", dict["Mem1:"]);
            Assert.Equal("ValueDeep1", dict["Mem1:KeyInMem1:Deep1"]);
            Assert.Equal("ValueInMem2", dict["Mem2:KeyInMem2"]);
            Assert.Equal("Value2", dict["Mem2"]);
            Assert.Equal("NoKeyValue2", dict["Mem2:"]);
            Assert.Equal("ValueDeep2", dict["Mem2:KeyInMem2:Deep2"]);
            Assert.Equal("Value3", dict["Mem3"]);
            Assert.Equal("NoKeyValue3", dict["Mem3:"]);
            Assert.Equal("ValueInMem3", dict["Mem3:KeyInMem3"]);
            Assert.Equal("ValueDeep3", dict["Mem3:KeyInMem3:Deep3"]);
        }

        [Fact]
        public void AsEnumerateStripsKeyFromChildren()
        {
            // Arrange
            var dic1 = new Dictionary<string, string>()
            {
                {"Mem1", "Value1"},
                {"Mem1:", "NoKeyValue1"},
                {"Mem1:KeyInMem1", "ValueInMem1"},
                {"Mem1:KeyInMem1:Deep1", "ValueDeep1"}
            };
            var dic2 = new Dictionary<string, string>()
            {
                {"Mem2", "Value2"},
                {"Mem2:", "NoKeyValue2"},
                {"Mem2:KeyInMem2", "ValueInMem2"},
                {"Mem2:KeyInMem2:Deep2", "ValueDeep2"}
            };
            var dic3 = new Dictionary<string, string>()
            {
                {"Mem3", "Value3"},
                {"Mem3:", "NoKeyValue3"},
                {"Mem3:KeyInMem3", "ValueInMem3"},
                {"Mem3:KeyInMem4", "ValueInMem4"},
                {"Mem3:KeyInMem3:Deep3", "ValueDeep3"},
                {"Mem3:KeyInMem3:Deep4", "ValueDeep4"}
            };
            var memConfigSrc1 = new InMemoryHelpSource { InitialData = dic1 };
            var memConfigSrc2 = new InMemoryHelpSource { InitialData = dic2 };
            var memConfigSrc3 = new InMemoryHelpSource { InitialData = dic3 };

            var helpBuilder = new HelpBuilder();

            // Act
            helpBuilder.Add(memConfigSrc1);
            helpBuilder.Add(memConfigSrc2);
            helpBuilder.Add(memConfigSrc3);

            var help = helpBuilder.Build();

            var dict = help.GetSection("Mem1").AsEnumerable(makePathsRelative: true).ToDictionary(k => k.Key, v => v.Value);
            Assert.Equal(3, dict.Count);
            Assert.Equal("NoKeyValue1", dict[""]);
            Assert.Equal("ValueInMem1", dict["KeyInMem1"]);
            Assert.Equal("ValueDeep1", dict["KeyInMem1:Deep1"]);

            var dict2 = help.GetSection("Mem2").AsEnumerable(makePathsRelative: true).ToDictionary(k => k.Key, v => v.Value);
            Assert.Equal(3, dict2.Count);
            Assert.Equal("NoKeyValue2", dict2[""]);
            Assert.Equal("ValueInMem2", dict2["KeyInMem2"]);
            Assert.Equal("ValueDeep2", dict2["KeyInMem2:Deep2"]);

            var dict3 = help.GetSection("Mem3").AsEnumerable(makePathsRelative: true).ToDictionary(k => k.Key, v => v.Value);
            Assert.Equal(5, dict3.Count);
            Assert.Equal("NoKeyValue3", dict3[""]);
            Assert.Equal("ValueInMem3", dict3["KeyInMem3"]);
            Assert.Equal("ValueInMem4", dict3["KeyInMem4"]);
            Assert.Equal("ValueDeep3", dict3["KeyInMem3:Deep3"]);
            Assert.Equal("ValueDeep4", dict3["KeyInMem3:Deep4"]);
        }


        [Fact]
        public void NewHelpProviderOverridesOldOneWhenKeyIsDuplicated()
        {
            // Arrange
            var dic1 = new Dictionary<string, string>()
                {
                    {"Key1:Key2", "ValueInMem1"}
                };
            var dic2 = new Dictionary<string, string>()
                {
                    {"Key1:Key2", "ValueInMem2"}
                };
            var memConfigSrc1 = new InMemoryHelpSource { InitialData = dic1 };
            var memConfigSrc2 = new InMemoryHelpSource { InitialData = dic2 };

            var helpBuilder = new HelpBuilder();

            // Act
            helpBuilder.Add(memConfigSrc1);
            helpBuilder.Add(memConfigSrc2);

            var help = helpBuilder.Build();

            // Assert
            Assert.Equal("ValueInMem2", help["Key1:Key2"]);
        }

        [Fact]
        public void NewHelpRootMayBeBuiltFromExistingWithDuplicateKeys()
        {
            var helpRoot = new HelpBuilder()
                                    .AddInMemoryCollection(new Dictionary<string, string>
                                        {
                                            {"keya:keyb", "valueA"},
                                        })
                                    .AddInMemoryCollection(new Dictionary<string, string>
                                        {
                                            {"KEYA:KEYB", "valueB"}
                                        })
                                    .Build();
            var newHelpRoot = new HelpBuilder()
                .AddInMemoryCollection(helpRoot.AsEnumerable())
                .Build();
            Assert.Equal("valueB", newHelpRoot["keya:keyb"]);
        }

        public class TestMemorySourceProvider : InMemoryHelpProvider, IHelpSource
        {
            public TestMemorySourceProvider(Dictionary<string, string> initialData)
                : base(new InMemoryHelpSource { InitialData = initialData })
            { }

            public IHelpProvider Build(IHelpBuilder builder)
            {
                return this;
            }
        }

        [Fact]
        public void SettingValueUpdatesAllHelpProviders()
        {
            // Arrange
            var dict = new Dictionary<string, string>()
            {
                {"Key1", "Value1"},
                {"Key2", "Value2"}
            };

            var memConfigSrc1 = new TestMemorySourceProvider(dict);
            var memConfigSrc2 = new TestMemorySourceProvider(dict);
            var memConfigSrc3 = new TestMemorySourceProvider(dict);

            var helpBuilder = new HelpBuilder();

            helpBuilder.Add(memConfigSrc1);
            helpBuilder.Add(memConfigSrc2);
            helpBuilder.Add(memConfigSrc3);

            var help = helpBuilder.Build();

            // Act
            help["Key1"] = "NewValue1";
            help["Key2"] = "NewValue2";

            var memConfigProvider1 = memConfigSrc1.Build(helpBuilder);
            var memConfigProvider2 = memConfigSrc2.Build(helpBuilder);
            var memConfigProvider3 = memConfigSrc3.Build(helpBuilder);

            // Assert
            Assert.Equal("NewValue1", help["Key1"]);
            Assert.Equal("NewValue1", memConfigProvider1.Get("Key1"));
            Assert.Equal("NewValue1", memConfigProvider2.Get("Key1"));
            Assert.Equal("NewValue1", memConfigProvider3.Get("Key1"));
            Assert.Equal("NewValue2", help["Key2"]);
            Assert.Equal("NewValue2", memConfigProvider1.Get("Key2"));
            Assert.Equal("NewValue2", memConfigProvider2.Get("Key2"));
            Assert.Equal("NewValue2", memConfigProvider3.Get("Key2"));
        }

        [Fact]
        public void CanGetHelpSection()
        {
            // Arrange
            var dic1 = new Dictionary<string, string>()
            {
                {"Data:DB1:Connection1", "MemVal1"},
                {"Data:DB1:Connection2", "MemVal2"}
            };
            var dic2 = new Dictionary<string, string>()
            {
                {"DataSource:DB2:Connection", "MemVal3"}
            };
            var dic3 = new Dictionary<string, string>()
            {
                {"Data", "MemVal4"}
            };
            var memConfigSrc1 = new InMemoryHelpSource { InitialData = dic1 };
            var memConfigSrc2 = new InMemoryHelpSource { InitialData = dic2 };
            var memConfigSrc3 = new InMemoryHelpSource { InitialData = dic3 };

            var helpBuilder = new HelpBuilder();
            helpBuilder.Add(memConfigSrc1);
            helpBuilder.Add(memConfigSrc2);
            helpBuilder.Add(memConfigSrc3);

            var help = helpBuilder.Build();

            // Act
            var helpFocus = help.GetSection("Data");

            var memVal1 = helpFocus["DB1:Connection1"];
            var memVal2 = helpFocus["DB1:Connection2"];
            var memVal3 = helpFocus["DB2:Connection"];
            var memVal4 = helpFocus["Source:DB2:Connection"];
            var memVal5 = helpFocus.Value;

            // Assert
            Assert.Equal("MemVal1", memVal1);
            Assert.Equal("MemVal2", memVal2);
            Assert.Equal("MemVal4", memVal5);

            Assert.Equal("MemVal1", helpFocus["DB1:Connection1"]);
            Assert.Equal("MemVal2", helpFocus["DB1:Connection2"]);
            Assert.Null(helpFocus["DB2:Connection"]);
            Assert.Null(helpFocus["Source:DB2:Connection"]);
            Assert.Equal("MemVal4", helpFocus.Value);
        }

        [Fact]
        public void CanGetHelpChildren()
        {
            // Arrange
            var dic1 = new Dictionary<string, string>()
            {
                {"Data:DB1:Connection1", "MemVal1"},
                {"Data:DB1:Connection2", "MemVal2"}
            };
            var dic2 = new Dictionary<string, string>()
            {
                {"Data:DB2Connection", "MemVal3"}
            };
            var dic3 = new Dictionary<string, string>()
            {
                {"DataSource:DB3:Connection", "MemVal4"}
            };
            var memConfigSrc1 = new InMemoryHelpSource { InitialData = dic1 };
            var memConfigSrc2 = new InMemoryHelpSource { InitialData = dic2 };
            var memConfigSrc3 = new InMemoryHelpSource { InitialData = dic3 };

            var helpBuilder = new HelpBuilder();
            helpBuilder.Add(memConfigSrc1);
            helpBuilder.Add(memConfigSrc2);
            helpBuilder.Add(memConfigSrc3);

            var help = helpBuilder.Build();

            // Act
            var helpSections = help.GetSection("Data").GetChildren().ToList();

            // Assert
            Assert.Equal(2, helpSections.Count());
            Assert.Equal("MemVal1", helpSections.FirstOrDefault(c => c.Key == "DB1")["Connection1"]);
            Assert.Equal("MemVal2", helpSections.FirstOrDefault(c => c.Key == "DB1")["Connection2"]);
            Assert.Equal("MemVal3", helpSections.FirstOrDefault(c => c.Key == "DB2Connection").Value);
            Assert.False(helpSections.Exists(c => c.Key == "DB3"));
            Assert.False(helpSections.Exists(c => c.Key == "DB3"));
        }

        [Fact]
        public void SourcesReturnsAddedHelpProviders()
        {
            // Arrange
            var dict = new Dictionary<string, string>()
            {
                {"Mem:KeyInMem", "MemVal"}
            };
            var memConfigSrc1 = new InMemoryHelpSource { InitialData = dict };
            var memConfigSrc2 = new InMemoryHelpSource { InitialData = dict };
            var memConfigSrc3 = new InMemoryHelpSource { InitialData = dict };

            var srcSet = new HashSet<IHelpSource>()
            {
                memConfigSrc1,
                memConfigSrc2,
                memConfigSrc3
            };

            var helpBuilder = new HelpBuilder();

            // Act
            helpBuilder.Add(memConfigSrc1);
            helpBuilder.Add(memConfigSrc2);
            helpBuilder.Add(memConfigSrc3);

            var help = helpBuilder.Build();

            // Assert
            Assert.Equal(new[] { memConfigSrc1, memConfigSrc2, memConfigSrc3 }, helpBuilder.Sources);
        }

        [Fact]
        public void SetValueThrowsExceptionNoSourceRegistered()
        {
            // Arrange
            var helpBuilder = new HelpBuilder();
            var help = helpBuilder.Build();

            var expectedMsg = "A help source is not registered. Please register one before setting a value.";

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => help["Title"] = "Welcome");

            // Assert
            Assert.Equal(expectedMsg, ex.Message);
        }

        [Fact]
        public void KeyStartingWithColonMeansFirstSectionHasEmptyName()
        {
            // Arrange
            var dict = new Dictionary<string, string>
            {
                [":Key2"] = "value"
            };
            var helpBuilder = new HelpBuilder();
            helpBuilder.AddInMemoryCollection(dict);
            var help = helpBuilder.Build();

            // Act
            var children = help.GetChildren().ToArray();

            // Assert
            Assert.Single(children);
            Assert.Equal(string.Empty, children.First().Key);
            Assert.Single(children.First().GetChildren());
            Assert.Equal("Key2", children.First().GetChildren().First().Key);
        }

        [Fact]
        public void KeyWithDoubleColonHasSectionWithEmptyName()
        {
            // Arrange
            var dict = new Dictionary<string, string>
            {
                ["Key1::Key3"] = "value"
            };
            var helpBuilder = new HelpBuilder();
            helpBuilder.AddInMemoryCollection(dict);
            var help = helpBuilder.Build();

            // Act
            var children = help.GetChildren().ToArray();

            // Assert
            Assert.Single(children);
            Assert.Equal("Key1", children.First().Key);
            Assert.Single(children.First().GetChildren());
            Assert.Equal(string.Empty, children.First().GetChildren().First().Key);
            Assert.Single(children.First().GetChildren().First().GetChildren());
            Assert.Equal("Key3", children.First().GetChildren().First().GetChildren().First().Key);
        }

        [Fact]
        public void KeyEndingWithColonMeansLastSectionHasEmptyName()
        {
            // Arrange
            var dict = new Dictionary<string, string>
            {
                ["Key1:"] = "value"
            };
            var helpBuilder = new HelpBuilder();
            helpBuilder.AddInMemoryCollection(dict);
            var help = helpBuilder.Build();

            // Act
            var children = help.GetChildren().ToArray();

            // Assert
            Assert.Single(children);
            Assert.Equal("Key1", children.First().Key);
            Assert.Single(children.First().GetChildren());
            Assert.Equal(string.Empty, children.First().GetChildren().First().Key);
        }

        [Fact]
        public void SectionWithValueExists()
        {
            // Arrange
            var dict = new Dictionary<string, string>()
            {
                {"Mem1", "Value1"},
                {"Mem1:KeyInMem1", "ValueInMem1"},
                {"Mem1:KeyInMem1:Deep1", "ValueDeep1"}
            };
            var helpBuilder = new HelpBuilder();
            helpBuilder.AddInMemoryCollection(dict);
            var help = helpBuilder.Build();

            // Act
            var sectionExists1 = help.GetSection("Mem1").Exists();
            var sectionExists2 = help.GetSection("Mem1:KeyInMem1").Exists();
            var sectionNotExists = help.GetSection("Mem2").Exists();

            // Assert
            Assert.True(sectionExists1);
            Assert.True(sectionExists2);
            Assert.False(sectionNotExists);
        }

        [Fact]
        public void SectionWithChildrenExists()
        {
            // Arrange
            var dict = new Dictionary<string, string>()
            {
                {"Mem1:KeyInMem1", "ValueInMem1"},
                {"Mem1:KeyInMem1:Deep1", "ValueDeep1"},
                {"Mem2:KeyInMem2:Deep1", "ValueDeep2"}
            };
            var helpBuilder = new HelpBuilder();
            helpBuilder.AddInMemoryCollection(dict);
            var help = helpBuilder.Build();

            // Act
            var sectionExists1 = help.GetSection("Mem1").Exists();
            var sectionExists2 = help.GetSection("Mem2").Exists();
            var sectionNotExists = help.GetSection("Mem3").Exists();

            // Assert
            Assert.True(sectionExists1);
            Assert.True(sectionExists2);
            Assert.False(sectionNotExists);
        }

        [Theory]
        [InlineData("Value1")]
        [InlineData("")]
        public void KeyWithValueAndWithoutChildrenExistsAsSection(string value)
        {
            // Arrange
            var dict = new Dictionary<string, string>()
            {
                {"Mem1", value}
            };
            var helpBuilder = new HelpBuilder();
            helpBuilder.AddInMemoryCollection(dict);
            var help = helpBuilder.Build();

            // Act
            var sectionExists = help.GetSection("Mem1").Exists();

            // Assert
            Assert.True(sectionExists);
        }

        [Fact]
        public void KeyWithNullValueAndWithoutChildrenIsASectionButNotExists()
        {
            // Arrange
            var dict = new Dictionary<string, string>()
            {
                {"Mem1", null}
            };
            var helpBuilder = new HelpBuilder();
            helpBuilder.AddInMemoryCollection(dict);
            var help = helpBuilder.Build();

            // Act
            var sections = help.GetChildren();
            var sectionExists = help.GetSection("Mem1").Exists();
            var sectionChildren = help.GetSection("Mem1").GetChildren();

            // Assert
            Assert.Single(sections, section => section.Key == "Mem1");
            Assert.False(sectionExists);
            Assert.Empty(sectionChildren);
        }

        [Fact]
        public void SectionWithChildrenHasNullValue()
        {
            // Arrange
            var dict = new Dictionary<string, string>()
            {
                {"Mem1:KeyInMem1", "ValueInMem1"},
            };
            var helpBuilder = new HelpBuilder();
            helpBuilder.AddInMemoryCollection(dict);
            var help = helpBuilder.Build();

            // Act
            var sectionValue = help.GetSection("Mem1").Value;

            // Assert
            Assert.Null(sectionValue);
        }

        [Fact]
        public void NullSectionDoesNotExist()
        {
            // Arrange
            // Act
            var sectionExists = HelpExtensions.Exists(null);

            // Assert
            Assert.False(sectionExists);
        }
    }
}