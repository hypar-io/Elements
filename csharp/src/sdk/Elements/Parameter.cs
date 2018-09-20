using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Hypar.Elements
{
    /// <summary>
    /// ParameterValue represents both the value and the type of a parameter.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public abstract class Parameter<TValue>
    {
        /// <summary>
        /// The value of the parameter.
        /// </summary>
        [JsonProperty("value")]
        public TValue Value{get;}

        /// <summary>
        /// Construct a parameter value given a value and a type.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        public Parameter(TValue value)
        {
            this.Value = value;
        }
    }

    /// <summary>
    /// An enumeration of unit types for a numeric parameter.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NumericParameterType
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
        Force
    }

    /// <summary>
    /// A parameter whose value is a number.
    /// </summary>
    public class NumericParameter  : Parameter<double>
    {
        /// <summary>
        /// The type of the value.
        /// </summary>
        /// <value></value>
        [JsonProperty("type")]
        public NumericParameterType Type{get;}

        /// <summary>
        /// Construct a numeric parameter.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public NumericParameter(double value, NumericParameterType type) : base(value)
        {
            this.Type = type;
        }
    }

    /// <summary>
    /// A parameter whose value is a string.
    /// </summary>
    public class StringParameter : Parameter<string>
    {
        /// <summary>
        /// Construct a string parameter.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public StringParameter(string value) : base(value){}
    }
}