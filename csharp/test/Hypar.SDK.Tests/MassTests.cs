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
            var a = new Vector3();
            var b = new Vector3(30, 10);
            var c = new Vector3(20, 50);
            var d = new Vector3(-10, 5);

            var profile = new Polyline(new[]{a,b,c,d});

            var mass = new Mass(profile, 0, profile, 40);
            model.AddElement(mass);
            model.SaveGlb("massTest1.glb");
        }

        [Fact]
        public void TopBottomSame_Construct_ThrowsException()
        {
            var model = new Model();
            var a = new Vector3();
            var b = new Vector3(30, 10);
            var c = new Vector3(20, 50);
            var d = new Vector3(-10, 5);
            var profile = new Polyline(new[]{a,b,c,d});
            Assert.Throws<ArgumentOutOfRangeException>(() => new Mass(profile, 0, profile, 0));
        }

        [Fact]
        public void TopBelowBottom_Construct_ThrowsException()
        {
            var model = new Model();
            var a = new Vector3();
            var b = new Vector3(30, 10);
            var c = new Vector3(20, 50);
            var d = new Vector3(-10, 5);
            var profile = new Polyline(new[]{a,b,c,d});
            var material = new Material("mass", new Color(1.0f, 1.0f, 0.0f, 0.5f), 0.0f, 0.0f);
            Assert.Throws<ArgumentOutOfRangeException>(() => new Mass(profile, 0, profile, -10));
        }

        [Fact]
        public void Transformed_Masses()
        {
            var model = new Model();
            var a = new Vector3();
            var b = new Vector3(30, 10);
            var c = new Vector3(20, 50);
            var d = new Vector3(-10, 5);
            var profile1 = new Polyline(new[]{a,b,c,d});
            var profile2 = profile1.Offset(1.0);
            var profile3 = profile2.Offset(1.0);
            var material1 = new Material("mass1", new Color(1.0f, 0.0f, 0.0f, 0.5f), 0.0f, 0.0f);
            var material2 = new Material("mass2", new Color(0.0f, 1.0f, 0.0f, 0.5f), 0.0f, 0.0f);
            var material3 = new Material("mass3", new Color(0.0f, 1.0f, 1.0f, 0.5f), 0.0f, 0.0f);
            var mass1 = new Mass(profile1, 0, profile1, 10.0);
            var mass2 = new Mass(profile2, 10.0, profile2, 20.0);
            var mass3 = new Mass(profile3, 20.0, profile3, 30.0);
            mass1.Material = material1;
            mass2.Material = material2;
            mass3.Material = material3;
            model.AddElements(new[]{mass1,mass2,mass3});
            model.SaveGlb("transformed_masses.glb");
            Console.WriteLine(profile1.Area);
            Console.WriteLine(profile2.Area);
            Console.WriteLine(profile3.Area);
        }
    }
}