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
            var profile2 = profile1.Offset(-1.0).ElementAt(0);
            var profile3 = profile2.Offset(-1.0).ElementAt(0);
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
            
            Console.WriteLine(profile1.Area);
            Console.WriteLine(profile2.Area);
            Console.WriteLine(profile3.Area);

            var f1 = new Floor(profile1, new Polyline[]{}, 0.0, 0.2, BuiltInMaterials.Concrete);
            var f2 = new Floor(profile2, new Polyline[]{}, 10.0, 0.2, BuiltInMaterials.Concrete);
            var f3 = new Floor(profile3, new Polyline[]{}, 20.0, 0.2, BuiltInMaterials.Concrete);
            model.AddElements(new[]{f1,f2,f3});

            model.SaveGlb("transformed_masses.glb");
        }

        [Fact]
        public void TwoPeaks_Offset_2Polylines()
        {
            var a = new Vector3();
            var b = new Vector3(5, 0);
            var c = new Vector3(5, 5);
            var d = new Vector3(0, 1);
            var e = new Vector3(-5, 5);
            var f = new Vector3(-5, 0);

            var plinew = new Polyline(new[]{a,b,c,d,e,f});
            var offset = plinew.Offset(-0.5);
            Assert.Equal(2, offset.Count());
            var masses = new List<Mass>();
            foreach(var pl in offset)
            {
                var mass = new Mass(pl, 0, pl, 10);
                masses.Add(mass);
            }
            var model = new Model();
            model.AddElements(masses);
            model.SaveGlb("two_peaks.glb");
        }
    }
}