using Newtonsoft.Json;
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
        [JsonProperty("red")]
        public float Red{get;}

        /// <summary>
        /// The green component of the color.
        /// </summary>
        [JsonProperty("green")]
        public float Green{get;}

        /// <summary>
        /// The blue component of the color.
        /// </summary>
        [JsonProperty("blue")]
        public float Blue{get;}

        /// <summary>
        /// The alpha component of the color.
        /// </summary>
        [JsonProperty("alpha")]
        public float Alpha{get;}

        /// <summary>
        /// Construct a color from its components.
        /// </summary>
        /// <param name="red">The red component between 0.0 and 1.0.</param>
        /// <param name="green">The green component between 0.0 and 1.0.</param>
        /// <param name="blue">The blue component between 0.0 and 1.0.</param>
        /// <param name="alpha">The alpha component between 0.0 and 1.0.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when any color component is less than 0.0.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when any color component is greater than 1.0.</exception>
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

        /// <summary>
        /// Is this color equal to the provided color?
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var m = obj as Color;
            if(m == null)
            {
                return false;
            }

            return m.Red == this.Red && m.Green == this.Green && m.Blue == this.Blue && m.Alpha == this.Alpha;
        }

        /// <summary>
        /// Get the hash code for this color.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return new {this.Red, this.Green, this.Blue, this.Alpha}.GetHashCode();
        }
    }
}