using Hypar.Elements;
using System;
using Xunit;

namespace Hypar.Tests
{
    public class MaterialTests
    {
        [Fact]
        public void ValidValues_Construct_Success()
        {
            var material = new Material("test", 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f);
            Assert.NotNull(material);
            Assert.Equal(1.0f, material.Red);
            Assert.Equal(1.0f, material.Blue);
            Assert.Equal(1.0f, material.Green);
            Assert.Equal(1.0f, material.Alpha);
            Assert.Equal(1.0f, material.SpecularFactor);
            Assert.Equal(1.0f, material.GlossinessFactor);
        }

        [Fact]
        public void InvalidValues_Construct_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Material("test", -1.0f, 1,0f, 1.0f, 1.0f, 1.0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Material("test", 3.0f, 1,0f, 1.0f, 1.0f, 1.0f));
        }
    }
}