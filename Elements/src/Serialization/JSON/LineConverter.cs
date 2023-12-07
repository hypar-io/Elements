using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Elements.Geometry;

namespace Elements.Serialization.JSON
{
    internal class LineConverter : JsonConverter<Line>
    {
        public override Line Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Vector3 start = default;
            Vector3 end = default;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();
                    switch (propertyName)
                    {
                        case "Start":
                            start = JsonSerializer.Deserialize<Vector3>(ref reader, options);
                            break;
                        case "End":
                            end = JsonSerializer.Deserialize<Vector3>(ref reader, options);
                            break;
                    }
                }

                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new Line(start, end);
                }
            }

            throw new JsonException("Error reading Line object.");
        }

        public override void Write(Utf8JsonWriter writer, Line value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("Start");
            JsonSerializer.Serialize(writer, value.Start, options);

            writer.WritePropertyName("End");
            JsonSerializer.Serialize(writer, value.End, options);

            writer.WriteEndObject();
        }
    }
}
