using System;

namespace Hypar.Geometry
{
    /// <summary>
    /// Color represents an RGBA color.
    /// </summary>
    public class Color
    {
        public static Color Aqua => new Color(0.3f, 0.7f, 0.7f, 0.0f);
        public static Color Beige => new Color(1.0f, 0.9f, 0.8f, 0.0f);
        public static Color Black => new Color(0.0f, 0.0f, 0.0f, 0.0f);
        public static Color Brown => new Color(0.6f, 0.4f, 0.2f, 0.0f);
        public static Color Cobalt => new Color(0.0f, 0.4f, 1.0f, 0.0f);
        public static Color Coral => new Color(1.0f, 0.8f, 0.7f, 0.0f);
        public static Color Crimson => new Color(1.0f, 0.0f, 0.0f, 0.0f);
        public static Color Cyan => new Color(0.3f, 0.9f, 0.9f, 0.0f);
        public static Color Darkgray => new Color(0.2f, 0.2f, 0.2f, 0.0f);
        public static Color Emerald => new Color(0.2f, 0.7f, 0.3f, 0.0f);
        public static Color Granite => new Color(0.4f, 0.4f, 0.4f, 0.0f);
        public static Color Gray => new Color(0.5f, 0.5f, 0.5f, 0.0f);
        public static Color Lavender => new Color(0.9f, 0.7f, 1.0f, 0.0f);
        public static Color Lime => new Color(0.8f, 0.9f, 0.3f, 0.0f);
        public static Color Magenta => new Color(0.9f, 0.2f, 0.9f, 0.0f);
        public static Color Maroon => new Color(0.5f, 0.0f, 0.3f, 0.0f);
        public static Color Mint => new Color(0.6f, 1.0f, 0.7f, 0.0f);
        public static Color Navy => new Color(0.0f, 0.0f, 0.5f, 0.0f);
        public static Color Olive => new Color(0.5f, 0.5f, 0.0f, 0.0f);
        public static Color Orange => new Color(1.0f, 0.5f, 0.1f, 0.0f);
        public static Color Pink => new Color(1.0f, 0.3f, 0.5f, 0.0f);
        public static Color Purple => new Color(0.7f, 0.1f, 1.0f, 0.0f);
        public static Color Sand => new Color(1.0f, 0.8f, 0.4f, 0.0f);
        public static Color Stone => new Color(0.1f, 0.1f, 0.1f, 0.0f);
        public static Color Teal => new Color(0.0f, 0.5f, 0.5f, 0.0f);
        public static Color White => new Color(1.0f, 1.0f, 1.0f, 0.0f);
        public static Color Yellow => new Color(1.0f, 0.9f, 0.1f, 0.0f);

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