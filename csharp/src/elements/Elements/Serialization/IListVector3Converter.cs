#pragma warning disable CS1591

using Hypar.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hypar.Elements.Serialization
{
    public class IListVector3Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType ==  typeof(IList<Vector3>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var pts = new List<Vector3>();
            var arr = JArray.Load(reader);
            var vals = arr.Values().ToArray();
            for(var i=0; i<vals.Length; i+=3)
            {
                pts.Add(new Vector3(vals[i].ToObject<double>(), vals[i+1].ToObject<double>(), vals[i+2].ToObject<double>()));
            }        
            return pts;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var arr = (IList<Vector3>)value;
            writer.WriteStartArray();
            var count = 0;
            foreach(var v in arr)
            {
                if(count > 0)
                {
                    writer.WriteRaw($",{v.X},{v.Y},{v.Z}");
                }
                else
                {
                    writer.WriteRaw($"{v.X},{v.Y},{v.Z}");
                }
                
                count++;
            }
            writer.WriteEndArray();
        }
    }
}