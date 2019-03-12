using Elements.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Elements.Serialization
{
    /// <summary>
    /// The serialization converter for elements.
    /// </summary>
    public class ElementConverter : JsonConverter
    {
        private IEnumerable<Type> _elementTypes;

        /// <summary>
        /// Construct an ElementConverter.
        /// </summary>
        public ElementConverter(IEnumerable<string> extensions)
        {
            try
            {
                // Loop over all extensions attempting to find
                // a matching assembly.
                var elementAsms = AppDomain.CurrentDomain.GetAssemblies().Where(a=>extensions.Contains(a.GetName().Name.ToLower()));
                _elementTypes = elementAsms.SelectMany(a=>a.GetTypes().Where(t=>typeof(IElement).IsAssignableFrom(t)));
            }
            catch(System.Reflection.ReflectionTypeLoadException ex)
            {
                foreach(var x in ex.LoaderExceptions)
                {
                    Console.WriteLine(x.Message);
                }
            }
        }

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
        public override bool CanRead
        {
            get{return true;}
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
        /// <exception cref="System.Exception">Thrown when a type matching the deserialized type name cannot be found.</exception>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var typeName = (string)obj.GetValue("type");

            // Find a type with the fullname 
            var foundType = _elementTypes.FirstOrDefault(t=>t.FullName.ToLower() == typeName);
            if(foundType == null)
            {
                throw new Exception($"The object with type name, {typeName}, is not available in the loaded assemblies. Is it possible that you're missing an extension assembly?");
            }
            return obj.ToObject(foundType, serializer);
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