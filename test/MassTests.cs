using Hypar.Elements;
using Hypar.Geometry;
using System;
using System.IO;
using Xunit;

namespace Hypar.Tests
{
    public class MassTests
    {
        [Fact]
        public void Default_Construct_Success()
        {
            var model = new Model();
            var a = new Vector2();
            var b = new Vector2(30, 10);
            var c = new Vector2(20, 50);
            var d = new Vector2(-10, 5);

            var profile = new Polygon2(new[]{a,b,c,d});

            var material = new Material("mass", 1.0f, 1.0f, 0.0f, 0.5f, 0.0f, 0.0f);
            model.AddMaterial(material);

            var mass = new Mass(profile, 0, profile, 40, material);
            model.AddElement(mass);

            model.SaveGlb("mass.glb");
            Assert.True(File.Exists("mass.glb"));
            Assert.Equal(1, model.Elements.Count);
        }

        [Fact]
        public void TopBottomSame_Construct_ThrowsException()
        {
            var model = new Model();
            var a = new Vector2();
            var b = new Vector2(30, 10);
            var c = new Vector2(20, 50);
            var d = new Vector2(-10, 5);
            var profile = new Polygon2(new[]{a,b,c,d});
            var material = new Material("mass", 1.0f, 1.0f, 0.0f, 0.5f, 0.0f, 0.0f);
            model.AddMaterial(material);
            Assert.Throws<ArgumentOutOfRangeException>(() => new Mass(profile, 0, profile, 0, material));
        }

        [Fact]
        public void TopBelowBottom_Construct_ThrowsException()
        {
            var model = new Model();
            var a = new Vector2();
            var b = new Vector2(30, 10);
            var c = new Vector2(20, 50);
            var d = new Vector2(-10, 5);
            var profile = new Polygon2(new[]{a,b,c,d});
            var material = new Material("mass", 1.0f, 1.0f, 0.0f, 0.5f, 0.0f, 0.0f);
            model.AddMaterial(material);
            Assert.Throws<ArgumentOutOfRangeException>(() => new Mass(profile, 1.0, profile, 0, material));
        }
    }
}