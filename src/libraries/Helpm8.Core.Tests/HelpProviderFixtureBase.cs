using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Helpm8.InMemory;
using Xunit;

namespace Helpm8.Core.Tests
{
    [ExcludeFromCodeCoverage]
    public abstract class HelpProviderFixtureBase
    {
        [Fact]
        public virtual void Load_from_single_provider()
        {
            var helpRoot = BuildHelpRoot(LoadThroughProvider(TestSection.TestHelp));

            AssertHelp(helpRoot);
        }

        [Fact]
        public virtual void Has_debug_view()
        {
            var helpRoot = BuildHelpRoot(LoadThroughProvider(TestSection.TestHelp));
            var providerTag = helpRoot.Providers.Single().ToString();

            var expected =
                $@"Key1=Value1 ({providerTag})
Section1:
  Key2=Value12 ({providerTag})
  Section2:
    Key3=Value123 ({providerTag})
    Key3a:
      0=ArrayValue0 ({providerTag})
      1=ArrayValue1 ({providerTag})
      2=ArrayValue2 ({providerTag})
Section3:
  Section4:
    Key4=Value344 ({providerTag})
";

            AssertDebugView(helpRoot, expected);
        }

        [Fact]
        public virtual void Null_values_are_included_in_the_help()
        {
            AssertHelp(BuildHelpRoot(LoadThroughProvider(TestSection.NullsTestHelp)), expectNulls: true, nullValue: "");
        }

        [Fact]
        public virtual void Combine_after_other_provider()
        {
            AssertHelp(
                BuildHelpRoot(
                    LoadUsingInMemoryProvider(TestSection.MissingSection2ValuesHelp),
                    LoadThroughProvider(TestSection.MissingSection4Help)));

            AssertHelp(
                BuildHelpRoot(
                    LoadUsingInMemoryProvider(TestSection.MissingSection4Help),
                    LoadThroughProvider(TestSection.MissingSection2ValuesHelp)));
        }

        [Fact]
        public virtual void Combine_before_other_provider()
        {
            AssertHelp(
                BuildHelpRoot(
                    LoadThroughProvider(TestSection.MissingSection2ValuesHelp),
                    LoadUsingInMemoryProvider(TestSection.MissingSection4Help)));

            AssertHelp(
                BuildHelpRoot(
                    LoadThroughProvider(TestSection.MissingSection4Help),
                    LoadUsingInMemoryProvider(TestSection.MissingSection2ValuesHelp)));
        }

        [Fact]
        public virtual void Second_provider_overrides_values_from_first()
        {
            AssertHelp(
                BuildHelpRoot(
                    LoadUsingInMemoryProvider(TestSection.NoValuesTestHelp),
                    LoadThroughProvider(TestSection.TestHelp)));
        }

        [Fact]
        public virtual void Combining_from_multiple_providers_is_case_insensitive()
        {
            AssertHelp(
                BuildHelpRoot(
                    LoadUsingInMemoryProvider(TestSection.DifferentCasedTestHelp),
                    LoadThroughProvider(TestSection.TestHelp)));
        }

        [Fact]
        public virtual void Load_from_single_provider_with_duplicates_throws()
        {
            AssertFormatOrArgumentException(
                () => BuildHelpRoot(LoadThroughProvider(TestSection.DuplicatesTestHelp)));
        }

        [Fact]
        public virtual void Load_from_single_provider_with_differing_case_duplicates_throws()
        {
            AssertFormatOrArgumentException(
                () => BuildHelpRoot(LoadThroughProvider(TestSection.DuplicatesDifferentCaseTestHelp)));
        }

        protected void AssertFormatOrArgumentException(Action test)
        {
            Exception caught = null;
            try
            {
                test();
            }
            catch (Exception e)
            {
                caught = e;
            }

            Assert.True(caught is ArgumentException
                        || caught is FormatException);
        }

        protected void AssertFormatOrArgumentException(Action test, string exceptionMessage)
        {
            Exception caught = null;
            try
            {
                test();
            }
            catch (Exception e)
            {
                caught = e;
            }

            Assert.True(caught is ArgumentException
                        || caught is FormatException);

            Assert.Equal(exceptionMessage, caught.Message);
        }

