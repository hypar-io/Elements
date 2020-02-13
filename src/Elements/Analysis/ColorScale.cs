using System;
using System.Collections.Generic;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements.Analysis
{
    /// <summary>
    /// A set of discrete colors.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Elements.Tests/ColorScaleTests.cs?name=example)]
    /// </example>
    public class ColorScale
    {
        /// <summary>
        /// The colors of the scale.
        /// </summary>
        public List<Color> Colors { get; } = new List<Color>();

        /// <summary>
        /// Construct a color scale.
        /// </summary>
        /// <param name="colors">The colors which define the color scale.</param>
        [JsonConstructor]
        public ColorScale(List<Color> colors)
        {
            this.Colors = colors;
        }

        /// <summary>
        /// Construct a color scale.
        /// </summary>
        /// <param name="colors">The colors which define the color scale's key values.</param>
        /// <param name="colorCount">The number of colors in the final color scale
        /// These values will be interpolated between the provided colors.</param>
        public ColorScale(List<Color> colors, int colorCount)
        {
            if (colors.Count > colorCount)
            {
                throw new ArgumentException("The color scale could not be created. The number of supplied colors is greater than the final color count.");
            }

            this.Colors = colors;

            while (this.Colors.Count < colorCount)
            {
                var startCount = this.Colors.Count;
                for (var i = 0; i < startCount; i += 2)
                {
                    if (this.Colors.Count >= colorCount)
                    {
                        break;
                    }
                    var a = this.Colors[i];
                    var b = this.Colors[i + 1];
                    var c = a.Lerp(b, 0.5);
                    this.Colors.Insert(i + 1, c);
                }
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
            var index = (int)Math.Floor(t * (this.Colors.Count - 1));
            return this.Colors[index];
        }
    }
}