using System.Runtime.Serialization;

namespace Hypar.Functions
{
    /// <summary>
    /// An enumeration of possible parameter types.
    /// </summary>
    public enum HyparParameterType
    {
        /// <summary>
        /// A numeric parameter.
        /// </summary>
        [EnumMember(Value = "number")]
        Number,
        /// <summary>
        /// A numeric parameter with a range.
        /// </summary>
        [EnumMember(Value = "range")]
        Range,
        /// <summary>
        /// A location parameter.
        /// </summary>
        [EnumMember(Value = "location")]
        Location, 
        /// <summary>
        /// A point parameter.
        /// </summary>
        [EnumMember(Value = "point")]
        Point,
        /// <summary>
        /// A data parameter.
        /// </summary>
        [EnumMember(Value = "data")]
        Data
    }
}