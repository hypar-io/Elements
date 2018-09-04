using System;

namespace Hypar.Geometry
{
    /// <summary>
    /// Color represents an RGBA color.
    /// </summary>
    public class Color
    {   
        /// <summary>
        /// The red component of the color.
        /// </summary>
        /// <value></value>
        public float Red{get;}

        /// <summary>
        /// The green component of the color.
        /// </summary>
        /// <value></value>
        public float Green{get;}

        /// <summary>
        /// The blue component of the color.
        /// </summary>
        /// <value></value>
        public float Blue{get;}

        /// <summary>
        /// The alpha component of the color.
        /// </summary>
        /// <value></value>
        public float Alpha{get;}

        /// <summary>
        /// Construct a color from its components.
        /// </summary>
        /// <param name="red">The red component between 0.0 and 1.0.</param>
        /// <param name="green">The green component between 0.0 and 1.0.</param>
        /// <param name="blue">The blue component between 0.0 and 1.0.</param>
        /// <param name="alpha">The alpha component between 0.0 and 1.0.</param>
        public Color(float red, float green, float blue, float alpha)
        {
            if(red < 0.0 || green < 0.0 || blue < 0.0 || alpha < 0.0)
            {
                throw new ArgumentOutOfRangeException("All components must have a value greater than 0.0.");
            }

            if(red > 1.0 || green > 1.0 || blue > 1.0 || alpha > 1.0)
            {
                throw new ArgumentOutOfRangeException("All components must have a value less than 1.0.");
            }

            this.Red = red;
            this.Green = green;
            this.Blue = blue;
            this.Alpha = alpha;
        }

        /// <summary>
        /// Get the color's components as an array.
        /// </summary>
        /// <returns>An array containing the color's components.</returns>
        public float[] ToArray()
        {
            return new[]{Red, Green, Blue, Alpha};
        }
    }
}