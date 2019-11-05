namespace Elements.Geometry
{
    /// <summary>
    /// The result of a ray intersection.
    /// </summary>
    public class RayIntersectionResult
    {
        /// <summary>
        /// The location of the intersection.
        /// </summary>
        public Vector3 Point { get; }

        /// <summary>
        /// The type of the intersection.
        /// </summary>
        public RayIntersectionResultType Type { get; }

        /// <summary>
        /// Construct an intersection result.
        /// </summary>
        /// <param name="point">The location of the intersection.</param>
        /// <param name="type">The type of the intersection.</param>
        public RayIntersectionResult(Vector3 point, RayIntersectionResultType type)
        {
            this.Point = point;
            this.Type = type;
        }
    }

    /// <summary>
    /// The types of possible ray intersection.
    /// </summary>
    public enum RayIntersectionResultType
    {
        /// <summary>
        /// There is no intersection because the ray and its target are parallel.
        /// </summary>
        Parallel,

        /// <summary>
        /// The ray's origin is 'behind' the target.
        /// </summary>
        Behind,

        /// <summary>
        /// An intersection occurred.
        /// </summary>
        Intersect,

        /// <summary>
        /// The ray intersects its target at a vertex.
        /// </summary> 
        IntersectsAtVertex
    }
}