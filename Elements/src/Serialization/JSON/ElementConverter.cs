using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elements.Serialization.JSON
{
    internal class ElementConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(Element).IsAssignableFrom(typeToConvert);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var elementConverter = typeof(ElementConverter<>);
            var typeArgs = new[] { typeToConvert };
            var converterType = elementConverter.MakeGenericType(typeArgs);
            var converter = Activator.CreateInstance(converterType) as JsonConverter;
            return converter;
        }
    }

    internal class ElementConverter<T> : JsonConverter<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var resolver = options.ReferenceHandler.CreateResolver() as ElementReferenceResolver;

            if (reader.TokenType == JsonTokenType.String)
            {
                var id = reader.GetString();
                return (T)resolver.ResolveReference(id.ToString());
            }


            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var root = doc.RootElement;

                var discriminator = root.GetProperty("discriminator").GetString();
                var derivedType = resolver.TypeCache[discriminator];

                // Use the type info to get all properties which are Element
                // references, and deserialize those first.

                // TODO: This *should* support serialization of elements in
                // any order, removing the requirement to do any kind of recursive
                // sub-element searching. We can remove that code from the model.
                PropertySerializationExtensions.DeserializeElementProperties(derivedType, root, resolver, resolver.DocumentElements);

                T e = (T)root.Deserialize(derivedType, options);
                if (typeof(Element).IsAssignableFrom(derivedType))
                {
                    resolver.AddReference(((Element)(object)e).Id.ToString(), e);
                }
                return e;
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var isElement = typeof(Element).IsAssignableFrom(value.GetType());
            if (writer.CurrentDepth > 2 && isElement)
            {
                writer.WriteStringValue(((Element)(object)value).Id.ToString());
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