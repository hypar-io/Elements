using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elements.Serialization.JSON
{
    internal static class PropertySerializationExtensions
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

        public static void DeserializeElementProperties(Type derivedType,
                                                   JsonElement root,
                                                   ReferenceResolver resolver,
                                                   JsonElement documentElements)
        {
            var elementProperties = derivedType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => typeof(Element).IsAssignableFrom(p.PropertyType));
            foreach (var elementProperty in elementProperties)
            {
                var prop = root.GetProperty(elementProperty.Name);
                if (prop.ValueKind == JsonValueKind.Null)
                {
                    // You'll get here when you've got a null reference to an element. 
                    // Resolve to an empty id, causing the resolver to return null.
                    resolver.ResolveReference(string.Empty);
                    continue;
                }

                if (prop.TryGetGuid(out var referencedId))
                {
                    if (resolver.ResolveReference(referencedId.ToString()) != null)
                    {
                        continue;
                    }

                    if (documentElements.TryGetProperty(referencedId.ToString(), out var propertyBody))
                    {
                        if (propertyBody.TryGetProperty("discriminator", out var elementBody))
                        {
                            var referencedElement = (Element)prop.Deserialize(elementProperty.PropertyType);
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
            }
        }
    }
}