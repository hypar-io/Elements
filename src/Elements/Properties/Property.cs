using Newtonsoft.Json;
using System;

namespace Elements.Properties
{
    /// <summary>
    /// A property with a string value.
    /// </summary>
    public partial class StringProperty: Property
    {
        /// <summary>
        /// Construct a Property.
        /// </summary>
        /// <param name="value">The value of the Property.</param>
        [JsonConstructor]
        public StringProperty(string value)
        {
            if(value.GetType() != typeof(string) && value.GetType() != typeof(double))
            {
                throw new ArgumentException("The provided parameter value must be a string or a double.");
            }
            this.Value = value;
        }
    }

    /// <summary>
    /// A property with a numeric value.
    /// </summary>
    public partial class NumericProperty: Property
    {
        /// <summary>
        /// Construct a Property.
        /// </summary>
        /// <param name="value">The value of the Property.</param>
        /// <param name="valueType">The value type of the Property.</param>
        [JsonConstructor]
        public NumericProperty(double value, NumericPropertyValueType valueType)
        {
            if(value.GetType() != typeof(string) && value.GetType() != typeof(double))
            {
                throw new ArgumentException("The provided parameter value must be a string or a double.");
            }
            this.Value = value;
            this.ValueType = valueType;
        }
    }
}