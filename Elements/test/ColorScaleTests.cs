using System;
using System.Collections.Generic;
using Elements.Analysis;
using Elements.Geometry;
using Xunit;

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
        public void GetColor()
        {
            var colorScale = new ColorScale(new List<Color>() { Colors.Cyan, Colors.Purple, Colors.Orange });
            Assert.Equal(colorScale.Colors[1], colorScale.GetColorForValue(0.5));
            Assert.Equal(colorScale.Colors[0], colorScale.GetColorForValue(0.0));
            Assert.Equal(colorScale.Colors[2], colorScale.GetColorForValue(1.0));

            var unevenColorScale = new ColorScale(new List<Color>() { Colors.Cyan, Colors.Purple, Colors.Orange }, new List<double>() { 0, 10, 15 });
            Assert.Equal(unevenColorScale.Colors[0], unevenColorScale.GetColorForValue(0.0));
            Assert.Equal(unevenColorScale.Colors[1], unevenColorScale.GetColorForValue(10.0));
            Assert.Equal(unevenColorScale.Colors[2], unevenColorScale.GetColorForValue(15.0));

            Assert.Throws<ArgumentException>(() => unevenColorScale.GetColorForValue(15.1));
        }

        [Fact]
        public void ThrowsOnListSizeMismatch()
        {
            Assert.Throws<ArgumentException>(() => new ColorScale(new List<Color>() { Colors.Cyan, Colors.Purple }, new List<double>() { 0, 1, 2 }));
        }
    }
}