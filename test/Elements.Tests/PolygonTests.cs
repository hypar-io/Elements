using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using Elements.Tests;

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

            Assert.Contains(vertices, p => p.X == 0.0 && p.Y == 0.0);
            Assert.Contains(vertices, p => p.X == 4.0 && p.Y == 0.0);
            Assert.Contains(vertices, p => p.X == 4.0 && p.Y == 1.0);
            Assert.Contains(vertices, p => p.X == 3.0 && p.Y == 1.0);
            Assert.Contains(vertices, p => p.X == 3.0 && p.Y == 4.0);
            Assert.Contains(vertices, p => p.X == 0.0 && p.Y == 4.0);
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

            Assert.Contains(vertices, p => p.X == 3.0 && p.Y == 1.0);
            Assert.Contains(vertices, p => p.X == 4.0 && p.Y == 1.0);
            Assert.Contains(vertices, p => p.X == 4.0 && p.Y == 4.0);
            Assert.Contains(vertices, p => p.X == 3.0 && p.Y == 4.0);
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

            Assert.Contains(vertices, p => p.X == 0.0 && p.Y == 0.0);
            Assert.Contains(vertices, p => p.X == 4.0 && p.Y == 0.0);
            Assert.Contains(vertices, p => p.X == 4.0 && p.Y == 1.0);
            Assert.Contains(vertices, p => p.X == 7.0 && p.Y == 1.0);
            Assert.Contains(vertices, p => p.X == 7.0 && p.Y == 5.0);
            Assert.Contains(vertices, p => p.X == 3.0 && p.Y == 5.0);
            Assert.Contains(vertices, p => p.X == 3.0 && p.Y == 4.0);
            Assert.Contains(vertices, p => p.X == 0.0 && p.Y == 4.0);

            vertices = p1.Union(ps).Vertices;

            Assert.Contains(vertices, p => p.X == 0.0 && p.Y == 0.0);
            Assert.Contains(vertices, p => p.X == 4.0 && p.Y == 0.0);
            Assert.Contains(vertices, p => p.X == 4.0 && p.Y == 1.0);
            Assert.Contains(vertices, p => p.X == 7.0 && p.Y == 1.0);
            Assert.Contains(vertices, p => p.X == 7.0 && p.Y == 2.0);
            Assert.Contains(vertices, p => p.X == 8.0 && p.Y == 2.0);
            Assert.Contains(vertices, p => p.X == 8.0 && p.Y == 3.0);
            Assert.Contains(vertices, p => p.X == 7.0 && p.Y == 3.0);
            Assert.Contains(vertices, p => p.X == 7.0 && p.Y == 5.0);
            Assert.Contains(vertices, p => p.X == 3.0 && p.Y == 5.0);
            Assert.Contains(vertices, p => p.X == 3.0 && p.Y == 4.0);
            Assert.Contains(vertices, p => p.X == 0.0 && p.Y == 4.0);

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

            Assert.Contains(vertices, p => p.X == 4.0 && p.Y == 1.0);
            Assert.Contains(vertices, p => p.X == 7.0 && p.Y == 1.0);
            Assert.Contains(vertices, p => p.X == 7.0 && p.Y == 5.0);
            Assert.Contains(vertices, p => p.X == 3.0 && p.Y == 5.0);
            Assert.Contains(vertices, p => p.X == 3.0 && p.Y == 4.0);
            Assert.Contains(vertices, p => p.X == 0.0 && p.Y == 4.0);
            Assert.Contains(vertices, p => p.X == 0.0 && p.Y == 0.0);
            Assert.Contains(vertices, p => p.X == 4.0 && p.Y == 0.0);
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
            var c = t.OfPolygon(a);
            var l = new Line(Vector3.Origin, new Vector3(0.0, 0.5, 0.5));
            var transforms = l.Frames(0.0, 0.0);

            _output.WriteLine("Transforms:");
            _output.WriteLine(transforms[0].ToString());
            _output.WriteLine(transforms[1].ToString());
            _output.WriteLine("");

            var start = transforms[0].OfPolygon(a);
            var end = transforms[1].OfPolygon(b);

            _output.WriteLine("Polygons:");
            _output.WriteLine(start.ToString());
            _output.WriteLine(end.ToString());
            _output.WriteLine("");

            var n1 = start.Plane().Normal;
            var n2 = end.Plane().Normal;

            _output.WriteLine("Normals:");
            _output.WriteLine(n1.ToString());
            _output.WriteLine(n2.ToString());
            _output.WriteLine("");

            // Check that the start and end have opposing normals.
            var dot = n1.Dot(n2);
            _output.WriteLine(dot.ToString());
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

            Assert.True(intersection[0].IsAlmostEqualTo(innerPolygon, 1 / Polyline.CLIPPER_SCALE));
            //TODO: decide if clipper_scale should be adjusted to reflect global tolerance settings so that 1 / polyline.clipper_scale = vector3.epsilon.
        }
    }
}