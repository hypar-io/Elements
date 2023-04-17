#pragma warning disable CS1591

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Elements.Serialization.JSON
{
    public class JsonInheritanceConverter : JsonConverter
    {
        internal static readonly string DefaultDiscriminatorName = "discriminator";

        private readonly string _discriminator;

        [System.ThreadStatic]
        private static bool _isReading;

        [System.ThreadStatic]
        private static bool _isWriting;

        [System.ThreadStatic]
        private static Dictionary<string, Type> _typeCache;
        private static Dictionary<string, Type> TypeCache
        {
            get
            {
                if (_typeCache == null)
                {
                    _typeCache = BuildAppDomainTypeCache(out _);
                }
                return _typeCache;
            }
        }

        [System.ThreadStatic]
        private static Dictionary<Guid, Element> _elements;

        public static Dictionary<Guid, Element> Elements
        {
            get
            {
                if (_elements == null)
                {
                    _elements = new Dictionary<Guid, Element>();
                }
                return _elements;
            }
        }

        public JsonInheritanceConverter()
        {
            _discriminator = DefaultDiscriminatorName;
        }

        public JsonInheritanceConverter(string discriminator)
        {
            _discriminator = discriminator;
        }

        /// <summary>
        /// The type cache needs to contains all types that will have a discriminator.
        /// This includes base types, like elements, and all derived types like Wall.
        /// We use reflection to find all public types available in the app domain
        /// that have a JsonConverterAttribute whose converter type is the
        /// Elements.Serialization.JSON.JsonInheritanceConverter.
        /// </summary>
        /// <returns>A dictionary containing all found types keyed by their full name.</returns>
        private static Dictionary<string, Type> BuildAppDomainTypeCache(out List<string> failedAssemblyErrors)
        {
            var typeCache = new Dictionary<string, Type>();

            failedAssemblyErrors = new List<string>();

            var skipAssembliesPrefices = new[] { "System", "SixLabors", "Newtonsoft" };
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a =>
            {
                var name = a.GetName().Name;
                return !skipAssembliesPrefices.Any(p => name.StartsWith(p));
            }))
            {
                var types = Array.Empty<Type>();
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    failedAssemblyErrors.Add($"Failed to load assembly: {assembly.FullName}");
                    continue;
                }
                foreach (var t in types)
                {
                    try
                    {
                        if (IsValidTypeForElements(t) && !typeCache.ContainsKey(t.FullName))
                        {
                            typeCache.Add(t.FullName, t);
                        }
                    }
                    catch (TypeLoadException)
                    {
                        failedAssemblyErrors.Add($"Failed to load type: {t.FullName}");
                        continue;
                    }
                }
            }

            return typeCache;
        }

        private static bool IsValidTypeForElements(Type t)
        {
            if (t.IsPublic && t.IsClass)
            {
                var attrib = t.GetCustomAttribute<JsonConverterAttribute>();
                if (attrib != null && attrib.ConverterType == typeof(JsonInheritanceConverter))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Call this method after assemblies have been loaded into the app
        /// domain to ensure that the converter's cache is up to date.
        /// </summary>
        internal static void RefreshAppDomainTypeCache(out List<string> errors)
        {
            _typeCache = BuildAppDomainTypeCache(out errors);
        }

        public static bool ElementwiseSerialization { get; set; } = false;

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            try
            {
                _isWriting = true;

                // Operate on all identifiable Elements with a path less than Entities.xxxxx
                // This will get all properties.
                if (value is Element element && !WritingTopLevelElement(writer.Path) && !ElementwiseSerialization)
                {
                    var ident = element;
                    writer.WriteValue(ident.Id);
                }
                else
                {
                    var jObject = Newtonsoft.Json.Linq.JObject.FromObject(value, serializer);
                    if (jObject.TryGetValue(_discriminator, out JToken token))
                    {
                        ((JValue)token).Value = GetDiscriminatorName(value);
                    }
                    else
                    {
                        jObject.AddFirst(new Newtonsoft.Json.Linq.JProperty(_discriminator, GetDiscriminatorName(value)));
                    }
                    writer.WriteToken(jObject.CreateReader());
                }
            }
            finally
            {
                _isWriting = false;
            }
        }

        private static bool WritingTopLevelElement(string path)
        {
            var parts = path.Split('.');
            if (parts.Length == 2 && parts[0] == "Elements" && Guid.TryParse(parts[1], out var _))
            {
                return true;
            }
            return false;
        }

        private static string GetDiscriminatorName(object value)
        {
            var t = value.GetType();
            if (t.IsGenericType)
            {
                return $"{t.FullName.Split('`').First()}<{String.Join(",", t.GenericTypeArguments.Select(arg => arg.FullName))}>";
            }
            else
            {
                return t.FullName.Split('`').First();
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
            if (typeof(Element).IsAssignableFrom(objectType) && !WritingTopLevelElement(reader.Path) && reader.Value != null)
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
            string discriminator = null;
            jObject.TryGetValue(_discriminator, out JToken discriminatorToken);
            if (discriminatorToken != null)
            {
                discriminator = Newtonsoft.Json.Linq.Extensions.Value<string>(discriminatorToken);
                subtype = GetObjectSubtype(objectType, discriminator, jObject);
            }
            else
            {
                // Without a discriminator the call to GetObjectSubtype will
                // fall through to returning either a [UserElement] type or
                // the object type.
                subtype = GetObjectSubtype(objectType, null, jObject);
            }

            try
            {
                _isReading = true;
                var obj = serializer.Deserialize(jObject.CreateReader(), subtype);

                // Write the id to the cache so that we can retrieve it next time
                // instead of de-serializing it again.
                if (typeof(Element).IsAssignableFrom(objectType) && WritingTopLevelElement(reader.Path))
                {
                    var ident = (Element)obj;
                    if (!Elements.ContainsKey(ident.Id))
                    {
                        Elements.Add(ident.Id, ident);
                    }
                }

                return obj;
            }
            catch (Exception ex)
            {
                var baseMessage = "This may happen if the type is not recognized in the system.";
                var moreInfoMessage = "See the inner exception for more details.";

                if (discriminator != null)
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

        private System.Type GetObjectSubtype(System.Type objectType, string discriminator, JObject jObject)
        {
            // Check the type cache.
            if (discriminator != null && TypeCache.ContainsKey(discriminator))
            {
                return TypeCache[discriminator];
            }

            // Check for proxy generics.
            if (discriminator != null && discriminator.StartsWith("Elements.ElementProxy<"))
            {
                var typeNames = discriminator.Split('<')[1].Split('>')[0].Split(','); // We do this split because in theory we can serialize with multiple generics
                var typeName = typeNames.FirstOrDefault();
                var generic = TypeCache[typeName];
                var proxy = typeof(ElementProxy<>).MakeGenericType(generic);
                return proxy;
            }

            // If it's not in the type cache see if it's got a representation.
            // Import it as a GeometricElement.
            if (jObject.TryGetValue("Representation", out _))
            {
                return typeof(GeometricElement);
            }

            // The default behavior for this converter, as provided by nJSONSchema
            // is to return the base objectType if a derived type can't be found.
            return objectType;
        }
    }
}