namespace Hypar.Geometry
{
    /// <summary>
    /// Color represents an RGBA color.
    /// </summary>
    public class Color
    {
        double Red{get;}
        double Green{get;}
        double Blue{get;}
        double Alpha{get;}

        /// <summary>
        /// Construct a color from its components.
        /// </summary>
        /// <param name="red">The red component between 0.0 and 1.0.</param>
        /// <param name="green">The green component between 0.0 and 1.0.</param>
        /// <param name="blue">The blue component between 0.0 and 1.0.</param>
        /// <param name="alpha">The alpha component between 0.0 and 1.0.</param>
        public Color(double red, double green, double blue, double alpha)
        {
            this.Red = red;
            this.Green = green;
            this.Blue = blue;
            this.Alpha = alpha;
        }

        /// <summary>
        /// Get the color's components as an array.
        /// </summary>
        /// <returns>An array containing the color's components.</returns>
        public double[] ToArray()
        {
            return new[]{Red, Green, Blue, Alpha};
        }
    }
}