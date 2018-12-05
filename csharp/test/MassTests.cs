using Hypar.Elements;
using Hypar.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using System.Linq;

namespace Hypar.Tests
{
    public class MassTests
    {
        [Fact]
        public void Example()
        {
            var a = new Vector3();
            var b = new Vector3(30, 10);
            var c = new Vector3(20, 50);
            var d = new Vector3(-10, 5);
            var poly = new Polygon(new []{a,b,c,d});
            var mass = new Mass(poly, 0.0, 5.0);
            var model = new Model();
            model.AddElement(mass);
            model.SaveGlb("mass.glb");
        }

        [Fact]
        public void Construct()
        {
            var model = new Model();
            var a = new Vector3();
            var b = new Vector3(30, 10);
            var c = new Vector3(20, 50);
            var d = new Vector3(-10, 5);

            var polygon = new Polygon(new[]{a,b,c,d});

            var mass = new Mass(polygon, 0, 40);
            model.AddElement(mass);
            model.SaveGlb("massTest1.glb");
        }

        [Fact]
        public void TopBottomSame_ThrowsException()
        {
            var model = new Model();
            var a = new Vector3();
            var b = new Vector3(30, 10);
            var c = new Vector3(20, 50);
            var d = new Vector3(-10, 5);
            var profile = new Polygon(new[]{a,b,c,d});
            Assert.Throws<ArgumentOutOfRangeException>(() => new Mass(profile, 0, 0));
        }

        [Fact]
        public void TopBelowBottom_ThrowsException()
        {
            var model = new Model();
            var a = new Vector3();
            var b = new Vector3(30, 10);
            var c = new Vector3(20, 50);
            var d = new Vector3(-10, 5);
            var profile = new Polygon(new[]{a,b,c,d});
            var material = new Material("mass", new Color(1.0f, 1.0f, 0.0f, 0.5f), 0.0f, 0.0f);
            Assert.Throws<ArgumentOutOfRangeException>(() => new Mass(profile, 0, -10));
        }

        [Fact]
        public void Transformed_Masses()
        {
            var model = new Model();
            var a = new Vector3();
            var b = new Vector3(30, 10);
            var c = new Vector3(20, 50);
            var d = new Vector3(-10, 5);
            var profile1 = new Polygon(new[]{a,b,c,d});
            var profile2 = profile1.Offset(-1.0).ElementAt(0);
            var profile3 = profile2.Offset(-1.0).ElementAt(0);
            var material1 = new Material("mass1", new Color(1.0f, 0.0f, 0.0f, 0.5f), 0.0f, 0.0f);
            var material2 = new Material("mass2", new Color(0.0f, 1.0f, 0.0f, 0.5f), 0.0f, 0.0f);
            var material3 = new Material("mass3", new Color(0.0f, 1.0f, 1.0f, 0.5f), 0.0f, 0.0f);
            var mass1 = new Mass(profile1, 0, 10.0, material1);
            var mass2 = new Mass(profile2, 10.0, 10.0, material2);
            var mass3 = new Mass(profile3, 20.0, 10.0, material3);
            model.AddElements(new[]{mass1,mass2,mass3});
            
            var floorType = new FloorType("test", 0.2);
            var f1 = new Floor(profile1, floorType, 0.0);
            var f2 = new Floor(profile2, floorType, 10.0);
            var f3 = new Floor(profile3, floorType, 20.0);
            model.AddElements(new[]{f1,f2,f3});

            model.SaveGlb("transformed_masses.glb");
        }

        [Fact]
        public void Volume()
        {
            var profile = Polygon.Rectangle(Vector3.Origin, 5, 5);
            var mass = new Mass(profile, 0.0, 5.0);
            Assert.Equal(125, mass.Volume());
        }

        [Fact]
        public void Transform()
        {
            var profile = Polygon.Rectangle();
            var mass = new Mass(profile, 0.0, 5.0);
            var t = new Vector3(5,0,0);
            mass.Transform.Move(t);
            for(var i=0; i<profile.Vertices.Count; i++)
            {
                Assert.Equal(profile.Vertices[i] + t, mass.Profile.Perimeter.Vertices[i] + t);
            }
        }
    }
}