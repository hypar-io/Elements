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
            if(line.Intersects(plane, out Vector3 result))
            {
                Assert.True(result.Equals(plane.Origin));
            }
        }

        [Fact]
        public void LineParallelToPlaneDoesNotIntersect()
        {
            var line = new Line(Vector3.Origin, Vector3.ZAxis);
            var plane = new Plane(new Vector3(5.1,0,0), Vector3.XAxis);
            Assert.False(line.Intersects(plane, out Vector3 result));
        }

        [Fact]
        public void LineInPlaneDoesNotIntersect()
        {
            var line = new Line(Vector3.Origin, new Vector3(5,0,0));
            var plane = new Plane(Vector3.Origin, Vector3.ZAxis);
            Assert.False(line.Intersects(plane, out Vector3 result));
        }

        [Fact]
        public void LineTooFarDoesNotIntersect()
        {
            var line = new Line(Vector3.Origin, new Vector3(5.0,0,0));
            var plane = new Plane(new Vector3(5.1,0,0), Vector3.XAxis);
            Assert.False(line.Intersects(plane, out Vector3 result));
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

        [Fact]
        public void DivideByCount()
        {
            var l = new Line(Vector3.Origin, new Vector3(5,0));
            var segments = l.DivideByCount(5);
            var len = l.Length();
            foreach(var s in segments)
            {
                Assert.Equal(s.Length(), len/6, 5);
            }
        }

        [Fact]
        public void DivideByLength()
        {
            var l = new Line(Vector3.Origin, new Vector3(5,0));
            var segments = l.DivideByLength(1.1, true);
            Assert.Equal(4, segments.Count);

            var segments1 = l.DivideByLength(1.1);
            Assert.Equal(5, segments1.Count);
        }

        [Fact]
        public void DivideByLengthFromCenter()
        {
            // 5 whole size panels.
            var l = new Line(Vector3.Origin, new Vector3(5,0));
            var segments = l.DivideByLengthFromCenter(1);
            Assert.Equal(5, segments.Count);

            // 3 whole size panels and two small end panels.
            segments = l.DivideByLengthFromCenter(1.5);
            Assert.Equal(5, segments.Count);
            Assert.Equal(0.25, segments[0].Length());

            // 1 panel.
            segments = l.DivideByLengthFromCenter(6);
            Assert.Equal(1, segments.Count);
        }

        [Fact]
        public void Trim()
        {
            var l1 = new Line(Vector3.Origin, new Vector3(5,0));
            var l2 = new Line(new Vector3(4,-2), new Vector3(4, 2));
            var result = l1.TrimTo(l2);
            Assert.Equal(4, result.Length());

            var result1 = l1.TrimTo(l2, true);
            Assert.Equal(1, result1.Length());
        }

        [Fact]
        public void Extend()
        {
            var l1 = new Line(Vector3.Origin, new Vector3(5,0));
            var l2 = new Line(new Vector3(6,-2), new Vector3(6, 2));
            var result = l1.ExtendTo(l2);
            Assert.Equal(6, result.Length());

            l2 = new Line(new Vector3(-1, -2), new Vector3(-1, 2));
            result = l1.ExtendTo(l2);
            Assert.Equal(6, result.Length());
        }
    }
}