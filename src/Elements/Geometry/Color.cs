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

        /// <summary>
        /// Converts this color to a string.
        /// </summary>
        /// <returns>Returns a string representation of the form "R: r, G: g, B: b, A: a".</returns>
        public override string ToString()
        {
            return $"R:{Red:0.00}, G:{Green:0.00}, B:{Blue:0.00}, A: {Alpha:0.00}";
        }

        /// <summary>
        /// Linearly interpolate between this color and the provide color.
        /// </summary>
        /// <param name="other">The other color.</param>
        /// <param name="t">A value between 0.0 and 1.0.</param>
        /// <returns></returns>
        public Color Lerp(Color other, double t)
        {
            return (1 - t) * this + t * other;
        }

        /// <summary>
        /// Multiply two colors.
        /// </summary>
        /// <param name="a">The first color.</param>
        /// <param name="b">The second color.</param>
        public static Color operator *(Color a, Color b)
        {
            return new Color(a.Red * b.Red, a.Green * b.Green, a.Blue * b.Blue, a.Alpha * b.Alpha);
        }

        /// <summary>
        /// Multiply a color and a scalar.
        /// </summary>
        /// <param name="a">The color.</param>
        /// <param name="t">The scalar.</param>
        public static Color operator *(double t, Color a)
        {
            return new Color(a.Red * t, a.Green * t, a.Blue * t, a.Alpha * t);
        }

        /// <summary>
        /// Add two colors.
        /// </summary>
        /// <param name="a">The first color.</param>
        /// <param name="b">The second color.</param>
        public static Color operator +(Color a, Color b)
        {
            return new Color(a.Red + b.Red, a.Green + b.Green, a.Blue + b.Blue, a.Alpha + b.Alpha);
        }
    }
}