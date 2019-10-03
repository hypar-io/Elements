using Elements.Serialization;
using Elements.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System;

namespace Elements
{
    /// <summary>
    /// A property with a string value.
    /// </summary>
    public class StringProperty : IPropertySingleValue<string>
    {
        /// <summary>
        /// The value of the Property.
        /// </summary>
        public string Value{get;}

        /// <summary>
        /// The UnitType of the Property.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public UnitType UnitType{get;}

        /// <summary>
        /// The description of the Property.
        /// </summary>
        public string Description{get;}

        /// <summary>
        /// Construct a Property.
        /// </summary>
        /// <param name="value">The value of the Property.</param>
        /// <param name="unitType">The unit type of the Property.</param>
        /// <param name="description">The description of the Property.</param>
        [JsonConstructor]
        public StringProperty(string value, UnitType unitType, string description = null)
        {
            if(value.GetType() != typeof(string) && value.GetType() != typeof(double))
            {
                throw new ArgumentException("The provided parameter value must be a string or a double.");
            }
            this.Value = value;
            this.UnitType = unitType;
            this.Description = description;
        }
    }

    /// <summary>
    /// A property with a numeric value.
    /// </summary>
    public class NumericProperty : IPropertySingleValue<double>
    {
        /// <summary>
        /// The value of the Property.
        /// </summary>
        public double Value{get;}

        /// <summary>
        /// The UnitType of the Property.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public UnitType UnitType{get;}

        /// <summary>
        /// The description of the Property.
        /// </summary>
        public string Description{get;}

        /// <summary>
        /// Construct a Property.
        /// </summary>
        /// <param name="value">The value of the Property.</param>
        /// <param name="unitType">The unit type of the Property.</param>
        /// <param name="description">The description of the Property.</param>
        [JsonConstructor]
        public NumericProperty(double value, UnitType unitType, string description = null)
        {
            if(value.GetType() != typeof(string) && value.GetType() != typeof(double))
            {
                throw new ArgumentException("The provided parameter value must be a string or a double.");
            }
            this.Value = value;
            this.UnitType = unitType;
            this.Description = description;
        }
    }
}