#pragma warning disable CS1591

using Elements.Geometry;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Elements.Serialization
{
    public class ICurveConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType is ICurve;
        }

        public override bool CanWrite
        {
            get{return false;}
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Try deserializing to the concrete types
            var o = JObject.Load(reader);
            object result;
            if(TryConvert<Arc>(o, serializer, out result))
            {
                return result;
            }
            if(TryConvert<Line>(o, serializer, out result))
            {
                return result;
            }
            if(TryConvert<Polyline>(o, serializer, out result))
            {
                return result;
            }
            
            throw new Exception($"The provided ICurve could not be deserialized to a known type.");
        }

        private bool TryConvert<T>(JObject o, JsonSerializer serializer, out object result)
        {
            try
            {   
                result = o.ToObject<T>(serializer);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}