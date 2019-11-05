namespace Elements.Geometry
{
    /// <summary>
    /// An infinite ray starting at origin and pointing towards direction.
    /// </summary>
    public class Ray
    {
        /// <summary>
        /// The origin of the ray.
        /// </summary>
        public Vector3 Origin { get; set; }

        /// <summary>
        /// The direction of the ray.
        /// </summary>
        public Vector3 Direction { get; set; }

        /// <summary>
        /// Construct a ray.
        /// </summary>
        /// <param name="origin">The origin of the ray.</param>
        /// <param name="direction">The direction of the ray.</param>
        public Ray(Vector3 origin, Vector3 direction)
        {
            this.Origin = origin;
            this.Direction = direction;
        }

        /// <summary>
        /// https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-rendering-a-triangle/ray-triangle-intersection-geometric-solution
        /// </summary>
        /// <param name="tri"></param>
        /// <param name="result"></param>
        public bool Intersects(Triangle tri, out RayIntersectionResult result)
        {
            result = null;
            var D = tri.Normal.Dot(tri.Vertices[0].Position);
            var denom = tri.Normal.Dot(this.Direction);
            if (denom == 0)
            {
                // Ray and triangle are parallel
                result = new RayIntersectionResult(null, RayIntersectionResultType.Parallel);
                return false;
            }
            double t = (tri.Normal.Dot(this.Origin) + D) / denom;
            var P = this.Origin + t * this.Direction;
            if (t < 0)
            {
                // Triangle is "behind" the ray.
                result = new RayIntersectionResult(null, RayIntersectionResultType.Behind);
                return false;
            }
            var edge0 = tri.Vertices[1].Position - tri.Vertices[0].Position;
            var edge1 = tri.Vertices[2].Position - tri.Vertices[1].Position;
            var edge2 = tri.Vertices[0].Position - tri.Vertices[2].Position;
            var C0 = P - tri.Vertices[0].Position;
            var C1 = P - tri.Vertices[1].Position;
            var C2 = P - tri.Vertices[2].Position;

            if(P.IsAlmostEqualTo(tri.Vertices[0].Position) || 
                P.IsAlmostEqualTo(tri.Vertices[1].Position) || 
                P.IsAlmostEqualTo(tri.Vertices[2].Position))
            {
                // Intersection occurs at a vertex of the triangle.
                result = new RayIntersectionResult(P, RayIntersectionResultType.IntersectsAtVertex);
                return true;
            }

            var x1 = tri.Normal.Dot(edge0.Cross(C0));
            var x2 = tri.Normal.Dot(edge1.Cross(C1));
            var x3 = tri.Normal.Dot(edge2.Cross(C2));

            if (x1 > 0 &&
                x2 > 0 &&
                x3 > 0)
            {
                result = new RayIntersectionResult(P, RayIntersectionResultType.Intersect);
                return true; // P is inside the triangle
            }
            result = null;
            return false;
        }
        
        /// <summary>
        /// Does this ray intersect the provided topography?
        /// </summary>
        /// <param name="topo">The topography.</param>
        /// <param name="xsect">The intersection result.</param>
        /// <returns>True if an intersection result occurs.
        /// The type of intersection should be checked in the intersection result. 
        /// False if no intersection occurs.</returns>
        public bool Intersects(Topography topo, out RayIntersectionResult xsect)
        {
            xsect = null;
            foreach (var t in topo.Mesh.Triangles)
            {
                if (this.Intersects(t, out xsect))
                {
                    return true;
                }
            }
            return false;
        }
    }
}