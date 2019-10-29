using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// A cartesian plane.
    /// </summary>
    public partial class Plane
    {
        internal static void ValidateConstructorParameters(Vector3 normal, Vector3 origin)
        {
            if(normal.IsParallelTo(origin))
            {
                throw new ArgumentException("The plane could not be constructed. The normal and origin are parallel.");
            }
        }

        /// <summary>
        /// Construct a plane by three points.
        /// The plane is constructed as a->b * b->c.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <exception>Thrown when any of a, b, or c are null.</exception>
        public Plane(Vector3 a, Vector3 b, Vector3 c)
        {
            if(a == null || b == null || c == null)
            {
                throw new ArgumentNullException();
            }

            this.Origin = a;
            var ab = (b-a).Normalized();
            var bc = (c-a).Normalized();
            this.Normal = ab.Cross(bc).Normalized();
        }

        /// <summary>
        /// Construct a plane.
        /// Only the first three points of the points array will be used.
        /// </summary>
        /// <param name="origin">The origin of the plane.</param>
        /// <param name="points">An array of vectors to be used to determine the normal of the plane.</param>
        /// <exception>Thrown when less than three points are provided.</exception>
        /// <exception>Thrown when coincident points are provided.</exception>
        public Plane(Vector3 origin, IList<Vector3> points)
        {
            if(points.Count < 3)
            {
                throw new ArgumentException("The plane could not be created. You must supply a minimum of 3 points.");
            }
            if(points[0] == points[1] || points[0] == points[2] || points[1] == points[2])
            {
                throw new ArgumentException("The plane could not be created. The points must not be coincident.");
            }
            this.Origin = origin;
            this.Normal = (points[0]-points[1]).Normalized().Cross((points[2] - points[0]).Normalized());
        }
    }
}