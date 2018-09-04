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
    }
}