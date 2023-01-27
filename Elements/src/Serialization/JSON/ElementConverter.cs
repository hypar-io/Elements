using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elements.Serialization.JSON
{
    /// <summary>
    /// Convert elements, lists of elements, and dictionaries of elements,
    /// and elements with generic type parameters.
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ElementConverter<T> : JsonConverter<T>
    {
        public bool ElementwiseSerialization { get; internal set; } = false;

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (options.ReferenceHandler == null)
            {
                throw new Exception("You are deserializing an element, but you don't have a reference resolver. Try using Element.Deserialize<T> instead.");
            }

            var resolver = options.ReferenceHandler.CreateResolver() as ElementReferenceResolver;

            if (reader.TokenType == JsonTokenType.String)
            {
                // Convert an id reference into an element.
                var id = reader.GetString();
                return (T)resolver.ResolveReference(id);
            }
            else
            {
                if (IsAcceptedCollectionType(typeToConvert, out var collectionType))
                {
                    var elements = Activator.CreateInstance(typeToConvert);
                    var mi = typeToConvert.GetMethod("Add");
                    switch (collectionType)
                    {
                        case CollectionType.List:
                            // At this point we'll be at the start of an array.
                            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                            {
                                var id = reader.GetString();
                                mi.Invoke(elements, new[] { resolver.ResolveReference(id) });
                            }

                            break;
                        case CollectionType.Dictionary:
                            var args = typeToConvert.GetGenericArguments();
                            // At this point we'll be at the start of an object
                            // This will be a dictionary that looks like id: id
                            using (var doc = JsonDocument.ParseValue(ref reader))
                            {
                                var root = doc.RootElement;
                                foreach (var prop in root.EnumerateObject())
                                {
                                    if (args[0] == typeof(Guid))
                                    {
                                        mi.Invoke(elements, new[] { Guid.Parse(prop.Name), resolver.ResolveReference(prop.Value.GetString()) });
                                    }
                                    else
                                    {
                                        mi.Invoke(elements, new[] { prop.Name, resolver.ResolveReference(prop.Value.GetString()) });
                                    }
                                }
                            }
                            break;
                    }
                    return (T)elements;
                }
                else
                {
                    using (var doc = JsonDocument.ParseValue(ref reader))
                    {
                        // Deserialize an element.
                        var root = doc.RootElement;

                        var discriminator = root.GetProperty("discriminator").GetString();
                        Type derivedType;

                        // Handle discriminators like ElementProxy<Elements.Mass>
                        if (discriminator.Contains("<"))
                        {
                            // Strip the element type from the discriminator.
                            int start = discriminator.LastIndexOf("<") + 1;
                            int end = discriminator.IndexOf(">", start);
                            string result = discriminator.Remove(start, end - start);
                            if (!TryGetElementTypeWithFallbackToGeometricElement(resolver, result, root, out derivedType))
                            {
                                return default;
                            }
                        }
                        else
                        {
                            if (!TryGetElementTypeWithFallbackToGeometricElement(resolver, discriminator, root, out derivedType))
                            {
                                return default;
                            }
                        }

                        if (derivedType.IsGenericType)
                        {
                            // Recover the type argument from the discriminator.
                            // TODO: Support multiple type arguments.
                            int start = discriminator.LastIndexOf("<") + 1;
                            int end = discriminator.IndexOf(">", start);
                            string elementType = discriminator.Substring(start, end - start);
                            if (!resolver.TypeCache.TryGetValue(elementType, out var genericType))
                            {
                                // This may occur if the object is a proxy of a type we don't have the class for. Fallback to `Element`.
                                genericType = typeof(Element);
                            }
                            var typeArgs = new[] { genericType };
                            var genericElementType = derivedType.MakeGenericType(typeArgs);

                            if (discriminator.Contains("Elements.ElementProxy"))
                            {
                                var id = root.GetProperty("Id").GetGuid();
                                var name = root.GetProperty("Name").GetString();
                                var elementId = root.GetProperty("elementId").GetGuid();
                                var dependency = root.GetProperty("dependency").GetString();
                                var genericElement = (T)Activator.CreateInstance(genericElementType, new object[] { elementId, dependency, id, name });

                                return genericElement;
                            }

                            throw new Exception("Generic element types other than ElementProxy<T> are not currently supported.");
                        }
                        else
                        {
                            // Use the type info to get all properties which are Element
                            // references, and deserialize those first.

                            // TODO: This *should* support serialization of elements in
                            // any order, removing the requirement to do any kind of recursive
                            // sub-element searching. We can remove that code from the model.
                            DeserializeElementProperties(derivedType, root, resolver, resolver.DocumentElements);

                            // Use this for debugging. If you can't figure out
                            // where serialization is going berserk, it'll print
                            // the element ids as they serialize. This is useful
                            // when things like stack overflows happen, to identify
                            // the last element entered before the overflow. Then you 
                            // can go and look at that element in the JSON and
                            // try to understand what's happening.
                            // if (root.TryGetProperty("Id", out var id))
                            // {
                            //     var strId = id.GetString();
                            //     Console.WriteLine($"Deserializing element {strId}");
                            // }
                            T e = (T)root.Deserialize(derivedType, options);
                            if (typeof(Element).IsAssignableFrom(derivedType))
                            {
                                resolver.AddReference(((Element)(object)e).Id.ToString(), e);
                            }
                            return e;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The type could not be found. See if it has the hallmarks
        /// of a geometric element and deserialize it as such if possible.
        /// </summary>
        /// <param name="resolver">The element reference resolver.</param>
        /// <param name="discriminator">The discriminator.</param>
        /// <param name="root">The json element within which we'll search for geometric element properties.</param>
        /// <param name="derivedType">The found derived type.</param>
        /// <returns>True if the element is a geometric element, otherwise false.</returns>
        private bool TryGetElementTypeWithFallbackToGeometricElement(ElementReferenceResolver resolver, string discriminator, JsonElement root, out Type derivedType)
        {
            if (!resolver.TypeCache.TryGetValue(discriminator, out derivedType))
            {
                if (root.TryGetProperty("Representation", out _))
                {
                    derivedType = typeof(GeometricElement);
                    return true;
                }
                else
                {
                    derivedType = null;
                    return false;
                }
            }
            return true;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var isElement = typeof(Element).IsAssignableFrom(value.GetType());
            if (writer.CurrentDepth > 2 && isElement && !ElementwiseSerialization)
            {
                writer.WriteStringValue(((Element)(object)value).Id.ToString());
            }
            else
            {
                writer.WriteStartObject();
                WriteProperties(value, writer, options);
                writer.WriteEndObject();
            }
        }

        public void WriteProperties(object value, Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            // Inject the discriminator into the serialized JSON.
            writer.WriteString("discriminator", GetDiscriminatorName(value));

            var pinfos = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var pinfo in pinfos)
            {
                // Skip ignored properties
                var ignoreAttrib = pinfo.GetCustomAttribute<JsonIgnoreAttribute>();
                if (ignoreAttrib != null)
                {
                    continue;
                }

                // Honor the renaming of a property
                var nameAttrib = pinfo.GetCustomAttribute<JsonPropertyNameAttribute>();

                writer.WritePropertyName(nameAttrib != null ? nameAttrib.Name : pinfo.Name);
                JsonSerializer.Serialize(writer, pinfo.GetValue(value), pinfo.PropertyType, options);
            }

            // Support public fields.
            var finfos = value.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var finfo in finfos)
            {
                var nameAttrib = finfo.GetCustomAttribute<JsonPropertyNameAttribute>();

                writer.WritePropertyName(nameAttrib != null ? nameAttrib.Name : finfo.Name);
                JsonSerializer.Serialize(writer, finfo.GetValue(value), finfo.FieldType, options);
            }
        }

        private void DeserializeElementProperties(Type derivedType,
                                                   JsonElement root,
                                                   ReferenceResolver resolver,
                                                   JsonElement documentElements)
        {
            var elementReferenceResolver = resolver as ElementReferenceResolver;

            var elementProperties = derivedType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p =>
            {
                // Properties which are elements.
                if (typeof(Element).IsAssignableFrom(p.PropertyType))
                {
                    return true;
                };

                return IsAcceptedCollectionType(p.PropertyType, out _);
            });

            foreach (var elementProperty in elementProperties)
            {
                if (!root.TryGetProperty(elementProperty.Name, out var prop) ||
                prop.ValueKind == JsonValueKind.Null)
                {
                    // You'll get here when you've got a null reference to an element,
                    // or you've got no element at all in the json. 
                    // Resolve to an empty id, causing the resolver to return null.
                    elementReferenceResolver.ResolveReference(string.Empty);
                    continue;
                }
                else if (prop.ValueKind == JsonValueKind.Array)
                {
                    foreach (var value in prop.EnumerateArray())
                    {
                        if (value.TryGetGuid(out var referenceId))
                        {
                            if (documentElements.TryGetProperty(referenceId.ToString(), out var foundElement))
                            {
                                if (foundElement.TryGetProperty("discriminator", out var discriminatorProp))
                                {
                                    if (elementReferenceResolver.TypeCache.TryGetValue(discriminatorProp.GetString(), out var discriminatorValue))
                                    {
                                        HandleReferenceId(value, referenceId, resolver, documentElements, discriminatorValue);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // The array element is not an element.
                            // Just deserialize it.
                        }
                    }
                    continue;
                }
                else if (prop.ValueKind == JsonValueKind.Object)
                {
                    foreach (var innerProp in prop.EnumerateObject())
                    {
                        if (innerProp.Value.TryGetGuid(out var referenceId))
                        {
                            if (documentElements.TryGetProperty(referenceId.ToString(), out var foundElement))
                            {
                                if (foundElement.TryGetProperty("discriminator", out var discriminatorProp))
                                {
                                    if (elementReferenceResolver.TypeCache.TryGetValue(discriminatorProp.GetString(), out var discriminatorValue))
                                    {
                                        HandleReferenceId(innerProp.Value, referenceId, resolver, documentElements, discriminatorValue);
                                    }
                                }
                            }
                        }
                    }
                    continue;
                }

                if (prop.TryGetGuid(out var referencedId))
                {
                    HandleReferenceId(prop, referencedId, resolver, documentElements, elementProperty.PropertyType);
                }
            }
        }

        private bool IsAcceptedCollectionType(Type propertyType, out CollectionType collectionType)
        {
            if (propertyType.IsGenericType)
            {
                var def = propertyType.GetGenericTypeDefinition();
                var args = propertyType.GetGenericArguments();

                // Properties which are List<Element>
                if (def == typeof(List<>))
                {
                    if (typeof(Element).IsAssignableFrom(args[0]))
                    {
                        collectionType = CollectionType.List;
                        return true;
                    }
                }
                // Properties which are Dictionary<Guid, Element> or Dictionary<string, Element>
                else if (def == typeof(Dictionary<,>))
                {
                    if ((args[0] == typeof(Guid) || args[0] == typeof(string)) && typeof(Element).IsAssignableFrom(args[1]))
                    {
                        collectionType = CollectionType.Dictionary;
                        return true;
                    }
                }
            }
            collectionType = CollectionType.None;
            return false;
        }

        internal static void HandleReferenceId(JsonElement elementToDeserialize,
                                              Guid referencedId,
                                              ReferenceResolver resolver,
                                              JsonElement documentElements,
                                              Type propertyType)
        {
            if (resolver.ResolveReference(referencedId.ToString()) != null)
            {
                return;
            }

            if (documentElements.TryGetProperty(referencedId.ToString(), out var propertyBody))
            {
                if (propertyBody.TryGetProperty("discriminator", out _))
                {
                    var referencedElement = (Element)elementToDeserialize.Deserialize(propertyType);
                    resolver.AddReference(referencedId.ToString(), referencedElement);
                }
            }
            else
            {
                // The reference cannot be found. It's either not 
                // a direct reference, as in the case of a cross-model
                // reference, or it's just broken.
                resolver.AddReference(referencedId.ToString(), null);
            }
        }

        private string GetDiscriminatorName(object value)
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
    }

    internal enum CollectionType
    {
        None,
        List,
        Dictionary
    }
}