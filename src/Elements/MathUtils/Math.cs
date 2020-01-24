using System;
namespace Elements.MathUtils
{
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
        /// <returns></returns>
        public static double MapFromDomain(this double value, Domain1d domain)
        {
            return (value - domain.Min) / domain.Length;
        }

        /// <summary>
        /// Map/scale a value between 0-1 to a target domain. Will not reject values outside 0-1. 
        /// </summary>
        /// <param name="value">The value to map.</param>
        /// <param name="domain">The domain to map to.</param>
        /// <returns></returns>
        public static double MapToDomain(this double value, Domain1d domain)
        {
            return value * domain.Length + domain.Min;
        }

    }
}