        public class AsOptions
        {
            public string Key1 { get; set; }

            public Section1AsOptions Section1 { get; set; }
            public Section3AsOptions Section3 { get; set; }
        }

        public class Section1AsOptions
        {
            public string Key2 { get; set; }

            public Section2AsOptions Section2 { get; set; }
        }

        public class Section2AsOptions
        {
            public string Key3 { get; set; }
            public string[] Key3a { get; set; }
        }

        public class Section3AsOptions
        {
            public Section4AsOptions Section4 { get; set; }
        }

        public class Section4AsOptions
        {
            public string Key4 { get; set; }
        }

        protected virtual void AssertDebugView(
            IHelpRoot help,
            string expected)
        {
            string RemoveLineEnds(string source) => source.Replace("\n", "").Replace("\r", "");

            var actual = help.GetDebugView();

            Assert.Equal(
                RemoveLineEnds(expected),
                RemoveLineEnds(actual));
        }

        protected virtual void AssertHelp(
            IHelpRoot help,
            bool expectNulls = false,
            string nullValue = null)
        {
            var value1 = expectNulls ? nullValue : "Value1";
            var value12 = expectNulls ? nullValue : "Value12";
            var value123 = expectNulls ? nullValue : "Value123";
            var arrayvalue0 = expectNulls ? nullValue : "ArrayValue0";
            var arrayvalue1 = expectNulls ? nullValue : "ArrayValue1";
            var arrayvalue2 = expectNulls ? nullValue : "ArrayValue2";
            var value344 = expectNulls ? nullValue : "Value344";

            Assert.Equal(value1, help["Key1"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(value12, help["Section1:Key2"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(value123, help["Section1:Section2:Key3"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(arrayvalue0, help["Section1:Section2:Key3a:0"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(arrayvalue1, help["Section1:Section2:Key3a:1"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(arrayvalue2, help["Section1:Section2:Key3a:2"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(value344, help["Section3:Section4:Key4"], StringComparer.InvariantCultureIgnoreCase);

            var section1 = help.GetSection("Section1");
            Assert.Equal(value12, section1["Key2"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(value123, section1["Section2:Key3"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(arrayvalue0, section1["Section2:Key3a:0"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(arrayvalue1, section1["Section2:Key3a:1"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(arrayvalue2, section1["Section2:Key3a:2"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal("Section1", section1.Path, StringComparer.InvariantCultureIgnoreCase);
            Assert.Null(section1.Value);

            var section2 = help.GetSection("Section1:Section2");
            Assert.Equal(value123, section2["Key3"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(arrayvalue0, section2["Key3a:0"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(arrayvalue1, section2["Key3a:1"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(arrayvalue2, section2["Key3a:2"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal("Section1:Section2", section2.Path, StringComparer.InvariantCultureIgnoreCase);
            Assert.Null(section2.Value);

            section2 = section1.GetSection("Section2");
            Assert.Equal(value123, section2["Key3"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(arrayvalue0, section2["Key3a:0"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(arrayvalue1, section2["Key3a:1"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(arrayvalue2, section2["Key3a:2"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal("Section1:Section2", section2.Path, StringComparer.InvariantCultureIgnoreCase);
            Assert.Null(section2.Value);

            var section3a = section2.GetSection("Key3a");
            Assert.Equal(arrayvalue0, section3a["0"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(arrayvalue1, section3a["1"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(arrayvalue2, section3a["2"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal("Section1:Section2:Key3a", section3a.Path, StringComparer.InvariantCultureIgnoreCase);
            Assert.Null(section3a.Value);

            var section3 = help.GetSection("Section3");
            Assert.Equal("Section3", section3.Path, StringComparer.InvariantCultureIgnoreCase);
            Assert.Null(section3.Value);

            var section4 = help.GetSection("Section3:Section4");
            Assert.Equal(value344, section4["Key4"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal("Section3:Section4", section4.Path, StringComparer.InvariantCultureIgnoreCase);
            Assert.Null(section4.Value);

            section4 = help.GetSection("Section3").GetSection("Section4");
            Assert.Equal(value344, section4["Key4"], StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal("Section3:Section4", section4.Path, StringComparer.InvariantCultureIgnoreCase);
            Assert.Null(section4.Value);

            var sections = help.GetChildren().ToList();

            Assert.Equal(3, sections.Count);

            Assert.Equal("Key1", sections[0].Key, StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal("Key1", sections[0].Path, StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(value1, sections[0].Value, StringComparer.InvariantCultureIgnoreCase);

            Assert.Equal("Section1", sections[1].Key, StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal("Section1", sections[1].Path, StringComparer.InvariantCultureIgnoreCase);
            Assert.Null(sections[1].Value);

            Assert.Equal("Section3", sections[2].Key, StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal("Section3", sections[2].Path, StringComparer.InvariantCultureIgnoreCase);
            Assert.Null(sections[2].Value);

            sections = section1.GetChildren().ToList();

            Assert.Equal(2, sections.Count);

            Assert.Equal("Key2", sections[0].Key, StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal("Section1:Key2", sections[0].Path, StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(value12, sections[0].Value, StringComparer.InvariantCultureIgnoreCase);

            Assert.Equal("Section2", sections[1].Key, StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal("Section1:Section2", sections[1].Path, StringComparer.InvariantCultureIgnoreCase);
            Assert.Null(sections[1].Value);

            sections = section2.GetChildren().ToList();

            Assert.Equal(2, sections.Count);

            Assert.Equal("Key3", sections[0].Key, StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal("Section1:Section2:Key3", sections[0].Path, StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(value123, sections[0].Value, StringComparer.InvariantCultureIgnoreCase);

            Assert.Equal("Key3a", sections[1].Key, StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal("Section1:Section2:Key3a", sections[1].Path, StringComparer.InvariantCultureIgnoreCase);
            Assert.Null(sections[1].Value);

            sections = section3a.GetChildren().ToList();

            Assert.Equal(3, sections.Count);

            Assert.Equal("0", sections[0].Key, StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal("Section1:Section2:Key3a:0", sections[0].Path, StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(arrayvalue0, sections[0].Value, StringComparer.InvariantCultureIgnoreCase);

            Assert.Equal("1", sections[1].Key, StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal("Section1:Section2:Key3a:1", sections[1].Path, StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(arrayvalue1, sections[1].Value, StringComparer.InvariantCultureIgnoreCase);

            Assert.Equal("2", sections[2].Key, StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal("Section1:Section2:Key3a:2", sections[2].Path, StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(arrayvalue2, sections[2].Value, StringComparer.InvariantCultureIgnoreCase);

            sections = section3.GetChildren().ToList();

            Assert.Single(sections);

            Assert.Equal("Section4", sections[0].Key, StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal("Section3:Section4", sections[0].Path, StringComparer.InvariantCultureIgnoreCase);
            Assert.Null(sections[0].Value);

            sections = section4.GetChildren().ToList();

            Assert.Single(sections);

            Assert.Equal("Key4", sections[0].Key, StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal("Section3:Section4:Key4", sections[0].Path, StringComparer.InvariantCultureIgnoreCase);
            Assert.Equal(value344, sections[0].Value, StringComparer.InvariantCultureIgnoreCase);
        }

        protected abstract (IHelpProvider Provider, Action Initializer) LoadThroughProvider(TestSection testHelp);

        protected virtual IHelpRoot BuildHelpRoot(
            params (IHelpProvider Provider, Action Initializer)[] providers)
        {
            var root = new HelpRoot(providers.Select(e => e.Provider).ToList());

            foreach (var initializer in providers.Select(e => e.Initializer))
            {
                initializer();
            }

            return root;
        }

        protected static (IHelpProvider Provider, Action Initializer) LoadUsingInMemoryProvider(TestSection testHelp)
        {
            var values = new List<KeyValuePair<string, string>>();
            SectionToValues(testHelp, "", values);

            return (new InMemoryHelpProvider(
                    new InMemoryHelpSource
                    {
                        InitialData = values
                    }),
                () => { }
            );
        }

        protected static void SectionToValues(
            TestSection section,
            string sectionName,
            IList<KeyValuePair<string, string>> values)
        {
            foreach (var tuple in section.Values.SelectMany(e => e.Value.Expand(e.Key)))
            {
                values.Add(new KeyValuePair<string, string>(sectionName + tuple.Key, tuple.Value));
            }

            foreach (var tuple in section.Sections)
            {
                SectionToValues(
                    tuple.Section,
                    sectionName + tuple.Key + ":",
                    values);
            }
        }

        protected class TestKeyValue
        {
            public object Value { get; }

            public TestKeyValue(string value)
            {
                Value = value;
            }

            public TestKeyValue(int value)
            {
                Value = value;
            }

            public TestKeyValue(string[] values)
            {
                Value = values;
            }

            public static implicit operator TestKeyValue(string value) => new TestKeyValue(value);
            public static implicit operator TestKeyValue(string[] values) => new TestKeyValue(values);

            public string[] AsArray => Value as string[];

            public string AsString => Value as string;

            public IEnumerable<(string Key, string Value)> Expand(string key)
            {
                if (AsArray == null)
                {
                    yield return (key, AsString);
                }
                else
                {
                    for (var i = 0; i < AsArray.Length; i++)
                    {
                        yield return ($"{key}:{i}", AsArray[i]);
                    }
                }
            }
        }

        protected class TestSection
        {
            public IEnumerable<(string Key, TestKeyValue Value)> Values { get; set; }
                = Enumerable.Empty<(string, TestKeyValue)>();

            public IEnumerable<(string Key, TestSection Section)> Sections { get; set; }
                = Enumerable.Empty<(string, TestSection)>();

            public static TestSection TestHelp { get; }
                = new TestSection
                {
                    Values = new[] { ("Key1", (TestKeyValue)"Value1") },
                    Sections = new[]
                    {
                        ("Section1", new TestSection
                        {
                            Values = new[] {("Key2", (TestKeyValue)"Value12")},
                            Sections = new[]
                            {
                                ("Section2", new TestSection
                                {
                                    Values = new[]
                                    {
                                        ("Key3", (TestKeyValue)"Value123"),
                                        ("Key3a", (TestKeyValue)new[] {"ArrayValue0", "ArrayValue1", "ArrayValue2"})
                                    },
                                })
                            }
                        }),
                        ("Section3", new TestSection
                        {
                            Sections = new[]
                            {
                                ("Section4", new TestSection
                                {
                                    Values = new[] {("Key4", (TestKeyValue)"Value344")}
                                })
                            }
                        })
                    }
                };

            public static TestSection NoValuesTestHelp { get; }
                = new TestSection
                {
                    Values = new[] { ("Key1", (TestKeyValue)"------") },
                    Sections = new[]
                    {
                        ("Section1", new TestSection
                        {
                            Values = new[] {("Key2", (TestKeyValue)"-------")},
                            Sections = new[]
                            {
                                ("Section2", new TestSection
                                {
                                    Values = new[]
                                    {
                                        ("Key3", (TestKeyValue)"-----"),
                                        ("Key3a", (TestKeyValue)new[] {"-----------", "-----------", "-----------"})
                                    },
                                })
                            }
                        }),
                        ("Section3", new TestSection
                        {
                            Sections = new[]
                            {
                                ("Section4", new TestSection
                                {
                                    Values = new[] {("Key4", (TestKeyValue)"--------")}
                                })
                            }
                        })
                    }
                };

            public static TestSection MissingSection2ValuesHelp { get; }
                = new TestSection
                {
                    Values = new[] { ("Key1", (TestKeyValue)"Value1") },
                    Sections = new[]
                    {
                        ("Section1", new TestSection
                        {
                            Values = new[] {("Key2", (TestKeyValue)"Value12")},
                            Sections = new[]
                            {
                                ("Section2", new TestSection
                                {
                                    Values = new[]
                                    {
                                        ("Key3a", (TestKeyValue)new[] {"ArrayValue0"})
                                    },
                                })
                            }
                        }),
                        ("Section3", new TestSection
                        {
                            Sections = new[]
                            {
                                ("Section4", new TestSection
                                {
                                    Values = new[] {("Key4", (TestKeyValue)"Value344")}
                                })
                            }
                        })
                    }
                };


            public static TestSection MissingSection4Help { get; }
                = new TestSection
                {
                    Values = new[] { ("Key1", (TestKeyValue)"Value1") },
                    Sections = new[]
                    {
                        ("Section1", new TestSection
                        {
                            Values = new[] {("Key2", (TestKeyValue)"Value12")},
                            Sections = new[]
                            {
                                ("Section2", new TestSection
                                {
                                    Values = new[]
                                    {
                                        ("Key3", (TestKeyValue)"Value123"),
                                        ("Key3a", (TestKeyValue)new[] {"ArrayValue0", "ArrayValue1", "ArrayValue2"})
                                    },
                                })
                            }
                        }),
                        ("Section3", new TestSection())
                    }
                };

            public static TestSection DifferentCasedTestHelp { get; }
                = new TestSection
                {
                    Values = new[] { ("KeY1", (TestKeyValue)"Value1") },
                    Sections = new[]
                    {
                        ("SectioN1", new TestSection
                        {
                            Values = new[] {("KeY2", (TestKeyValue)"Value12")},
                            Sections = new[]
                            {
                                ("SectioN2", new TestSection
                                {
                                    Values = new[]
                                    {
                                        ("KeY3", (TestKeyValue)"Value123"),
                                        ("KeY3a", (TestKeyValue)new[] {"ArrayValue0", "ArrayValue1", "ArrayValue2"})
                                    },
                                })
                            }
                        }),
                        ("SectioN3", new TestSection
                        {
                            Sections = new[]
                            {
                                ("SectioN4", new TestSection
                                {
                                    Values = new[] {("KeY4", (TestKeyValue)"Value344")}
                                })
                            }
                        })
                    }
                };

            public static TestSection DuplicatesTestHelp { get; }
                = new TestSection
                {
                    Values = new[]
                    {
                        ("Key1", (TestKeyValue)"Value1"),
                        ("Key1", (TestKeyValue)"Value1")
                    },
                    Sections = new[]
                    {
                        ("Section1", new TestSection
                        {
                            Values = new[] {("Key2", (TestKeyValue)"Value12")},
                            Sections = new[]
                            {
                                ("Section2", new TestSection
                                {
                                    Values = new[]
                                    {
                                        ("Key3", (TestKeyValue)"Value123"),
                                        ("Key3a", (TestKeyValue)new[] {"ArrayValue0", "ArrayValue1", "ArrayValue2"})
                                    },
                                }),
                                ("Section2", new TestSection
                                {
                                    Values = new[]
                                    {
                                        ("Key3", (TestKeyValue)"Value123"),
                                        ("Key3a", (TestKeyValue)new[] {"ArrayValue0", "ArrayValue1", "ArrayValue2"})
                                    },
                                })

                            }
                        }),
                        ("Section3", new TestSection
                        {
                            Sections = new[]
                            {
                                ("Section4", new TestSection
                                {
                                    Values = new[] {("Key4", (TestKeyValue)"Value344")}
                                })
                            }
                        })
                    }
                };

            public static TestSection DuplicatesTestHelpWithReplacingValues { get; }
                = new TestSection
                {
                    Values = new[]
                    {
                        ("Key1", (TestKeyValue)"Value1"),
                        ("Key1", (TestKeyValue)"Value1Replaced")
                    },
                    Sections = new[]
                    {
                        ("Section1", new TestSection
                        {
                            Values = new[] {("Key2", (TestKeyValue)"Value12")},
                            Sections = new[]
                            {
                                ("Section2", new TestSection
                                {
                                    Values = new[]
                                    {
                                        ("Key3", (TestKeyValue)"Value123"),
                                        ("Key3a", (TestKeyValue)new[] {"ArrayValue0", "ArrayValue1", "ArrayValue2"})
                                    },
                                }),
                                ("Section2", new TestSection
                                {
                                    Values = new[]
                                    {
                                        ("Key3", (TestKeyValue)"Value123Replaced"),
                                        ("Key3a", (TestKeyValue)new[] {"ArrayValue0Replaced", "ArrayValue1Replaced", "ArrayValue2Replaced"})
                                    },
                                })

                            }
                        }),
                        ("Section3", new TestSection
                        {
                            Sections = new[]
                            {
                                ("Section4", new TestSection
                                {
                                    Values = new[] {("Key4", (TestKeyValue)"Value344")}
                                })
                            }
                        })
                    }
                };

            public static TestSection DuplicatesDifferentCaseTestHelp { get; }
                = new TestSection
                {
                    Values = new[]
                    {
                        ("Key1", (TestKeyValue)"Value1"),
                        ("KeY1", (TestKeyValue)"Value1")
                    },
                    Sections = new[]
                    {
                        ("Section1", new TestSection
                        {
                            Values = new[] {("Key2", (TestKeyValue)"Value12")},
                            Sections = new[]
                            {
                                ("Section2", new TestSection
                                {
                                    Values = new[]
                                    {
                                        ("Key3", (TestKeyValue)"Value123"),
                                        ("Key3a", (TestKeyValue)new[] {"ArrayValue0", "ArrayValue1", "ArrayValue2"})
                                    },
                                }),
                                ("SectioN2", new TestSection
                                {
                                    Values = new[]
                                    {
                                        ("KeY3", (TestKeyValue)"Value123"),
                                        ("KeY3a", (TestKeyValue)new[] {"ArrayValue0", "ArrayValue1", "ArrayValue2"})
                                    },
                                })

                            }
                        }),
                        ("Section3", new TestSection
                        {
                            Sections = new[]
                            {
                                ("Section4", new TestSection
                                {
                                    Values = new[] {("Key4", (TestKeyValue)"Value344")}
                                })
                            }
                        })
                    }
                };

            public static TestSection NullsTestHelp { get; }
                = new TestSection
                {
                    Values = new[] { ("Key1", new TestKeyValue((string)null)) },
                    Sections = new[]
                    {
                        ("Section1", new TestSection
                        {
                            Values = new[] {("Key2", new TestKeyValue((string)null))},
                            Sections = new[]
                            {
                                ("Section2", new TestSection
                                {
                                    Values = new[]
                                    {
                                        ("Key3", new TestKeyValue((string)null)),
                                        ("Key3a", (TestKeyValue)new string[] {null, null, null})
                                    },
                                })
                            }
                        }),
                        ("Section3", new TestSection
                        {
                            Sections = new[]
                            {
                                ("Section4", new TestSection
                                {
                                    Values = new[] {("Key4", new TestKeyValue((string)null))}
                                })
                            }
                        })
                    }
                };

            public static TestSection ExtraValuesTestHelp { get; }
                = new TestSection
                {
                    Values = new[]
                    {
                        ("Key1", (TestKeyValue)"Value1"),
                        ("Key1r", (TestKeyValue)"Value1r")
                    },
                    Sections = new[]
                    {
                        ("Section1", new TestSection
                        {
                            Values = new[]
                            {
                                ("Key2", (TestKeyValue)"Value12"),
                                ("Key2r", (TestKeyValue)"Value12r")
                            },
                            Sections = new[]
                            {
                                ("Section2", new TestSection
                                {
                                    Values = new[]
                                    {
                                        ("Key3", (TestKeyValue)"Value123"),
                                        ("Key3a", (TestKeyValue)new[] {"ArrayValue0", "ArrayValue1", "ArrayValue2", "ArrayValue2r"}),
                                        ("Key3ar", (TestKeyValue)new[] {"ArrayValue0r"})
                                    },
                                })
                            }
                        }),
                        ("Section3", new TestSection
                        {
                            Sections = new[]
                            {
                                ("Section4", new TestSection
                                {
                                    Values = new[] {("Key4", (TestKeyValue)"Value344")}
                                })
                            }
                        }),
                        ("Section5r", new TestSection
                        {
                            Sections = new[]
                            {
                                ("Section6r", new TestSection
                                {
                                    Values = new[] {("Key5r", (TestKeyValue)"Value565r")}
                                })
                            }
                        })
                    }
                };

            public static TestSection UnsupportedValuesTestHelp { get; }
                = new TestSection
                {
                    Values = new[]
                    {
                        ("Key1", new TestKeyValue(55))
                    },
                };
        }
    }
}