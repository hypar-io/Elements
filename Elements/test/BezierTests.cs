using Elements;
using Elements.Geometry;
using Elements.Tests;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Elements.Geometry.Tests
{
    public class BezierTests : ModelTest
    {
        ITestOutputHelper _output;

        public BezierTests(ITestOutputHelper output)
        {
            this._output = output;
            this.GenerateIfc = false;
        }

        [Fact, Trait("Category", "Examples")]
        public void Bezier()
        {
            this.Name = "Elements_Geometry_Bezier";

            // <example>
            var a = Vector3.Origin;
            var b = new Vector3(5, 0, 1);
            var c = new Vector3(5, 5, 2);
            var d = new Vector3(0, 5, 3);
            var e = new Vector3(0, 0, 4);
            var f = new Vector3(5, 0, 5);
            var ctrlPts = new List<Vector3> { a, b, c, d, e, f };

            var bezier = new Bezier(ctrlPts);
            // </example>

            this.Model.AddElement(new ModelCurve(bezier));
        }

        [Fact]
        public void Bezier_Length_ZeroLength()
        {
            var a = Vector3.Origin;
            var b = Vector3.Origin;
            var c = Vector3.Origin;
            var ctrlPts = new List<Vector3> { a, b, c };
            var bezier = new Bezier(ctrlPts);

            var targetLength = 0;
            Assert.Equal(targetLength, bezier.Length());
        }

        [Fact]
        public void Bezier_Length_OffsetFromOrigin()
        {
            var b = new Vector3(5, 0, 1);
            var c = new Vector3(5, 5, 2);
            var d = new Vector3(0, 5, 3);
            var e = new Vector3(0, 0, 4);
            var f = new Vector3(5, 0, 5);
            var ctrlPts = new List<Vector3> { b, c, d, e, f };
            var bezier = new Bezier(ctrlPts);

            var expectedLength = 11.85;  // approximation as the linear interpolation used for calculating length is not hugely accurate
            Assert.Equal(expectedLength, bezier.Length(), 2);
            var divisions = 500; // brittle as it relies on number of samples within Bezier being unchanged
            var polylineLength = bezier.ToPolyline(divisions).Length();
            Assert.Equal(polylineLength, bezier.Length());
        }

        [Fact]
        public void IntersectsLine()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0, 5);
            var c = new Vector3(5, 5);
            var d = new Vector3(5, 0);
            var ctrlPts = new List<Vector3> { a, b, c, d };
            var bezier = new Bezier(ctrlPts);

            var line = new Line(new Vector3(0, 2), new Vector3(5, 2));
            Assert.True(bezier.Intersects(line, out var results));
            Assert.Equal(2, results.Count);
            Assert.All(results, r => Assert.True(r.DistanceTo(line) < Vector3.EPSILON));

            line = new Line(new Vector3(5, 0, 5), new Vector3(5, 0, 0));
            Assert.True(bezier.Intersects(line, out results));
            Assert.Single(results);
            Assert.Contains(new Vector3(5, 0), results);

            line = new Line(new Vector3(5, 5), new Vector3(0, 5));
            Assert.False(bezier.Intersects(line, out results));
        }

        [Fact]
        public void IntersectsCircle()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0, 5);
            var c = new Vector3(5, 5);
            var d = new Vector3(5, 0);
            var ctrlPts = new List<Vector3> { a, b, c, d };
            var bezier = new Bezier(ctrlPts);

            var arc = new Arc(new Vector3(2.5, 2.5), 2.5, 180, 270);
            Assert.True(bezier.Intersects(arc, out var results));
            Assert.Single(results);

            Transform t = new Transform(new Vector3(2.5, 0), Vector3.YAxis);
            arc = new Arc(new Vector3(2.5, 0), 2.5, 0, -180);
            Assert.True(bezier.Intersects(arc, out results));
            Assert.Equal(2, results.Count);
            Assert.Contains(new Vector3(5, 0), results);
            Assert.Contains(new Vector3(0, 0), results);
        }

        [Fact]
        public void IntersectsEllipse()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0, 5);
            var c = new Vector3(5, 5);
            var d = new Vector3(5, 0);
            var ctrlPts = new List<Vector3> { a, b, c, d };
            var bezier = new Bezier(ctrlPts);

            var arc = new EllipticalArc(new Vector3(2.5, 2), 3.5, 1, 0, 270);
            Assert.True(bezier.Intersects(arc, out var results));
            Assert.Equal(3, results.Count);

            arc = new EllipticalArc(new Vector3(2.5, 0), 2.5, 2, 0, -180);
            Assert.True(bezier.Intersects(arc, out results));
            Assert.Equal(2, results.Count);
            Assert.Contains(new Vector3(5, 0), results);
            Assert.Contains(new Vector3(0, 0), results);
        }

        [Fact]
        public void IntersectsPolycurve()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0, 5);
            var c = new Vector3(5, 5);
            var d = new Vector3(5, 0);
            var ctrlPts = new List<Vector3> { a, b, c, d };
            var bezier = new Bezier(ctrlPts);

            var polygon = new Polygon(new Vector3[]{
                (0, 3), (6, 3), (4, 1), (-2, 1)
            }); 
            
            Assert.True(bezier.Intersects(polygon, out var results));
            Assert.Equal(4, results.Count);
            Assert.Contains(new Vector3(0.93475, 3), results);
            Assert.Contains(new Vector3(4.06525, 3), results);
            Assert.Contains(new Vector3(4.75132, 1.75133), results);
            Assert.Contains(new Vector3(0.07367, 1), results);
        }

        [Fact]
        public void IntersectsBezier()
        {
            var a = Vector3.Origin;
            var b = new Vector3(0, 5);
            var c = new Vector3(5, 5);
            var d = new Vector3(5, 0);
            var ctrlPts = new List<Vector3> { a, b, c, d };
            var bezier = new Bezier(ctrlPts);

            var other = new Bezier(new List<Vector3> { b, a, d, c });
            Assert.True(bezier.Intersects(other, out var results));
            Assert.Equal(2, results.Count);
            Assert.Contains(new Vector3(0.5755, 2.5), results);
            Assert.Contains(new Vector3(4.4245, 2.5), results);
        }
    }
}