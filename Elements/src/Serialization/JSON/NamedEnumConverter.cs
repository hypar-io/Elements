using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elements.Serialization.JSON
{
    /// <summary>
    /// A drop-in replacement for `JsonStringEnumConverter` which respects `[EnumMember]` attributes.
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    public class NamedEnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, System.Enum
    {
        private readonly Dictionary<TEnum, string> _enumToString = new Dictionary<TEnum, string>();
        private readonly Dictionary<string, TEnum> _stringToEnum = new Dictionary<string, TEnum>();
        private readonly Dictionary<int, TEnum> _numberToEnum = new Dictionary<int, TEnum>();

        /// <summary>
        /// Create a new converter.
        /// </summary>
        public NamedEnumConverter()
        {
            var type = typeof(TEnum);

            foreach (var value in Enum.GetValues(typeof(TEnum)))
            {
                var enumMember = type.GetMember(value.ToString())[0];
                var attr = enumMember.GetCustomAttributes(typeof(EnumMemberAttribute), false)
                  .Cast<EnumMemberAttribute>()
                  .FirstOrDefault();

                _stringToEnum.Add(value.ToString(), (TEnum)value);
                var num = Convert.ToInt32(type.GetField("value__")
                        .GetValue(value));
                if (attr?.Value != null)
                {
                    _enumToString.Add((TEnum)value, attr.Value);
                    _stringToEnum.Add(attr.Value, (TEnum)value);
                    _numberToEnum.Add(num, (TEnum)value);
                }
                else
                {
                    _enumToString.Add((TEnum)value, value.ToString());
                }
            }
        }

        /// <inheritdoc/>
        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var type = reader.TokenType;
            if (type == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (_stringToEnum.TryGetValue(stringValue, out var enumValue))
                {
                    return enumValue;
                }
            }
            else if (type == JsonTokenType.Number)
            {
                var numValue = reader.GetInt32();
                _numberToEnum.TryGetValue(numValue, out var enumValue);
                return enumValue;
            }

            return default;
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(_enumToString[value]);
        }
    }

}