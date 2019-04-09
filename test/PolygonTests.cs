using Elements;
using Elements.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Elements.Geometry.Tests
{
    public class PolygonTests
    {
        private readonly ITestOutputHelper _output;

        public PolygonTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Fact]
        public void Centroid()
        {
            var polygon = new Polygon
            (
                new[]
                {
                    new Vector3(),
                    new Vector3(3, 3),
                    new Vector3(6, 0),
                    new Vector3(6, 8),
                    new Vector3(3, 5),
                    new Vector3(0, 8)
                }
            );
            var centroid = polygon.Centroid();
            Assert.Equal(3, centroid.X);
            Assert.Equal(4, centroid.Y);
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

            var plinew = new Polygon(new[]{a,b,c});
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

            var plinew = new Polygon(new[]{a,b,c,d,e,f});
            var offset = plinew.Offset(-0.5);
            Assert.Equal(2, offset.Count());
        }

        [Fact]
        public void Construct()
        {
            var a = new Vector3();
            var b = new Vector3(1,0);
            var c = new Vector3(1,1);
            var d = new Vector3(0,1);
            var p = new Polygon(new[]{a,b,c,d});
            Assert.Equal(4, p.Segments().Count());
        }

        [Fact]
        public void Area()
        {
            var a = Polygon.Rectangle(1.0, 1.0);
            Assert.Equal(1.0, a.Area());

            var b = Polygon.Rectangle(2.0,2.0);
            Assert.Equal(4.0, b.Area());

            var p1 = Vector3.Origin;
            var p2 = Vector3.XAxis;
            var p3 = new Vector3(1.0, 1.0);
            var p4 = new Vector3(0.0, 1.0);
            var pp = new Polygon(new[]{p1,p2,p3,p4});
            Assert.Equal(1.0, pp.Area());
        }

        [Fact]
        public void Length()
        {
            var a = new Vector3();
            var b = new Vector3(1,0);
            var c = new Vector3(1,1);
            var d = new Vector3(0,1);
            var p = new Polygon(new[]{a,b,c,d});
            Assert.Equal(4, p.Length());
        }

        [Fact]
        public void PointAt()
        {
            var a = new Vector3();
            var b = new Vector3(1,0);
            var c = new Vector3(1,1);
            var d = new Vector3(0,1);
            var p = new Polygon(new[]{a,b,c,d});
            Assert.Equal(4, p.Segments().Count());
            Assert.Equal(new Vector3(1.0, 1.0), p.PointAt(0.5));

            var r = Polygon.Rectangle(2,2);
            Assert.Equal(new Vector3(1,1,0), r.PointAt(0.5));
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

            var plinew = new Polygon(new[]{a,b,c,d,e,f});
            var offset = plinew.Offset(-0.5);
            Assert.Equal(2, offset.Count());
        }

        [Fact]
        public void SameVertices_ThrowsException()
        {
            var a = new Vector3();
            var b = new Vector3(0.000001,0,0);
            Assert.Throws<ArgumentException>(()=>new Polygon(new[]{a,a,a}));
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
            var l = new Line(Vector3.Origin, new Vector3(0.0,0.5,0.5));
            var transforms = l.Frames(0.0,0.0);

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
    }
}