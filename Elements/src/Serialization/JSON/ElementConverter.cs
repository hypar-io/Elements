using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Elements.Geometry;

namespace Elements.Serialization.JSON
{
    internal class ElementConverterFactory : JsonConverterFactory
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

    internal class ElementConverter<TElement> : JsonConverter<TElement> where TElement : Element
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
                // Reuse the existing discriminator factory converter.
                o.Converters.Add(options.Converters.First(c => c.GetType() == typeof(DiscriminatorConverterFactory)));
                o.Converters.Add(new ElementIdConverterFactory(_elements));
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
}