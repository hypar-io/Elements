using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Elements.GeoJSON
{
    public class GeometryConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (typeof(Geometry).IsAssignableFrom(objectType))
            {
                return true;
            }
            return false;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            string typeName = (jsonObject["type"]).ToString();
            switch (typeName)
            {
                case "Point":
                    return jsonObject.ToObject<Point>();
                case "Line":
                    return jsonObject.ToObject<Line>();
                case "MultiPoint":
                    return jsonObject.ToObject<MultiPoint>();
                case "LineString":
                    return jsonObject.ToObject<LineString>();
                case "MultiLineString":
                    return jsonObject.ToObject<MultiLineString>();
                case "Polygon":
                    return jsonObject.ToObject<Polygon>();
                case "MultiPolygon":
                    return jsonObject.ToObject<MultiPolygon>();
                case "GeometryCollection":
                    return jsonObject.ToObject<GeometryCollection>();
                default:
                    throw new Exception($"The type found in the GeoJSON, {typeName}, could not be resolved.");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}