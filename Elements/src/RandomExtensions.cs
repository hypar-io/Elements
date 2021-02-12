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
        /// <param name="random">The `Random` object to generate from</param>
        public static Color NextColor(this Random random)
        {
            return new Color(random.NextDouble(), random.NextDouble(), random.NextDouble(), 1);
        }

        /// <summary>
        /// Generate a new material with a random color assigned. Useful for debugging purposes.
        /// </summary>
        /// <param name="random">The `Random` object to generate from</param>
        /// <param name="unlit">Whether or not to treat the material as unlit.</param>
        public static Material NextMaterial(this Random random, bool unlit = true)
        {
            var color = random.NextColor();
            return new Material(color, 0.1, 0.3, unlit, null, true, Guid.NewGuid(), color.ToString());
        }
    }

}