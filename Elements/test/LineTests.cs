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

            Assert.False(lineA.IsAlmostEqualTo(lineB, false));
            Assert.True(lineA.IsAlmostEqualTo(lineB, false, 1E-3));
            Assert.True(lineA.IsAlmostEqualTo(lineC, false));
            Assert.False(lineA.IsAlmostEqualTo(lineA.Reversed(), true));
        }

        [Fact]
        public void Construct()
        {
            var a = new Vector3();
            var b = new Vector3(1, 0);
            var l = new Line(a, b);
            Assert.Equal(1.0, l.Length());
            Assert.Equal(new Vector3(0.5, 0), l.PointAt(0.5));
            Assert.Equal(a, l.PointAt(-1e-10));
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
        public void DivideIntoEqualSegmentsSingle()
        {
            var l = new Line(Vector3.Origin, new Vector3(100, 0));
            var segments = l.DivideIntoEqualSegments(1);
            Assert.Single(segments);
            Assert.True(segments.First().Start.IsAlmostEqualTo(l.Start, 1e-10));
            Assert.True(segments.First().End.IsAlmostEqualTo(l.End, 1e-10));
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
            Line passesThroughEdge = new Line(new Vector3(0, 5, 0), new Vector3(10, 5, 0));
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
            var i8 = passesThroughEdge.Trim(Polygon, out var o8, true);
            Assert.Single(i8);
            Assert.Equal(2, o8.Count);
        }

        [Fact]
        public void LineTrimWithPolygonInfinite()
        {
            var polygon = new Polygon(new[]
            {
                new Vector3(0, 0),
                new Vector3(0, 5),
                new Vector3(2, 5),
                new Vector3(2, 2),
                new Vector3(3, 3),
                new Vector3(4, 2),
                new Vector3(4, 5),
                new Vector3(6, 5),
                new Vector3(6, 0)
            });

            var line = new Line(new Vector3(10, 3), new Vector3(8, 3));
            var inside = line.Trim(polygon, out var outside, infinite: true);
            Assert.Equal(2, inside.Count);
            // Sorted in line direction.
            Assert.Equal(new Vector3(6, 3), inside[0].Start);
            Assert.Equal(new Vector3(4, 3), inside[0].End);
            Assert.Equal(new Vector3(2, 3), inside[1].Start);
            Assert.Equal(new Vector3(0, 3), inside[1].End);

            // (3; 3) point splits outside point into two segments.
            // Outer outside segments are discarded when infinite is false.
            Assert.Equal(2, outside.Count);
            Assert.Equal(new Vector3(4, 3), outside[0].Start);
            Assert.Equal(new Vector3(3, 3), outside[0].End);
            Assert.Equal(new Vector3(3, 3), outside[1].Start);
            Assert.Equal(new Vector3(2, 3), outside[1].End);
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

        [Fact]
        public void IntersectsBox()
        {
            BBox3 box = new BBox3(new Vector3(0, 0, 0), new Vector3(10, 10, 10));

            //1. Line goes inside
            Line l = new Line(new Vector3(-5, -5, 5), new Vector3(5, 5, 5));
            l.Intersects(box, out var results, infinite: false);
            Assert.True(results.Count == 1);
            Assert.Equal(results[0], new Vector3(0, 0, 5));
            l.Intersects(box, out results, infinite: true);
            Assert.True(results.Count == 2);
            Assert.Equal(results[1], new Vector3(10, 10, 5));

            //2. Line goes though. Intersections are ordered in line direction
            l = new Line(new Vector3(1, 1, 15), new Vector3(1, 1, -5));
            l.Intersects(box, out results, infinite: false);
            Assert.True(results.Count == 2);
            Assert.Equal(results[0], new Vector3(1, 1, 10));
            Assert.Equal(results[1], new Vector3(1, 1, 0));

            //3. Line touches corner of box as it goes by
            l = new Line(new Vector3(-10, 10, 3), new Vector3(-5, 5, 3));
            l.Intersects(box, out results, infinite: true);
            Assert.True(results.Count == 1);
            Assert.Equal(results[0], new Vector3(0, 0, 3));

            //4. Line overlaps with box side
            l = new Line(new Vector3(-5, 0, 4), new Vector3(15, 0, 8));
            l.Intersects(box, out results, infinite: false);
            Assert.True(results.Count == 2);
            Assert.Equal(results[0], new Vector3(0, 0, 5));
            Assert.Equal(results[1], new Vector3(10, 0, 7));

            //5. Line touches two sides of box and is misaligned slightly
            l = new Line(new Vector3(5, box.Min.Y + Vector3.EPSILON * 0.99, box.Min.Z), new Vector3(5, box.Max.Y - Vector3.EPSILON * 0.99, box.Min.Z));
            Assert.True(box.Min.Y.ApproximatelyEquals(l.Start.Y));
            Assert.True(box.Max.Y.ApproximatelyEquals(l.End.Y));
            l.Intersects(box, out results, infinite: false);
            Assert.True(results.Count == 2);
            Assert.True(results[0].IsAlmostEqualTo(l.Start));
            Assert.True(results[1].IsAlmostEqualTo(l.End));

            //6. Short line touches two sides of box and is misaligned slightly (it requires increased tolerance to get correct results )
            var newBox = new BBox3(new Vector3(-30, 19.60979, 0), new Vector3(-29.5, 20.39021, 0));
            l = new Line(new Vector3(-30, newBox.Min.Y + Vector3.EPSILON * 0.99, 0), new Vector3(-30, newBox.Max.Y - Vector3.EPSILON * 0.99, 0));
            Assert.True(newBox.Min.Y.ApproximatelyEquals(l.Start.Y));
            Assert.True(newBox.Max.Y.ApproximatelyEquals(l.End.Y));
            l.Intersects(newBox, out results, infinite: false);
            Assert.True(results.Count == 2);
            Assert.True(results[0].IsAlmostEqualTo(l.Start));
            Assert.True(results[1].IsAlmostEqualTo(l.End));
        }

        [Fact]
        public void ExtendWithMultipleIntersectionsAndMaxDistance()
        {
            Name = "ExtendWithMultipleIntersectionsAndMaxDistance";
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

            // Extends in both directions, and stops at earliest intersection.
            var defaultExtend = line.ExtendTo(polygon, 10);

            Assert.Equal(-1, defaultExtend.Start.X);
            Assert.Equal(2, defaultExtend.End.X);

            // Extends in both directions, and stops at earliest intersection.
            // The distance from line points to polygon segments is greater than maxDistance, so the line must remain unchanged.
            var extendWithMaxDistance = line.ExtendTo(polygon, 0.5);

            Assert.Equal(line.Start.X, extendWithMaxDistance.Start.X);
            Assert.Equal(line.End.X, extendWithMaxDistance.End.X);

            // Extend both sides to furthest intersection, but no further than maxDistance.
            var furthestExtend = line.ExtendTo(polygon, 2.5, true, true);

            Assert.Equal(-1, furthestExtend.Start.X);
            Assert.Equal(3, furthestExtend.End.X);
        }

        [Fact]
        public void HashCodesForDifferentComponentsAreNotEqual()
        {
            var a = new Vector3(1, 2, 3);
            var b = new Vector3(3, 2, 1);
            var l1 = new Line(a, b);
            var l2 = new Line(b, a);
            Assert.NotEqual(l1.GetHashCode(), l2.GetHashCode());
        }

        [Fact]
        public void FitLineAndCollinearity()
        {
            var points1 = new List<Vector3> {
                (0,0,0),
                (5,Vector3.EPSILON * 0.5,0),
                (8,0,Vector3.EPSILON * 0.5),
                (-4, 0, 0)
            };
            Assert.True(points1.AreCollinearByDistance());

            points1.Add(
                  (-6, Vector3.EPSILON * 2, 0)
            );
            Assert.False(points1.AreCollinearByDistance());
        }

        [Fact]
        public void IsCollinear()
        {
            var line = new Line(Vector3.Origin, new Vector3(5, 5, 5));

            var collinearLine = new Line(new Vector3(10, 10, 10), new Vector3(20, 20, 20));
            Assert.True(line.IsCollinear(collinearLine));

            var nonCollinearLine = new Line(new Vector3(-5, 5, 5), new Vector3(10, 10, -5));
            Assert.False(line.IsCollinear(nonCollinearLine));

            var reversedLine = new Line(new Vector3(5, 5, 5), Vector3.Origin);
            Assert.True(line.IsCollinear(reversedLine));

            var sameLine = new Line(Vector3.Origin, new Vector3(5, 5, 5));
            Assert.True(line.IsCollinear(sameLine));

            var ovelappingLine = new Line(new Vector3(2, 2, 2), new Vector3(-10, -10, -10));
            Assert.True(line.IsCollinear(ovelappingLine));

            var sharedStartLine = new Line(Vector3.Origin, new Vector3(10, 10, -5));
            Assert.False(line.IsCollinear(sharedStartLine));

            var sharedEndLine = new Line(new Vector3(-5, 5, 5), new Vector3(5, 5, 5));
            Assert.False(line.IsCollinear(sharedEndLine));

            var parallelLine = new Line(new Vector3(3, 3, 0), new Vector3(8, 8, 5));
            Assert.True(line.Direction().IsParallelTo(parallelLine.Direction()));
            Assert.False(line.IsCollinear(parallelLine));

            var almostSameLine = new Line(Vector3.Origin, new Vector3(5, 5.00000000001, 5));
            Assert.True(line.IsAlmostEqualTo(almostSameLine, false));
            Assert.True(line.IsCollinear(almostSameLine));

            var longLine = new Line(new Vector3(458.8830, -118.7170, 13.8152), new Vector3(458.8830, -80.4465, 13.8152));
            var nearlySameLine = new Line(new Vector3(458.9005, 29.6573, 13.7977), new Vector3(458.9005, 33.5632, 13.7977));
            Assert.False(longLine.IsCollinear(nearlySameLine));

            // collinear within tolerance
            var line1 = new Line(new Vector3(0, 0, 0), new Vector3(10, 0, 0));
            var line2 = new Line(new Vector3(5, 0.01, 0), new Vector3(15, 0.01, 0));
            Assert.False(line1.IsCollinear(line2));
            Assert.True(line1.IsCollinear(line2, 0.1));
        }

        [Fact]
        public void TryGetOverlap()
        {
            var line = new Line(Vector3.Origin, new Vector3(5, 5, 5));

            var nonCollinearLine = new Line(new Vector3(-5, 5, 5), new Vector3(10, 10, -5));
            Assert.False(line.TryGetOverlap(nonCollinearLine, out Line nonCollinearOverlap));
            Assert.Null(nonCollinearOverlap);

            var sharedEndLine = new Line(new Vector3(10, 10, 10), new Vector3(5, 5, 5));
            Assert.False(line.TryGetOverlap(sharedEndLine, out Line sharedEndOverlap));
            Assert.Null(sharedEndOverlap);

            var sharedStartLine = new Line(Vector3.Origin, new Vector3(-10, -10, -10));
            Assert.False(line.TryGetOverlap(sharedStartLine, out Line sharedStartOverlap));
            Assert.Null(sharedStartOverlap);

            var collinearLine = new Line(new Vector3(10, 10, 10), new Vector3(20, 20, 20));
            Assert.False(line.TryGetOverlap(collinearLine, out Line collinearOverlap));
            Assert.Null(collinearOverlap);

            var sameLine = new Line(Vector3.Origin, new Vector3(5, 5, 5));
            Assert.True(line.TryGetOverlap(sameLine, out Line sameLineOverlap));
            Assert.NotNull(sameLineOverlap);
            Assert.True(sameLineOverlap.IsAlmostEqualTo(line, false));

            var ovelappingLine = new Line(new Vector3(2, 2, 2), new Vector3(-10, -10, -10));
            var expectedLine = new Line(Vector3.Origin, new Vector3(2, 2, 2));
            Assert.True(line.TryGetOverlap(ovelappingLine, out Line overlapLine));
            Assert.NotNull(overlapLine);
            Assert.True(overlapLine.IsAlmostEqualTo(expectedLine, false));

            var almostSameLine = new Line(Vector3.Origin, new Vector3(5, 5.00000000001, 5));
            Assert.True(line.IsAlmostEqualTo(almostSameLine, false));
            Assert.True(line.TryGetOverlap(almostSameLine, out Line almostSameOverlap));
            Assert.True(line.IsAlmostEqualTo(almostSameOverlap, false));

            //This failed in previous iteration when coordinate sum was used for points sorting
            var firstLineWihNearZeroSum = new Line(new Vector3(-3, 3, 0), new Vector3(-1, 1.00000002, 0));
            var secondLineWihNearZeroSum = new Line(new Vector3(-2, 2.00000001, 0), new Vector3(0, 0, 0));
            Assert.True(firstLineWihNearZeroSum.TryGetOverlap(secondLineWihNearZeroSum, out _));

            // TryGetOverlap within tolerance
            var line1 = new Line(new Vector3(0, 0, 0), new Vector3(10, 0, 0));
            // consistently off
            var line2 = new Line(new Vector3(5, 0.01, 0), new Vector3(15, 0.01, 0));
            Assert.False(line1.TryGetOverlap(line2, out _));
            Assert.True(line1.TryGetOverlap(line2, 0.1, out _));
            // at an angle
            var line3 = new Line(new Vector3(5, 0, 0), new Vector3(15, 0.01, 0));
            Assert.True(line1.TryGetOverlap(line3, 0.1, out var overlap));
            Assert.Equal(new Line((5, 0, 0), (10, 0, 0)), overlap);
        }


        [Fact]
        public void GetParameterAt()
        {
            var start = Vector3.Origin;
            var end = new Vector3(5, 5, 5);
            var line = new Line(start, end);

            Assert.Equal(0, line.GetParameterAt(start));

            var almostEqualStart = new Vector3(0.000001, 0.000005, 0);
            Assert.True(start.IsAlmostEqualTo(almostEqualStart));
            Assert.Equal(0, line.GetParameterAt(almostEqualStart));

            Assert.Equal(1, line.GetParameterAt(end));

            var almostEqualEnd = new Vector3(5.0000005, 5.000001, 5);
            Assert.True(end.IsAlmostEqualTo(almostEqualEnd));
            Assert.Equal(1, line.GetParameterAt(almostEqualEnd));

            var vectorOutsideLine = new Vector3(1, 2, 3);
            Assert.False(line.PointOnLine(vectorOutsideLine, true));
            Assert.Equal(-1, line.GetParameterAt(vectorOutsideLine));

            var middle = new Vector3(2.5, 2.5, 2.5);
            Assert.Equal(0.5, line.GetParameterAt(middle));

            var vector = new Vector3(3.2, 3.2, 3.2);
            var uValue = line.GetParameterAt(vector);
            var expectedVector = line.PointAt(uValue);
            Assert.InRange(uValue, 0, 1);
            Assert.True(vector.IsAlmostEqualTo(expectedVector));
        }

        [Theory]
        [MemberData(nameof(MergedCollinearLineData))]
        public void MergedCollinearLine(Line line, Line lineToMerge, Line expectedResult)
        {
            var result = line.MergedCollinearLine(lineToMerge);
            Assert.True(expectedResult.IsAlmostEqualTo(result, true));
            Assert.True(line.Direction().IsAlmostEqualTo(result.Direction()));
        }

        public static IEnumerable<object[]> MergedCollinearLineData()
        {
            var start = Vector3.Origin;
            var end = new Vector3(5, 5, 5);
            var line = new Line(start, end);
            return new List<object[]>
            {
                new object[] {line, new Line(new Vector3(10, 10, 10), end), new Line(start, new Vector3(10, 10, 10))},
                new object[] {line, new Line(start, new Vector3(-10, -10, -10)), new Line(new Vector3( -10, -10, -10), end)},
                new object[] {line, new Line(new Vector3(10, 10, 10), new Vector3(20, 20, 20)), new Line(start, new Vector3(20, 20, 20))},
                new object[] {line, line, line},
                new object[] {line, new Line(new Vector3(2, 2, 2), new Vector3(-10, -10, -10)), new Line(new Vector3( -10, -10, -10), end)},
                new object[] {line, new Line(start, new Vector3(5, 5.00000000001, 5)), line},
                new object[] {line, new Line(new Vector3(2, 2, 2), new Vector3(-10, -10, -10)), new Line(new Vector3( -10, -10, -10), end)},
                new object[] {new Line(new Vector3(-3, 3, 0), new Vector3(-1, 1.00000002, 0)), new Line(new Vector3(-2, 2.00000001, 0), Vector3.Origin), new Line(new Vector3(-3, 3, 0), Vector3.Origin)}
            };
        }

        [Fact]
        public void MergedCollinearLineThrowsException()
        {
            var line = new Line(Vector3.Origin, new Vector3(5, 5, 5));
            var nonCollinearLine = new Line(new Vector3(-5, 5, 5), new Vector3(10, 10, -5));
            Assert.Throws<ArgumentException>(() => line.MergedCollinearLine(nonCollinearLine));
        }

        [Fact]
        public void BestFitLine()
        {
            // points symmetrical about the expected horizontal line
            var points0 = new[]
            {
                new Vector3(0, 0),
                new Vector3(0, 4),
                new Vector3(2, 1),
                new Vector3(2, 3),
                new Vector3(4, 1),
                new Vector3(4, 3),
                new Vector3(6, 0),
                new Vector3(6, 4)
            };
            var line0 = Line.BestFit(points0);
            Assert.Equal(line0, new Line(new Vector3(0, 2), new Vector3(6, 2)));

            // collinear points
            var points1 = new[]
            {
                new Vector3(1, 1),
                new Vector3(1.25, 2),
                new Vector3(1.5, 3),
                new Vector3(2, 5)
            };
            var line1 = Line.BestFit(points1);
            Assert.Equal(new Line(points1[0], points1[3]), line1);

            // points symmetrical about the expected vertical line
            var points2 = new[]
           {
                new Vector3(0, 0),
                new Vector3(0, 2),
                new Vector3(0, 4),
                new Vector3(2, 0),
                new Vector3(2, 2),
                new Vector3(2, 4)
            };
            var line2 = Line.BestFit(points2);
            Assert.Equal(line2, new Line(new Vector3(1, 0), new Vector3(1, 4)));

            // random points
            var points3 = new[]
            {
                new Vector3(1.21,1.69),
                new Vector3(3,5.89),
                new Vector3(5.16,4.11),
                new Vector3(8.31,5.49),
                new Vector3(10.21,8.65)
            };
            var line3 = Line.BestFit(points3);
            var mCoef = 0.56150040095236275;
            var bCoef = 2.03395076348772;
            Assert.True(line3.PointOnLine(new Vector3(2, mCoef * 2 + bCoef)));
            Assert.True(line3.PointOnLine(new Vector3(7, mCoef * 7 + bCoef)));
        }

        [Theory]
        [MemberData(nameof(ProjectedData))]
        public void Projected(Line line, Plane plane, Line expectedLine)
        {
            var result = line.Projected(plane);
            Assert.Equal(expectedLine, result);
        }

        [Fact]
        public void PointOnLine()
        {
            Vector3 start = new Vector3(0, 0, 0);
            Vector3 end = new Vector3(10, 0, 0);

            //1. End points
            Assert.False(Line.PointOnLine(start, start, end));
            Assert.False(Line.PointOnLine(start, start, end));
            Assert.True(Line.PointOnLine(start, start, end, true));

            //2. Almost end point
            Vector3 test = new Vector3(1e-6, 0, 0);
            Assert.True(Line.PointOnLine(test, start, end));
            Assert.True(Line.PointOnLine(test, start, end, true));
            test = new Vector3(1e-6, 1e-6, 0);
            Assert.True(Line.PointOnLine(test, start, end));
            Assert.True(Line.PointOnLine(test, start, end, true));

            //3. Midpoint
            test = new Vector3(4, 0, 0);
            Assert.True(Line.PointOnLine(test, start, end));
            Assert.True(Line.PointOnLine(test, start, end, true));

            //3. Almost midpoint
            test = new Vector3(4, 5e-6, 0);
            Assert.True(Line.PointOnLine(test, start, end));
            Assert.True(Line.PointOnLine(test, start, end, true));

            //4. Point not on the line
            test = new Vector3(5, 1, 0);
            Assert.False(Line.PointOnLine(test, start, end));
            Assert.False(Line.PointOnLine(test, start, end, true));

            //5. Large line
            Vector3 farEnd = new Vector3(150, 0, 0);
            test = new Vector3(100, 0.1, 0);
            Assert.False(Line.PointOnLine(test, start, farEnd));
            Assert.False(Line.PointOnLine(test, start, farEnd, true));

            //6. Collinear point outside line
            test = new Vector3(15, 0, 0);
            Assert.False(Line.PointOnLine(test, start, end));
            Assert.False(Line.PointOnLine(test, start, end, true));
            test = new Vector3(-5, 0, 0);
            Assert.False(Line.PointOnLine(test, start, end));
            Assert.False(Line.PointOnLine(test, start, end, true));
        }

        [Fact]
        public void IsOnPlane()
        {
            var plane = new Plane(Vector3.Origin, Vector3.ZAxis);

            var lineOnPlane = new Line(new Vector3(1, 2), new Vector3(3, 2));
            Assert.True(lineOnPlane.IsOnPlane(plane));

            var tolerance = 0.001;
            var lineOnPlaneWithTolerance = new Line(new Vector3(1, 2, 0.0009), new Vector3(3, 2, -0.0009));
            Assert.True(lineOnPlaneWithTolerance.IsOnPlane(plane, tolerance));

            var lineAbovePlane = new Line(new Vector3(1, 2, 3), new Vector3(3, 2, 3));
            Assert.False(lineAbovePlane.IsOnPlane(plane));

            var lineWithStartOnPlane = new Line(new Vector3(1, 2), new Vector3(3, 2, 3));
            Assert.False(lineWithStartOnPlane.IsOnPlane(plane));

            var lineWithEndOnPlane = new Line(new Vector3(1, 2, 3), new Vector3(3, 2));
            Assert.False(lineWithEndOnPlane.IsOnPlane(plane));

            var lineIntersectingPlane = new Line(new Vector3(1, 2, -3), new Vector3(3, 2, 3));
            Assert.True(lineIntersectingPlane.Intersects(plane, out var _));
            Assert.False(lineIntersectingPlane.IsOnPlane(plane));
        }

        public static IEnumerable<object[]> ProjectedData()
        {
            var line = new Line(Vector3.Origin, new Vector3(5, 5, 5));
            return new List<object[]>
            {
                new object[] {line, new Plane(Vector3.Origin, Vector3.ZAxis), new Line(Vector3.Origin, new Vector3(5, 5, 0))},
                new object[] {line, new Plane(Vector3.Origin, Vector3.XAxis), new Line(Vector3.Origin, new Vector3(0, 5, 5))},
                new object[] {line, new Plane(new Vector3(2, 2, 2), Vector3.YAxis), new Line(new Vector3(0, 2, 0), new Vector3(5, 2, 5))},
                new object[] {new Line(Vector3.Origin, new Vector3(0, 5, 5)), new Plane(Vector3.Origin, Vector3.XAxis), new Line(Vector3.Origin, new Vector3(0, 5, 5))},
            };
        }

        [Fact]
        public void LinesOffset()
        {
            var lines = new List<Line> {
                new Line((3,2), (0,4)),
                new Line((0,4), (3,7)),
                new Line((3,7), (1,10)),
                new Line((3,7), (6,8)),
                new Line((6,8), (6,11)),
                new Line((6,11), (9,11)),
                new Line((9,11), (9,7)),
                new Line((9,7), (6,4)),
                new Line((6,4), (7,0)),
                new Line((7,0), (9,2)),
                new Line((9,2), (11,0)),
                new Line((11,0), (13,2)),
                new Line((13,2), (12,6)),
                new Line((12,6), (9,7)),
            };

            var offset = lines.Offset(0.5);

            Model.AddElements(lines.Select(p => new ModelCurve(p, BuiltInMaterials.XAxis)));
            Model.AddElements(offset.Select(p => new ModelCurve(p, BuiltInMaterials.YAxis)));

            Assert.Equal(2, offset.Count());
        }

        [Fact]
        public void LinesOffset_ThreeSeparatePolygons()
        {
            var lines = new List<Line> {
                new Line((-23.738996, 125.715021, 0), (40.722023, 186.716267, 0)),
                new Line((-39.945297, 180.889282, 0), (-23.738996, 125.715021, 0)),
                new Line((-0.06687, 28.113027, 0), (-53.784385, -10.490746, 0)),
                new Line((-0.06687, 108.41616, 0), (-0.06687, 28.113027, 0)),
                new Line((-0.06687, 28.113027, 0), (70.403226, -6.666788, 0)),
                new Line((91.161859, 86.747061, 0), (140.144949, 143.742255, 0)),
                new Line((91.161859, 86.747061, 0), (95.167911, 158.855996, 0)),
                new Line((53.468552, 134.637591, 0), (91.161859, 86.747061, 0))
            };

            var offset = lines.Offset(12);

            Assert.Equal(3, offset.Count());
        }

        [Fact]
        public void LinesOffset_ClosedShape()
        {
            var lines = new List<Line> {
                new Line((-0.015494, -18.985642, 0), (-1.76639, 24.869265, 0)),
                new Line((-1.76639, 24.869265, 0), (9.866722, -13.880061, 0)),
                new Line((24.806017, 6.880776, 0), (86.757219, -32.955245, 0)),
                new Line((86.757219, -32.955245, 0), (51.313489, 62.054296, 0)),
                new Line((51.313489, 62.054296, 0), (24.806017, 6.880776, 0))
            };

            var offset = lines.Offset(3);

            Assert.Equal(3, offset.Count());
        }

        [Fact]
        public void LineDistanceSame()
        {
            Vector3 pt = new Vector3(-2.685818406894334, -2.476879934864206, -0.5565494179776103);
            Vector3 v = new Vector3(-2.3537985903693657, 2.407193267710168, -2.771078003386066);
            double q1 = 0.21721224020356145, q2 = -1.006813808941921, q3 = 2.112519713576212;
            Line line1 = new Line(pt, pt + q1 * v);
            Line line2 = new Line(pt + q2 * v, pt + q3 * v);
            Assert.Equal(0, line1.DistanceTo(line2), 12);
        }

        [Fact]
        public void LineDistanceIntersect()
        {
            Vector3 pt = new Vector3(2.798721214152833, -0.3556044049478837, -2.9550511796484766);
            Vector3 v1 = new Vector3(2.465855774694745, 2.6356139841825836, 0.4933654383536945);
            Vector3 v2 = new Vector3(1.0293808889279106, -2.4963706389774964, 1.5988855967507778);
            double q1 = -1.5791413478212935, q2 = -2.81511586964034, q3 = 1.732636303417701, q4 = -0.9234662064172614;
            Line line1 = new Line(pt + q1 * v1, pt + q2 * v1);
            Line line2 = new Line(pt + q3 * v2, pt + q4 * v2);
            Assert.Equal(0, line1.DistanceTo(line2), 12);
        }

        [Fact]
        public void LineDistanceParallel()
        {
            Vector3 pt = new Vector3(2.798721214152833, -0.3556044049478837, -2.9550511796484766);
            Vector3 v1 = new Vector3(2.465855774694745, 2.6356139841825836, 0.4933654383536945);
            Vector3 v2 = new Vector3(1.633720404719513, -1.8504262591965435, 1.7198011154858075);
            double q1 = -1.5791413478212935, q2 = -2.81511586964034, q3 = 1.732636303417701, q4 = -0.9234662064172614;
            Line line1 = new Line(pt + q1 * v1, pt + q2 * v1);
            Line line2 = new Line(pt + q3 * v1 + v2, pt + q4 * v1 + v2);
            Assert.Equal(v2.Length(), line1.DistanceTo(line2), 12);
        }

        [Fact]
        public void LineDistanceGeneral()
        {
            Vector3 pt = new Vector3(-0.500280764727953, -2.9389849832575896, 1.9512390555224588);
            Vector3 v1 = new Vector3(-1.2081608688024432, -0.7895298630691459, -1.8380319057295544);
            Vector3 v2 = new Vector3(1.561390631684935, -1.268325457190592, -0.48150972505691025);
            Vector3 v3 = new Vector3(0.4345936745767045, 0.9194325262466677, -0.6806076130136314);
            double q1 = 0.31932651341597884, q2 = -0.8705916819804074, q3 = 2.7483887105972915, q4 = -2.4522354067395895;
            Line line1 = new Line(pt + q1 * v2, pt + q2 * v2);
            Line line2 = new Line(pt + q3 * v3 + v1, pt + q4 * v3 + v1);
            Assert.Equal(v1.Length(), line1.DistanceTo(line2), 12);
        }
    }
}
