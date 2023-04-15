using Elements;
using Elements.Geometry;
using Elements.Tests;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Hypar.Tests
{
    public class ArcTests : ModelTest
    {
        ITestOutputHelper _output;

        public ArcTests(ITestOutputHelper output)
        {
            this._output = output;
            this.GenerateIfc = false;
        }

        [Fact, Trait("Category", "Examples")]
        public void Arc()
        {
            this.Name = "Elements_Geometry_Arc";

            // <example>
            var arc = new Arc(Vector3.Origin, 2.0, 0.0, 90.0);
            Assert.True(new Vector3(2, 0, 0).IsAlmostEqualTo(arc.Start));
            Assert.True(new Vector3(0, 2, 0).IsAlmostEqualTo(arc.End));

            var arc1 = new Arc(Vector3.Origin, 2.0, 0.0, -90.0);
            Assert.True(new Vector3(2, 0, 0).IsAlmostEqualTo(arc1.Start));
            Assert.True(new Vector3(0, -2, 0).IsAlmostEqualTo(arc1.End));

            // A transformed arc.
            var arc2 = new Arc(new Transform(Vector3.Origin, Vector3.XAxis), 2.0, 0, Math.PI);
            // </example>

            this.Model.AddElement(new ModelCurve(arc, BuiltInMaterials.XAxis));
            this.Model.AddElement(new ModelCurve(arc1, BuiltInMaterials.YAxis));
            this.Model.AddElement(new ModelCurve(arc2, BuiltInMaterials.ZAxis));
        }

        [Fact]
        public void GetTransformsTransformedCurveSucceeds()
        {
            this.Name = nameof(GetTransformsTransformedCurveSucceeds);
            var arc = new Arc(new Transform(Vector3.Origin, Vector3.XAxis), 5, 0, Math.PI);
            var parameters = arc.GetSubdivisionParameters();
            foreach (var p in parameters)
            {
                var t = arc.TransformAt(p);
                this.Model.AddElements(t.ToModelCurves());
            }
            this.Model.AddElement(new ModelCurve(arc, BuiltInMaterials.ZAxis));
        }

        [Fact]
        public void GetSampleParametersReversedCurveSucceeds()
        {
            var arc = new Arc(Vector3.Origin, 2.0, 0.0, -90.0);
            var parameters = arc.GetSubdivisionParameters();
            foreach (var p in parameters)
            {
                arc.PointAt(p);
            }
        }

        [Fact]
        public void ZeroSweep_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => new Arc(Vector3.Origin, 2.0, 0.0, 0.0));
        }

        [Fact]
        public void ZeroRadius_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Arc(Vector3.Origin, 0.0, 0.0, 90.0));
        }

        [Fact]
        public void PointAt()
        {
            var arc = new Arc(Vector3.Origin, 5.0, 0.0, 90.0);
            Assert.Equal(new Vector3(0, 5, 0), arc.End);
            Assert.Equal(new Vector3(0, 5, 0), arc.PointAt(arc.Domain.Max));
            Assert.Equal(new Vector3(5 * Math.Cos(Math.PI / 4), 5 * Math.Sin(Math.PI / 4), 0), arc.PointAt(arc.Domain.Length / 2));
            Assert.Equal(new Vector3(5 * Math.Cos(Math.PI / 2), 5 * Math.Sin(Math.PI / 2), 0), arc.PointAt(arc.Domain.Max));

            arc = new Arc(Vector3.Origin, 5.0, 0.0, 180.0);
            Assert.Equal(new Vector3(-5, 0, 0), arc.PointAt(arc.Domain.Max));
            Assert.Equal(new Vector3(0, 5, 0), arc.PointAt(arc.Domain.Length / 2));
            Assert.Equal(new Vector3(5, 0, 0), arc.PointAt(arc.Domain.Min));
            Assert.Equal(new Vector3(5, 0, 0), arc.PointAt(arc.Domain.Min + -1e-15));
        }

        [Fact]
        public void TransformAt()
        {
            var arc = new Arc(Vector3.Origin, 5.0, 0.0, 180.0);
            var t = arc.TransformAt(Math.PI / 2);
            Assert.Equal(new Vector3(0, 1, 0), t.XAxis);
            Assert.Equal(new Vector3(0, 0, 1), t.YAxis);
            Assert.Equal(new Vector3(1, 0, 0), t.ZAxis);
        }

        [Fact]
        public void Frames()
        {
            var arc = new Arc(Vector3.Origin, 5.0, 0.0, 180.0);
            var frames = arc.Frames();

            var arc1 = new Arc(Vector3.Origin, 5.0, 0.0, 180.0);
            var frames1 = arc.Frames(0.1, 0.1);
        }

        [Fact]
        public void Complement()
        {
            var arc = new Arc(Vector3.Origin, 1, 10, 20);
            var comp = arc.Complement();
            Assert.Equal(-340, comp.StartAngle);
            Assert.Equal(10, comp.EndAngle);

            arc = new Arc(Vector3.Origin, 1, -10, 10);
            comp = arc.Complement();
            Assert.Equal(-350, comp.StartAngle);
            Assert.Equal(-10, comp.EndAngle);
        }

        [Fact]
        public void ToPolyline()
        {
            var arc = new Arc(Vector3.Origin, 1, 10, 20);
            var p = arc.ToPolyline(10);
            Assert.Equal(10, p.Segments().Length);
            Assert.Equal(arc.Start, p.Vertices[0]);
            Assert.Equal(arc.End, p.Vertices[p.Vertices.Count - 1]);
        }

        [Fact]
        public void ToPolygon()
        {
            var c = new Circle(Vector3.Origin, 1);
            var p = c.ToPolygon(10);
            Assert.Equal(10, p.Segments().Length);
            Assert.Equal(c.PointAt(0), p.Vertices[0]);
            Assert.Equal(c.PointAt(Math.PI * 2), p.Vertices[0]);
        }

        [Fact]
        public void Span()
        {
            var a = new Arc(1.0, 0, -360);
            var b = new Arc(1.0, 0, 360);
            var c = new Arc(1.0, -45.0, 45.0);
            Assert.Throws<ArgumentOutOfRangeException>(() => new Arc(1.0, 0, 361));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Arc(1.0, 0, -361));
        }

        [Fact]
        public void ArcByThreePoints()
        {
            Name = nameof(ArcByThreePoints);

            var a = new Vector3();
            var b = new Vector3(1.5, 2.0);
            var c = new Vector3(3, 5);
            VisualizeArcByThreePoints(a, b, c);

            var d = new Vector3(1, 2, 0);
            var e = new Vector3(2, 3, 1);
            var f = new Vector3(3, 5, 2);
            VisualizeArcByThreePoints(d, e, f);

            var g = new Vector3(1, 2, 0);
            var h = new Vector3(1, 3, 1);
            var i = new Vector3(1, 5, 2);
            VisualizeArcByThreePoints(g, h, i);
        }

        private void VisualizeArcByThreePoints(Vector3 a, Vector3 b, Vector3 c)
        {
            var arc = Elements.Geometry.Arc.ByThreePoints(a, b, c);
            var mc = new ModelCurve(arc);
            this.Model.AddElement(mc);
            this.Model.AddElement(new ModelCurve(arc.BasisCurve, BuiltInMaterials.YAxis));
            var pt1 = new Arc(a, 0.05, 0, 360);
            var pt2 = new Arc(b, 0.05, 0, 360);
            var pt3 = new Arc(c, 0.05, 0, 360);
            this.Model.AddElement(new ModelCurve(pt2, BuiltInMaterials.XAxis));
            this.Model.AddElement(new ModelCurve(pt1, BuiltInMaterials.XAxis));
            this.Model.AddElement(new ModelCurve(pt3, BuiltInMaterials.XAxis));
        }

        [Fact]
        public void ArcByThreePointsCoincidentThrows()
        {
            var a = new Vector3();
            var b = new Vector3();
            var c = new Vector3(3, 5);
            Elements.Geometry.Arc.ByThreePoints(a, b, c);
            Assert.Throws<ArgumentException>(() => Elements.Geometry.Arc.ByThreePoints(a, b, c));
        }

        [Fact]
        public void ArcByThreePointsColinearThrows()
        {
            var a = new Vector3();
            var b = new Vector3(1, 0);
            var c = new Vector3(2, 0);
            Elements.Geometry.Arc.ByThreePoints(a, b, c);
            Assert.Throws<ArgumentException>(() => Elements.Geometry.Arc.ByThreePoints(a, b, c));
        }

        [Fact]
        public void Fillet()
        {
            Name = (nameof(Fillet));

            var a = new Line(new Vector3(-3, 0), new Vector3(-1, 3, 1));
            var b = new Line(new Vector3(3, 0), new Vector3(1, 3, 1));
            var c = new Line(new Vector3(-3, 0), new Vector3(-1, -3, 1));
            var d = new Line(new Vector3(3, 0), new Vector3(1, -3, 1));

            // this.Model.AddElements(new[] { new ModelCurve(a, BuiltInMaterials.XAxis), new ModelCurve(b, BuiltInMaterials.XAxis), new ModelCurve(c, BuiltInMaterials.ZAxis), new ModelCurve(d, BuiltInMaterials.ZAxis) });

            var profile = new Circle(0.05).ToPolygon(20);
            var arc1 = Elements.Geometry.Arc.Fillet(a, b, 2);
            this.Model.AddElement(new ModelCurve(arc1, BuiltInMaterials.XAxis));
            this.Model.AddElement(new Beam(arc1, profile));

            var arc2 = Elements.Geometry.Arc.Fillet(c, d, 2);
            this.Model.AddElements(new[] { new ModelCurve(arc2, BuiltInMaterials.ZAxis), });
            this.Model.AddElement(new Beam(arc2, profile));

            var arc3 = Elements.Geometry.Arc.Fillet(a, c, 2);
            this.Model.AddElements(new ModelCurve(arc3, BuiltInMaterials.YAxis));
            this.Model.AddElement(new Beam(arc3, profile));

            var arc4 = Elements.Geometry.Arc.Fillet(b, d, 2);
            this.Model.AddElements(new ModelCurve(arc4, BuiltInMaterials.YAxis));
            this.Model.AddElement(new Beam(arc4, profile));

            var arcs = new[] { arc1, arc2, arc3, arc4 };
            var allPoints = arcs.SelectMany(a => new[] { a.Start, a.End }).ToList();

            // A brute forces search for end to end arcs.
            foreach (var arc in arcs)
            {
                var start = arc.End;
                var minDistance = double.MaxValue;
                Vector3 found = Vector3.Origin;
                foreach (var testArc in arcs)
                {
                    if (testArc.Equals(arc))
                    {
                        continue;
                    }

                    var distanceToEnd = testArc.End.DistanceTo(start);
                    if (distanceToEnd < minDistance)
                    {
                        minDistance = distanceToEnd;
                        found = testArc.End;
                    }

                    var distanceToStart = testArc.Start.DistanceTo(start);
                    if (distanceToStart < minDistance)
                    {
                        minDistance = distanceToStart;
                        found = testArc.Start;
                    }
                }
                allPoints.Remove(found);
                allPoints.Remove(arc.End);

                var beam = new Beam(new Line(start, found), profile);
                this.Model.AddElement(beam);
            }
            var lastBeam = new Beam(new Line(allPoints[0], allPoints[1]), profile);
            this.Model.AddElement(lastBeam);
        }
    }
}