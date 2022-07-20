using Helpm8.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Helpm8.Core.Tests
{
    public class HelpManagerFixture
    {
        [Fact]
        public void AutoUpdates()
        {
            var help = new HelpManager();

            help.AddInMemoryCollection(new Dictionary<string, string>
            {
                { "TestKey", "TestValue" },
            });

            Assert.Equal("TestValue", help["TestKey"]);
        }

        [Fact]
        public void SettingValuesWorksWithoutManuallyAddingSource()
        {
            var help = new HelpManager
            {
                ["TestKey"] = "TestValue",
            };

            Assert.Equal("TestValue", help["TestKey"]);
        }

        [Fact]
        public void DisposesProvidersOnDispose()
        {
            var provider1 = new TestHelpProvider("foo", "foo-value");
            var provider2 = new DisposableTestHelpProvider("bar", "bar-value");
            var provider3 = new TestHelpProvider("baz", "baz-value");
            var provider4 = new DisposableTestHelpProvider("qux", "qux-value");
            var provider5 = new DisposableTestHelpProvider("quux", "quux-value");

            var help = new HelpManager();
            IHelpBuilder builder = help;

            builder.Add(new TestHelpSource(provider1));
            builder.Add(new TestHelpSource(provider2));
            builder.Add(new TestHelpSource(provider3));
            builder.Add(new TestHelpSource(provider4));
            builder.Add(new TestHelpSource(provider5));

            Assert.Equal("foo-value", help["foo"]);
            Assert.Equal("bar-value", help["bar"]);
            Assert.Equal("baz-value", help["baz"]);
            Assert.Equal("qux-value", help["qux"]);
            Assert.Equal("quux-value", help["quux"]);

            help.Dispose();

            Assert.True(provider2.IsDisposed);
            Assert.True(provider4.IsDisposed);
            Assert.True(provider5.IsDisposed);
        }

        [Fact]
        public void DisposesProvidersOnRemoval()
        {
            var provider1 = new TestHelpProvider("foo", "foo-value");
            var provider2 = new DisposableTestHelpProvider("bar", "bar-value");
            var provider3 = new TestHelpProvider("baz", "baz-value");
            var provider4 = new DisposableTestHelpProvider("qux", "qux-value");
            var provider5 = new DisposableTestHelpProvider("quux", "quux-value");

            var source1 = new TestHelpSource(provider1);
            var source2 = new TestHelpSource(provider2);
            var source3 = new TestHelpSource(provider3);
            var source4 = new TestHelpSource(provider4);
            var source5 = new TestHelpSource(provider5);

            var help = new HelpManager();
            IHelpBuilder builder = help;

            builder.Add(source1);
            builder.Add(source2);
            builder.Add(source3);
            builder.Add(source4);
            builder.Add(source5);

            Assert.Equal("foo-value", help["foo"]);
            Assert.Equal("bar-value", help["bar"]);
            Assert.Equal("baz-value", help["baz"]);
            Assert.Equal("qux-value", help["qux"]);
            Assert.Equal("quux-value", help["quux"]);

            builder.Sources.Remove(source2);
            builder.Sources.Remove(source4);

            // While only provider2 and provider4 need to be disposed here, we do not assert provider5 is not disposed
            // because even though it's unnecessary, Help disposes all providers on removal and rebuilds
            // all the sources. While not optimal, this should be a pretty rare scenario.
            Assert.True(provider2.IsDisposed);
            Assert.True(provider4.IsDisposed);

            help.Dispose();

            Assert.True(provider2.IsDisposed);
            Assert.True(provider4.IsDisposed);
            Assert.True(provider5.IsDisposed);
        }

        [Fact]
        public async Task ProviderCanBlockLoadWaitingOnConcurrentRead()
        {
            using var mre = new ManualResetEventSlim(false);
            var provider = new BlockLoadOnMreProvider(mre, timeout: TimeSpan.FromSeconds(30));

            var help = new HelpManager();
            IHelpBuilder builder = help;

            // builder.Add(source) will block on provider.Load().
            var loadTask = Task.Run(() => builder.Add(new TestHelpSource(provider)));
            await provider.LoadStartedTask;

            // Read help while provider.Load() is blocked waiting on us.
            _ = help["key"];

            // Unblock provider.Load()
            mre.Set();

            // This will throw if provider.Load() timed out instead of unblocking gracefully after the read.
            await loadTask;
        }

        public static TheoryData ConcurrentReadActions
        {
            get
            {
                return new TheoryData<Action<IHelp>>
                {
                    help => _ = help["key"],
                    help => help.GetChildren(),
                    help => help.GetSection("key").GetChildren(),
                };
            }
        }

        [Theory]
        [MemberData(nameof(ConcurrentReadActions))]
        public async Task ProviderDisposeDelayedWaitingOnConcurrentRead(Action<IHelp> concurrentReadAction)
        {
            using var mre = new ManualResetEventSlim(false);
            var provider = new BlockReadOnMreProvider(mre, timeout: TimeSpan.FromSeconds(30));

            var help = new HelpManager();
            IHelpBuilder builder = help;

            builder.Add(new TestHelpSource(provider));

            // Reading help will block on provider.TryRead() or provider.GetChildKeys().
            var readTask = Task.Run(() => concurrentReadAction(help));
            await provider.ReadStartedTask;

            // Removing the source normally disposes the provider except when there provider is in use as is the case here.
            builder.Sources.Clear();

            Assert.False(provider.IsDisposed);

            // Unblock TryRead() or GetChildKeys()
            mre.Set();

            // This will throw if TryRead() or GetChildKeys() timed out instead of unblocking gracefully after setting the MRE.
            await readTask;

            // The provider should be disposed when the concurrentReadAction releases the last reference to the provider.
            Assert.True(provider.IsDisposed);
        }

        [Fact]
        public void DisposingHelpManagerCausesOnlySourceChangesToThrow()
        {
            var help = new HelpManager
            {
                ["TestKey"] = "TestValue",
            };

            help.Dispose();

            Assert.Equal("TestValue", help["TestKey"]);
            help["TestKey"] = "TestValue2";
            Assert.Equal("TestValue2", help["TestKey"]);

            Assert.Throws<ObjectDisposedException>(() => help.AddInMemoryCollection());
            Assert.Throws<ObjectDisposedException>(() => help.Sources.Clear());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ChainedHelpIsDisposedOnDispose(bool shouldDispose)
        {
            var provider = new DisposableTestHelpProvider("foo", "foo-value");
            var chainedHelp = new HelpRoot(new IHelpProvider[] {
                provider
            });

            var help = new HelpManager();

            help.AddHelp(chainedHelp, shouldDisposeHelp: shouldDispose);

            Assert.False(provider.IsDisposed);

            help.Dispose();

            Assert.Equal(shouldDispose, provider.IsDisposed);
        }

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
            var memHelpSrc1 = new InMemoryHelpSource { InitialData = dic1 };
            var memHelpSrc2 = new InMemoryHelpSource { InitialData = dic2 };
            var memHelpSrc3 = new InMemoryHelpSource { InitialData = dic3 };

            var help = new HelpManager();
            IHelpBuilder helpBuilder = help;

            // Act
            helpBuilder.Add(memHelpSrc1);
            helpBuilder.Add(memHelpSrc2);
            helpBuilder.Add(memHelpSrc3);

            var memVal1 = help["mem1:keyinmem1"];
            var memVal2 = help["Mem2:KeyInMem2"];
            var memVal3 = help["MEM3:KEYINMEM3"];

            // Assert
            Assert.Contains(memHelpSrc1, helpBuilder.Sources);
            Assert.Contains(memHelpSrc2, helpBuilder.Sources);
            Assert.Contains(memHelpSrc3, helpBuilder.Sources);

            Assert.Equal("ValueInMem1", memVal1);
            Assert.Equal("ValueInMem2", memVal2);
            Assert.Equal("ValueInMem3", memVal3);

            Assert.Equal("ValueInMem1", help["mem1:keyinmem1"]);
            Assert.Equal("ValueInMem2", help["Mem2:KeyInMem2"]);
            Assert.Equal("ValueInMem3", help["MEM3:KEYINMEM3"]);
            Assert.Null(help["NotExist"]);
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
            var memHelpSrc1 = new InMemoryHelpSource { InitialData = dic1 };
            var memHelpSrc2 = new InMemoryHelpSource { InitialData = dic2 };
            var memHelpSrc3 = new InMemoryHelpSource { InitialData = dic3 };

            var help = new HelpManager();
            IHelpBuilder helpBuilder = help;

            // Act
            helpBuilder.Add(memHelpSrc1);
            helpBuilder.Add(memHelpSrc2);
            helpBuilder.Add(memHelpSrc3);

            var chained = new HelpManager();
            chained.AddHelp(help);
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
            var memHelpSrc1 = new InMemoryHelpSource { InitialData = dic1 };
            var memHelpSrc2 = new InMemoryHelpSource { InitialData = dic2 };
            var memHelpSrc3 = new InMemoryHelpSource { InitialData = dic3 };

            var help1 = new HelpManager();
            IHelpBuilder helpBuilder = help1;

            // Act
            helpBuilder.Add(memHelpSrc1);
            helpBuilder.Add(memHelpSrc2);

            var help2 = new HelpManager();

            help2
                .AddHelp(help1)
                .Add(memHelpSrc3);

            var dict = help2.AsEnumerable(makePathsRelative: removePath).ToDictionary(k => k.Key, v => v.Value);

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
            var memHelpSrc1 = new InMemoryHelpSource { InitialData = dic1 };
            var memHelpSrc2 = new InMemoryHelpSource { InitialData = dic2 };
            var memHelpSrc3 = new InMemoryHelpSource { InitialData = dic3 };

            var help = new HelpManager();
            IHelpBuilder helpBuilder = help;

            // Act
            helpBuilder.Add(memHelpSrc1);
            helpBuilder.Add(memHelpSrc2);
            helpBuilder.Add(memHelpSrc3);
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
            var memHelpSrc1 = new InMemoryHelpSource { InitialData = dic1 };
            var memHelpSrc2 = new InMemoryHelpSource { InitialData = dic2 };
            var memHelpSrc3 = new InMemoryHelpSource { InitialData = dic3 };

            var help = new HelpManager();
            IHelpBuilder helpBuilder = help;

            // Act
            helpBuilder.Add(memHelpSrc1);
            helpBuilder.Add(memHelpSrc2);
            helpBuilder.Add(memHelpSrc3);

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
            var memHelpSrc1 = new InMemoryHelpSource { InitialData = dic1 };
            var memHelpSrc2 = new InMemoryHelpSource { InitialData = dic2 };

            var help = new HelpManager();
            IHelpBuilder helpBuilder = help;

            // Act
            helpBuilder.Add(memHelpSrc1);
            helpBuilder.Add(memHelpSrc2);

            // Assert
            Assert.Equal("ValueInMem2", help["Key1:Key2"]);
        }

        [Fact]
        public void NewHelpRootMayBeBuiltFromExistingWithDuplicateKeys()
        {
            var helpRoot = new HelpManager();

            helpRoot.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"keya:keyb", "valueA"},
            });
            helpRoot.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"KEYA:KEYB", "valueB"},
            });

            var newHelpRoot = new HelpManager();

            newHelpRoot.AddInMemoryCollection(helpRoot.AsEnumerable());

            Assert.Equal("valueB", newHelpRoot["keya:keyb"]);
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

            var memHelpSrc1 = new TestInMemoryHelpSourceProvider(dict);
            var memHelpSrc2 = new TestInMemoryHelpSourceProvider(dict);
            var memHelpSrc3 = new TestInMemoryHelpSourceProvider(dict);

            var help = new HelpManager();
            IHelpBuilder helpBuilder = help;

            helpBuilder.Add(memHelpSrc1);
            helpBuilder.Add(memHelpSrc2);
            helpBuilder.Add(memHelpSrc3);

            // Act
            help["Key1"] = "NewValue1";
            help["Key2"] = "NewValue2";

            var memHelpProvider1 = memHelpSrc1.Build(helpBuilder);
            var memHelpProvider2 = memHelpSrc2.Build(helpBuilder);
            var memHelpProvider3 = memHelpSrc3.Build(helpBuilder);

            // Assert
            Assert.Equal("NewValue1", help["Key1"]);
            Assert.Equal("NewValue1", Get(memHelpProvider1, "Key1"));
            Assert.Equal("NewValue1", Get(memHelpProvider2, "Key1"));
            Assert.Equal("NewValue1", Get(memHelpProvider3, "Key1"));
            Assert.Equal("NewValue2", help["Key2"]);
            Assert.Equal("NewValue2", Get(memHelpProvider1, "Key2"));
            Assert.Equal("NewValue2", Get(memHelpProvider2, "Key2"));
            Assert.Equal("NewValue2", Get(memHelpProvider3, "Key2"));
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
            var memHelpSrc1 = new InMemoryHelpSource { InitialData = dic1 };
            var memHelpSrc2 = new InMemoryHelpSource { InitialData = dic2 };
            var memHelpSrc3 = new InMemoryHelpSource { InitialData = dic3 };

            var help = new HelpManager();
            IHelpBuilder helpBuilder = help;

            helpBuilder.Add(memHelpSrc1);
            helpBuilder.Add(memHelpSrc2);
            helpBuilder.Add(memHelpSrc3);

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
            var memHelpSrc1 = new InMemoryHelpSource { InitialData = dic1 };
            var memHelpSrc2 = new InMemoryHelpSource { InitialData = dic2 };
            var memHelpSrc3 = new InMemoryHelpSource { InitialData = dic3 };

            var help = new HelpManager();
            IHelpBuilder helpBuilder = help;

            helpBuilder.Add(memHelpSrc1);
            helpBuilder.Add(memHelpSrc2);
            helpBuilder.Add(memHelpSrc3);

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
            var memHelpSrc1 = new InMemoryHelpSource { InitialData = dict };
            var memHelpSrc2 = new InMemoryHelpSource { InitialData = dict };
            var memHelpSrc3 = new InMemoryHelpSource { InitialData = dict };

            var help = new HelpManager();
            IHelpBuilder helpBuilder = help;

            // Act

            // A MemoryHelpSource is added by default, so there will be no error unless we clear it
            helpBuilder.Sources.Clear();
            helpBuilder.Add(memHelpSrc1);
            helpBuilder.Add(memHelpSrc2);
            helpBuilder.Add(memHelpSrc3);

            // Assert
            Assert.Equal(new[] { memHelpSrc1, memHelpSrc2, memHelpSrc3 }, helpBuilder.Sources);
        }

        [Fact]
        public void SetValueThrowsExceptionNoSourceRegistered()
        {
            // Arrange
            var help = new HelpManager();

            // A MemoryHelpSource is added by default, so there will be no error unless we clear it
            help["Title"] = "Welcome";

            ((IHelpBuilder)help).Sources.Clear();

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => help["Title"] = "Welcome");

            // Assert
            Assert.Equal("A help source is not registered. Please register one before setting a value.", ex.Message);
        }

        [Fact]
        public void KeyStartingWithColonMeansFirstSectionHasEmptyName()
        {
            // Arrange
            var dict = new Dictionary<string, string>
            {
                [":Key2"] = "value"
            };
            var help = new HelpManager();
            help.AddInMemoryCollection(dict);

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

            var help = new HelpManager();
            ((IHelpBuilder)help).AddInMemoryCollection(dict);

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

            var help = new HelpManager();
            ((IHelpBuilder)help).AddInMemoryCollection(dict);

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

            var help = new HelpManager();
            ((IHelpBuilder)help).AddInMemoryCollection(dict);

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

            var help = new HelpManager();
            ((IHelpBuilder)help).AddInMemoryCollection(dict);

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

            var help = new HelpManager();
            ((IHelpBuilder)help).AddInMemoryCollection(dict);

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

            var help = new HelpManager();
            ((IHelpBuilder)help).AddInMemoryCollection(dict);

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


            var help = new HelpManager();
            ((IHelpBuilder)help).AddInMemoryCollection(dict);

            // Act
            var sectionValue = help.GetSection("Mem1").Value;

            // Assert
            Assert.Null(sectionValue);
        }

        [Fact]
        public void ProviderWithNullReloadToken()
        {
            // Arrange
            var help = new HelpManager();
            IHelpBuilder builder = help;

            // Assert
            Assert.NotNull(builder.Build());
        }

        [Fact]
        public void BuildReturnsThis()
        {
            // Arrange
            var help = new HelpManager();

            // Assert
            Assert.Same(help, ((IHelpBuilder)help).Build());
        }

        [Fact]
        public void ClearHelpSources()
        {
            // Arrange
            var dic1 = new Dictionary<string, string>()
            {
                {"Mem1:KeyInMem1", "ValueInMem1"},
            };
            var dic2 = new Dictionary<string, string>()
            {
                {"Mem2:KeyInMem2", "ValueInMem2"},
            };

            var memHelpSrc1 = new InMemoryHelpSource { InitialData = dic1 };
            var memHelpSrc2 = new InMemoryHelpSource { InitialData = dic2 };

            var help = new HelpManager();
            IHelpBuilder helpBuilder = help;

            helpBuilder.Add(memHelpSrc1);
            helpBuilder.Add(memHelpSrc2);

            // Act
            help.Sources.Clear();

            // Assert
            Assert.DoesNotContain(memHelpSrc1, help.Sources);
            Assert.DoesNotContain(memHelpSrc2, help.Sources);
            Assert.Null(help["Mem1:KeyInMem1"]);
            Assert.Null(help["Mem2:KeyInMem2"]);
        }

        private static string Get(IHelpProvider provider, string key)
        {
            if (!provider.TryGet(key, out string value))
            {
                throw new InvalidOperationException("Key not found");
            }

            return value;
        }

        private class TestHelpSource : IHelpSource
        {
            private readonly IHelpProvider _provider;

            public TestHelpSource(IHelpProvider provider)
            {
                _provider = provider;
            }

            public IHelpProvider Build(IHelpBuilder builder)
            {
                return _provider;
            }
        }

        private class TestHelpProvider : HelpProvider
        {
            public TestHelpProvider(string key, string value)
                => Data.Add(key, value);
        }

        private class TestInMemoryHelpSourceProvider : InMemoryHelpProvider, IHelpSource
        {
            public TestInMemoryHelpSourceProvider(Dictionary<string, string> initialData)
                : base(new InMemoryHelpSource { InitialData = initialData })
            { }

            public IHelpProvider Build(IHelpBuilder builder)
            {
                return this;
            }
        }

        private class DisposableTestHelpProvider : HelpProvider, IDisposable
        {
            public bool IsDisposed { get; set; }

            public DisposableTestHelpProvider(string key, string value)
                => Data.Add(key, value);

            public void Dispose()
                => IsDisposed = true;
        }

        private class BlockLoadOnMreProvider : HelpProvider
        {
            private readonly ManualResetEventSlim _mre;
            private readonly TimeSpan _timeout;

            private readonly TaskCompletionSource<object> _loadStartedTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            public BlockLoadOnMreProvider(ManualResetEventSlim mre, TimeSpan timeout)
            {
                _mre = mre;
                _timeout = timeout;
            }

            public Task LoadStartedTask => _loadStartedTcs.Task;

            public override void Load()
            {
                _loadStartedTcs.SetResult(null);
                Assert.True(_mre.Wait(_timeout), "BlockLoadOnMREProvider.Load() timed out.");
            }
        }

        private class BlockReadOnMreProvider : HelpProvider, IDisposable
        {
            private readonly ManualResetEventSlim _mre;
            private readonly TimeSpan _timeout;

            private readonly TaskCompletionSource<object> _readStartedTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            public BlockReadOnMreProvider(ManualResetEventSlim mre, TimeSpan timeout)
            {
                _mre = mre;
                _timeout = timeout;
            }

            public Task ReadStartedTask => _readStartedTcs.Task;

            public bool IsDisposed { get; set; }

#pragma warning disable CS8632
            public override bool TryGet(string key, out string? value)
#pragma warning restore CS8632
            {
                _readStartedTcs.SetResult(null);
                Assert.True(_mre.Wait(_timeout), "BlockReadOnMREProvider.TryGet() timed out.");
                return base.TryGet(key, out value);
            }

#pragma warning disable CS8632
            public override IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
#pragma warning restore CS8632
            {
                _readStartedTcs.SetResult(null);
                Assert.True(_mre.Wait(_timeout), "BlockReadOnMREProvider.GetChildKeys() timed out.");
                return base.GetChildKeys(earlierKeys, parentPath);
            }

            public void Dispose() => IsDisposed = true;
        }
    }
}