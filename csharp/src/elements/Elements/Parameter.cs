using Hypar.Elements.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System;

namespace Hypar.Elements
{
    /// <summary>
    /// ParameterValue represents both the value and the type of a parameter.
    /// </summary>
    [JsonConverter(typeof(ParameterConverter))]
    public class Parameter
    {
        /// <summary>
        /// The value of the Parameter.
        /// </summary>
        [JsonProperty("value")]
        public object Value{get;}

        /// <summary>
        /// The type of the Parameter.
        /// </summary>
        /// <value></value>
        public ParameterType Type{get;}

        /// <summary>
        /// Construct a parameter value given a value and a type.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="parameterType">The type of the parameter.</param>
        public Parameter(object value, ParameterType parameterType)
        {
            if(value.GetType() != typeof(string) && value.GetType() != typeof(double))
            {
                throw new ArgumentException("The provided parameter value must be a string or a double.");
            }
            this.Value = value;
            this.Type = parameterType;
        }
    }

    /// <summary>
    /// An enumeration of unit types for a numeric parameter.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ParameterType
    {
        /// <summary>
        /// No unit assigned.
        /// </summary>
        [EnumMember(Value = "none")]
        None,
        /// <summary>
        /// A length in meters.
        /// </summary>
        [EnumMember(Value = "distance")]
        Distance,
        /// <summary>
        /// An area in square meters.
        /// </summary>
        [EnumMember(Value = "area")]
        Area,
        /// <summary>
        /// A volume in cubic meters.
        /// </summary>
        [EnumMember(Value = "volume")]
        Volume,
        /// <summary>
        /// A mass in kilograms.
        /// </summary>
        [EnumMember(Value = "mass")]
        Mass,
        /// <summary>
        /// A force in Newtons.
        /// </summary>
        [EnumMember(Value = "force")]
        Force,
        /// <summary>
        /// A string value.
        /// </summary>
        [EnumMember(Value = "text")]
        Text
    }
}