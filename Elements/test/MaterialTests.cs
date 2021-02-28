using Elements.Geometry;
using System;
using System.Linq;
using Xunit;

namespace Elements.Tests
{
    public class MaterialTests : ModelTest
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
            Assert.Throws<ArgumentOutOfRangeException>(() => new Material("test", new Color(-1.0f, 1, 0f, 1.0f), 1.0f, 1.0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Material("test", new Color(3.0f, 1, 0f, 1.0f), 1.0f, 1.0f));
        }

        [Fact]
        public void StaticColor()
        {
            var material = new Material("test", Colors.Mint, 0.2f, 0.2f);
            Assert.Equal(material.Color, Colors.Mint);
        }

        [Fact]
        public void TextureTest()
        {
            this.Name = "TextureTest";
            var m = new Material("test", Colors.Gray, 0.0f, 0.0f, "./Textures/Concrete.jpg", true);
            var mass = new Mass(new Circle(Vector3.Origin, 5).ToPolygon(), 10, m);
            this.Model.AddElement(mass);
        }

        [Fact]
        public void NormalTextureTest()
        {
            this.Name = "NormalTextureTest";
            var m = new Material("test", Colors.Sand, 0.5f, 0.5f)
            {
                NormalTexture = "./Textures/Wood_Normals.jpg"
            };
            var mass = new Mass(new Circle(Vector3.Origin, 5).ToPolygon(), 10, m);
            this.Model.AddElement(mass);
        }

        [Fact]
        public void BuiltInMaterialsAlwaysHaveSameId()
        {
            var m1 = BuiltInMaterials.Wood;
            this.Model.AddElement(m1);
            var json = this.Model.ToJson();
            var newModel = Model.FromJson(json);
            var m2 = newModel.AllElementsOfType<Material>().First();
            Assert.Equal(m1.Id, m2.Id);
        }
    }
}