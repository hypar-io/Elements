using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using Elements.Tests;
using Elements.Serialization.glTF;

namespace Elements.Geometry.Tests
{
    public class PolygonTests : ModelTest
    {
        private readonly ITestOutputHelper _output;

        public PolygonTests(ITestOutputHelper output)
        {
            this._output = output;
            this.GenerateIfc = false;
        }

        [Fact, Trait("Category", "Examples")]
        public void PolygonConstruct()
        {
            this.Name = "Elements_Geometry_Polygon";

            // <example>
            // Create a polygon.
            var star = Polygon.Star(5, 3, 5);
            // </example>

            this.Model.AddElement(new ModelCurve(star));
        }

        [Fact]
        public void Centroid()
        {
            // Square in Quadrant I
            var polygon = new Polygon
            (
                new[]
                {
                    Vector3.Origin,
                    new Vector3(6.0, 0.0),
                    new Vector3(6.0, 6.0),
                    new Vector3(0.0, 6.0),
                }
            );
            var centroid = polygon.Centroid();
            Assert.Equal(3.0, centroid.X);
            Assert.Equal(3.0, centroid.Y);

            // Square in Quadrant II
            polygon = new Polygon
            (
                new[]
                {
                    Vector3.Origin,
                    new Vector3(-6.0, 0.0),
                    new Vector3(-6.0, 6.0),
                    new Vector3(0.0, 6.0),
                }
            );
            centroid = polygon.Centroid();
            Assert.Equal(-3.0, centroid.X);
            Assert.Equal(3.0, centroid.Y);

            // Square in Quadrant III
            polygon = new Polygon
            (
                new[]
                {
                    Vector3.Origin,
                    new Vector3(-6.0, 0.0),
                    new Vector3(-6.0, -6.0),
                    new Vector3(0.0, -6.0),
                }
            );
            centroid = polygon.Centroid();
            Assert.Equal(-3.0, centroid.X);
            Assert.Equal(-3.0, centroid.Y);

            // Square in Quadrant IV
            polygon = new Polygon
            (
                new[]
                {
                    Vector3.Origin,
                    new Vector3(6.0, 0.0),
                    new Vector3(6.0, -6.0),
                    new Vector3(0.0, -6.0),
                }
            );
            centroid = polygon.Centroid();
            Assert.Equal(3.0, centroid.X);
            Assert.Equal(-3.0, centroid.Y);

            // Bow Tie in Quadrant I
            polygon = new Polygon
            (
                new[]
                {
                    new Vector3(1.0, 1.0),
                    new Vector3(4.0, 4.0),
                    new Vector3(7.0, 1.0),
                    new Vector3(7.0, 9.0),
                    new Vector3(4.0, 6.0),
                    new Vector3(1.0, 9.0)
                }
            );
            centroid = polygon.Centroid();
            Assert.Equal(4.0, centroid.X);
            Assert.Equal(5.0, centroid.Y);

            // Bow Tie in Quadrant III
            polygon = new Polygon
            (
                new[]
                {
                    new Vector3(-1.0, -1.0),
                    new Vector3(-4.0, -4.0),
                    new Vector3(-7.0, -1.0),
                    new Vector3(-7.0, -9.0),
                    new Vector3(-4.0, -6.0),
                    new Vector3(-1.0, -9.0)
                }
            );
            centroid = polygon.Centroid();
            Assert.Equal(-4.0, centroid.X);
            Assert.Equal(-5.0, centroid.Y);
        }

        [Fact]
        public void Contains()
        {
            var v1 = new Vector3();
            var v2 = new Vector3(7.5, 7.5);
            var p1 = new Polygon
            (
                new[]
                {
                    new Vector3(0.0, 0.0),
                    new Vector3(20.0, 0.0),
                    new Vector3(20.0, 20.0),
                    new Vector3(0.0, 20.0)
                }
            );
            var p2 = new Polygon
            (
                new[]
                {
                    new Vector3(0.0, 0.0),
                    new Vector3(10.0, 5.0),
                    new Vector3(10.0, 10.0),
                    new Vector3(5.0, 10.0)
                }
            );
            var p3 = new Polygon
            (
                new[]
                {
                    new Vector3(5.0, 5.0),
                    new Vector3(10.0, 5.0),
                    new Vector3(10.0, 10.0),
                    new Vector3(5.0, 10.0)
                }
            );

            Assert.False(p1.Contains(v1));
            Assert.True(p1.Contains(v2));
            Assert.False(p1.Contains(p2));
            Assert.True(p1.Contains(p3));
            Assert.False(p3.Contains(p1));
        }

