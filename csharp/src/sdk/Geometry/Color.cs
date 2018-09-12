using System;

namespace Hypar.Geometry
{
    /// <summary>
    /// Color represents an RGBA color.
    /// </summary>
    public class Color
    {
        public static readonly float[] Aqua = { 0.3f, 0.7f, 0.7f, 0.0f };
        public static readonly float[] Beige = { 1.0f, 0.9f, 0.8f, 0.0f };
        public static readonly float[] Black = { 0.0f, 0.0f, 0.0f, 0.0f };
        public static readonly float[] Brown = { 0.6f, 0.4f, 0.2f, 0.0f };
        public static readonly float[] Cobalt = { 0.0f, 0.4f, 1.0f, 0.0f };
        public static readonly float[] Coral = { 1.0f, 0.8f, 0.7f, 0.0f };
        public static readonly float[] Crimson = { 1.0f, 0.0f, 0.0f, 0.0f };
        public static readonly float[] Cyan = { 0.3f, 0.9f, 0.9f, 0.0f };
        public static readonly float[] Darkgray = { 0.2f, 0.2f, 0.2f, 0.0f };
        public static readonly float[] Emerald = { 0.2f, 0.7f, 0.3f, 0.0f };
        public static readonly float[] Granite = { 0.4f, 0.4f, 0.4f, 0.0f };
        public static readonly float[] Gray = { 0.5f, 0.5f, 0.5f, 0.0f };
        public static readonly float[] Lavender = { 0.9f, 0.7f, 1.0f, 0.0f };
        public static readonly float[] Lime = { 0.8f, 0.9f, 0.3f, 0.0f };
        public static readonly float[] Magenta = { 0.9f, 0.2f, 0.9f, 0.0f };
        public static readonly float[] Maroon = { 0.5f, 0.0f, 0.3f, 0.0f };
        public static readonly float[] Mint = { 0.6f, 1.0f, 0.7f, 0.0f };
        public static readonly float[] Navy = { 0.0f, 0.0f, 0.5f, 0.0f };
        public static readonly float[] Olive = { 0.5f, 0.5f, 0.0f, 0.0f };
        public static readonly float[] Orange = { 1.0f, 0.5f, 0.1f, 0.0f };
        public static readonly float[] Pink = { 1.0f, 0.3f, 0.5f, 0.0f };
        public static readonly float[] Purple = { 0.7f, 0.1f, 1.0f, 0.0f };
        public static readonly float[] Sand = { 1.0f, 0.8f, 0.4f, 0.0f };
        public static readonly float[] Stone = { 0.1f, 0.1f, 0.1f, 0.0f };
        public static readonly float[] Teal = { 0.0f, 0.5f, 0.5f, 0.0f };
        public static readonly float[] White = { 1.0f, 1.0f, 1.0f, 0.0f };
        public static readonly float[] Yellow = { 1.0f, 0.9f, 0.1f, 0.0f };

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
        /// Construct a color from an red, green, blue, and alpha value array.
        /// </summary>
        /// <param name="rgba">An array of the red, blue, green, and alpha color components between 0.0 and 1.0.</param>
        public Color(float[] rgba)
        {
            if (rgba.Length != 4)
            {
                throw new ArgumentOutOfRangeException("Requires an array of four values between 0.0 and 1.0.");
            }

            if (rgba[0] < 0.0 || rgba[1] < 0.0 || rgba[2] < 0.0 || rgba[3] < 0.0)
            {
                throw new ArgumentOutOfRangeException("All components must have a value greater than 0.0.");
            }

            if (rgba[0] > 1.0 || rgba[1] > 1.0 || rgba[2] > 1.0 || rgba[3] > 1.0)
            {
                throw new ArgumentOutOfRangeException("All components must have a value less than 1.0.");
            }

            this.Red = rgba[0];
            this.Green = rgba[1];
            this.Blue = rgba[2];
            this.Alpha = rgba[3];
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