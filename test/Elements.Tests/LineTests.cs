using System;
using Elements.Tests;
using Xunit;

namespace Elements.Geometry.Tests
{
    public class LineTests : ModelTest
    {
        public LineTests()
        {
            this.GenerateIfc = false;
        }

        [Fact, Trait("Category", "Examples")]
        public void LineExample()
        {
            this.Name = "Elements_Geometry_Line";

            // <example>
            var a = new Vector3();
            var b = new Vector3(5, 5, 5);
            var l = new Line(a, b);
            // </example>

            this.Model.AddElement(new ModelCurve(l));
        }

        [Fact]
        public void Construct()
        {
            var a = new Vector3();
            var b = new Vector3(1, 0);
            var l = new Line(a, b);
            Assert.Equal(1.0, l.Length());
            Assert.Equal(new Vector3(0.5, 0), l.PointAt(0.5));
        }

        [Fact]
        public void ZeroLength_ThrowsException()
        {
            var a = new Vector3();
            Assert.Throws<ArgumentException>(() => new Line(a, a));
        }

        [Fact]
        public void Intersects()
        {
            var line = new Line(Vector3.Origin, new Vector3(5.0, 0, 0));
            var plane = new Plane(new Vector3(2.5, 0, 0), Vector3.XAxis);
            if (line.Intersects(plane, out Vector3 result))
            {
                Assert.True(result.Equals(plane.Origin));
            }
        }

        [Fact]
        public void LineParallelToPlaneDoesNotIntersect()
        {
            var line = new Line(Vector3.Origin, Vector3.ZAxis);
            var plane = new Plane(new Vector3(5.1, 0, 0), Vector3.XAxis);
            Assert.False(line.Intersects(plane, out Vector3 result));
        }

        [Fact]
        public void OverlappingLineDoesNotIntersect()
        {
            var line1 = new Line(new Vector3(0, 0, 0), new Vector3(4, 0, 0));
            var line2 = new Line(new Vector3(2, 0, 0), new Vector3(8, 0, 0));
            Assert.False(line1.Intersects2D(line2));
        }

        [Fact]
        public void LineInPlaneDoesNotIntersect()
        {
            var line = new Line(Vector3.Origin, new Vector3(5, 0, 0));
            var plane = new Plane(Vector3.Origin, Vector3.ZAxis);
            Assert.False(line.Intersects(plane, out Vector3 result));
        }

        [Fact]
        public void LineTooFarDoesNotIntersect()
        {
            var line = new Line(Vector3.Origin, new Vector3(5.0, 0, 0));
            var plane = new Plane(new Vector3(5.1, 0, 0), Vector3.XAxis);
            Assert.False(line.Intersects(plane, out Vector3 result));
        }

        [Fact]
        public void IntersectsQuick()
        {
            var l1 = new Line(Vector3.Origin, new Vector3(5, 0, 0));
            var l2 = new Line(new Vector3(2.5, -2.5, 0), new Vector3(2.5, 2.5, 0));
            var l3 = new Line(new Vector3(0, -1, 0), new Vector3(5, -1, 0));
            var l4 = new Line(new Vector3(5, 0, 0), new Vector3(10, 0, 0));
            Assert.True(l1.Intersects2D(l2));     // Intersecting.
            Assert.False(l1.Intersects2D(l3));    // Not intersecting.
            Assert.False(l1.Intersects2D(l4));    // Coincident.
        }

        [Fact]
        public void DivideByCount()
        {
            var l = new Line(Vector3.Origin, new Vector3(5, 0));
            var segments = l.DivideByCount(5);
            var len = l.Length();
            foreach (var s in segments)
            {
                Assert.Equal(s.Length(), len / 6, 5);
            }
        }

        [Fact]
        public void DivideByLength()
        {
            var l = new Line(Vector3.Origin, new Vector3(5, 0));
            var segments = l.DivideByLength(1.1, true);
            Assert.Equal(4, segments.Count);

            var segments1 = l.DivideByLength(1.1);
            Assert.Equal(5, segments1.Count);
        }

