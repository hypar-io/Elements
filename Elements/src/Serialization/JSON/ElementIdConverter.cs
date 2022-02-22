using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elements.Serialization.JSON
{
    internal class ElementIdConverterFactory : JsonConverterFactory
    {
        private readonly Dictionary<Guid, Element> _elements;

        public ElementIdConverterFactory(Dictionary<Guid, Element> elements)
        {
            _elements = elements;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(Element).IsAssignableFrom(typeToConvert);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var elementIdConverterType = typeof(ElementIdConverter<>);
            var typeArgs = new[] { typeToConvert };
            var converterType = elementIdConverterType.MakeGenericType(typeArgs);
            var converter = Activator.CreateInstance(converterType, new[] { _elements }) as JsonConverter;
            return converter;
        }
    }

    internal class ElementIdConverter<TElement> : JsonConverter<TElement> where TElement : Element
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
}