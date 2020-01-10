#pragma warning disable CS1591

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Elements.Serialization.JSON
{
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.24.0 (Newtonsoft.Json v9.0.0.0)")]
    public class JsonInheritanceConverter : Newtonsoft.Json.JsonConverter
    {
        internal static readonly string DefaultDiscriminatorName = "discriminator";

        private readonly string _discriminator;

        [System.ThreadStatic]
        private static bool _isReading;

        [System.ThreadStatic]
        private static bool _isWriting;

        private Dictionary<string, Type> _typeCache;

        [System.ThreadStatic]
        private static Dictionary<Guid, Element> _elements;
        
        public static Dictionary<Guid, Element> Elements
        {
            get
            {
                if(_elements == null)
                {
                    _elements = new Dictionary<Guid, Element>();
                }
                return _elements;
            }
        }

        public JsonInheritanceConverter()
        {
            _discriminator = DefaultDiscriminatorName;
            _typeCache = BuildUserElementTypeCache();
        }

        public JsonInheritanceConverter(string discriminator)
        {
            _discriminator = discriminator;
            _typeCache = BuildUserElementTypeCache();
        }

        private Dictionary<string, Type> BuildUserElementTypeCache()
        {
            var typeCache = new Dictionary<string, Type>();

            // Build the user element type cache
            var asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var asm in asms)
            {
                try
                {
                    var userTypes = asm.GetTypes().Where(t=>t.GetCustomAttributes(typeof(UserElement), true).Length > 0);
                    foreach(var ut in userTypes)
                    {
                        typeCache.Add(ut.FullName, ut);
                    }
                }
                catch
                {
                    continue;
                }
            }

            return typeCache;
        }

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            try
            {
                _isWriting = true;

                // Operate on all identifiables with a path less than Entities.xxxxx
                // This will get all properties.
                if(value is Element && writer.Path.Split('.').Length == 1)
                {
                    var ident = (Element)value;
                    writer.WriteValue(ident.Id);
                }
                else
                {
                    var jObject = Newtonsoft.Json.Linq.JObject.FromObject(value, serializer);
                    jObject.AddFirst(new Newtonsoft.Json.Linq.JProperty(_discriminator, GetSubtypeDiscriminator(value.GetType())));
                    writer.WriteToken(jObject.CreateReader());
                }
            }
            finally
            {
                _isWriting = false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (_isWriting)
                {
                    _isWriting = false;
                    return false;
                }
                return true;
            }
        }

        public override bool CanRead
        {
            get
            {
                if (_isReading)
                {
                    _isReading = false;
                    return false;
                }
                return true;
            }
        }

        public override bool CanConvert(System.Type objectType)
        {
            return true;
        }

        public override object ReadJson(Newtonsoft.Json.JsonReader reader, System.Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            // The serialized value is an identifier, so the expectation is 
            // that the element with that id has already been deserialized.
            if(typeof(Element).IsAssignableFrom(objectType) && reader.Path.Split('.').Length == 1)
            {
                var id = Guid.Parse(reader.Value.ToString());
                return Elements[id];
            }

            var jObject = serializer.Deserialize<Newtonsoft.Json.Linq.JObject>(reader);
            if (jObject == null)
            {
                return null;
            }
            
            // We need to handle both cases where we're receiving JSON that has a
            // discriminator, like that produced by serializing using this
            // converter, and the case where the JSON does not have a discriminator,
            // but the type is known during deserialization.
            Type subtype;
            JToken discriminatorToken;
            string discriminator = null;
            jObject.TryGetValue(_discriminator, out discriminatorToken);
            if(discriminatorToken != null)
            {   
                discriminator = Newtonsoft.Json.Linq.Extensions.Value<string>(discriminatorToken);
                subtype = GetObjectSubtype(objectType, discriminator);
            
                var objectContract = serializer.ContractResolver.ResolveContract(subtype) as Newtonsoft.Json.Serialization.JsonObjectContract;
                if (objectContract == null || System.Linq.Enumerable.All(objectContract.Properties, p => p.PropertyName != _discriminator))
                {
                    jObject.Remove(_discriminator);
                }
            }
            else
            {
                // Without a discriminator the call to GetObjectSubtype will
                // fall through to returning either a [UserElement] type or 
                // the object type.
                subtype = GetObjectSubtype(objectType, null);
            }

            try
            {
                _isReading = true;
                var obj = serializer.Deserialize(jObject.CreateReader(), subtype);

                // Write the id to the cache so that we can retrieve it next time
                // instead of de-serializing it again.
                if(typeof(Element).IsAssignableFrom(objectType) && reader.Path.Split('.').Length > 1)
                {
                    var ident = (Element)obj;
                    if(!Elements.ContainsKey(ident.Id))
                    {
                        Elements.Add(ident.Id, ident);
                    }
                }

                return obj;
            }
            catch(Exception ex)
            {
                var baseMessage = "This may happen if the type is not recognized in the system.";
                var moreInfoMessage = "See the inner exception for more details.";

                if(discriminator != null)
                {
                    throw new Exception($"An object with the discriminator, {discriminator}, could not be deserialized. {baseMessage} {moreInfoMessage}", ex);
                }
                else
                {
                    throw new Exception($"{baseMessage} {moreInfoMessage}", ex);
                }
            }
            finally
            {
                _isReading = false;
            }
        }

        private System.Type GetObjectSubtype(System.Type objectType, string discriminator)
        {
            // Check the existing inheritance attributes.
            // This block will be hit when a type contains
            // an inheritance attribute specifying one of its subtypes.
            foreach (var attribute in System.Reflection.CustomAttributeExtensions.GetCustomAttributes<JsonInheritanceAttribute>(System.Reflection.IntrospectionExtensions.GetTypeInfo(objectType), true))
            {
                if (attribute.Key == discriminator)
                    return attribute.Type;
            }

            // If the inheritance attributes is not supplied, as in the case
            // of a user-provided type, then we use the type cache of all
            // types with the UserElementType attribute.
            if(discriminator != null && _typeCache.ContainsKey(discriminator))
            {
                return _typeCache[discriminator];
            }

            // The default behavior for this converter, as provided by nJSONSchema
            // is to return the base objectType if a derived type can't be found.
            return objectType;
        }

        private string GetSubtypeDiscriminator(System.Type objectType)
        {
            foreach (var attribute in System.Reflection.CustomAttributeExtensions.GetCustomAttributes<JsonInheritanceAttribute>(System.Reflection.IntrospectionExtensions.GetTypeInfo(objectType), true))
            {
                if (attribute.Type == objectType)
                    return attribute.Key;
            }
            
            return objectType.FullName;
        }
    }
}