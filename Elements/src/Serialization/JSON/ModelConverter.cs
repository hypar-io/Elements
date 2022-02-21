using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Spatial;

namespace Elements.Serialization.JSON
{
    public static class PropertySerializationExtensions
    {
        public static void WriteProperties(this object value, Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            writer.WriteString("discriminator", value.GetDiscriminatorName());

            var pinfos = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var pinfo in pinfos)
            {
                // Skip ignored properties
                var attrib = pinfo.GetCustomAttribute<JsonIgnoreAttribute>();
                if (attrib != null)
                {
                    continue;
                }

                // TODO: Use the Newtonsoft version as well.
                var newtonIgnore = pinfo.GetCustomAttribute<Newtonsoft.Json.JsonIgnoreAttribute>();
                if (newtonIgnore != null)
                {
                    continue;
                }

                writer.WritePropertyName(pinfo.Name);
                JsonSerializer.Serialize(writer, pinfo.GetValue(value), pinfo.PropertyType, options);
            }
        }

        private static string GetDiscriminatorName(this object value)
        {
            var t = value.GetType();
            if (t.IsGenericType)
            {
                return $"{t.FullName.Split('`').First()}<{string.Join(",", t.GenericTypeArguments.Select(arg => arg.FullName))}>";
            }
            else
            {
                return t.FullName.Split('`').First();
            }
        }

        public static JsonSerializerOptions CopyAndRemoveConverter(this JsonSerializerOptions options, Type converterType)
        {
            var copy = new JsonSerializerOptions(options);
            for (var i = copy.Converters.Count - 1; i >= 0; i--)
                if (copy.Converters[i].GetType() == converterType)
                    copy.Converters.RemoveAt(i);
            return copy;
        }

