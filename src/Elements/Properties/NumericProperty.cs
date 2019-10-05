using Newtonsoft.Json;
using System;

namespace Elements.Properties
{
    /// <summary>
    /// A property with a numeric value.
    /// </summary>
    public partial class NumericProperty
    {
        /// <summary>
        /// Construct a Property.
        /// </summary>
        /// <param name="value">The value of the Property.</param>
        /// <param name="unitType">The unit type of the Property.</param>
        [JsonConstructor]
        public NumericProperty(double value, NumericPropertyUnitType unitType)
        {
            if(value.GetType() != typeof(string) && value.GetType() != typeof(double))
            {
                throw new ArgumentException("The provided parameter value must be a string or a double.");
            }
            this.Value = value;
            this.UnitType = unitType;
        }
    }
}