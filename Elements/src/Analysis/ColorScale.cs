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
        public List<Domain1d> Domains { get; } = null;

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
        /// Construct a color scale
        /// </summary>
        /// <param name="colors">The color scale's key values.</param>
        /// <param name="colorCount">The number of colors in the final color scale
        /// These values will be interpolated between the provided colors.</param>
        [Obsolete("colorCount is no longer required, and this constructor will simply create an evenly interpolated scale between 0 and 1.")]
        public ColorScale(List<Color> colors, int colorCount) : this(colors)
        {

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
        /// <param name="t">A number between 0.0 and 1.0</param>
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

            var foundDomainIdx = (int)domainIdx;
            var domain = this.Domains[foundDomainIdx];
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
            if (t < this.Domains.First().Min || t > this.Domains.Last().Max)
            {
                throw new ArgumentException($"Your value {t} is outside of the the expected bounds of {this.Domains.First().Min} - {this.Domains.Last().Max}");
            }

            for (var i = 0; i < this.Domains.Count; i++)
            {
                if (this.Domains[i].Min <= t && this.Domains[i].Max >= t)
                {
                    return i;
                }
            }
            return null;
        }
    }
}