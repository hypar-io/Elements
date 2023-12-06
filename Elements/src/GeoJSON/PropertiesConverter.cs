using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elements.GeoJSON
{
    /// <summary>
    /// Convert geojson properties.
    /// </summary>
    public class PropertiesConverter : JsonConverter<Dictionary<string, object>>
    {
        /// <summary>
        /// Read properties.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override Dictionary<string, object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var properties = new Dictionary<string, object>();
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                foreach (var el in root.EnumerateObject())
                {
                    var key = el.Name;
                    switch (el.Value.ValueKind)
                    {
                        // TODO: Support additional types of geojson properties
                        // as needed.
                        case JsonValueKind.Number:
                            if (el.Value.TryGetInt32(out var intValue))
                            {
                                properties.Add(key, intValue);
                            }
                            else if (el.Value.TryGetDouble(out var doubleValue))
                            {
                                properties.Add(key, doubleValue);
                            }
                            break;
                        case JsonValueKind.String:
                            properties.Add(key, el.Value.GetString());
                            break;
                    }
                }
            }
            return properties;
        }

        /// <summary>
        /// Write properties.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value);
        }
    }
}