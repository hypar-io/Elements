using System.Runtime.Serialization;

namespace Hypar.Functions
{
    /// <summary>
    /// An enumeration of possible parameter types.
    /// </summary>
    public enum ParameterType
    {
        /// <summary>
        /// A numeric parameter.
        /// </summary>
        [EnumMember(Value = "number")]
        Number,
        /// <summary>
        /// A location parameter.
        /// </summary>
        [EnumMember(Value = "location")]
        Location, 
        /// <summary>
        /// A point parameter.
        /// </summary>
        [EnumMember(Value = "point")]
        Point
    }
}