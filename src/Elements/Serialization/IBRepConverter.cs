#pragma warning disable CS1591

using Elements.Geometry;
using Elements.Geometry.Interfaces;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Elements.Serialization
{
    public class IBRepConverter : JsonConverter
    {
        private List<Type> _elementTypes;

        public IBRepConverter()
        {
            try
            {
                _elementTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t=>typeof(IBRep).IsAssignableFrom(t)).ToList();
            }
            catch(System.Reflection.ReflectionTypeLoadException ex)
            {
                foreach(var x in ex.LoaderExceptions)
                {
                    Console.WriteLine(x.Message);
                }
            }
        }

        public override bool CanConvert(Type objectType)
        {
            var typeMatch = objectType == typeof(IBRep);
            // if(typeMatch)
            // {
            //     Console.WriteLine($"Type match for deserialization: {objectType.Name} -> IBRep");
            // }
            return typeMatch;
        }

        public override bool CanWrite
        {
            get{return false;}
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var typeName = (string)obj.GetValue("type");

            // Find a type with the fullname 
            var foundType = _elementTypes.FirstOrDefault(t=>t.FullName.ToLower() == typeName);
            if(foundType == null)
            {
                throw new Exception($"The object with type name, {typeName}, could not be deserialzed.");
            }
            return obj.ToObject(foundType, serializer);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}