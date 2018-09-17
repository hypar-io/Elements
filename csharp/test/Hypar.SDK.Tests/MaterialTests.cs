using Hypar.Elements;
using Hypar.Geometry;
using System;
using Xunit;

namespace Hypar.Tests
{
    public class MaterialTests
    {
        [Fact]
        public void Construct()
        {
            var material = new Material("test", new Color(1.0f, 1.0f, 1.0f, 1.0f), 1.0f, 1.0f);
            Assert.NotNull(material);
            Assert.Equal(1.0f, material.Color.Red);
            Assert.Equal(1.0f, material.Color.Blue);
            Assert.Equal(1.0f, material.Color.Green);
            Assert.Equal(1.0f, material.Color.Alpha);
            Assert.Equal(1.0f, material.SpecularFactor);
            Assert.Equal(1.0f, material.GlossinessFactor);
        }

        [Fact]
        public void InvalidValues_Construct_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Material("test", new Color(-1.0f, 1,0f, 1.0f), 1.0f, 1.0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Material("test", new Color(3.0f, 1,0f, 1.0f), 1.0f, 1.0f));
        }

        [Fact]
        public void StaticColor()
        {
            var material = new Material("test", Color.Mint, 0.2f, 0.2f);
            Assert.Equal(material.Color, Color.Mint);
        }
    }
}