using System;

namespace Elements.Geometry
{
    /// <summary>
    /// An infinite ray starting at origin and pointing towards direction.
    /// </summary>
    public class Ray: IEquatable<Ray>
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
        /// https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
        /// </summary>
        /// <param name="tri">The triangle to intersect.</param>
        /// <param name="result">The intersection result.</param>
        /// <returns>True if an intersection occurs, otherwise false. If true, check the intersection result for the type and location of intersection.</returns>
        public bool Intersects(Triangle tri, out Vector3 result) 
        {
            result = default(Vector3);

            var vertex0 = tri.Vertices[0].Position;
            var vertex1 = tri.Vertices[1].Position;
            var vertex2 = tri.Vertices[2].Position;
            var edge1 = (vertex1 - vertex0);
            var edge2 = (vertex2 - vertex0);
            var h = this.Direction.Cross(edge2);
            var s = this.Origin - vertex0;
            double a, f, u, v;

            a = edge1.Dot(h);
            if (a > -Vector3.Epsilon && a < Vector3.Epsilon) {
                return false;    // This ray is parallel to this triangle.
            }
            f = 1.0 / a;
            u = f * (s.Dot(h));
            if (u < 0.0 || u > 1.0) {
                return false;
            }
            var q = s.Cross(edge1);
            v = f * this.Direction.Dot(q);
            if (v < 0.0 || u + v > 1.0) {
                return false;
            }
            // At this stage we can compute t to find out where the intersection point is on the line.
            double t = f * edge2.Dot(q);
            if (t > Vector3.Epsilon && t < 1/Vector3.Epsilon) // ray intersection
            {
                result = this.Origin + this.Direction * t;
                return true;
            } else // This means that there is a line intersection but not a ray intersection.
            {
                return false;
            }
        }
        
        /// <summary>
        /// Does this ray intersect the provided topography?
        /// </summary>
        /// <param name="topo">The topography.</param>
        /// <param name="result">The location of intersection.</param>
        /// <returns>True if an intersection result occurs.
        /// The type of intersection should be checked in the intersection result. 
        /// False if no intersection occurs.</returns>
        public bool Intersects(Topography topo, out Vector3 result)
        {
            result = default(Vector3);
            foreach (var t in topo.Mesh.Triangles)
            {
                if (this.Intersects(t, out result))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Is this ray equal to the provided ray?
        /// </summary>
        /// <param name="other">The ray to test.</param>
        /// <returns>Returns true if the two rays are equal, otherwise false.</returns>
        public bool Equals(Ray other)
        {
            if(other == null)
            {
                return false;
            }
            return this.Origin.Equals(other.Origin) && this.Direction.Equals(other.Direction);
        }
    }
}