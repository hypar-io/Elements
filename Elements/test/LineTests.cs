using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Elements.Tests;
using Newtonsoft.Json;
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
        public void Equality()
        {
            var p = new Vector3(1, 1, 1);
            var lineA = new Line(Vector3.Origin, p);
            var lineB = new Line(Vector3.Origin, p + new Vector3(1E-4, 1E-4, 1E-4));
            var lineC = new Line(Vector3.Origin, p + new Vector3(1E-6, 1E-6, 1E-6));

            Assert.NotEqual(lineA, lineB);
            Assert.Equal(lineA, lineC);
            Assert.NotEqual(lineA, lineA.Reversed());

            var comparer = new LineComparer(false);
            Assert.NotEqual(lineA, lineB, comparer);
            Assert.Equal(lineA, lineC, comparer);
            Assert.Equal(lineA, lineA.Reversed(), comparer);

            var pickyComparer = new LineComparer(true, 1E-7);
            Assert.NotEqual(lineA, lineB, pickyComparer);
            Assert.NotEqual(lineA, lineC, pickyComparer);
            Assert.NotEqual(lineA, lineA.Reversed(), pickyComparer);

            // Check that a line will succeed in creating identical hashcode even if the two endpoints are equidistant from origin
            var lineD = new Line(new Vector3(1, 0, 0), new Vector3(0, 1, 0));
            var h1 = comparer.GetHashCode(lineD);
            var h2 = comparer.GetHashCode(lineD.Reversed());
            Assert.Equal(h1, h2);
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

            // Plane at line end.
            var plane2 = new Plane(new Vector3(5.0, 0, 0), Vector3.XAxis);
            var intersectsPlane2 = line.Intersects(plane2, out _);
            Assert.True(intersectsPlane2);

            // Plane almost at line end.
            var plane4 = new Plane(new Vector3(4.99999, 0, 0), Vector3.XAxis);
            var intersectsPlane4 = line.Intersects(plane4, out _);
            Assert.True(intersectsPlane4);

            // Plane at line start
            var plane3 = new Plane(new Vector3(0.0, 0, 0), Vector3.XAxis);
            var intersectsPlane3 = line.Intersects(plane3, out _);
            Assert.True(intersectsPlane3);

            // Plane almost at line start.
            var plane5 = new Plane(new Vector3(0.00001, 0, 0), Vector3.XAxis);
            var intersectsPlane5 = line.Intersects(plane5, out _);
            Assert.True(intersectsPlane5);
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
        public void DivideIntoEqualSegments()
        {
            var l = new Line(Vector3.Origin, new Vector3(100, 0));
            var segments = l.DivideIntoEqualSegments(41);
            var len = l.Length();
            Assert.Equal(41, segments.Count);
            foreach (var s in segments)
            {
                Assert.Equal(s.Length(), len / 41, 5);
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
        public void TrimLineThatStartsAtPolygonEdge()
        {
            var polygon = JsonConvert.DeserializeObject<Polygon>(File.ReadAllText("../../../models/Geometry/ConcavePolygon.json"));
            var line = JsonConvert.DeserializeObject<Line>(File.ReadAllText("../../../models/Geometry/LineThatFailsTrim.json"));
            var lines = line.Trim(polygon, out var _);

            Assert.Equal(4.186147, lines[0].Length(), 2);
        }

        [Fact]
        public void LineIntersectAtEnd()
        {
            var line1 = JsonConvert.DeserializeObject<Line>("{\"discriminator\":\"Elements.Geometry.Line\",\"Start\":{\"X\":22.63192881973488,\"Y\":23.07673264883112,\"Z\":0.0},\"End\":{\"X\":26.239210000000003,\"Y\":32.009170000000005,\"Z\":0.0}}");
            var line2 = JsonConvert.DeserializeObject<Line>("{\"discriminator\":\"Elements.Geometry.Line\",\"Start\":{\"X\":26.23921,\"Y\":32.00917,\"Z\":0.0},\"End\":{\"X\":24.47373,\"Y\":32.72215,\"Z\":0.0}}");
            line1.Intersects(line2, out var intersection, false, true);
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
            Assert.Single(i1);
            Assert.Equal(2, o1.Count);
            var i2 = fullyInside.Trim(Polygon, out var o2);
            Assert.Single(i2);
            Assert.Empty(o2);
            var i3 = fullyOutside.Trim(Polygon, out var o3);
            Assert.Empty(i3);
            Assert.Single(o3);
            var i4 = startsInsideAndCrossesOnce.Trim(Polygon, out var o4);
            Assert.Single(i4);
            Assert.Single(o4);
            var i5 = startsOutsideAndLandsOnEdge.Trim(Polygon, out var o5);
            Assert.Single(i5);
            Assert.Single(o5);
            var i6 = crossesAtVertexStaysOutside.Trim(Polygon, out var o6);
            Assert.Empty(i6);
            Assert.Equal(2, o6.Count);
            var i7 = passesThroughAtVertex.Trim(Polygon, out var o7);
            Assert.Single(i7);
            Assert.Single(o7);
        }

        [Fact]
        public void ExtendToProfile()
        {
            Name = "ExtendToProfile";
            var polygons = JsonConvert.DeserializeObject<List<Polygon>>("[{\"Vertices\":[{\"X\":-3.3239130434782611,\"Y\":2.534782608695652,\"Z\":0.0},{\"X\":1.924698097225491,\"Y\":6.0910303088278326,\"Z\":0.0},{\"X\":2.5714592294272105,\"Y\":3.5518940120358997,\"Z\":0.0},{\"X\":1.1956521739130428,\"Y\":2.1760869565217393,\"Z\":0.0},{\"X\":1.7456521739130428,\"Y\":0.47826086956521707,\"Z\":0.0},{\"X\":0.69347826086956554,\"Y\":-1.2195652173913047,\"Z\":0.0},{\"X\":2.9652173913043476,\"Y\":-1.3391304347826083,\"Z\":0.0},{\"X\":3.2760869565217394,\"Y\":-2.7978260869565217,\"Z\":0.0},{\"X\":1.5065217391304346,\"Y\":-3.347826086956522,\"Z\":0.0},{\"X\":-0.43043478260869589,\"Y\":-1.482608695652174,\"Z\":0.0},{\"X\":-1.4586956521739132,\"Y\":-1.7695652173913039,\"Z\":0.0},{\"X\":-3.3239130434782611,\"Y\":-2.1043478260869568,\"Z\":0.0}]},{\"Vertices\":[{\"X\":0.070768827751857444,\"Y\":-0.75686629107050352,\"Z\":0.0},{\"X\":1.1928650019807621,\"Y\":0.39683822609442632,\"Z\":0.0},{\"X\":-1.1303482038171115,\"Y\":2.5936180601481968,\"Z\":0.0},{\"X\":-1.3358024329012763,\"Y\":2.2617304593199297,\"Z\":0.0}]}]");
            var profile = new Profile(polygons);
            var mcs = profile.ToModelCurves(material: BuiltInMaterials.XAxis);
            Model.AddElements(mcs);
            var intersectsAtVertex = JsonConvert.DeserializeObject<Vector3>("{\"X\":1.9248644534137522,\"Y\":-2.3794833726732039,\"Z\":0.0}");
            var doesntIntersect = JsonConvert.DeserializeObject<Vector3>("{\"X\":-3.5077580495618728,\"Y\":3.5797588753975274,\"Z\":0.0}");
            var hitsVoid = JsonConvert.DeserializeObject<Vector3>("{\"X\":-0.73780074305628141,\"Y\":-0.976285760075414,\"Z\":0.0}");
            var lineDir = new Vector3(0.1, 0.1);
            var iavLine = new Line(intersectsAtVertex, intersectsAtVertex + lineDir).ExtendTo(profile);
            var diLine = new Line(doesntIntersect, doesntIntersect + lineDir).ExtendTo(profile);
            var hvLine = new Line(hitsVoid, hitsVoid + lineDir).ExtendTo(profile);

            var otherLines = new List<Line> { new Line(new Vector3(5, 5), new Vector3(6, 6)) };
            var hitsParallelSegment = new Line(new Vector3(3, 3), new Vector3(4, 4)).ExtendTo(otherLines, false);

            Model.AddElements(new[] { iavLine, diLine, hvLine }.Select(l => new ModelCurve(l)));
            Assert.Equal(2.45915, iavLine.Length(), 4);
            Assert.Equal(lineDir.Length(), diLine.Length(), 4);
            Assert.Equal(2.02291, hvLine.Length(), 4);
            Assert.Equal(new Vector3(3, 3).DistanceTo(new Vector3(5, 5)), hitsParallelSegment.Length());
        }

        [Fact]
        public void ExtendWithMultipleIntersections()
        {
            Name = "ExtendWithMultipleIntersections";
            var line = new Line(new Vector3(0, 0), new Vector3(1, 0));

            var vertices = new List<Vector3>()
                {
                    new Vector3(-1, -1),
                    new Vector3(2, -1),
                    new Vector3(2, 1),
                    new Vector3(3, 1),
                    new Vector3(3, -1),
                    new Vector3(4, -1),
                    new Vector3(4, 2),
                    new Vector3(-1, 2)
                };

            var polygon = new Polygon(vertices);

            // Extends in both directions, and stops at earliest intersection
            var defaultExtend = line.ExtendTo(polygon);

            Assert.Equal(-1, defaultExtend.Start.X);
            Assert.Equal(2, defaultExtend.End.X);

            // Extend only endpoint
            var singleDirectionExtend = line.ExtendTo(polygon, false);

            Assert.Equal(0, singleDirectionExtend.Start.X);
            Assert.Equal(2, singleDirectionExtend.End.X);

            // Extend both sides to furthest intersection
            var furthestExtend = line.ExtendTo(polygon, true, true);

            Assert.Equal(-1, furthestExtend.Start.X);
            Assert.Equal(4, furthestExtend.End.X);

            // Extend endpoint only to furthest intersection
            var singleFurthestExtend = line.ExtendTo(polygon, false, true);

            Assert.Equal(0, singleFurthestExtend.Start.X);
            Assert.Equal(4, singleFurthestExtend.End.X);

            // If no intersections found, returns line with same endpoints
            var noIntersection = line.ExtendTo(Polygon.Rectangle(new Vector3(1, 1), new Vector3(2, 2)));

            Assert.Equal(line.Start.X, noIntersection.Start.X);
            Assert.Equal(line.End.X, noIntersection.End.X);
        }
    }
}