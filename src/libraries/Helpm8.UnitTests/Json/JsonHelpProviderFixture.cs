using Helpm8.Core.Tests;
using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Xunit;
// ReSharper disable IdentifierTypo

namespace Helpm8.Json.Tests
{
    public class JsonHelpProviderFixture : HelpProviderFixtureBase
    {
        [Fact]
        public override void Load_from_single_provider_with_duplicates_throws()
        {
            // JSON provider doesn't throw for duplicate values with the same case
            AssertHelp(BuildHelpRoot(LoadThroughProvider(TestSection.DuplicatesTestHelp)));
        }

        protected override (IHelpProvider Provider, Action Initializer) LoadThroughProvider(TestSection testHelp)
        {
            var jsonBuilder = new StringBuilder();
            SectionToJson(jsonBuilder, testHelp, includeComma: false);

            var provider = new JsonHelpProvider(
                new JsonHelpSource
                {
                    Optional = true
                });

            var json = jsonBuilder.ToString();

            JsonLoadSettings settings = new JsonLoadSettings()
            {
                CommentHandling = CommentHandling.Ignore,
                DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace,
            };
            json = JObject.Parse(json, settings).ToString(); // standardize the json (removing trailing commas)
            return (provider, () => provider.Load(TestStreamHelpers.StringToStream(json)));
        }

        private void SectionToJson(StringBuilder jsonBuilder, TestSection section, bool includeComma = true)
        {
            string ValueToJson(object value) => value == null ? "null" : $"\"{value}\"";

            jsonBuilder.AppendLine("{");

            foreach (var tuple in section.Values)
            {
                jsonBuilder.AppendLine(tuple.Value.AsArray != null
                    ? $"\"{tuple.Key}\": [{string.Join(", ", tuple.Value.AsArray.Select(ValueToJson))}],"
                    : $"\"{tuple.Key}\": {ValueToJson(tuple.Value.AsString)},");
            }

            foreach (var tuple in section.Sections)
            {
                jsonBuilder.Append($"\"{tuple.Key}\": ");
                SectionToJson(jsonBuilder, tuple.Section);
            }

            if (includeComma)
            {
                jsonBuilder.AppendLine("},");
            }
            else
            {
                jsonBuilder.AppendLine("}");
            }
        }

        [Fact]
        public void Load_from_single_provider_with_duplicates_replaces_values()
        {
            // JSON provider should replace for duplicate keys with the latest value
            AssertHelpWithReplacedValues(
                BuildHelpRoot(LoadThroughProvider(TestSection.DuplicatesTestHelpWithReplacingValues)));
        }

        [Fact]
        public void Root_is_no_object_throws()
        {
            var provider = new JsonHelpProvider(
                new JsonHelpSource
                {
                    Optional = true
                });

            var json = "{\n\"Key\":\"Value\"\n}";
            AssertFormatOrArgumentException(
                () => BuildHelpRoot((provider, () => provider.Load(TestStreamHelpers.StringToStream(json)))));
        }

        [Fact]
        public void Value_type_not_a_string_throws()
        {
            var provider = new JsonHelpProvider(
                new JsonHelpSource
                {
                    Optional = true
                });

            var json = "{\n\"Key\":55\n}";
            AssertFormatOrArgumentException(
                () => BuildHelpRoot((provider, () => provider.Load(TestStreamHelpers.StringToStream(json)))),
                "Unsupported JSON token 'Integer' was found.");
        }


        protected virtual void AssertHelpWithReplacedValues(
            IHelpRoot help,
            bool expectNulls = false,
            string nullValue = null)
        {
            var value1 = expectNulls ? nullValue : "Value1Replaced";
            var value12 = expectNulls ? nullValue : "Value12";
            var value123 = expectNulls ? nullValue : "Value123Replaced";
            var arrayvalue0 = expectNulls ? nullValue : "ArrayValue0Replaced";
            var arrayvalue1 = expectNulls ? nullValue : "ArrayValue1Replaced";
            var arrayvalue2 = expectNulls ? nullValue : "ArrayValue2Replaced";
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

    }
}