        [Fact]
        public void Covers()
        {
            var v1 = new Vector3();
            var v2 = new Vector3(7.5, 7.5);
            var p1 = new Polygon
            (
                new[]
                {
                new Vector3(0.0, 0.0),
                new Vector3(20.0, 0.0),
                new Vector3(20.0, 20.0),
                new Vector3(0.0, 20.0)
                }
            );
            var p2 = new Polygon
            (
                new[]
                {
                new Vector3(0.0, 0.0),
                new Vector3(10.0, 5.0),
                new Vector3(10.0, 10.0),
                new Vector3(5.0, 10.0)
                }
            );
            var p3 = new Polygon
            (
                new[]
                {
                new Vector3(5.0, 5.0),
                new Vector3(10.0, 5.0),
                new Vector3(10.0, 10.0),
                new Vector3(5.0, 10.0)
                }
            );
            Assert.True(p1.Covers(v1));
            Assert.True(p1.Covers(p2.Reversed()));
            Assert.True(p3.Covers(v2));
            Assert.False(p3.Covers(v1));
            Assert.True(p1.Covers(p3));
            Assert.True(p1.Covers(p2));
            Assert.False(p3.Covers(p1));
        }

        [Fact]
        public void Disjoint()
        {
            var v1 = new Vector3();
            var v2 = new Vector3(27.5, 27.5);
            var p1 = new Polygon
            (
                new[]
                {
                new Vector3(0.0, 0.0),
                new Vector3(20.0, 0.0),
                new Vector3(20.0, 20.0),
                new Vector3(0.0, 20.0)
                }
            );
            var p2 = new Polygon
            (
                new[]
                {
                new Vector3(0.0, 0.0),
                new Vector3(10.0, 5.0),
                new Vector3(10.0, 10.0),
                new Vector3(5.0, 10.0)
                }
            );
            var p3 = new Polygon
            (
                new[]
                {
                new Vector3(25.0, 25.0),
                new Vector3(210.0, 25.0),
                new Vector3(210.0, 210.0),
                new Vector3(25.0, 210.0)
                }
            );
            var ps = new List<Polygon> { p2, p3 };

            Assert.True(p1.Disjoint(v2));
            Assert.False(p1.Disjoint(v1));
            Assert.True(p1.Disjoint(p3));
            Assert.False(p1.Disjoint(p2));
        }

        [Fact]
        public void Intersects()
        {
            var p1 = new Polygon
            (
                new[]
                {
                new Vector3(0.0, 0.0),
                new Vector3(20.0, 0.0),
                new Vector3(20.0, 20.0),
                new Vector3(0.0, 20.0)
                }
            );
            var p2 = new Polygon
            (
                new[]
                {
                new Vector3(0.0, 0.0),
                new Vector3(10.0, 5.0),
                new Vector3(10.0, 10.0),
                new Vector3(5.0, 10.0)
                }
            );
            var p3 = new Polygon
            (
                new[]
                {
                new Vector3(25.0, 25.0),
                new Vector3(210.0, 25.0),
                new Vector3(210.0, 210.0),
                new Vector3(25.0, 210.0)
                }
            );

            Assert.True(p1.Intersects(p2));
            Assert.False(p1.Intersects(p3));
        }

        [Fact]
        public void Touches()
        {
            var p1 = new Polygon
            (
                new[]
                {
                    new Vector3(),
                    new Vector3(4.0, 0.0),
                    new Vector3(4.0, 4.0),
                    new Vector3(0.0, 4.0)
                }
            );
            var p2 = new Polygon
            (
                new[]
                {
                    new Vector3(0.0, 1.0),
                    new Vector3(7.0, 1.0),
                    new Vector3(7.0, 2.0),
                    new Vector3(0.0, 2.0)
                }
            );
            var p3 = new Polygon
            (
                new[]
                {
                    new Vector3(),
                    new Vector3(4.0, 0.0),
                    new Vector3(4.0, 4.0),
                    new Vector3(0.0, 4.0)
                }
            );
            var p4 = new Polygon
            (
                new[]
                {
                    new Vector3(4.0, 0.0),
                    new Vector3(8.0, 0.0),
                    new Vector3(8.0, 4.0),
                    new Vector3(4.0, 8.0)
                }
            );
            Assert.False(p1.Touches(p2));
            Assert.True(p3.Touches(p4));
        }

