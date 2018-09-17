using Hypar.Elements;
using Hypar.Geometry;
using System;
using Xunit;

namespace Hypar.Tests
{
    public class SpaceTests
    {
        [Fact]
        public void Example()
        {
            var a = new Vector3();
            var b = new Vector3(30, 10);
            var c = new Vector3(20, 50);
            var d = new Vector3(-10, 5);
            var profile = new Polygon(new[]{a,b,c,d});
            var space = new Space(profile, null,  0, 10);
            var model = new Model();
            model.AddElement(space);
            model.SaveGlb("space.glb");
        }

        [Fact]
        public void NegativeHeight_ThrowsException()
        {
            var model = new Model();
            var a = new Vector3();
            var b = new Vector3(30, 10);
            var c = new Vector3(20, 50);
            var d = new Vector3(-10, 5);
            var profile = new Polygon(new[]{a,b,c,d});
            Assert.Throws<ArgumentOutOfRangeException>(() => new Space(profile, null, 0, -10));
        }
    }
}