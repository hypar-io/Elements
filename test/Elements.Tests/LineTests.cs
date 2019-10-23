using Elements.Geometry;
using System;
using Xunit;

namespace Elements.Geometry.Tests
{
    public class LineTests
    {
        [Fact]
        public void Example()
        {
            var a = new Vector3();
            var b = new Vector3(5,5,5);
            var l = new Line(a,b);
        }

        [Fact]
        public void Construct()
        {
            var a = new Vector3();
            var b = new Vector3(1, 0);
            var l = new Line(a,b);
            Assert.Equal(1.0, l.Length());
            Assert.Equal(new Vector3(0.5,0), l.PointAt(0.5));
        }

        [Fact]
        public void ZeroLength_ThrowsException()
        {
            var a = new Vector3();
            Assert.Throws<ArgumentException>(()=>new Line(a,a));
        }

        [Fact]
        public void Intersects()
        {
            var line = new Line(Vector3.Origin, new Vector3(5.0,0,0));
            var plane = new Plane(new Vector3(2.5,0,0), Vector3.XAxis);
            var x = line.Intersect(plane);
            Assert.Equal(x, plane.Origin);
        }

        [Fact]
        public void Parallel_DoesNotIntersect()
        {
            var line = new Line(Vector3.Origin, Vector3.ZAxis);
            var plane = new Plane(new Vector3(5.1,0,0), Vector3.XAxis);
            var x = line.Intersect(plane);
            Assert.Null(x);
        }

        [Fact]
        public void TooFar_DoesNotIntersect()
        {
            var line = new Line(Vector3.Origin, new Vector3(5.0,0,0));
            var plane = new Plane(new Vector3(5.1,0,0), Vector3.XAxis);
            var x = line.Intersect(plane);
            Assert.Null(x);
        }

        [Fact]
        public void IntersectsQuick()
        {
            var l1 = new Line(Vector3.Origin, new Vector3(5,0,0));
            var l2 = new Line(new Vector3(2.5, -2.5, 0), new Vector3(2.5,2.5,0));
            var l3 = new Line(new Vector3(0,-1,0), new Vector3(5,-1,0));
            var l4 = new Line(new Vector3(5,0,0), new Vector3(10,0,0));
            Assert.True(l1.Intersects2D(l2));     // Intersecting.
            Assert.False(l1.Intersects2D(l3));    // Not intersecting.
            Assert.False(l1.Intersects2D(l4));    // Coincident.
        }
    }
}