        [Fact]
        public void Difference()
        {
            var p1 = new Polygon
            (
                new[]
                {
                    new Vector3(),
                    new Vector3(4, 0),
                    new Vector3(4, 4),
                    new Vector3(0, 4)
                }
            );
            var p2 = new Polygon
            (
                new[]
                {
                    new Vector3(3, 1),
                    new Vector3(7, 1),
                    new Vector3(7, 5),
                    new Vector3(3, 5)
                }
            );
            var vertices = p1.Difference(p2).First().Vertices;

            Assert.Contains(vertices, p => p.IsAlmostEqualTo(0.0, 0.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(4.0, 0.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(4.0, 1.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(3.0, 1.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(3.0, 4.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(0.0, 4.0));
        }

        [Fact]
        public void Intersection()
        {
            var p1 = new Polygon
            (
                new[]
                {
                    new Vector3(),
                    new Vector3(4.0, 0.0),
                    new Vector3(4.0, 4.0),
                    new Vector3(0.0, 4.0)
                }
            );
            var p2 = new Polygon
            (
                new[]
                {
                    new Vector3(3.0, 1.0),
                    new Vector3(7.0, 1.0),
                    new Vector3(7.0, 5.0),
                    new Vector3(3.0, 5.0)
                }
            );
            var p3 = new Polygon
            (
                new[]
                {
                    new Vector3(3.0, 2.0),
                    new Vector3(6.0, 2.0),
                    new Vector3(6.0, 3.0),
                    new Vector3(3.0, 3.0)
                }
            );
            var ps = new List<Polygon> { p2, p3 };
            var vertices = p1.Intersection(p2).First().Vertices;

            Assert.Contains(vertices, p => p.IsAlmostEqualTo(3.0, 1.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(4.0, 1.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(4.0, 4.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(3.0, 4.0));
        }

        [Fact]
        public void Union()
        {
            var p1 = new Polygon
            (
                new[]
                {
                    new Vector3(),
                    new Vector3(4.0, 0.0),
                    new Vector3(4.0, 4.0),
                    new Vector3(0.0, 4.0)
                }
            );
            var p2 = new Polygon
            (
                new[]
                {
                    new Vector3(3.0, 1.0),
                    new Vector3(7.0, 1.0),
                    new Vector3(7.0, 5.0),
                    new Vector3(3.0, 5.0)
                }
            );
            var p3 = new Polygon
            (
                new[]
                {
                    new Vector3(3.0, 2.0),
                    new Vector3(8.0, 2.0),
                    new Vector3(8.0, 3.0),
                    new Vector3(3.0, 3.0)
                }
            );
            var ps = new List<Polygon> { p2, p3 };

            var vertices = p1.Union(p2).Vertices;

            Assert.Contains(vertices, p => p.IsAlmostEqualTo(0.0, 0.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(4.0, 0.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(4.0, 1.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(7.0, 1.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(7.0, 5.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(3.0, 5.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(3.0, 4.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(0.0, 4.0));

            vertices = p1.Union(ps).Vertices;

            Assert.Contains(vertices, p => p.IsAlmostEqualTo(0.0, 0.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(4.0, 0.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(4.0, 1.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(7.0, 1.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(7.0, 2.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(8.0, 2.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(8.0, 3.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(7.0, 3.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(7.0, 5.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(3.0, 5.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(3.0, 4.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(0.0, 4.0));

        }

        [Fact]
        public void XOR()
        {
            var p1 = new Polygon
            (
                new[]
                {
                    new Vector3(),
                    new Vector3(4, 0),
                    new Vector3(4, 4),
                    new Vector3(0, 4)
                }
            );
            var p2 = new Polygon
            (
                new[]
                {
                    new Vector3(3, 1),
                    new Vector3(7, 1),
                    new Vector3(7, 5),
                    new Vector3(3, 5)
                }
            );
            var vertices = p1.XOR(p2).First().Vertices;

            Assert.Contains(vertices, p => p.IsAlmostEqualTo(4.0, 1.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(7.0, 1.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(7.0, 5.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(3.0, 5.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(3.0, 4.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(0.0, 4.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(0.0, 0.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(4.0, 0.0));
        }

        [Fact]
        public void Offset()
        {
            var a = new Vector3();
            var b = new Vector3(2, 5);
            var c = new Vector3(-3, 5);

            var plinew = new Polygon(new[] { a, b, c });
            var offset = plinew.Offset(0.2);

            Assert.True(offset.Length == 1);
        }

        [Fact]
        public void TwoPeaks__Offset_2Polylines()
        {
            var a = new Vector3();
            var b = new Vector3(5, 0);
            var c = new Vector3(5, 5);
            var d = new Vector3(0, 1);
            var e = new Vector3(-5, 5);
            var f = new Vector3(-5, 0);

            var plinew = new Polygon(new[] { a, b, c, d, e, f });
            var offset = plinew.Offset(-0.5);
            Assert.Equal(2, offset.Count());
        }

        [Fact]
        public void Construct()
        {
            var a = new Vector3();
            var b = new Vector3(1, 0);
            var c = new Vector3(1, 1);
            var d = new Vector3(0, 1);
            var p = new Polygon(new[] { a, b, c, d });
            Assert.Equal(4, p.Segments().Count());
        }

        [Fact]
        public void Area()
        {
            var a = Polygon.Rectangle(1.0, 1.0);
            Assert.Equal(1.0, a.Area());

            var b = Polygon.Rectangle(2.0, 2.0);
            Assert.Equal(4.0, b.Area());

            var p1 = Vector3.Origin;
            var p2 = Vector3.XAxis;
            var p3 = new Vector3(1.0, 1.0);
            var p4 = new Vector3(0.0, 1.0);
            var pp = new Polygon(new[] { p1, p2, p3, p4 });
            Assert.Equal(1.0, pp.Area());
        }

        [Fact]
        public void Length()
        {
            var a = new Vector3();
            var b = new Vector3(1, 0);
            var c = new Vector3(1, 1);
            var d = new Vector3(0, 1);
            var p = new Polygon(new[] { a, b, c, d });
            Assert.Equal(4, p.Length());
        }

        [Fact]
        public void PointAt()
        {
            var a = new Vector3();
            var b = new Vector3(1, 0);
            var c = new Vector3(1, 1);
            var d = new Vector3(0, 1);
            var p = new Polygon(new[] { a, b, c, d });
            Assert.Equal(4, p.Segments().Count());
            Assert.Equal(new Vector3(1.0, 1.0), p.PointAt(0.5));

            var r = Polygon.Rectangle(2, 2);
            Assert.Equal(new Vector3(1, 1, 0), r.PointAt(0.5));
        }

        [Fact]
        public void TwoPeaks_Offset_2Polylines()
        {
            var a = new Vector3();
            var b = new Vector3(5, 0);
            var c = new Vector3(5, 5);
            var d = new Vector3(0, 1);
            var e = new Vector3(-5, 5);
            var f = new Vector3(-5, 0);

            var plinew = new Polygon(new[] { a, b, c, d, e, f });
            var offset = plinew.Offset(-0.5);
            Assert.Equal(2, offset.Count());
        }

        [Fact]
        public void SameVertices_ThrowsException()
        {
            var a = new Vector3();
            Assert.Throws<ArgumentException>(() => new Polygon(new[] { a, a, a }));
        }

        [Fact]
        public void Reverse()
        {
            var a = Polygon.Ngon(3, 1.0);
            var b = a.Reversed();

            // Check that the vertices are properly reversed.
            Assert.Equal(a.Vertices.Reverse(), b.Vertices);
            var t = new Transform();
            var c = a.Transformed(t);
            var l = new Line(Vector3.Origin, new Vector3(0.0, 0.5, 0.5));
            var transforms = l.Frames(0.0, 0.0);

            var start = (Polygon)a.Transformed(transforms[0]);
            var end = (Polygon)b.Transformed(transforms[1]);

            var n1 = start.Plane();
            var n2 = end.Plane();

            // Check that the start and end have opposing normals.
            var dot = n1.Normal.Dot(n2.Normal);
            Assert.Equal(-1.0, dot, 5);
        }

        [Fact]
        public void Planar()
        {
            var a = Vector3.Origin;
            var b = new Vector3(5, 0, 0);
            var c = new Vector3(5, 0, 5);
            var p = new Polygon(new[] { a, b, c });
        }

        [Fact]
        public void DeserializesWithoutDiscriminator()
        {
            // We've received a Polygon and we know that we're receiving
            // a Polygon. The Polygon should deserialize without a 
            // discriminator.
            string json = @"
            {
                ""Vertices"": [
                    {""X"":1,""Y"":1,""Z"":2},
                    {""X"":2,""Y"":1,""Z"":2},
                    {""X"":2,""Y"":2,""Z"":2},
                    {""X"":1,""Y"":2,""Z"":2}
                ]
            }
            ";
            var polygon = JsonConvert.DeserializeObject<Polygon>(json);

            // We've created a new Polygon, which will have a discriminator
            // because it was created using the JsonInheritanceConverter.
            var newJson = JsonConvert.SerializeObject(polygon);
            var newPolygon = (Polygon)JsonConvert.DeserializeObject<Polygon>(newJson);

            Assert.Equal(polygon.Vertices.Count, newPolygon.Vertices.Count);
        }

        [Fact]
        public void Fillet()
        {
            var model = new Model();

            var shape1 = Polygon.L(10, 10, 5);
            var contour1 = shape1.Fillet(0.5);
            var poly1 = contour1.ToPolygon();
            var mass1 = new Mass(poly1);
            Assert.Equal(shape1.Segments().Count() * 2, contour1.Count());

            var t = new Transform(15, 0, 0);
            var shape2 = Polygon.Ngon(3, 5);
            var contour2 = shape2.Fillet(0.5);
            var poly2 = contour2.ToPolygon();
            var mass2 = new Mass(poly2, transform: t);
            Assert.Equal(shape2.Segments().Count() * 2, contour2.Count());

            var shape3 = Polygon.Star(5, 3, 5);
            var contour3 = shape3.Fillet(0.5);
            t = new Transform(30, 0, 0);
            var poly3 = contour3.ToPolygon();
            var mass3 = new Mass(poly3, transform: t);
            Assert.Equal(shape3.Segments().Count() * 2, contour3.Count());
        }

        [Fact]
        public void PolygonDifferenceWithManyCoincidentEdges()
        {
            // an angle of 47 remains known to fail. This may be a fundamental limitation of clipper w/r/t
            // polygon differences with coincident edges at an angle.
            // var rotations = new[] { 0, 47, 90 };
            var rotations = new[] { 0, 90 };
            var areas = new List<double>();
            foreach (var rotation in rotations)
            {

                var tR = new Transform(Vector3.Origin, rotation);
                var polygon = Polygon.Rectangle(Vector3.Origin, new Vector3(100.0, 50.0)).TransformedPolygon(tR);
                var subtracts = new List<Polygon>();

                var side1 = Polygon.Rectangle(Vector3.Origin, new Vector3(1.0, 20.0));
                var side2 = Polygon.Rectangle(new Vector3(0.0, 30.0), new Vector3(1.0, 50.0));
                for (var i = 1; i < 99; i++)
                {
                    var translate = new Transform(i, 0, 0);
                    subtracts.Add(side1.TransformedPolygon(translate).TransformedPolygon(tR));
                    subtracts.Add(side2.TransformedPolygon(translate).TransformedPolygon(tR));
                }
                var polygons = polygon.Difference(subtracts);

                areas.Add(polygons.First().Area());
            }
            var targetArea = areas[0];
            for (int i = 1; i < areas.Count; i++)
            {
                Assert.Equal(targetArea, areas[i], 4);
            }
        }

        [Fact]
        public void PolygonIsAlmostEqualAfterBoolean()
        {
            var innerPolygon = new Polygon(new[]
            {
                new Vector3(-0.81453490602472578, 0.20473478280229102),
                new Vector3(0.2454762730485458, 0.20473478280229102),
                new Vector3(0.2454762730485458, 5.4378426037008651),
                new Vector3(-0.81453490602472578, 5.4378426037008651)
            });

            var outerPolygon = new Polygon(new[]
            {
                new Vector3(-14.371519985751306, -4.8816304299427005),
                new Vector3(-17.661873645682569, 9.2555712951713573),
                new Vector3(12.965610421927806, 9.2555712951713573),
                new Vector3(12.965610421927806, 3.5538269529982784),
                new Vector3(6.4046991240848143, 3.5538269529982784),
                new Vector3(1.3278034769444158, -4.8816304299427005)
            });

            var intersection = innerPolygon.Intersection(outerPolygon);

            Assert.True(intersection[0].IsAlmostEqualTo(innerPolygon, Vector3.EPSILON));
        }

        [Fact]
        public void PolygonPointsAtToTheEnd()
        {
            this.Name = "PolygonPointsAtToTheEnd";

            var polyCircle = new Circle(Vector3.Origin, 5).ToPolygon(7);
            var polyline = new Polyline(polyCircle.Vertices.Take(polyCircle.Vertices.Count - 1).ToList());

            // Ensure that the PointAt function for u=1.0 is at the
            // end of the polygon AND at the end of the polyline.
            Assert.True(polyCircle.PointAt(1.0).IsAlmostEqualTo(polyCircle.Start));
            Assert.True(polyline.PointAt(1.0).IsAlmostEqualTo(polyline.Vertices[polyline.Vertices.Count - 1]));

            this.Model.AddElement(new ModelCurve(polyCircle));

            var circle = new Circle(Vector3.Origin, 0.1).ToPolygon();
            for (var u = 0.0; u <= 1.0; u += 0.05)
            {
                var pt = polyCircle.PointAt(u);
                this.Model.AddElement(new ModelCurve(circle.Transformed(new Transform(pt)), BuiltInMaterials.XAxis));
            }
        }
    }
}