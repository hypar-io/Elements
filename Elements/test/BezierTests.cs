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

        public void Bezier_ArcLength()
        {
            var a = new Vector3(50, 150, 0);
            var b = new Vector3(105, 66, 0);
            var c = new Vector3(170, 230, 0);
            var d = new Vector3(200, 150, 0);
            var ctrlPts = new List<Vector3> { a, b, c, d };
            var bezier = new Bezier(ctrlPts);

            var expectedLength = 184.38886379602502;  // approximation as the integral function used for calculating length is not 100% accurate
            Assert.Equal(expectedLength, bezier.ArcLength(0.0, 1.0), 2);
        }

        [Fact]
        public void GetParameterAt()
        {
            var tolerance = 0.00001;

            var a = new Vector3(1, 5, 0);
            var b = new Vector3(5, 20, 0);
            var c = new Vector3(5, -10, 0);
            var d = new Vector3(9, 5, 0);
            var ctrlPts = new List<Vector3> { a, b, c, d };
            var bezier = new Bezier(ctrlPts);

            var samplePt = new Vector3(0, 0, 0);
            Assert.Null(bezier.ParameterAt(samplePt, tolerance));

            samplePt = new Vector3(6.625, 0.7812, 0.0);
            Assert.True((double)bezier.ParameterAt(samplePt, tolerance) - 0.75 <= tolerance * 10);
        }

        [Fact]
        public void GetPointAt()
        {
            var a = new Vector3(1, 5, 0);
            var b = new Vector3(5, 20, 0);
            var c = new Vector3(5, -10, 0);
            var d = new Vector3(9, 5, 0);
            var ctrlPts = new List<Vector3> { a, b, c, d };
            var bezier = new Bezier(ctrlPts);

            var testPt = new Vector3(3.3750, 9.21875, 0.0000);
            Assert.True(testPt.Equals(bezier.PointAt(0.25)));

            testPt = new Vector3(4.699, 6.11375, 0.0000);
            Assert.True(testPt.Equals(bezier.PointAtNormalized(0.45)));

            testPt = new Vector3(4.515904, 6.75392, 0.0000);
            Assert.True(testPt.Equals(bezier.PointAtLength(8.0)));

            testPt = new Vector3(3.048823, 9.329262, 0.0000);
            Assert.True(testPt.Equals(bezier.PointAtNormalizedLength(0.25)));
        }

        [Fact]
        public void DivideByLength()
        {
            var a = new Vector3(50, 150, 0);
            var b = new Vector3(105, 66, 0);
            var c = new Vector3(170, 230, 0);
            var d = new Vector3(200, 150, 0);
            var ctrlPts = new List<Vector3> { a, b, c, d };
            var bezier = new Bezier(ctrlPts);

            var testPts = new List<Vector3>(){
                new Vector3(50.00, 150.00, 0.00),
                new Vector3(90.705919, 125.572992, 0.00),
                new Vector3(134.607122, 148.677130, 0.00),
                new Vector3(177.600231, 172.675064, 0.00),
                new Vector3(200.00, 150.00, 0.00),
            };

            var ptsFromBezier = bezier.DivideByLength(50.0);

            for (int i = 0; i < ptsFromBezier.Length; i++)
            {
                Assert.True(ptsFromBezier[i].Equals(testPts[i]));
            }
        }

        [Fact]
        public void Split()
        {
            var a = new Vector3(50, 150, 0);
            var b = new Vector3(105, 66, 0);
            var c = new Vector3(170, 230, 0);
            var d = new Vector3(200, 150, 0);
            var ctrlPts = new List<Vector3> { a, b, c, d };
            var bezier = new Bezier(ctrlPts);

            var testBeziers = new List<Bezier>(){
                new Bezier(new List<Vector3>() {
                    new Vector3(50.00, 150.00, 0.00),
                    new Vector3(63.75, 129.00, 0.00),
                    new Vector3(78.1250, 123.50, 0.00),
                    new Vector3(92.421875, 125.8125, 0.00)
                    }
                ),
                new Bezier(new List<Vector3>() {
                    new Vector3(92.421875, 125.8125, 0.00),
                    new Vector3(121.015625, 130.4375, 0.00),
                    new Vector3(149.296875, 166.3125, 0.00),
                    new Vector3(171.640625, 171.9375, 0.00)
                    }
                ),
                new Bezier(new List<Vector3>() {
                    new Vector3(171.640625, 171.9375, 0.00),
                    new Vector3(182.8125, 174.7500, 0.00),
                    new Vector3(192.50, 170.00, 0.00),
                    new Vector3(200.00, 150.00, 0.00)
                    }
                ),
            };

            var beziers = bezier.Split(new List<double>() { 0.25, 0.75 });

            for (int i = 0; i < beziers.Count; i++)
            {
                Assert.True(beziers[i].ControlPoints[0].Equals(testBeziers[i].ControlPoints[0]));
                Assert.True(beziers[i].ControlPoints[1].Equals(testBeziers[i].ControlPoints[1]));
                Assert.True(beziers[i].ControlPoints[2].Equals(testBeziers[i].ControlPoints[2]));
                Assert.True(beziers[i].ControlPoints[3].Equals(testBeziers[i].ControlPoints[3]));
            }

            testBeziers = new List<Bezier>(){
                new Bezier(new List<Vector3>() {
                    new Vector3(50.00, 150.00, 0.00),
                    new Vector3(61.88, 131.856, 0.00),
                    new Vector3(74.22656, 125.282688, 0.00),
                    new Vector3(86.586183, 125.321837, 0.00)
                    }
                ),
                new Bezier(new List<Vector3>() {
                    new Vector3(86.586183, 125.321837, 0.00),
                    new Vector3(114.738659, 125.411011, 0.00),
                    new Vector3(142.958913, 159.807432, 0.00),
                    new Vector3(165.887648, 169.916119, 0.00)
                    }
                ),
                new Bezier(new List<Vector3>() {
                    new Vector3(165.887648, 169.916119, 0.00),
                    new Vector3(179.49576, 175.915584, 0.00),
                    new Vector3(191.24, 173.36, 0.00),
                    new Vector3(200.00, 150.00, 0.00)
                    }
                ),
            };

            var normalizedBeziers = bezier.Split(new List<double>() { 0.25, 0.75 }, true);

            for (int i = 0; i < normalizedBeziers.Count; i++)
            {
                Assert.True(normalizedBeziers[i].ControlPoints[0].Equals(testBeziers[i].ControlPoints[0]));
                Assert.True(normalizedBeziers[i].ControlPoints[1].Equals(testBeziers[i].ControlPoints[1]));
                Assert.True(normalizedBeziers[i].ControlPoints[2].Equals(testBeziers[i].ControlPoints[2]));
                Assert.True(normalizedBeziers[i].ControlPoints[3].Equals(testBeziers[i].ControlPoints[3]));
            }
>>>>>>> 374fe346(Polish Bezier, IHasCurveLength)
        }
    }
}