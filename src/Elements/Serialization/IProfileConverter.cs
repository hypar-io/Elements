#pragma warning disable CS1591

using Elements.Geometry;
using Elements.Geometry.Interfaces;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Elements.Serialization
{
    public class IProfileConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            var convert = typeof(Profile).IsAssignableFrom(objectType);
            return convert;
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
            if(TryConvert<Profile>(o, serializer, out result))
            {
                return result;
            }
            
            throw new Exception($"The provided IProfile could not be deserialized to a known type.");
        }

        private bool TryConvert<T>(JObject o, JsonSerializer serializer, out object result)
        {
            try
            {   
                // Don't reuse the serializer here to avoid
                // id->Profile conversion behavior.
                result = o.ToObject<T>();
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