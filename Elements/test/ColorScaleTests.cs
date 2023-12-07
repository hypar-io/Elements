using System;
using System.Collections.Generic;
using Elements.Analysis;
using Elements.Geometry;
using Xunit;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Elements.Tests
{
    public class ColorScaleTests : ModelTest
    {
        [Fact, Trait("Category", "Examples")]
        public void ColorScale()
        {
            this.Name = "Elements_Analysis_ColorScale";
            // <example>
            // Construct a color scale specifying only
            // a few colors. The rest will be interpolated.
            var colorScale = new ColorScale(new List<Color>() { Colors.Cyan, Colors.Purple, Colors.Orange });

            var i = 0;
            foreach (var c in colorScale.Colors)
            {
                var panel = new Panel(Polygon.Rectangle(1, 1), new Material($"Material{i}", c));
                panel.Transform.Move(new Vector3(i * 1.1, 0, 0));
                this.Model.AddElement(panel);
                i++;
            }
            // </example>
        }

        [Fact]
        public void GetInterpolatedColor()
        {
            var defaultColorScale = new ColorScale(new List<Color>() { Colors.Cyan, Colors.Purple, Colors.Orange });
            Assert.Equal(defaultColorScale.Colors[1], defaultColorScale.GetColor(0.5));
            Assert.Equal(defaultColorScale.Colors[0], defaultColorScale.GetColor(0.0));
            Assert.Equal(defaultColorScale.Colors[2], defaultColorScale.GetColor(1.0));

            var unevenColorScale = new ColorScale(new List<Color>() { Colors.Cyan, Colors.Purple, Colors.Orange }, new List<double>() { 0, 10, 15 });
            Assert.Equal(unevenColorScale.Colors[0], unevenColorScale.GetColor(0.0));
            Assert.Equal(unevenColorScale.Colors[1], unevenColorScale.GetColor(10.0));
            Assert.Equal(unevenColorScale.Colors[2], unevenColorScale.GetColor(15.0));

            Assert.Throws<ArgumentException>(() => unevenColorScale.GetColor(15.1));
        }

        [Fact]
        public void GetBandedColor()
        {
            var numColors = 10;
            var colorScale = new ColorScale(new List<Color>() { Colors.Cyan, Colors.Purple, Colors.Orange }, numColors);
            Assert.Equal(10, colorScale.Colors.Count);
            for (var i = 0; i < numColors; i++)
            {
                Assert.Equal(colorScale.Colors[i], colorScale.GetColor((double)i / (numColors - 1)));
            }
        }

        [Fact]
        public void ThrowsOnSmallerColorCountMismatch()
        {
            Assert.Throws<ArgumentException>(() => new ColorScale(new List<Color>() { Colors.Cyan, Colors.Purple }, 1));
        }

        [Fact]
        public void ThrowsOnListSizeMismatch()
        {
            Assert.Throws<ArgumentException>(() => new ColorScale(new List<Color>() { Colors.Cyan, Colors.Purple }, new List<double>() { 0, 1, 2 }));
        }

        [Fact]
        public void ThrowsOnUnsortedValues()
        {
            Assert.Throws<ArgumentException>(() => new ColorScale(new List<Color>() { Colors.Cyan, Colors.Purple }, new List<double>() { 0, 2, 1 }));
        }

        [Fact]
        public void ThrowsOnDuplicatedValues()
        {
            Assert.Throws<ArgumentException>(() => new ColorScale(new List<Color>() { Colors.Cyan, Colors.Purple }, new List<double>() { 0, 1, 1 }));
        }

        [Fact]
        public void DeserializesCorrectly()
        {
            var bandedColorScale = new ColorScale(new List<Color>() { Colors.Cyan, Colors.Purple, Colors.Orange }, 10);
            var bandedSerialized = JsonSerializer.Serialize(bandedColorScale);
            var bandedDeserialized = JsonSerializer.Deserialize<ColorScale>(bandedSerialized);
            Assert.Equal(bandedColorScale.GetColor(0.12345), bandedDeserialized.GetColor(0.12345));

            var linearColorScale = new ColorScale(new List<Color>() { Colors.Cyan, Colors.Purple, Colors.Orange });
            var linearSerialized = JsonSerializer.Serialize(linearColorScale);
            var linearDeserialized = JsonSerializer.Deserialize<ColorScale>(linearSerialized);
            Assert.Equal(linearColorScale.GetColor(0.12345), linearDeserialized.GetColor(0.12345));

            Assert.NotEqual(linearColorScale.GetColor(0.12345), bandedColorScale.GetColor(0.12345));
        }
    }
}