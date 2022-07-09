using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Elements.Geometry;

namespace Elements.Serialization.JSON
{
    internal class ModelConverter : JsonConverter<Model>
    {
        public override Model Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var elements = new Dictionary<Guid, Element>();
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                var elementsElement = root.GetProperty("Elements");
                var transform = JsonSerializer.Deserialize<Transform>(root.GetProperty("Transform"));

                foreach (var element in elementsElement.EnumerateObject())
                {
                    // TODO: This try/catch is only here to protect against
                    // situations like null property values when the serializer
                    // expects a value, or validation errors. Unlike json.net, system.text.json doesn't
                    // have null value handling on read. 
                    try
                    {
                        var id = Guid.Parse(element.Name);
                        var e = JsonSerializer.Deserialize<Element>(element.Value, options);
                        if (e != null)
                        {
                            elements.Add(id, e);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        continue;
                    }
                }

                var model = new Model(transform, elements);
                return model;
            }
        }

        public override void Write(Utf8JsonWriter writer, Model value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}