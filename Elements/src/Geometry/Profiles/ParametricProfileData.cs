using System.Collections.Generic;

namespace Elements.Geometry.Profiles
{
    /// <summary>
    /// A container for data required to create a parametric profile.
    /// </summary>
    public class ParametericProfileData
    {
        /// <summary>
        /// A collection of vector expressions.
        /// </summary>
        public List<VectorExpression> PerimeterVectorExpressions { get; set; }

        /// <summary>
        /// A collection of vector expressions.
        /// </summary>
        public List<List<VectorExpression>> VoidVectorExpressions { get; set; }

        /// <summary>
        /// Property values available to the expressions.
        /// </summary>
        /// <value></value>
        public Dictionary<string, double> PropertyValues { get; set; }

        /// <summary>
        /// Create profile data.
        /// </summary>
        public ParametericProfileData(List<VectorExpression> perimeterVectorExpressions, Dictionary<string, double> propertyValues, List<List<VectorExpression>> voidVectorExpressions = null)

        {
            PerimeterVectorExpressions = perimeterVectorExpressions;
            PropertyValues = propertyValues;
            VoidVectorExpressions = voidVectorExpressions;
        }
    }
}