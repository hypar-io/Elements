using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Spatial;

namespace Elements.Serialization.JSON
{
    internal class DiscriminatorConverterFactory : JsonConverterFactory
    {
        private readonly Dictionary<string, Type> _typeCache;

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

    internal class DiscriminatorConverter<TDiscriminated> : JsonConverter<TDiscriminated>
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
}