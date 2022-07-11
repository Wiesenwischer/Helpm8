using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Helpm8.Json
{
    internal sealed class JsonHelpFileParser
    {
        private JsonHelpFileParser() { }

        private readonly Dictionary<string, string?> _data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<string> _paths = new Stack<string>();

        public static IDictionary<string, string?> Parse(Stream input)
            => new JsonHelpFileParser().ParseStream(input);

        private IDictionary<string, string?> ParseStream(Stream input)
        {
            //var jsonDocumentOptions = new JsonDocumentOptions
            //{
            //    CommentHandling = JsonCommentHandling.Skip,
            //    AllowTrailingCommas = true,
            //};

            using var reader = new StreamReader(input);

            JToken rootElement = JToken.ReadFrom(new JsonTextReader(reader));
            {
                if (rootElement.Type != JTokenType.Object)
                {
                    throw new FormatException(
                        $"Top-level JSON element must be an object. Instead, '{rootElement.Type}' was found.");
                }
                VisitElement((JObject)rootElement);
            }

            return _data;
        }

        private void VisitElement(JObject element)
        {
            var isEmpty = true;

            foreach (JProperty property in element.Properties())
            {
                isEmpty = false;
                EnterContext(property.Name);
                VisitValue(property.Value);
                ExitContext();
            }

            if (isEmpty && _paths.Count > 0)
            {
                _data[_paths.Peek()] = null;
            }
        }

        private void VisitValue(JToken value)
        {
            Debug.Assert(_paths.Count > 0);

            switch (value.Type)
            {
                case JTokenType.Object:
                    VisitElement((JObject)value);
                    break;

                case JTokenType.Array:
                    int index = 0;
                    foreach (JToken arrayElement in value.Values())
                    {
                        EnterContext(index.ToString());
                        VisitValue(arrayElement);
                        ExitContext();
                        index++;
                    }
                    break;

                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Null:
                    string key = _paths.Peek();
                    if (_data.ContainsKey(key))
                    {
                        throw new FormatException(
                            $"A duplicate key '{key}' was found.");
                    }

                    _data[key] = value.Value<string>() ?? string.Empty;
                    break;

                default:
                    throw new FormatException($"Unsupported JSON token '{value.Type}' was found.");
            }
        }

        private void EnterContext(string context) =>
            _paths.Push(_paths.Count > 0 ?
                _paths.Peek() + HelpPath.KeyDelimiter + context :
                context);

        private void ExitContext() => _paths.Pop();
    }
}