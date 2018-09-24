using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System;

namespace Hypar.Elements
{

    /// <summary>
    /// The serialization converter for elements.
    /// </summary>
    public class ElementConverter : JsonConverter
    {
        /// <summary>
        /// Can this converter converter objects of the provided type?
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Element);
        }

        /// <summary>
        /// Can this converter read json?
        /// </summary>
        /// <value></value>
        public override bool CanRead
        {
            get{return true;}
        }

        /// <summary>
        /// Can this converter write json?
        /// </summary>
        /// <value></value>
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
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var typeName = (string)obj.GetValue("type");
            switch(typeName)
            {
                case "panel":
                    return obj.ToObject<Panel>();
                case "floor":
                    return obj.ToObject<Floor>();
                case "mass":
                    return obj.ToObject<Mass>();
                case "space":
                    return obj.ToObject<Space>();
                case "column":
                    return obj.ToObject<Column>();
                case "beam":
                    return obj.ToObject<Beam>();
                case "brace":
                    return obj.ToObject<Brace>();
                default:
                    throw new Exception($"The object with type name, {typeName}, could not be deserialzed.");
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