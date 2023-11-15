using System.Collections.Generic;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class TransformTests : ModelTest
    {
        [Fact, Trait("Category", "Examples")]
        public void Example()
        {
            this.Name = "Elements_Geometry_Transform";
            // <example>
            var m1 = new Mass(Polygon.Rectangle(1.0, 1.0), 1.0, new Material("yellow", Colors.Yellow));
            this.Model.AddElement(m1);

            Profile prof = Polygon.Rectangle(1.0, 1.0);

            var j = 1.0;
            var count = 10;
            for (var i = 0.0; i < 360.0; i += 360.0 / (double)count)
            {
                var m2 = new Mass(prof, 1.0, new Material($"color_{j}", new Color((float)j - 1.0f, 0.0f, 0.0f, 1.0f)), new Transform());

                // Scale the mass.
                m2.Transform.Scale(new Vector3(j, j, j));

                // Move the mass.
                m2.Transform.Move(new Vector3(3, 0, 0));

                // Rotate the mass.
                m2.Transform.Rotate(Vector3.ZAxis, i);
                this.Model.AddElement(m2);
                j += 1.0 / (double)count;
            }
            // </example>
        }

        [Fact]
        public void Transform_OfPoint()
        {
            var o = new Vector3(1, 1, 0);
            var t = new Transform(o, Vector3.XAxis, Vector3.YAxis.Negate());
            var v = new Vector3(0.5, 0.5, 0.0);
            var vt = t.OfPoint(v);
            Assert.Equal(1.5, vt.X);
            Assert.Equal(1.0, vt.Y);
            Assert.Equal(0.5, vt.Z);
        }

        [Fact]
        public void Transform_OfVector()
        {
            var o = new Vector3(1, 1, 0);
            var t = new Transform(o, Vector3.XAxis, Vector3.YAxis.Negate());
            var v = new Vector3(0.5, 0.5, 0.0);
            var vt = t.OfVector(v);
            Assert.Equal(0.5, vt.X);
            Assert.Equal(0.0, vt.Y);
            Assert.Equal(0.5, vt.Z);
        }

        [Fact]
        public void Transform_OfPlane()
        {
            var o = new Vector3(1, 1, 0);
            var t = new Transform(o, Vector3.XAxis, Vector3.YAxis.Negate());
            var p = new Plane(new Vector3(0.5, 0.5, 0.0), new Vector3(0, 0, 1));
            var pt = t.OfPlane(p);
            Assert.Equal(1.5, pt.Origin.X);
            Assert.Equal(1.0, pt.Origin.Y);
            Assert.Equal(0.5, pt.Origin.Z);
            Assert.Equal(0.0, pt.Normal.X);
            Assert.Equal(-1.0, pt.Normal.Y);
            Assert.Equal(0.0, pt.Normal.Z);
        }

        [Fact]
        public void Involutary_Transform_Inverse()
        {
            // our library should handle involutary matrices well
            // https://en.wikipedia.org/wiki/Involutory_matrix 
            var origin = new Vector3(0, 0);
            var xAxis = new Vector3(-1, 0, 0);
            var zAxis = new Vector3(0, 1, 0);
            var transform = new Transform(origin, xAxis, zAxis);

            var inverted = new Transform(transform);
            inverted.Invert();

            Assert.Equal(transform, inverted);
        }

        [Fact]
        public void Transform_Inverted()
        {
            var polygon = Polygon.Rectangle(3, 5);
            var transform = new Transform((4, 3), 72);
            var pgonTransformed = polygon.TransformedPolygon(transform);
            var pgonTransformedBack = pgonTransformed.TransformedPolygon(transform.Inverted());
            for (int i = 0; i < pgonTransformedBack.Vertices.Count; i++)
            {
                var vertex = pgonTransformedBack.Vertices[i];
                Assert.True(vertex.IsAlmostEqualTo(polygon.Vertices[i]));
            }
        }


        [Fact]
        public void Transform_ScaleAboutPoint()
        {
            var transformOrigin = new Vector3(10, -5, 4);
            var pointToTransform = new Vector3(2, 9, 18);
            var scaleFactor = 2.5;
            var t = new Transform();
            t.Scale(scaleFactor, transformOrigin);
            var vt = t.OfPoint(pointToTransform);
            Assert.Equal(-10, vt.X);
            Assert.Equal(30, vt.Y);
            Assert.Equal(39, vt.Z);
        }

        [Fact]
        public void Transform_Translate()
        {
            var t = new Transform(new Vector3(5, 0, 0), Vector3.XAxis, Vector3.YAxis.Negate());
            var v = new Vector3(0.5, 0.5, 0.0);
            var vt = t.OfPoint(v);
            Assert.Equal(5.5, vt.X);
            Assert.Equal(0.0, vt.Y);
            Assert.Equal(0.5, vt.Z);
        }

        [Fact]
        public void TransformFromUp()
        {
            var t = new Transform(Vector3.Origin, Vector3.ZAxis);
            Assert.Equal(Vector3.XAxis, t.XAxis);
            Assert.Equal(Vector3.YAxis, t.YAxis);

            t = new Transform(Vector3.Origin, Vector3.XAxis);
            Assert.Equal(Vector3.YAxis, t.XAxis);
        }

        [Fact]
        public void OrientAlongCurve()
        {
            this.Name = "TransformsOrientedAlongCurve";
            var arc = new Arc(Vector3.Origin, 10.0, 45.0, 135.0);
            for (var i = arc.Domain.Min; i <= arc.Domain.Max; i += arc.Domain.Length / 10)
            {
                var t = Elements.Geometry.Transform.CreateHorizontalFrameAlongCurve(arc, i);
                var m = new Mass(Polygon.Rectangle(1.0, 1.0), 0.5, transform: t);
                this.Model.AddElement(m);
                this.Model.AddElements(t.ToModelCurves());
            }
        }

        [Fact]
        public void HorizontalFrameAlongCurve()
        {
            Name = nameof(HorizontalFrameAlongCurve);

            var nonXYPolygon = new Polygon(
                (0, 0),
                (5, 0, 5),
                (5, 5, 5),
                (0, 5, 0)
            );
            for (var t = 0.0; t <= 1.0; t += 0.1)
            {
                var frame = Elements.Geometry.Transform.CreateHorizontalFrameAlongCurve(nonXYPolygon, t);
                var m = new Mass(Polygon.Rectangle(1.0, 1.0), 0.5, transform: frame);
                this.Model.AddElement(m);
                this.Model.AddElement(nonXYPolygon);
            }

        }

        [Fact]
        public void AllTransformsRunInTheSameDirection()
        {
            this.Name = "AllTransformsRunInTheSameDirection";

            var curves = new List<BoundedCurve>();

            var line = new Line(Vector3.Origin, new Vector3(1, 2, 5));
            curves.Add(line);

            var t = new Transform();
            t.Move(new Vector3(3, 0, 0));
            t.Rotate(Vector3.XAxis, 45);
            var l = Polygon.L(5, 3, 1);
            l = (Polygon)l.Transformed(t);
            curves.Add(l);

            var bez = new Bezier(new List<Vector3>(){
                new Vector3(10, 0, 0),
                new Vector3(10, 15, 5),
                new Vector3(10, 0, 10),
                new Vector3(10, 15, 15)
            }, FrameType.RoadLike);
            curves.Add(bez);

            var lp = Polygon.L(1, 1.5, 0.1);
            foreach (var curve in curves)
            {
                this.Model.AddElement(new Beam(curve, lp));
                this.Model.AddElement(new ModelCurve(curve));
                Transform last = null;
                for (var i = 0.0; i <= 1.0; i += 0.1)
                {
                    var tu = curve.TransformAt(i);
                    this.Model.AddElements(tu.ToModelCurves());
                    if (last == null)
                    {
                        last = tu;
                        continue;
                    }
                    // This test ensures that from one transform to
                    // the next, there is not a sudden flip in the
                    // Y vector.
                    Assert.True(tu.YAxis.Dot(last.YAxis) > 0);
                    last = tu;
                }
            }

            var centerLine = new Line(new Vector3(15, 0, -1), new Vector3(15, 0, 10));
            var upT = centerLine.TransformAt(0);
            this.Model.AddElements(upT.ToModelCurves());

            var beam = new Beam(centerLine, Polygon.Rectangle(0.1, 0.1));
            this.Model.AddElement(beam);

            var centerLineRev = centerLine.Reversed().TransformedLine(new Transform(new Vector3(2, 0, 0)));
            var downT = centerLineRev.TransformAt(0);
            this.Model.AddElements(downT.ToModelCurves());

            var beamDown = new Beam(centerLineRev, Polygon.Rectangle(0.1, 0.1));
            this.Model.AddElement(beamDown);

            Assert.Equal(upT.YAxis, downT.YAxis.Negate());
        }

        [Fact]
        public void Transform_RotateAboutPoint()
        {
            Name = nameof(Transform_RotateAboutPoint);
            var worldOrigin = new Transform();
            Model.AddElements(worldOrigin.ToModelCurves());
            var rectangle = Polygon.Rectangle((3, 4), (5, 8));
            var center = rectangle.Centroid();
            var rectangleRotatedAboutCenter = rectangle.TransformedPolygon(new Transform().RotatedAboutPoint(center, Vector3.ZAxis, 90));
            Model.AddElement(rectangle);
            Model.AddElement(rectangleRotatedAboutCenter);
            Assert.Equal(center, rectangleRotatedAboutCenter.Centroid());
        }
    }

}