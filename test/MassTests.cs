using Elements.Geometry;
using System;
using Xunit;
using System.Linq;
using Elements.ElementTypes;

namespace Elements.Tests
{
    public class MassTests : ModelTest
    {
        [Fact]
        public void TopBottomSame_ThrowsException()
        {
            var a = new Vector3();
            var b = new Vector3(30, 10);
            var c = new Vector3(20, 50);
            var d = new Vector3(-10, 5);
            var profile = new Polygon(new[] { a, b, c, d });
            Assert.Throws<ArgumentOutOfRangeException>(() => new Mass(profile, 0));
        }

        [Fact]
        public void TopBelowBottom_ThrowsException()
        {
            var a = new Vector3();
            var b = new Vector3(30, 10);
            var c = new Vector3(20, 50);
            var d = new Vector3(-10, 5);
            var profile = new Polygon(new[] { a, b, c, d });
            var material = new Material("mass", new Color(1.0f, 1.0f, 0.0f, 0.5f), 0.0f, 0.0f);
            Assert.Throws<ArgumentOutOfRangeException>(() => new Mass(profile, -10));
        }

        [Fact]
        public void Transformed_Masses()
        {
            this.Name = "TransformedMasses";
            var a = new Vector3();
            var b = new Vector3(30, 10);
            var c = new Vector3(20, 50);
            var d = new Vector3(-10, 5);
            var profile1 = new Polygon(new[] { a, b, c, d });
            var profile2 = profile1.Offset(-1.0).ElementAt(0);
            var profile3 = profile2.Offset(-1.0).ElementAt(0);
            var material1 = new Material("mass1", new Color(1.0f, 0.0f, 0.0f, 0.5f), 0.0f, 0.0f);
            var material2 = new Material("mass2", new Color(0.0f, 1.0f, 0.0f, 0.5f), 0.0f, 0.0f);
            var material3 = new Material("mass3", new Color(0.0f, 1.0f, 1.0f, 0.5f), 0.0f, 0.0f);
            var mass1 = new Mass(profile1, 10.0, material1);
            var mass2 = new Mass(profile2, 10.0, material2, new Transform(new Vector3(0,0,10.0)));
            var mass3 = new Mass(profile3, 10.0, material3, new Transform(new Vector3(0,0,20.0)));
            this.Model.AddElements(new[] { mass1, mass2, mass3 });

            var floorType = new FloorType("test", 0.2);
            var f1 = new Floor(profile1, floorType, 0.0);
            var f2 = new Floor(profile2, floorType, 10.0);
            var f3 = new Floor(profile3, floorType, 20.0);
            this.Model.AddElements(new[] { f1, f2, f3 });
        }

        [Fact]
        public void Volume()
        {
            var profile = Polygon.Rectangle(5, 5);
            var mass = new Mass(profile, 5.0);
            Assert.Equal(125, mass.Volume());
        }

        [Fact]
        public void Transform()
        {
            var profile = Polygon.Rectangle(1.0, 1.0);
            var mass = new Mass(profile, 5.0, BuiltInMaterials.Mass, new Transform());
            var t = new Vector3(5, 0, 0);
            mass.Transform.Move(t);
            for (var i = 0; i < profile.Vertices.Count; i++)
            {
                Assert.Equal(profile.Vertices[i] + t, mass.Profile.Perimeter.Vertices[i] + t);
            }
        }
    }
}