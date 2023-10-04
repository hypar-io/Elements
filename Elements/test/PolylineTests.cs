using Elements.Tests;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Elements.Geometry.Tests
{
    public class PolylineTests : ModelTest
    {
        public PolylineTests()
        {
            this.GenerateIfc = false;
        }

        [Fact, Trait("Category", "Examples")]
        public void Polyline()
        {
            this.Name = "Elements_Geometry_Polyline";

            // <example>
            var a = new Vector3();
            var b = new Vector3(10, 10);
            var c = new Vector3(20, 5);
            var d = new Vector3(25, 10);

            var pline = new Polyline(new[] { a, b, c, d });
            var offset = pline.Offset(1, EndType.Square);
            // </example>

            this.Model.AddElement(new ModelCurve(pline, BuiltInMaterials.XAxis));
            this.Model.AddElement(new ModelCurve(offset[0], BuiltInMaterials.YAxis));

            Assert.Equal(4, pline.Vertices.Count);
            Assert.Equal(3, pline.Segments().Length);
        }

        [Fact]
        public void Polyline_ClosedOffset()
        {
            var length = 10;
            var offsetAmt = 1;
            var a = new Vector3();
            var b = new Vector3(length, 0);
            var pline = new Polyline(new[] { a, b });
            var offsetResults = pline.Offset(offsetAmt, EndType.Square);
            Assert.Single<Polygon>(offsetResults);
            var offsetResult = offsetResults[0];
            Assert.Equal(4, offsetResult.Vertices.Count);
            // offsets to a rectangle that's offsetAmt longer than the segment in
            // each direction, and 2x offsetAmt in width, so the long sides are
            // each length + 2x offsetAmt, and the short sides are each 2x offsetAmt.
            var targetLength = 2 * length + 8 * offsetAmt;
            Assert.Equal(targetLength, offsetResult.Length(), 2);
        }

        [Fact]
        public void Polyline_OffsetOnSide_SingleSegment()
        {
            var line = new Polyline(new List<Vector3>(){
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 0),
            });

            var polygons = line.OffsetOnSide(2, false);
            Assert.Single(polygons);

            // A rectangle extruded down from the original line.
            Assert.Equal(new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, -2, 0), new Vector3(10, -2, 0), new Vector3(10, 0, 0) }, polygons.First().Vertices);
        }

        [Fact]
        public void Polyline_OffsetOnSide_threeSegment()
        {
            var line = new Polyline(new List<Vector3>()
            {
                new Vector3(13540, 430),
                new Vector3(13540, -1240),
                new Vector3(9840, -1240),
                new Vector3(6914, -1190),
            });

            var polygons = line.OffsetOnSide(2, false);
            Assert.Equal(3, polygons.Length);

            // A 3 segment line with the end segment endpoint almost parallel to the second line
            Assert.Equal(new Vector3[] { new Vector3(13540, 430, 0), new Vector3(13538, 430, 0), new Vector3(13538, -1238, 0), new Vector3(13540, -1240, 0) }, polygons.First().Vertices);
        }

        [Fact]
        public void Polyline_OffsetOnSide_SingleSegmentFlipped()
        {
            var line = new Polyline(new List<Vector3>(){
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 0),
            });
            var polygons = line.OffsetOnSide(2, true);
            Assert.Single(polygons);

            // A rectangle extruded up from the original line.
            Assert.Equal(new Vector3[] { new Vector3(10, 0, 0), new Vector3(10, 2, 0), new Vector3(0, 2, 0), new Vector3(0, 0, 0) }, polygons.First().Vertices);
        }

        [Fact]
        public void Polyline_OffsetOnSide_RightAngleJoin()
        {
            var line = new Polyline(new List<Vector3>(){
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 0),
                new Vector3(10, 10, 0),
            });
            var polygons = line.OffsetOnSide(2, false);
            Assert.Equal(2, polygons.Count());

            // Extruding the two lines into rectangles, but joining at (12, -2) - forming a 45 degree join edge.
            Assert.Equal(new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, -2, 0), new Vector3(12, -2, 0), new Vector3(10, 0, 0) }, polygons[0].Vertices);
            Assert.Equal(new Vector3[] { new Vector3(10, 0, 0), new Vector3(12, -2, 0), new Vector3(12, 10, 0), new Vector3(10, 10, 0) }, polygons[1].Vertices);
        }

        [Fact]
        public void Polyline_OffsetOnSide_OuterJoin()
        {
            var line = new Polyline(new List<Vector3>(){
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 0),
                new Vector3(0, 5, 0),
            });
            var polygons = line.OffsetOnSide(2, false);
            Assert.Equal(2, polygons.Count());

            // If the join is outside the polyline, and the intersection point would be far away, blunt the edge by adding a line.
            var bottomOfFlatEdge = new Vector3(12.683281572999748, 0.8944271909999159, 0);
            var topOfFlatEdge = new Vector3(0.8944271909999159, 6.7888543819998315, 0);
            Assert.Equal(new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, -2, 0), new Vector3(12, -2, 0), bottomOfFlatEdge, new Vector3(10, 0, 0) }, polygons[0].Vertices);
            Assert.Equal(new Vector3[] { new Vector3(10, 0, 0), bottomOfFlatEdge, topOfFlatEdge, new Vector3(0, 5, 0) }, polygons[1].Vertices);
        }

        [Fact]
        public void Polyline_OffsetOnSide_ClosedLoop()
        {
            // A square.
            var line = new Polyline(new List<Vector3>(){
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 0),
                new Vector3(10, 10, 0),
                new Vector3(0, 10, 0),
                new Vector3(0, 0, 0),
            });
            var polygons = line.OffsetOnSide(2, false);
            Assert.Equal(4, polygons.Count());

            // This is generally identical to the right angle test, except every rectangle is a trapezoid with 45 degree angles
            Assert.Equal(new Vector3[] { new Vector3(0, 10, 0), new Vector3(-2, 12, 0), new Vector3(-2, -2, 0), new Vector3(0, 0, 0) }, polygons[0].Vertices);
            Assert.Equal(new Vector3[] { new Vector3(0, 0, 0), new Vector3(-2, -2, 0), new Vector3(12, -2, 0), new Vector3(10, 0, 0) }, polygons[1].Vertices);
            Assert.Equal(new Vector3[] { new Vector3(10, 0, 0), new Vector3(12, -2, 0), new Vector3(12, 12, 0), new Vector3(10, 10, 0) }, polygons[2].Vertices);
            Assert.Equal(new Vector3[] { new Vector3(10, 10, 0), new Vector3(12, 12, 0), new Vector3(-2, 12, 0), new Vector3(0, 10, 0) }, polygons[3].Vertices);
        }

        [Fact]
        public void SharedSegments_OpenMirroredSquares_NoResults()
        {
            var s = 1;

            var a = new Polygon(new List<Vector3>(){
                new Vector3(0, s, 0),
                new Vector3(-s, s, 0),
                new Vector3(-s, 0, 0),
                new Vector3(0, 0, 0),
            });
            var b = new Polygon(new List<Vector3>(){
                new Vector3(0, s, 0),
                new Vector3(s, s, 0),
                new Vector3(s, 0, 0),
                new Vector3(0, 0, 0),
            });

            var matches = Polygon.SharedSegments(a, b);

            Assert.Empty(matches);
        }

        [Fact]
        public void OffsetOpen()
        {
            Name = nameof(OffsetOpen);

            var right = new Polyline(
                (0, 10),
                (0, 0),
                (10, 0)
            );
            var acute = new Polyline(
                (5, 10),
                (0, 0),
                (10, 0)
            );
            var obtuse = new Polyline(
                (-5, 10),
                (0, 0),
                (10, 0)
            );
            var straight = new Polyline(
                (-5, 0),
                (0, 0),
                (10, 0)
            );
            var line = new Polyline(
                (0, 0),
                (10, 0)
            );
            var tightCorner = new Polyline(
                (0, 10),
                (0, 1),
                (1, 0),
                (10, 0)
            );
            var z = new Polyline(
                (0, 10),
                (10, 10),
                (0, 0),
                (10, 0)
            );

            Assert.Throws<System.Exception>(() =>
            {
                right.OffsetOpen(-10);
            });

            var testPolylines = new[] { right, acute, obtuse, straight, line, tightCorner, z };
            for (int i = 0; i < testPolylines.Length; i++)
            {
                var xform = new Transform(i * 40, 0, 0);
                Polyline p = testPolylines[i].TransformedPolyline(xform);
                Model.AddElement(p);
                for (double j = -9; j < 9; j += 0.5)
                {
                    if (j == 0) continue;
                    var offset = p.OffsetOpen(j);
                    Model.AddElement(new ModelCurve(offset, BuiltInMaterials.XAxis));
                }
            }
        }

        [Fact]
        public void GetParameterAt()
        {
            var start = new Vector3(1, 2);
            var end = new Vector3(3, 4);

            var polyline = new Polyline(new[] { start, new Vector3(1, 4), end });

            Assert.Equal(0, polyline.GetParameterAt(start));
            Assert.Equal(2, polyline.GetParameterAt(end));
            Assert.Equal(-1, polyline.GetParameterAt(Vector3.Origin));

            var point = new Vector3(2, 4);
            var resultParameter = polyline.GetParameterAt(point);
            Assert.Equal(1.5, resultParameter);
            var testPoint = polyline.PointAt(resultParameter);
            Assert.True(point.IsAlmostEqualTo(testPoint));
        }

        [Fact]
        public void Intersects()
        {
            var polyline = new Polyline(
                new Vector3(-5, -5),
                new Vector3(-5, 5),
                new Vector3(5, 5),
                new Vector3(5, -5));

            var notIntersectingLine = new Line(new Vector3(-3, 0), new Vector3(3, 0));

            Assert.False(polyline.Intersects(notIntersectingLine, out var result));
            Assert.Empty(result);

            Assert.True(polyline.Intersects(notIntersectingLine, out result, infinite: true));
            Assert.Collection(result,
                x => Assert.True(x.IsAlmostEqualTo(new Vector3(-5, 0))),
                x => Assert.True(x.IsAlmostEqualTo(new Vector3(5, 0))));

            var sharedStartLine = new Line(new Vector3(-5, 0), new Vector3(3, 0));
            Assert.False(polyline.Intersects(sharedStartLine, out result));
            Assert.Empty(result);

            Assert.True(polyline.Intersects(sharedStartLine, out result, includeEnds: true));
            Assert.Collection(result,
                x => Assert.True(x.IsAlmostEqualTo(sharedStartLine.Start)));

            var collinearSegmentLine = new Line(new Vector3(-5, -5), new Vector3(-5, 5));
            Assert.False(polyline.Intersects(collinearSegmentLine, out result));

            Assert.True(polyline.Intersects(collinearSegmentLine, out result, includeEnds: true));
            Assert.Collection(result,
                x => Assert.True(x.IsAlmostEqualTo(collinearSegmentLine.End)));

            var lPolygon = Polygon.L(10, 10, 4);
            var upperLine = new Line(new Vector3(5, 1), new Vector3(5, 2));
            Assert.False(lPolygon.Intersects(upperLine, out result));
            Assert.True(lPolygon.Intersects(upperLine, out result, infinite: true));
            Assert.Collection(result,
                x => x.IsAlmostEqualTo(new Vector3(5, 0)),
                x => x.IsAlmostEqualTo(new Vector3(5, 4)));

            var verticiesLine = new Line(Vector3.Origin, new Vector3(4, 4));
            Assert.False(lPolygon.Intersects(verticiesLine, out result));
            Assert.True(lPolygon.Intersects(verticiesLine, out result, includeEnds: true));
            Assert.Collection(result,
                x => x.IsAlmostEqualTo(verticiesLine.Start),
                x => x.IsAlmostEqualTo(verticiesLine.End));
        }

        [Fact]
        public void PolygonIntersectsReturnsFalse()
        {
            var polygon = Polygon.Rectangle(2, 4);
            var polyline = new Polyline(
                new Vector3(-3, -2),
                new Vector3(-3, 2),
                new Vector3(3, 2),
                new Vector3(3, -2));

            var result = polyline.Intersects(polygon, out var sharedSegments);

            Assert.False(result);
            Assert.Empty(sharedSegments);
        }

        [Theory]
        [MemberData(nameof(GetPolygonIntersectsTestData))]
        public void PolygonIntersectsReturnsOneSegment(Polyline polyline, Polyline expectedResult)
        {
            var polygon = Polygon.Rectangle(6, 4);
            var result = polyline.Intersects(polygon, out var sharedSegments);

            Assert.True(result);
            Assert.Collection(sharedSegments, x => Assert.True(x.Equals(expectedResult)));
        }

        public static IEnumerable<object[]> GetPolygonIntersectsTestData()
        {
            //Polyline is inside boundary
            yield return new object[]
            {
                new Polyline(new Vector3(2, 0), new Vector3(-2, 0), new Vector3(-2, 1)),
                new Polyline(new Vector3(2, 0), new Vector3(-2, 0), new Vector3(-2, 1))
            };

            //Polyline both ends outside boundary
            yield return new object[]
            {
                new Polyline(new Vector3(-1, 5), new Vector3(-1, 0), Vector3.Origin, new Vector3(0, -5)),
                new Polyline(new Vector3(-1, 2), new Vector3(-1, 0), Vector3.Origin, new Vector3(0, -2))
            };

            //Polyline end is on polygon boundary
            yield return new object[]
            {
                new Polyline(new Vector3(-1, 5), new Vector3(-1, 0), Vector3.Origin, new Vector3(0, -2)),
                new Polyline(new Vector3(-1, 2), new Vector3(-1, 0), Vector3.Origin, new Vector3(0, -2))
            };

            //Polyline start is on polygon boundary
            yield return new object[]
            {
                new Polyline(new Vector3(-1, -5), new Vector3(-1, 0), Vector3.Origin, new Vector3(0, 2)),
                new Polyline(new Vector3(-1, -2), new Vector3(-1, 0), Vector3.Origin, new Vector3(0, 2))
            };

            //Polyline end is inside boundary
            yield return new object[]
            {
                new Polyline(new Vector3(1, 5), new Vector3(1,0), Vector3.Origin),
                new Polyline(new Vector3(1, 2), new Vector3(1, 0), Vector3.Origin)
            };

            //Polyline start is inside boundary
            yield return new object[]
            {
                new Polyline(new Vector3(-1, -5), new Vector3(-1, 0), Vector3.Origin),
                new Polyline(new Vector3(-1, -2), new Vector3(-1, 0), Vector3.Origin)
            };

            //Polyline end is boundary vertex
            yield return new object[]
            {
                new Polyline(new Vector3(-1, -5), new Vector3(-1, 0), new Vector3(3, 2)),
                new Polyline(new Vector3(-1, -2), new Vector3(-1, 0), new Vector3(3, 2))
            };

            //Polyline start is boundary vertex
            yield return new object[]
            {
                new Polyline(new Vector3(-3, -2), new Vector3(-1, 0), new Vector3(-1, 5)),
                new Polyline(new Vector3(-3, -2), new Vector3(-1, 0), new Vector3(-1, 2))
            };

            //Polyline segment is part of boundary
            yield return new object[]
            {
                new Polyline(new Vector3(-1, 5), new Vector3(-1, 0), new Vector3(3, 0), new Vector3(3, 1)),
                new Polyline(new Vector3(-1, 2), new Vector3(-1, 0), new Vector3(3, 0))
            };
        }

        [Fact]
        public void PolygonIntersectsReturnsTwoSegmentsWhenThreeIntersectionsAndStartInside()
        {
            var polygon = Polygon.Rectangle(6, 4);

            var polyline = new Polyline(
                new Vector3(-1, -1),
                new Vector3(-1, 3),
                new Vector3(1, 3),
                new Vector3(1, -3));

            var result = polyline.Intersects(polygon, out var sharedSegments);

            Assert.True(result);

            var firstExpected = new Polyline(
                new Vector3(-1, -1),
                new Vector3(-1, 2));

            var secondExpected = new Polyline(
                new Vector3(1, 2),
                new Vector3(1, -2));

            Assert.True(polygon.Contains(polyline.Start));
            Assert.Collection(sharedSegments,
                x => Assert.True(x.Equals(firstExpected)),
                x => Assert.True(x.Equals(secondExpected)));
        }

        [Fact]
        public void PolygonIntersectsReturnsTwoSegmentsWhenThreeIntersectionsAndEndInside()
        {
            var polygon = Polygon.Rectangle(6, 4);

            var polyline = new Polyline(
                new Vector3(-1, -3),
                new Vector3(-1, 3),
                new Vector3(1, 3),
                new Vector3(1, -1));

            var result = polyline.Intersects(polygon, out var sharedSegments);

            Assert.True(result);

            var firstExpected = new Polyline(
                new Vector3(-1, -2),
                new Vector3(-1, 2));

            var secondExpected = new Polyline(
                new Vector3(1, 2),
                new Vector3(1, -1));

            Assert.True(polygon.Contains(polyline.End));
            Assert.Collection(sharedSegments,
                x => Assert.True(x.Equals(firstExpected)),
                x => Assert.True(x.Equals(secondExpected)));
        }

        [Fact]
        public void PolygonIntersectsReturnsTwoSegmentsWhenTwoIntersectionsAndBothEndsInside()
        {
            var polygon = Polygon.Rectangle(6, 4);

            var polyline = new Polyline(
                new Vector3(-1, -1),
                new Vector3(-1, 3),
                new Vector3(1, 3),
                new Vector3(1, -1));

            var result = polyline.Intersects(polygon, out var sharedSegments);

            Assert.True(result);

            var firstExpected = new Polyline(
                new Vector3(-1, -1),
                new Vector3(-1, 2));

            var secondExpected = new Polyline(
                new Vector3(1, 2),
                new Vector3(1, -1));

            Assert.True(polygon.Contains(polyline.Start));
            Assert.True(polygon.Contains(polyline.End));
            Assert.Collection(sharedSegments,
                x => Assert.True(x.Equals(firstExpected)),
                x => Assert.True(x.Equals(secondExpected)));
        }

        [Fact]
        public void PolygonIntersectsReturnsThreeSegmentsWhenSixIntersections()
        {
            var polygon = Polygon.Rectangle(6, 4);

            var polyline = new Polyline(
                new Vector3(-2, -3),
                new Vector3(-2, 3),
                new Vector3(-1, 3),
                new Vector3(-1, -3),
                new Vector3(1, -3),
                new Vector3(1, 3));

            var result = polyline.Intersects(polygon, out var sharedSegments);

            Assert.True(result);

            var firstExpected = new Polyline(
                new Vector3(-2, -2),
                new Vector3(-2, 2));

            var secondExpected = new Polyline(
                new Vector3(-1, 2),
                new Vector3(-1, -2));

            var thirdExpected = new Polyline(
               new Vector3(1, -2),
               new Vector3(1, 2));


            Assert.Collection(sharedSegments,
                x => Assert.True(x.Equals(firstExpected)),
                x => Assert.True(x.Equals(secondExpected)),
                x => Assert.True(x.Equals(thirdExpected)));
        }

        [Fact]
        public void PolygonIntersectsOfLShape()
        {
            var polygon = Polygon.L(10, 10, 3);
            var polyline = new Polyline(new Vector3(6, -2),
                new Vector3(6, 0),
                new Vector3(2, 0),
                new Vector3(2, 2),
                new Vector3(0, 2),
                new Vector3(0, 5),
                new Vector3(-2, 5));

            var expectedSubsegment = new Polyline(new Vector3(2, 0), new Vector3(2, 2), new Vector3(0, 2));

            var result = polyline.Intersects(polygon, out var sharedSegments);

            Assert.True(result);
            Assert.Collection(sharedSegments, sharedSegment => Assert.Equal(expectedSubsegment, sharedSegment));
        }

        [Fact]
        public void GetSubsegment()
        {
            var polyline = new Polyline(
                new Vector3(-5, -5),
                new Vector3(-5, 5),
                new Vector3(5, 5),
                new Vector3(5, -5));

            var start = new Vector3(-5, -3);
            var end = new Vector3(5, 3);

            var result = polyline.GetSubsegment(start, end);

            var expectedResult = new Polyline(
                start,
                new Vector3(-5, 5),
                new Vector3(5, 5),
                end);

            Assert.Equal(expectedResult, result);

            var reversedResult = polyline.GetSubsegment(end, start);
            var reversedExpectedResult = expectedResult.Reversed();

            Assert.Equal(reversedExpectedResult, reversedResult);

            var pointOutsidePolyline = Vector3.Origin;
            Assert.Equal(-1d, polyline.GetParameterAt(pointOutsidePolyline), 5);
            Assert.Null(polyline.GetSubsegment(pointOutsidePolyline, start));
            Assert.Null(polyline.GetSubsegment(start, pointOutsidePolyline));

            var middlePoint = new Vector3(0, 5);
            var startSubsegment = polyline.GetSubsegment(polyline.Start, middlePoint);
            var startSubsegmentExpected = new Polyline(polyline.Start, new Vector3(-5, 5), middlePoint);
            Assert.Equal(startSubsegmentExpected, startSubsegment);

            var endSubsegment = polyline.GetSubsegment(middlePoint, polyline.End);
            var endSubsegmentExpected = new Polyline(middlePoint, new Vector3(5, 5), polyline.End);
            Assert.Equal(endSubsegmentExpected, endSubsegment);
        }

        [Fact]
        public void PolylineFrameNormalsAreConsistent()
        {
            Name = nameof(PolylineFrameNormalsAreConsistent);

            Polyline curve = new Polyline(
                (0, 0, 0),
                (1, 0, 0),
                (2, 4, 3),
                (5, 3, 1),
                (10, 0, 0)
            );
            var frames = curve.Frames();

            for (int i = 0; i < frames.Length - 1; i++)
            {
                var currFrame = frames[i];
                var nextFrame = frames[i + 1];
                var currNormal = currFrame.ZAxis;
                var nextNormal = nextFrame.ZAxis;
                Assert.True(currNormal.Dot(nextNormal) > 0.0);
            }

            Bezier bezier = new Bezier(curve.Vertices.ToList()).TransformedBezier(new Transform(30, 0, 0));
            var bFrames = bezier.Frames();

            var parameters = new List<double>();
            for (int i = 0; i < 20; i++)
            {
                parameters.Add(i / 19.0);
            }

            var movedCrv = curve.TransformedPolyline(new Transform(15, 0, 0));

            var transformAtFrames = parameters.Select(p => movedCrv.TransformAt(p));

            Model.AddElements(frames.SelectMany(f => f.ToModelCurves()));
            Model.AddElements(bFrames.SelectMany(f => f.ToModelCurves()));
            Model.AddElements(transformAtFrames.SelectMany(f => f.ToModelCurves()));
            Model.AddElement(curve);
            Model.AddElement(movedCrv);
            Model.AddElement(bezier);
        }

        [Fact]
        public void PolylineForceAngleCompliance()
        {
            var path = new List<Vector3>
            {
                new Vector3(0, 10, 2),
                new Vector3(0, 8, 2),
                new Vector3(4, 5, 2),
                new Vector3(4, 3, 2),
                new Vector3(1, 2, 2),
                new Vector3(1, 0, 2),
                new Vector3(0, 0, 2)
            };
            var polyline = new Polyline(path);
            var angles = new List<double> { 90, 45 };
            var normalizedPathStart = polyline.ForceAngleCompliance(angles, Vector3.ZAxis, out var distanceStart, NormalizationType.Start);
            var normalizedPathMiddle = polyline.ForceAngleCompliance(angles, Vector3.ZAxis, out var distanceMiddle, NormalizationType.Middle);
            var normalizedPathEnd = polyline.ForceAngleCompliance(angles, Vector3.ZAxis, out var distanceEnd, NormalizationType.End);

            Assert.True(CheckPolylineAngles(angles, normalizedPathStart));
            Assert.True(CheckPolylineAngles(angles, normalizedPathMiddle));
            Assert.True(CheckPolylineAngles(angles, normalizedPathEnd));

            // Get 45 degree between first, second and third segments. All other angles of the polyline are 90 degrees.
            var pathStartSegments = normalizedPathStart.Segments();
            Assert.True(pathStartSegments[1].Direction().AngleTo(pathStartSegments[0].Direction()).ApproximatelyEquals(45));
            Assert.True(pathStartSegments[2].Direction().AngleTo(pathStartSegments[1].Direction()).ApproximatelyEquals(45));
            Assert.Equal(new Vector3(0, 9, 2), pathStartSegments[1].Start);
            Assert.Equal(new Vector3(4, 5, 2), pathStartSegments[1].End);

            var pathMiddleSegments = normalizedPathMiddle.Segments();
            Assert.True(pathMiddleSegments[1].Direction().AngleTo(pathMiddleSegments[0].Direction()).ApproximatelyEquals(45));
            Assert.True(pathMiddleSegments[2].Direction().AngleTo(pathMiddleSegments[1].Direction()).ApproximatelyEquals(45));
            Assert.Equal(new Vector3(0, 8.5, 2), pathMiddleSegments[1].Start);
            Assert.Equal(new Vector3(4, 4.5, 2), pathMiddleSegments[1].End);

            var pathEndSegments = normalizedPathEnd.Segments();
            Assert.True(pathEndSegments[1].Direction().AngleTo(pathEndSegments[0].Direction()).ApproximatelyEquals(45));
            Assert.True(pathEndSegments[2].Direction().AngleTo(pathEndSegments[1].Direction()).ApproximatelyEquals(45));
            Assert.Equal(new Vector3(0, 8, 2), pathEndSegments[1].Start);
            Assert.Equal(new Vector3(4, 4, 2), pathEndSegments[1].End);

            // Check that the first vertex has not changed.
            Assert.Equal(polyline.Start, normalizedPathStart.Start);
            Assert.Equal(polyline.Start, normalizedPathMiddle.Start);
            Assert.Equal(polyline.Start, normalizedPathEnd.Start);
        }

        private static bool CheckPolylineAngles(List<double> supportedAngles, Polyline polyline)
        {
            for (var i = 1; i < polyline.Vertices.Count - 1; i++)
            {
                var a = polyline.Vertices[i - 1];
                var b = polyline.Vertices[i];
                var c = polyline.Vertices[i + 1];
                var angle = (b - a).AngleTo((c - b));
                var isAngleSupported = supportedAngles.Any(ang => angle.ApproximatelyEquals(ang, 0.1) || (angle - 90).ApproximatelyEquals(ang, 0.1));
                if (!angle.ApproximatelyEquals(180, 0.1)
                    && !angle.ApproximatelyEquals(0, 0.1)
                    && !isAngleSupported)
                {
                    return false;
                }
            }

            return true;
        }

        [Fact]
        public void PolylineForceAngleCompliance3dPath()
        {
            Name = nameof(PolylineForceAngleCompliance3dPath);
            var path = new List<Vector3>
            {
                new Vector3(2, 1, 2),
                new Vector3(2, 3, 2),
                new Vector3(4, 4.5, 2),
                new Vector3(4, 5, 3),
                new Vector3(4, 7, 3),
                new Vector3(7, 8, 3),
                new Vector3(7, 5, 3),
                new Vector3(7, 2, 5)
            };

            var polyline = new Polyline(path);
            var angles = new List<double> { 90, 45 };
            var normalizedPathStart = polyline.ForceAngleCompliance(angles, Vector3.ZAxis, NormalizationType.Start);
            var normalizedPathMiddle = polyline.ForceAngleCompliance(angles, Vector3.ZAxis, NormalizationType.Middle);
            var normalizedPathEnd = polyline.ForceAngleCompliance(angles, Vector3.ZAxis, NormalizationType.End);
            Model.AddElement(new ModelCurve(polyline, BuiltInMaterials.Black));
            Model.AddElement(new ModelCurve(normalizedPathStart, BuiltInMaterials.XAxis));
            Model.AddElement(new ModelCurve(normalizedPathMiddle, BuiltInMaterials.YAxis));
            Model.AddElement(new ModelCurve(normalizedPathEnd, BuiltInMaterials.ZAxis));

            Assert.True(CheckPolylineAngles(angles, normalizedPathStart));
            Assert.True(CheckPolylineAngles(angles, normalizedPathMiddle));
            Assert.True(CheckPolylineAngles(angles, normalizedPathEnd));
        }

        [Fact]
        public void PolylineForceAngleComplianceWithCollinearPoints()
        {
            Name = nameof(PolylineForceAngleComplianceWithCollinearPoints);
            var angles = new List<double> { 90, 45 };
            var polyline = new Polyline(new List<Vector3> { new Vector3(), new Vector3(1, 3), new Vector3(5, 3), new Vector3(10, 3) });

            var normalizedPathMiddle = polyline.ForceAngleCompliance(angles, Vector3.ZAxis, out var distanceMiddle, NormalizationType.Middle);
            var normalizedPathEnd = polyline.ForceAngleCompliance(angles, Vector3.ZAxis, out var distanceEnd, NormalizationType.End);
            var normalizedPathEndYAxisReferenceVector = polyline.ForceAngleCompliance(angles, new Vector3(0, 1), out var distanceStartYAxis, NormalizationType.End);

            Model.AddElement(new ModelCurve(polyline, BuiltInMaterials.Black));
            Model.AddElement(new ModelCurve(normalizedPathMiddle, BuiltInMaterials.XAxis));
            Model.AddElement(new ModelCurve(normalizedPathEnd, BuiltInMaterials.YAxis));
            Model.AddElement(new ModelCurve(normalizedPathEndYAxisReferenceVector, BuiltInMaterials.ZAxis));

            Assert.True(CheckPolylineAngles(angles, normalizedPathMiddle));
            // TODO This case is currently not working. Fix ForceAngleCompliance to handle this.
            Assert.False(CheckPolylineAngles(angles, normalizedPathEnd));
            Assert.True(CheckPolylineAngles(angles, normalizedPathEndYAxisReferenceVector));
        }


        [Fact]
        public void PolylineForceAngleComplianceWithZeroAngle()
        {
            Name = nameof(PolylineForceAngleComplianceWithZeroAngle);
            var angles = new List<double> { 0, 45, 90 };
            var polyline = new Polyline(new List<Vector3> { new Vector3(), new Vector3(1, 3), new Vector3(5, 3.05), new Vector3(10, 3) });

            var normalizedPathMiddle = polyline.ForceAngleCompliance(angles, Vector3.ZAxis, out var distanceMiddle, NormalizationType.Middle);
            var normalizedPathEnd = polyline.ForceAngleCompliance(angles, Vector3.ZAxis, out var distanceEnd, NormalizationType.End);
            var normalizedPathEndYAxisReferenceVector = polyline.ForceAngleCompliance(angles, new Vector3(0, 1), out var distanceStartYAxis, NormalizationType.End);

            Model.AddElement(new ModelCurve(polyline, BuiltInMaterials.Black));
            Model.AddElement(new ModelCurve(normalizedPathMiddle, BuiltInMaterials.XAxis));
            Model.AddElement(new ModelCurve(normalizedPathEnd, BuiltInMaterials.YAxis));
            Model.AddElement(new ModelCurve(normalizedPathEndYAxisReferenceVector, BuiltInMaterials.ZAxis));

            Assert.True(CheckPolylineAngles(angles, normalizedPathMiddle));
            Assert.True(CheckPolylineAngles(angles, normalizedPathEnd));
            Assert.True(CheckPolylineAngles(angles, normalizedPathEndYAxisReferenceVector));
        }

        [Fact]
        public void PolylineTransformAt()
        {
            var polyline = new Polyline((0, 0), (5, 0), (5, 10), (-5, 10));

            var transform = polyline.TransformAt(0);
            Assert.Equal((0, 0), transform.Origin);
            Assert.Equal(Vector3.XAxis.Negate(), transform.ZAxis);

            //transform = polyline.TransformAt(0.5);
            //Assert.Equal((2.5, 0), transform.Origin);
            //Assert.Equal(Vector3.XAxis.Negate(), transform.ZAxis);

            transform = polyline.TransformAt(1);
            Assert.Equal((5, 0), transform.Origin);
            Assert.Equal((Vector3.XAxis + Vector3.YAxis).Unitized().Negate(), transform.ZAxis);

            //transform = polyline.TransformAt(1.25);
            //Assert.Equal((5, 2.5), transform.Origin);
            //Assert.Equal(Vector3.YAxis.Negate(), transform.ZAxis);

            transform = polyline.TransformAt(2);
            Assert.Equal((5, 10), transform.Origin);
            Assert.Equal((Vector3.YAxis - Vector3.XAxis).Unitized().Negate(), transform.ZAxis);

            transform = polyline.TransformAt(2.75);
            Assert.Equal((-2.5, 10), transform.Origin);
            Assert.Equal(Vector3.XAxis, transform.ZAxis);
        }
    }
}