        [Fact]
        public void DivideByLengthFromCenter()
        {
            // 5 whole size panels.
            var l = new Line(Vector3.Origin, new Vector3(5, 0));
            var segments = l.DivideByLengthFromCenter(1);
            Assert.Equal(5, segments.Count);

            // 3 whole size panels and two small end panels.
            segments = l.DivideByLengthFromCenter(1.5);
            Assert.Equal(5, segments.Count);
            Assert.Equal(0.25, segments[0].Length());

            // 1 panel.
            segments = l.DivideByLengthFromCenter(6);
            Assert.Single<Line>(segments);
        }

        [Fact]
        public void Trim()
        {
            var l1 = new Line(Vector3.Origin, new Vector3(5, 0));
            var l2 = new Line(new Vector3(4, -2), new Vector3(4, 2));
            var result = l1.TrimTo(l2);
            Assert.Equal(4, result.Length());

            var result1 = l1.TrimTo(l2, true);
            Assert.Equal(1, result1.Length());
        }

        [Fact]
        public void Extend()
        {
            var l1 = new Line(Vector3.Origin, new Vector3(5, 0));
            var l2 = new Line(new Vector3(6, -2), new Vector3(6, 2));
            var result = l1.ExtendTo(l2);
            Assert.Equal(6, result.Length());

            l2 = new Line(new Vector3(-1, -2), new Vector3(-1, 2));
            result = l1.ExtendTo(l2);
            Assert.Equal(6, result.Length());
        }

        [Fact]
        public void LineTrimToInYZ()
        {
            Line l1 = new Line(new Vector3(0, 0, 0), new Vector3(0, 0, 10));
            Line l2 = new Line(new Vector3(0, 5, 2), new Vector3(0, -5, 2));

            Line l3 = l1.TrimTo(l2);

            Assert.NotNull(l3);
        }


        [Fact]
        public void LineTrimWithPolygon()
        {
            Line startsOutsideAndCrossesTwice = new Line(new Vector3(0, 0, 0), new Vector3(5, 6, 0));
            Line fullyInside = new Line(new Vector3(2, 2, 0), new Vector3(3, 3, 0));
            Line fullyOutside = new Line(new Vector3(8, 9, 0), new Vector3(7, 6, 0));
            Line startsInsideAndCrossesOnce = new Line(new Vector3(2, 2, 0), new Vector3(7, 4, 0));
            Line startsOutsideAndLandsOnEdge = new Line(new Vector3(-2, 4, 0), new Vector3(4, 5, 0));
            Line crossesAtVertexStaysOutside = new Line(new Vector3(6, 2, 0), new Vector3(4, 0));
            Line passesThroughAtVertex = new Line(new Vector3(6, 0, 0), new Vector3(4, 2, 0));
            var Polygon = new Polygon(new[]
            {
                new Vector3(1,1,0),
                new Vector3(5,1,0),
                new Vector3(5,5,0),
                new Vector3(1,5,0)
            });

            var i1 = startsOutsideAndCrossesTwice.Trim(Polygon, out var o1);
            Assert.Equal(1, i1.Count);
            Assert.Equal(2, o1.Count);
            var i2 = fullyInside.Trim(Polygon, out var o2);
            Assert.Equal(1, i2.Count);
            Assert.Equal(0, o2.Count);
            var i3 = fullyOutside.Trim(Polygon, out var o3);
            Assert.Equal(0, i3.Count);
            Assert.Equal(1, o3.Count);
            var i4 = startsInsideAndCrossesOnce.Trim(Polygon, out var o4);
            Assert.Equal(1, i4.Count);
            Assert.Equal(1, o4.Count);
            var i5 = startsOutsideAndLandsOnEdge.Trim(Polygon, out var o5);
            Assert.Equal(1, i5.Count);
            Assert.Equal(1, o5.Count);
            var i6 = crossesAtVertexStaysOutside.Trim(Polygon, out var o6);
            Assert.Equal(0, i6.Count);
            Assert.Equal(2, o6.Count);
            var i7 = passesThroughAtVertex.Trim(Polygon, out var o7);
            Assert.Equal(1, i7.Count);
            Assert.Equal(1, o7.Count);

        }
    }
}