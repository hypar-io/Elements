using Elements.Validators;
using System;

namespace Elements.Geometry
{
    /// <summary>A color with red, green, blue, and alpha components.</summary>
    public struct Color : IEquatable<Color>
    {
        /// <summary>The red component of the color between 0.0 and 1.0.</summary>
        [Newtonsoft.Json.JsonProperty("Red", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Range(0.0D, 1.0D)]
        public double Red { get; set; }

        /// <summary>The green component of the color between 0.0 and 1.0.</summary>
        [Newtonsoft.Json.JsonProperty("Green", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Range(0.0D, 1.0D)]
        public double Green { get; set; }

        /// <summary>The blue component of the color between 0.0 and 1.0.</summary>
        [Newtonsoft.Json.JsonProperty("Blue", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Range(0.0D, 1.0D)]
        public double Blue { get; set; }

        /// <summary>The alpha component of the color between 0.0 and 1.0.</summary>
        [Newtonsoft.Json.JsonProperty("Alpha", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Range(0.0D, 1.0D)]
        public double Alpha { get; set; }

        /// <summary>
        /// Create a color.
        /// </summary>
        /// <param name="red">The red component.</param>
        /// <param name="green">The green component.</param>
        /// <param name="blue">The blue component.</param>
        /// <param name="alpha">The alpha component.</param>
        [Newtonsoft.Json.JsonConstructor]
        public Color(double @red, double @green, double @blue, double @alpha)
        {
            if (!Validator.DisableValidationOnConstruction)
            {
                if (red < 0.0 || green < 0.0 || blue < 0.0 || alpha < 0.0)
                {
                    throw new ArgumentOutOfRangeException("All components must have a value greater than 0.0.");
                }

                if (red > 1.0 || green > 1.0 || blue > 1.0 || alpha > 1.0)
                {
                    throw new ArgumentOutOfRangeException("All components must have a value less than or equal to 1.0.");
                }
            }

            this.Red = @red;
            this.Green = @green;
            this.Blue = @blue;
            this.Alpha = @alpha;
        }

        /// <summary>
        /// Get the color's components as an array.
        /// </summary>
        /// <returns>An array containing the color's components.</returns>
        public float[] ToArray()
        {
            return new[] { (float)Red, (float)Green, (float)Blue, (float)Alpha };
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
            return new { this.Red, this.Green, this.Blue, this.Alpha }.GetHashCode();
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
        /// Linearly interpolate between this color and the other color.
        /// </summary>
        /// <param name="other">The other color.</param>
        /// <param name="t">A value between 0.0 and 1.0.</param>
        /// <returns></returns>
        public Color Lerp(Color other, double t)
        {
            if (t < 0 || t > 1)
            {
                throw new ArgumentException("The value of t must be between 0.0 and 1.0.");
            }
            return (1 - t) * this + t * other;
        }

        /// <summary>
        /// Multiply two colors.
        /// Resulting values will be clamped in the range of 0.0 to 1.0.
        /// </summary>
        /// <param name="a">The first color.</param>
        /// <param name="b">The second color.</param>
        public static Color operator *(Color a, Color b)
        {
            return new Color(Math.Min(1, a.Red * b.Red),
                             Math.Min(1, a.Green * b.Green),
                             Math.Min(1, a.Blue * b.Blue),
                             Math.Min(1, a.Alpha * b.Alpha));
        }

        /// <summary>
        /// Multiply a color and a scalar.
        /// Resulting values will be clamped in the range of 0.0 to 1.0.
        /// </summary>
        /// <param name="a">The color.</param>
        /// <param name="t">The scalar.</param>
        public static Color operator *(double t, Color a)
        {
            if (t < 0)
            {
                throw new ArgumentException("The value of t must be greater than 0.0.");
            }
            return new Color(Math.Min(1, a.Red * t),
                             Math.Min(1, a.Green * t),
                             Math.Min(1, a.Blue * t),
                             Math.Min(1, a.Alpha * t));
        }

        /// <summary>
        /// Add two colors.
        /// Resulting values will be clamped in the range of 0.0 to 1.0.
        /// </summary>
        /// <param name="a">The first color.</param>
        /// <param name="b">The second color.</param>
        public static Color operator +(Color a, Color b)
        {
            return new Color(Math.Min(1, a.Red + b.Red),
                             Math.Min(1, a.Green + b.Green),
                             Math.Min(1, a.Blue + b.Blue),
                             Math.Min(1, a.Alpha + b.Alpha));
        }

        /// <summary>
        /// Are the two Colors equal?
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static bool operator ==(Color a, Color b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Are the two Colors equal?
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static bool operator !=(Color a, Color b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// Automatically convert a tuple (R,G,B,A) to a color.
        /// </summary>
        /// <param name="color">An (R,G,B,A) tuple of doubles.</param>
        public static implicit operator Color((double R, double G, double B, double A) color)
        {
            return new Color(color.R, color.G, color.B, color.A);
        }

        /// <summary>
        /// Automatically convert a tuple (R,G,B) to a color.
        /// </summary>
        /// <param name="color">An (R,G,B) tuple of doubles.</param>
        public static implicit operator Color((double R, double G, double B) color)
        {
            return new Color(color.R, color.G, color.B, 1.0);
        }
    }
}