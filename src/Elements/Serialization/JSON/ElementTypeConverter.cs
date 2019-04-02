using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Elements.Serialization.JSON
{
    /// <summary>
    /// Converter for objects of type ElementType.
    /// </summary>
    public class ElementTypeConverter : JsonConverter
    {
        /// <summary>
        /// Can this converter convert an object of type objectType?
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ElementType);
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
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var typeName = (string)obj.GetValue("type");
            switch(typeName)
            {
                case "wallType":
                    return obj.ToObject<WallType>(serializer);
                case "floorType":
                    return obj.ToObject<FloorType>(serializer);
                case "structuralFramingType":
                    return obj.ToObject<StructuralFramingType>(serializer);
                default:
                    throw new Exception($"The ElementType with type name, {typeName}, could not be deserialzed.");
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