        public static Type GetObjectSubtype(Type objectType, string discriminator, Dictionary<string, Type> typeCache)
        {
            // Check the type cache.
            if (discriminator != null && typeCache.ContainsKey(discriminator))
            {
                return typeCache[discriminator];
            }

            // Check for proxy generics.
            if (discriminator != null && discriminator.StartsWith("Elements.ElementProxy<"))
            {
                var typeNames = discriminator.Split('<')[1].Split('>')[0].Split(','); // We do this split because in theory we can serialize with multiple generics
                var typeName = typeNames.FirstOrDefault();
                var generic = typeCache[typeName];
                var proxy = typeof(ElementProxy<>).MakeGenericType(generic);
                return proxy;
            }

            // If it's not in the type cache see if it's got a representation.
            // Import it as a GeometricElement.
            // if (jObject.TryGetValue("Representation", out _))
            // {
            //     return typeof(GeometricElement);
            // }

            // The default behavior for this converter, as provided by nJSONSchema
            // is to return the base objectType if a derived type can't be found.
            return objectType;
        }
    }

    public class DiscriminatorConverterFactory : JsonConverterFactory
    {
        private Dictionary<string, Type> _typeCache;

        public DiscriminatorConverterFactory(Dictionary<string, Type> typeCache = null)
        {
            _typeCache = typeCache;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(Curve)
                   || typeToConvert == typeof(Grid1d)
                   || typeToConvert == typeof(Grid2d)
                   || typeToConvert == typeof(SolidOperation);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert == typeof(Curve))
            {
                return new DiscriminatorConverter<Curve>(_typeCache);
            }
            else if (typeToConvert == typeof(Grid1d))
            {
                return new DiscriminatorConverter<Grid1d>(_typeCache);
            }
            else if (typeToConvert == typeof(Grid2d))
            {
                return new DiscriminatorConverter<Grid2d>(_typeCache);
            }
            else if (typeToConvert == typeof(SolidOperation))
            {
                return new DiscriminatorConverter<SolidOperation>(_typeCache);
            }

            throw new JsonException("A discriminator converter could not be found.");
        }
    }

    public class DiscriminatorConverter<TDiscriminated> : JsonConverter<TDiscriminated>
    {
        private Dictionary<string, Type> _typeCache;

        public DiscriminatorConverter(Dictionary<string, Type> typeCache)
        {
            _typeCache = typeCache;
        }

        public override TDiscriminated Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;
                var discriminator = root.GetProperty("discriminator").GetString();
                var subType = _typeCache[discriminator];
                return (TDiscriminated)JsonSerializer.Deserialize(root, subType, options);
            }
        }

        public override void Write(Utf8JsonWriter writer, TDiscriminated value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            value.WriteProperties(writer, options);
            writer.WriteEndObject();
        }
    }

    public class ElementConverterFactory : JsonConverterFactory
    {
        private readonly Dictionary<Guid, Element> _elements;

        private readonly Dictionary<string, Type> _typeCache;

        /// <summary>
        /// An element representing the Elements property on the 
        /// model object being deserialized. This is used to do 
        /// forward-looking deserialization of nested references.
        /// </summary>
        private readonly JsonElement _documentElements;

        /// <summary>
        /// Construct an element converter factory.
        /// </summary>
        /// <param name="elements">A cache of elements.</param>
        /// <param name="typeCache">A cache of types.</param>
        /// <param name="documentElements">The elements node of a model being deserialized.</param>
        public ElementConverterFactory(Dictionary<Guid, Element> elements,
                                       Dictionary<string, Type> typeCache = null,
                                       JsonElement documentElements = default)
        {
            _elements = elements;
            _documentElements = documentElements;
            _typeCache = typeCache;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(Element).IsAssignableFrom(typeToConvert);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert == typeof(Material))
            {
                return new ElementConverter<Material>(_elements, _typeCache, _documentElements);
            }
            else if (typeToConvert == typeof(Profile))
            {
                return new ElementConverter<Profile>(_elements, _typeCache, _documentElements);
            }
            else if (typeToConvert == typeof(GeometricElement))
            {
                return new ElementConverter<GeometricElement>(_elements, _typeCache, _documentElements);
            }
            else
            {
                return new ElementConverter<Element>(_elements, _typeCache, _documentElements);
            }
        }
    }

    public class ElementIdConverter<TElement> : JsonConverter<TElement> where TElement : Element
    {
        private readonly Dictionary<Guid, Element> _elements;

        public ElementIdConverter()
        {
            _elements = new Dictionary<Guid, Element>();
        }

        public ElementIdConverter(Dictionary<Guid, Element> elements)
        {
            _elements = elements;
        }

        public override TElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var id = reader.GetGuid();
            if (_elements.ContainsKey(id))
            {
                return (TElement)_elements[id];
            }
            else
            {
                return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, TElement value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Id.ToString());
        }
    }

    public class ElementConverter<TElement> : JsonConverter<TElement> where TElement : Element
    {
        private readonly Dictionary<Guid, Element> _elements;
        private readonly Dictionary<string, Type> _typeCache;
        private readonly JsonElement _documentElements;

        public ElementConverter(Dictionary<Guid, Element> elements,
                                Dictionary<string, Type> typeCache,
                                JsonElement documentElements)
        {
            _elements = elements;
            _typeCache = typeCache;
            _documentElements = documentElements;
        }

        public override TElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;

                var discriminator = root.GetProperty("discriminator").GetString();
                var subType = _typeCache[discriminator];

                // Use the type info to get all properties which are Element
                // references, and deserialize those first.

                // TODO: This *should* support serialization of elements in
                // any order, removing the requirement to do any kind of recursive
                // sub-element searching. We can remove that code from the model.
                var elementProperties = subType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => typeof(Element).IsAssignableFrom(p.PropertyType));
                foreach (var elementProperty in elementProperties)
                {
                    var prop = root.GetProperty(elementProperty.Name);
                    if (prop.TryGetGuid(out var referencedId))
                    {
                        if (_elements.ContainsKey(referencedId))
                        {
                            continue;
                        }

                        if (_documentElements.TryGetProperty(referencedId.ToString(), out var propertyBody))
                        {
                            if (propertyBody.TryGetProperty("discriminator", out var elementBody))
                            {
                                var referencedElement = (Element)prop.Deserialize(elementProperty.PropertyType);
                                _elements[referencedId] = referencedElement;
                            }
                        }
                        else
                        {
                            // The reference cannot be found. It's either not 
                            // a direct reference, as in the case of a cross-model
                            // reference, or it's just broken. 
                            _elements[referencedId] = null;
                        }
                    }
                }

                // Deserialize without further specifying the converter to avoid an infinite loop.
                var o = new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                };
                o.Converters.Add(new DiscriminatorConverterFactory(_typeCache));
                TElement e = (TElement)root.Deserialize(subType, o);
                _elements.Add(e.Id, e);
                return e;
            }
        }

        public override void Write(Utf8JsonWriter writer, TElement value, JsonSerializerOptions options)
        {
            if (writer.CurrentDepth > 2)
            {
                writer.WriteStringValue(value.Id.ToString());
            }
            else
            {
                writer.WriteStartObject();
                value.WriteProperties(writer, options);
                writer.WriteEndObject();
            }
        }
    }

    public class ModelConverter : JsonConverter<Model>
    {

        public override Model Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Model value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            // Write the transform
            writer.WritePropertyName("Transform");
            JsonSerializer.Serialize(writer, value.Transform);

            //Write the elements dictionary
            writer.WritePropertyName("Elements");
            writer.WriteStartObject();
            foreach (var kvp in value.Elements)
            {
                writer.WritePropertyName(kvp.Key.ToString());
                JsonSerializer.Serialize(writer, kvp.Value, kvp.Value.GetType(), options);
            }
            writer.WriteEndObject();

            writer.WriteEndObject();
        }
    }
}