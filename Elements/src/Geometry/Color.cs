using Elements.Validators;
using Newtonsoft.Json;
using System;
using System.Globalization;

namespace Elements.Geometry
{
    /// <summary>A color with red, green, blue, and alpha components.</summary>
    public struct Color : IEquatable<Color>
    {
        /// <summary>The red component of the color between 0.0 and 1.0.</summary>
        [JsonProperty("Red", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Range(0.0D, 1.0D)]
        public double Red { get; set; }

        /// <summary>The green component of the color between 0.0 and 1.0.</summary>
        [JsonProperty("Green", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Range(0.0D, 1.0D)]
        public double Green { get; set; }

        /// <summary>The blue component of the color between 0.0 and 1.0.</summary>
        [JsonProperty("Blue", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Range(0.0D, 1.0D)]
        public double Blue { get; set; }

        /// <summary>The alpha component of the color between 0.0 and 1.0.</summary>
        [JsonProperty("Alpha", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Range(0.0D, 1.0D)]
        public double Alpha { get; set; }

        /// <summary>
        /// Create a color.
        /// </summary>
        /// <param name="red">The red component.</param>
        /// <param name="green">The green component.</param>
        /// <param name="blue">The blue component.</param>
        /// <param name="alpha">The alpha component.</param>
        [JsonConstructor]
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
        /// Convert a hex code or an english name to a color. 
        /// </summary>
        /// <param name="hexOrName">The hex code (e.g. #F05C6D) or common color name (e.g. "Goldenrod") to turn into a color. (Recognized names are from the UNIX X11 named color values — see https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.colors?view=windowsdesktop-6.0 for a complete listing.)</param>
        public Color(string hexOrName)
        {
            if (hexOrName.StartsWith("#"))
            {
                //replace # occurences
                if (hexOrName.IndexOf('#') != -1)
                    hexOrName = hexOrName.Replace("#", "");

                var r = int.Parse(hexOrName.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                var g = int.Parse(hexOrName.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                var b = int.Parse(hexOrName.Substring(4, 2), NumberStyles.AllowHexSpecifier);
                Red = r / 255.0;
                Green = g / 255.0;
                Blue = b / 255.0;
                Alpha = 1.0;
                return;
            }
            var sysColor = System.Drawing.Color.FromName(hexOrName);
            if (sysColor.A + sysColor.R + sysColor.G + sysColor.B == 0)
            {
                throw new ArgumentException($"The color name '{hexOrName}' is not recognized.");
            }
            Red = sysColor.R / 255.0;
            Green = sysColor.G / 255.0;
            Blue = sysColor.B / 255.0;
            Alpha = sysColor.A / 255.0;
        }

        /// <summary>
        /// Create an Elements Color from a System.Drawing.Color
        /// </summary>
        /// <param name="color">A System.Drawing.Color value.</param>
        public Color(System.Drawing.Color color)
        {
            Red = color.R / 255.0;
            Green = color.G / 255.0;
            Blue = color.B / 255.0;
            Alpha = color.A / 255.0;
        }

        /// <summary>
        /// Get the color's components as an array.
        /// </summary>
        /// <returns>An array containing the color's components.</returns>
        public float[] ToArray(bool convertToLinearColorSpace = false)
        {
            if (convertToLinearColorSpace)
            {
                return new[] { (float)SRGBToLinear(Red), (float)SRGBToLinear(Green), (float)SRGBToLinear(Blue), (float)Alpha };
            }
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

        /// <summary>
        /// Automatically convert a hex code or an english name to a color.
        /// </summary>
        /// <param name="hexOrName">The hex code (e.g. #F05C6D) or common color name (e.g. "Goldenrod") to turn into a color. (Recognized names are from the UNIX X11 named color values — see https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.colors?view=windowsdesktop-6.0 for a complete listing.)</param>
        public static implicit operator Color(string hexOrName)
        {
            if (hexOrName == null)
            {
                // This is an unfortunate necessity — C# 7.0 does not support an explicit
                // attribute on a function argument that specifies non-null. Ideally, a `null`
                // wouldn't be assumed to be a string and fall into this implicit operator in the first place,
                // but there's no way to say "I work on strings that are not null" (At least until later versions 
                // of C#). In an ideal world we'd want this to be caught by the compiler instead of at runtime.
                // (IOW, we'd love for a statement like `Color c = null;` to be rejected by static analysis,
                // but this is not possible if we want to support an implicit string coversion, so we throw a 
                // runtime exception instead.) 
                throw new ArgumentNullException("Cannot convert null to a Color. Color is a non-nullable type.");
            }
            return new Color(hexOrName);
        }

        /// <summary>
        /// Convert a gamma color space component to a linear color space value.
        /// </summary>
        /// <param name="c">The gamma color component value.</param>
        /// <returns>A linear color space component value.</returns>
        public static double SRGBToLinear(double c)
        {
            return (c < 0.04045) ? c * 0.0773993808 : Math.Pow(c * 0.9478672986 + 0.0521327014, 2.4);
        }

        /// <summary>
        /// Convert a linear color space component to a gamma color space value.
        /// </summary>
        /// <param name="c">The linear color component value.</param>
        /// <returns>A gamma color space component value.</returns>
        public static double LinearToSRGB(double c)
        {
            return (c < 0.0031308) ? 12.92 * c : (1.055 * Math.Pow(c, 0.41666)) - 0.055;
        }

        /// <summary>
        /// Convert the color to hexadecimal.
        /// </summary>
        /// <returns></returns>
        public string ToHex()
        {
            var r = (byte)(Red * 255.0);
            var g = (byte)(Green * 255.0);
            var b = (byte)(Blue * 255.0);
            // var a = (byte)(Alpha * 255.0);
            return "#" + r.ToString("X2") + g.ToString("X2") + b.ToString("X2"); // + a.ToString("X2");
        }
    }
}