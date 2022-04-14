using System;
using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// Extension methods for generating new random objects from an instance of System.Random.
    /// </summary>
    public static class RandomExtensions
    {
        /// <summary>
        /// Generate a new color with a random R, G, and B component. Useful for debugging purposes.
        /// </summary>
        /// <param name="random">The `Random` object.</param>
        public static Color NextColor(this Random random)
        {
            return new Color(random.NextDouble(), random.NextDouble(), random.NextDouble(), 1);
        }

        /// <summary>
        /// Generate a new material with a random color assigned. Useful for debugging purposes.
        /// </summary>
        /// <param name="random">The `Random` object.</param>
        /// <param name="unlit">Whether or not to treat the material as unlit.</param>
        public static Material NextMaterial(this Random random, bool unlit = true)
        {
            var color = random.NextColor();
            return new Material(color.ToString(), color, 0.1, 0.3, null, unlit, true);
        }

        /// <summary>
        /// Generate a ray in a random direction in the specified plane.
        /// </summary>
        /// <param name="random">The `Random` object.</param>
        /// <param name="origin">The origin of the ray.</param>
        /// <param name="normal">The normal of the plane in which the
        /// resulting ray will lie.</param>
        /// <returns>A ray pointing in a random direction, along
        /// the plane.</returns>
        public static Ray NextRayInPlane(this Random random, Vector3 origin, Vector3 normal)
        {
            var v1 = new Vector3(random.NextDouble(), random.NextDouble(), random.NextDouble()).Unitized();
            var d = v1.Cross(normal);
            return new Ray(origin, d);
        }

        /// <summary>
        /// Generate a ray in a random direction.
        /// </summary>
        /// <param name="random">The `Random` object.</param>
        /// <param name="origin">The origin of the ray.</param>
        /// <returns></returns>
        public static Ray NextRay(this Random random, Vector3 origin)
        {
            var v1 = new Vector3(random.NextDouble(), random.NextDouble(), random.NextDouble()).Unitized();
            return new Ray(origin, v1);
        }

        /// <summary>
        /// Generate a new random vector with an optional bounds.
        /// </summary>
        /// <param name="random">The `Random` object to generate from</param>
        /// <param name="bounds">If specified, the bounds within which the
        /// vectors will be generated. If not specified, vectors will be
        /// generated in the range (-1,-1,-1) to (1,1,1).</param>
        /// <returns></returns>
        public static Vector3 NextVector(this Random random, BBox3 bounds = default)
        {
            if (bounds == default)
            {
                bounds = new BBox3((-1, -1, -1), (1, 1, 1));
            }
            return new Vector3(
                random.NextDouble().MapToDomain(bounds.XDomain),
                random.NextDouble().MapToDomain(bounds.YDomain),
                random.NextDouble().MapToDomain(bounds.ZDomain));
        }
    }

}