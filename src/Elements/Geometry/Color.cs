using System;

namespace Elements.Geometry
{
    /// <summary>
    /// An RGBA color.
    /// </summary>
    public partial class Color
    {
        /// <summary>
        /// Construct a default color.
        /// </summary>
        public Color()
        {
            this.Red = 0.5;
            this.Green = 0.5;
            this.Blue = 0.5;
            this.Alpha = 0.0;
        }

        /// <summary>
        /// Get the color's components as an array.
        /// </summary>
        /// <returns>An array containing the color's components.</returns>
        public float[] ToArray()
        {
            return new[]{(float)Red, (float)Green, (float)Blue, (float)Alpha};
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

        internal static void ValidateConstructorParameters(double alpha, double blue, double green, double red)
        {
            if(red < 0.0 || green < 0.0 || blue < 0.0 || alpha < 0.0)
            {
                throw new ArgumentOutOfRangeException("All components must have a value greater than 0.0.");
            }

            if(red > 1.0 || green > 1.0 || blue > 1.0 || alpha > 1.0)
            {
                throw new ArgumentOutOfRangeException("All components must have a value less than 1.0.");
            }
        }
    }
}