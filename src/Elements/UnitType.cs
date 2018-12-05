using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Elements
{
        /// <summary>
    /// An enumeration of unit types for a numeric parameter.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UnitType
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