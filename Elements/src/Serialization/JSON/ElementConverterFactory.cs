using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elements.Serialization.JSON
{
    internal class ElementConverterFactory : JsonConverterFactory
    {
        private readonly bool _elementwiseSerialization;

        /// <summary>
        /// Should the elements be serialized completely? If this option is false,
        /// elements will be serialized to ids.
        /// </summary>
        /// <param name="elementwiseSerialization"></param>
        public ElementConverterFactory(bool elementwiseSerialization = false)
        {
            _elementwiseSerialization = elementwiseSerialization;
        }

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
            var pi = converterType.GetProperty("ElementwiseSerialization");
            pi.SetValue(converter, _elementwiseSerialization);
            return converter;
        }
    }
}