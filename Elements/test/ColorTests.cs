using System;
using System.Collections.Generic;
using Elements.Analysis;
using Elements.Geometry;
using Xunit;
using System.Text.Json.Serialization;

namespace Elements.Tests
{
    public class ColorTests
    {
        [Fact]
        public void ColorFromHex()
        {
            var color = new Color("#FF0000");
            Assert.Equal(1.0, color.Red);
            Assert.Equal(0.0, color.Green);
            Assert.Equal(0.0, color.Blue);
            Assert.Equal(1.0, color.Alpha);
        }

        [Fact]
        public void ColorFromName()
        {
            var color = new Color("red");
            Assert.Equal(1.0, color.Red);
            Assert.Equal(0.0, color.Green);
            Assert.Equal(0.0, color.Blue);
            Assert.Equal(1.0, color.Alpha);
        }

        [Fact]
        public void ColorFromUnkownNameThrows()
        {
            Assert.Throws<ArgumentException>(() => new Color("Floof"));
        }

        [Fact]
        public void ImplictConversion()
        {
            var material = new Material("Green")
            {
                Color = "#00ff00"
            };
            Assert.Equal(1.0, material.Color.Green);
        }

        [Fact]
        public void ColorCantBeNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                Color c = null;
            });
        }
    }
}