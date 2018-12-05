using Hypar.Interfaces;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hypar.Elements.Serialization
{
    public class PropertyDictionaryConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dictionary<string, IProperty>);
        }

        public override bool CanWrite
        {
            get{return false;}
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var properties = new Dictionary<string, IProperty>();
            foreach(var t in obj)
            {
                var unitType = (string)t.Value["unit_type"];
                switch(unitType)
                {
                    case "none":
                    case "text":
                        properties.Add(t.Key, t.Value.ToObject<StringProperty>(serializer));
                        break;
                    case "area":
                    case "force":
                    case "mass":
                    case "volume":
                        properties.Add(t.Key, t.Value.ToObject<NumericProperty>(serializer));
                        break;
                }
            }
            return properties;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}