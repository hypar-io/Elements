using System.Collections.Generic;
using System.Linq;
using Elements.Tests;
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
            Assert.Equal(1, polyline.GetParameterAt(end));
            Assert.Equal(-1, polyline.GetParameterAt(Vector3.Origin));

            var point = new Vector3(2, 4);
            var result = polyline.GetParameterAt(point);
            var expectedResult = 0.75d;
            Assert.True(result.ApproximatelyEquals(expectedResult));
            Assert.True(point.IsAlmostEqualTo(polyline.PointAt(result)));
        }
    }
}