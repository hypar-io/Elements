using System;
using System.Collections.Generic;
using Elements.Geometry;
using Newtonsoft.Json;
using Elements;
using System.Linq;

namespace Elements.Analysis
{
    /// <summary>
    /// A range of colors interpolated between
    /// a number of key values.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/ColorScaleTests.cs?name=example)]
    /// </example>
    public class ColorScale
    {
        /// <summary>
        /// The colors of the scale.
        /// </summary>
        public List<Color> Colors { get; } = new List<Color>();

        /// <summary>
        /// The domain of the scale
        /// </summary>
        private List<Domain1d> Domains { get; } = null;

        /// <summary>
        /// Construct a color scale.
        /// </summary>
        /// <param name="colors">The colors which define the color scale.</param>
        /// <param name="domains">The domains which the colors map to</param>
        [JsonConstructor]
        public ColorScale(List<Color> colors, List<Domain1d> domains)
        {
            this.Colors = colors;
            this.Domains = domains;
        }

        /// <summary>
        /// Construct a color scale with a discrete number of color bands
        /// </summary>
        /// <param name="colors">The color scale's key values.</param>
        /// <param name="colorCount">The number of colors in the final color scale
        /// These values will be interpolated between the provided colors.</param>
        public ColorScale(List<Color> colors, int colorCount)
        {
            if (colors.Count > colorCount)
            {
                throw new ArgumentException("The color scale could not be created. The number of supplied colors is greater than the final color count.");
            }

            this.Domains = new List<Domain1d>();

            var numDomains = (double)(colorCount);
            var colorDomains = new Domain1d(0, 1).DivideByCount(colors.Count - 1).ToList();

            for (int i = 0; i < numDomains; i += 1)
            {
                var domain = new Domain1d((i / numDomains), (i + 1) / numDomains);
                var value = i < numDomains / 2 ? domain.Min : domain.Max;
                var colorDomainIdx = GetDomainIndex(colorDomains, value);
                if (colorDomainIdx == null)
                {
                    throw new Exception("The color scale could not be created. An internal calculation error has occurred.");
                }
                var foundColorDomainIdx = (int)colorDomainIdx;
                var foundColorDomain = colorDomains[foundColorDomainIdx];
                var tween = (value - foundColorDomain.Min) / foundColorDomain.Length;
                var color = colors[foundColorDomainIdx].Lerp(colors[foundColorDomainIdx + 1], tween);
                this.Colors.Add(color);
                this.Domains.Add(domain);
            }
        }

        /// <summary>
        /// Construct a ColorScale from a list of colors and an optional list of values
        /// </summary>
        /// <param name="colors">The color scale's key values.</param>
        /// <param name="values">List of values each color corresponds to on your scale. If specified, it expects one value per color, in ascending numerical order.</param>
        public ColorScale(List<Color> colors, List<double> values = null)
        {
            this.Colors = colors;

            if (values == null)
            {
                var domain = new Domain1d(0, 1);
                this.Domains = domain.DivideByCount(colors.Count - 1).ToList();
            }
            else if (colors.Count == values.Count)
            {
                this.Domains = new List<Domain1d>();

                for (var i = 0; i < values.Count - 1; i++)
                {
                    this.Domains.Add(new Domain1d(values[i], values[i + 1]));
                }
            }
            else
            {
                throw new ArgumentException("If you provide a list of custom values, it must match your list of colors in its count of items");
            }
        }

        /// <summary>
        /// Get the color from the color scale most closely
        /// approximating the provided value.
        /// </summary>
        /// <param name="t">A number within the numerical parameters from when you constructed your color scale.</param>
        /// <param name="discrete">If true, returns exactly one of the colors initially created, rather than a smoothly interpolated value.</param>
        /// <returns>A color.</returns>
        public Color GetColorForValue(double t)
        {
            if (this.Domains == null)
            {
                throw new ArgumentException("Your domains have not been calculated. Make sure this was initiated as a linear numerical scale");
            }
            var domainIdx = GetDomainIndex(t);

            if (domainIdx == null)
            {
                throw new ArgumentException($"Value {t} was not found in color scale");
            }

            // Discrete color scales end up with matching length lists,
            // non-discrete color scales expect there to be one more color than the number of domains
            var isDiscrete = this.Colors.Count != this.Domains.Count + 1;

            var foundDomainIdx = (int)domainIdx;
            var domain = this.Domains[foundDomainIdx];
            if (isDiscrete)
            {
                var color = t == domain.Max && foundDomainIdx < this.Colors.Count - 1 ? this.Colors[foundDomainIdx + 1] : this.Colors[foundDomainIdx];
                return color;
            }
            var colorMin = this.Colors[foundDomainIdx];
            var colorMax = this.Colors[foundDomainIdx + 1];
            var unitizedDistance = (t - domain.Min) / (domain.Max - domain.Min);
            return colorMin.Lerp(colorMax, unitizedDistance);
        }

        /// <summary>
        /// Find the index number of the domain that a value corresponds to.
        /// </summary>
        /// <param name="t">Value to search for</param>
        /// <returns></returns>
        private int? GetDomainIndex(double t)
        {
            return GetDomainIndex(this.Domains, t);
        }

        /// <summary>
        /// Returns the index of the first domain that contains the value.
        /// </summary>
        /// <param name="domains">Sorted list of domains</param>
        /// <param name="t">Value to search for</param>
        /// <returns></returns>
        private static int? GetDomainIndex(List<Domain1d> domains, double t)
        {
            if (t < domains.First().Min || t > domains.Last().Max)
            {
                throw new ArgumentException($"Your value {t} is outside of the the expected bounds of {domains.First().Min} - {domains.Last().Max}");
            }

            for (var i = 0; i < domains.Count; i++)
            {
                if (domains[i].Min <= t && domains[i].Max >= t)
                {
                    return i;
                }
            }
            return null;
        }
    }
}