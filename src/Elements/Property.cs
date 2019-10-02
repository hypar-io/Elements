using Newtonsoft.Json;
using System;

namespace Elements
{
    /// <summary>
    /// A property with a string value.
    /// </summary>
    public partial class StringProperty
    {
        /// <summary>
        /// The description of the Property.
        /// </summary>
        public string Description{get;}

        /// <summary>
        /// Construct a Property.
        /// </summary>
        /// <param name="value">The value of the Property.</param>
        /// <param name="description">The description of the Property.</param>
        [JsonConstructor]
        public StringProperty(string value, string description = null)
        {
            if(value.GetType() != typeof(string) && value.GetType() != typeof(double))
            {
                throw new ArgumentException("The provided parameter value must be a string or a double.");
            }
            this.Value = value;
            this.Description = description;
        }
    }

    /// <summary>
    /// A property with a numeric value.
    /// </summary>
    public partial class NumericProperty
    {
        /// <summary>
        /// The description of the Property.
        /// </summary>
        public string Description{get;}

        /// <summary>
        /// Construct a Property.
        /// </summary>
        /// <param name="value">The value of the Property.</param>
        /// <param name="valueType">The value type of the Property.</param>
        /// <param name="description">The description of the Property.</param>
        [JsonConstructor]
        public NumericProperty(double value, NumericPropertyValueType valueType, string description = null)
        {
            if(value.GetType() != typeof(string) && value.GetType() != typeof(double))
            {
                throw new ArgumentException("The provided parameter value must be a string or a double.");
            }
            this.Value = value;
            this.ValueType = valueType;
            this.Description = description;
        }
    }
}