using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Elements.Geometry;

namespace Elements.Serialization.JSON
{
    internal class VectorListToByteArrayConverter : JsonConverter<List<Vector3>>
    {
        public override List<Vector3> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, List<Vector3> value, JsonSerializerOptions options)
        {
            var valueAsArray = new double[value.Count * 3];
            for (int i = 0; i < value.Count; i++)
            {
                valueAsArray[i * 3] = value[i].X;
                valueAsArray[i * 3 + 1] = value[i].Y;
                valueAsArray[i * 3 + 2] = value[i].Z;
            }
            JsonSerializer.Serialize(writer, valueAsArray, options);
        }
    }
}
