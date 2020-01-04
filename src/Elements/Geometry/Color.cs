using System;

namespace Elements.Geometry
{
    /// <summary>
    /// An RGBA color.
    /// </summary>
    public partial struct Color: IEquatable<Color>
    {
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
            var m = (Color)obj;
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

        /// <summary>
        /// Is this color equal to the provided color?
        /// </summary>
        /// <param name="other">The color to test.</param>
        /// <returns>Returns true if the two colors are equal, otherwise false.</returns>
        public bool Equals(Color other)
        {
            return this.Red == other.Red && this.Blue == other.Blue && this.Green == other.Green && this.Alpha == other.Alpha;
        }
    }
}