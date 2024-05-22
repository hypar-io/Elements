using System;
using System.Collections.Generic;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements.Serialization.JSON
{
    internal class VectorListToByteArrayConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<Vector3>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();

        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var points = value as List<Vector3>;
            var valueAsArray = new double[points.Count * 3];
            for (int i = 0; i < points.Count; i++)
            {
                valueAsArray[i * 3] = points[i].X;
                valueAsArray[i * 3 + 1] = points[i].Y;
                valueAsArray[i * 3 + 2] = points[i].Z;
            }
            serializer.Serialize(writer, valueAsArray);
        }
    }
}