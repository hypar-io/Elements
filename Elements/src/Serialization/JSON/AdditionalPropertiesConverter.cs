using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elements.Serialization.JSON
{
    /// <summary>
    /// Convert the additional properties collection.
    /// This converter is required because System.Text.Json will read 
    /// string values in the dictionary as JsonValue objects.
    /// </summary>
    public class AdditionalPropertiesConverter : JsonConverter<IDictionary<string, object>>
    {
        /// <summary>
        /// Read the dictionary.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override IDictionary<string, object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = new Dictionary<string, object>();
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                foreach (var p in root.EnumerateObject())
                {
                    switch (p.Value.ValueKind)
                    {
                        case JsonValueKind.Number:
                            if (p.Value.TryGetInt32(out var intValue))
                            {
                                result.Add(p.Name, intValue);
                            }
                            else if (p.Value.TryGetDouble(out var doubleValue))
                            {
                                result.Add(p.Name, doubleValue);
                            }
                            break;
                        case JsonValueKind.String:
                            if (p.Value.TryGetGuid(out var guidValue))
                            {
                                result.Add(p.Name, guidValue);
                            }
                            else
                            {
                                result.Add(p.Name, p.Value.GetString());
                            }
                            break;
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Write the dictionary.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, IDictionary<string, object> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value);
        }
    }
}