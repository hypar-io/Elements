using Newtonsoft.Json;

namespace Elements.Geometry.Profiles
{
    /// <summary>
    /// A collection of expressions describing the X, Y, and Z coordinates
    /// of a vertex.
    /// </summary>
    public class VectorExpression
    {
        /// <summary>
        /// The expression of the X coordinate of the vector.
        /// </summary>
        public string X { get; set; }

        /// <summary>
        /// The expression of the Y coordinate of the vector.
        /// </summary>
        public string Y { get; set; }

        /// <summary>
        /// The expression of the Z coordinate of the vector.
        /// </summary>
        public string Z { get; set; }

        /// <summary>
        /// Create a vector expression.
        /// </summary>
        /// <param name="x">The expression of the X coordinate of the vector.</param>
        /// <param name="y">The expression of the Y coordinate of the vector.</param>
        /// <param name="z">The expression of the Z coordinate of the vector.</param>
        [JsonConstructor]
        public VectorExpression(string x = null, string y = null, string z = null)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}