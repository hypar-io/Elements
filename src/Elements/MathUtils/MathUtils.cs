using System;
using System.Text;

namespace Elements.MathUtils
{
    /// <summary>
    /// Extension and utility methods for mathematical operations. 
    /// </summary>
    public static class MathExtensions
    {
        /// <summary>
        /// Map/Scale a value from one domain to another. 3 mapped from (2,4) to (10, 20) would be 15.
        /// </summary>
        /// <param name="value">The value to map.</param>
        /// <param name="source">The source domain to map from.</param>
        /// <param name="target">The target domain to map to.</param>
        /// <returns></returns>
        public static double MapBetweenDomains(this double value, Domain1d source, Domain1d target)
        {
            return value.MapFromDomain(source).MapToDomain(target);
        }

        /// <summary>
        /// Map/Normalize a value from a domain to the domain (0,1).
        /// </summary>
        /// <param name="value">The value to map</param>
        /// <param name="domain">The domain to map from.</param>
        /// <returns>(value - domain.Min) / domain.Length</returns>
        public static double MapFromDomain(this double value, Domain1d domain)
        {
            return (value - domain.Min) / domain.Length;
        }

        /// <summary>
        /// Map/scale a value between 0-1 to a target domain. Will not reject values outside 0-1. 
        /// </summary>
        /// <param name="value">The value to map.</param>
        /// <param name="domain">The domain to map to.</param>
        /// <returns>value * domain.Length + domain.Min</returns>
        public static double MapToDomain(this double value, Domain1d domain)
        {
            return value * domain.Length + domain.Min;
        }

        /// <summary>
        /// Test if two values are approximately equal to each other with an optional tolerance value.
        /// </summary>
        /// <param name="value">The first value to test</param>
        /// <param name="other">The other value to test</param>
        /// <param name="tolerance">The threshold for equality</param>
        /// <returns>True if |other - value| &lt; tolerance</returns>
        public static bool ApproximatelyEquals(this double value, double other, double tolerance = 0.01)
        {
            return Math.Abs(other - value) < tolerance;
        }

        /// <summary>
        /// Create a string A, B, C, ... AA, AB ... from an int value
        /// </summary>
        /// <param name="value">The value to turn into a character string</param>
        /// <returns>A string of Upper-case characters e.g. 1=A, 2=B, 27=AA</returns>
        public static string NumberToString(int value)
        {
            // Modified from https://forums.asp.net/t/1419722.aspx?generate+a+sequence+of+letters+in+C+
            StringBuilder sb = new StringBuilder();
            value++; // (so that 0 = A)
            do
            {
                value--;
                int remainder = 0;
                value = Math.DivRem(value, 26, out remainder);
                sb.Insert(0, Convert.ToChar('A' + remainder));

            } while (value > 0);

            return sb.ToString();
        }

    }
}
