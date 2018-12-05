using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// A cartesian plane.
    /// </summary>
    public class Plane
    {
        /// <summary>
        /// The origin of the plane.
        /// </summary>
        [JsonProperty("origin")]
        public Vector3 Origin{get;}

        /// <summary>
        /// The normal of the plane.
        /// </summary>
        /// <value></value>
        [JsonProperty("normal")]
        public Vector3 Normal{get;}

        /// <summary>
        /// Construct a plane.
        /// </summary>
        /// <param name="origin">The origin of the plane.</param>
        /// <param name="normal">The normal of the plane.</param>
        public Plane(Vector3 origin, Vector3 normal)
        {
            this.Origin = origin;
            this.Normal = normal;
        }

        /// <summary>
        /// Construct a plane.
        /// Only the first three points of the points array will be used.
        /// </summary>
        /// <param name="origin">The origin of the plane.</param>
        /// <param name="points">An array of vectors to be used to determine the normal of the plane.</param>
        /// <exception cref="System.ArgumentException">Thrown when less than three points are provided.</exception>
        /// <exception cref="System.ArgumentException">Thrown when coincident points are provided.</exception>
        public Plane(Vector3 origin, IList<Vector3> points)
        {
            if(points.Count < 3)
            {
                throw new ArgumentException("The plane could not be constructed. You must supply a minimum of 3 points.");
            }
            if(points[0] == points[1] || points[0] == points[2] || points[1] == points[2])
            {
                throw new ArgumentException("The plane could not be constructed. The points must not be coincident.");
            }
            this.Origin = origin;
            this.Normal = (points[0]-points[1]).Normalized().Cross((points[2] - points[0]).Normalized());
        }
    }
}