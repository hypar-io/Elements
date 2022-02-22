using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elements.Serialization.JSON
{
    internal class ModelConverter : JsonConverter<Model>
    {

        public override Model Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Model value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            // Write the transform
            writer.WritePropertyName("Transform");
            JsonSerializer.Serialize(writer, value.Transform);

            //Write the elements dictionary
            writer.WritePropertyName("Elements");
            writer.WriteStartObject();
            foreach (var kvp in value.Elements)
            {
                writer.WritePropertyName(kvp.Key.ToString());
                JsonSerializer.Serialize(writer, kvp.Value, kvp.Value.GetType(), options);
            }
            writer.WriteEndObject();

            writer.WriteEndObject();
        }
    }
}