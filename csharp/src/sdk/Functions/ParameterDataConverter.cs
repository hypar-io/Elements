using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Hypar.Functions
{
    /// <summary>
    /// Converter for types which inherit from ParameterData
    /// </summary>
    public class ParameterDataConverter : JsonConverter
    {
        /// <summary>
        /// Can this converter convert an object of the specified type?
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ParameterBase);
        }

        /// <summary>
        /// Can this converter write json?
        /// </summary>
        public override bool CanWrite
        {
            get{return false;}
        }

        /// <summary>
        /// Read json.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            string typeName = (jsonObject["type"]).ToString();
            switch(typeName)
            {
                case "number":
                    return jsonObject.ToObject<NumberParameter>(serializer);
                case "point":
                    return jsonObject.ToObject<PointParameter>(serializer);
                case "location":
                    return jsonObject.ToObject<LocationParameter>(serializer);
                default:
                    return jsonObject.ToObject<ParameterBase>(serializer);
            }
        }

        /// <summary>
        /// Write json.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}