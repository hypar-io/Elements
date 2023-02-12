using Elements.Geometry;

namespace Elements.Serialization.SVG
{
    internal class DimensionLine
    {
        /// <summary>
        /// Initializes a new instance of DimensionLine class
        /// </summary>
        /// <param name="direction">The direction of the dimension line</param>
        /// <param name="isNegative">If the dimension line is negative, the text is in the direction of the normal vector.
        /// Otherwise - text is in the negative direction of the normal vector</param>
        public DimensionLine(Vector3 direction, bool isNegative)
        {
            Direction = direction;
            IsNegative = isNegative;
            Normal = direction.Cross(Vector3.ZAxis);
        }

        public Vector3 Direction { get; }
        public Vector3 Normal { get; }
        public bool IsNegative { get; }
    }
}