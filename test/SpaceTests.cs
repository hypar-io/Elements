using Elements;
using Elements.Geometry;
using System;
using System.Collections.Generic;
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
            var space = new Space(profile, 0, 10);
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
            Assert.Throws<ArgumentOutOfRangeException>(() => new Space(profile, 0, -10));
        }

        [Fact]
        public void Transform()
        {
            var p = Polygon.Rectangle();
            var space = new Space(p);
            var t = new Vector3(5,5,5);
            space.Transform.Move(t);
            var p1 = space.Profile.Perimeter;
            for(var i=0; i<p.Vertices.Length; i++)
            {
                Assert.Equal(p.Vertices[i] + t, p1.Vertices[i] + t);
            }
        }
    }
}