using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elements.Serialization.JSON
{
    internal enum CollectionType
    {
        None,
        List,
        Dictionary
    }

    internal static class PropertySerializationExtensions
    {
        public static void WriteProperties(this object value, Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            // Inject the discriminator into the serialized JSON.
            writer.WriteString("discriminator", value.GetDiscriminatorName());

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

        public static void DeserializeElementProperties(Type derivedType,
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


        internal static bool IsAcceptedCollectionType(Type propertyType, out CollectionType collectionType)
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

        private static void HandleReferenceId(JsonElement elementToDeserialize,
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
    }
}