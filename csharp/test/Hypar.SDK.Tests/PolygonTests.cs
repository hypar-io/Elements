using Hypar.Elements;
using Hypar.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;

namespace Hypar.Tests
{
    public class PolygonTests
    {
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
            var centroid = polygon.Centroid;
            Assert.Equal(3, centroid.X);
            Assert.Equal(4, centroid.Y);
        }

        [Fact]
        public void Contains()
        {
            var p1 = new Polygon
            (
                new[]
                {
                    new Vector3(0, 0),
                    new Vector3(20, 0),
                    new Vector3(20, 20),
                    new Vector3(0, 20)
                }
            );
            var p2 = new Polygon
            (
                new[]
                {
                    new Vector3(0, 0),
                    new Vector3(10, 5),
                    new Vector3(10, 10),
                    new Vector3(5, 10)
                }
            );
            var p3 = new Polygon
            (
                new[]
                {
                    new Vector3(5, 5),
                    new Vector3(10, 5),
                    new Vector3(10, 10),
                    new Vector3(5, 10)
                }
            );

            Assert.False(p1.Contains(p2));
            Assert.True(p1.Contains(p3));
            Assert.False(p3.Contains(p1));
        }

        [Fact]
        public void Covers()
        {
            var p1 = new Polygon
            (
                new[]
                {
                    new Vector3(0, 0),
                    new Vector3(20, 0),
                    new Vector3(20, 20),
                    new Vector3(0, 20)
                }
            );
            var p2 = new Polygon
            (
                new[]
                {
                    new Vector3(0, 0),
                    new Vector3(10, 0),
                    new Vector3(10, 10),
                    new Vector3(0, 10)
                }
            );
            Assert.True(p1.Covers(p2));
        }

        [Fact]
        public void Disjoint()
        {
            var p1 = new Polygon
            (
                new[]
                {
                    new Vector3(0, 0),
                    new Vector3(10, 0),
                    new Vector3(10, 10),
                    new Vector3(0, 10)
                }
            );
            var p2 = new Polygon
            (
                new[]
                {
                    new Vector3(20, 20),
                    new Vector3(40, 0),
                    new Vector3(40, 40),
                    new Vector3(0, 20)
                }
            );
            Assert.True(p1.Disjoint(p2));
        }

        [Fact]
        public void Intersects()
        {
            var p1 = new Polygon
            (
                new[]
                {
                    new Vector3(0, 5),
                    new Vector3(4, 5),
                    new Vector3(4, 10),
                    new Vector3(0, 10)
                }
            );
            var p2 = new Polygon
            (
                new[]
                {
                    new Vector3(1, 4),
                    new Vector3(5, 0),
                    new Vector3(8, 4),
                    new Vector3(5, 8)
                }
            );
            Assert.True(p1.Intersects(p2));
        }

        [Fact]
        public void Touches()
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
            Assert.False(p1.Touches(p2));

            p1 = new Polygon
            (
                new[]
                {
                    new Vector3(),
                    new Vector3(4, 0),
                    new Vector3(4, 4),
                    new Vector3(0, 4)
                }
            );
            p2 = new Polygon
            (
                new[]
                {
                    new Vector3(4, 0),
                    new Vector3(8, 0),
                    new Vector3(8, 4),
                    new Vector3(4, 8)
                }
            );
            Assert.True(p1.Touches(p2));
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
            var polygon = p1.Difference(p2);
            var vertices = new List<Vector3>(polygon.Vertices);

            Assert.True(vertices.Exists(vtx => vtx.X == 0 && vtx.Y == 0));
            Assert.True(vertices.Exists(vtx => vtx.X == 4 && vtx.Y == 0));
            Assert.True(vertices.Exists(vtx => vtx.X == 4 && vtx.Y == 1));
            Assert.True(vertices.Exists(vtx => vtx.X == 3 && vtx.Y == 1));
            Assert.True(vertices.Exists(vtx => vtx.X == 3 && vtx.Y == 4));
            Assert.True(vertices.Exists(vtx => vtx.X == 0 && vtx.Y == 4));
        }

        [Fact]
        public void Intersection()
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
            var polygons = p1.Intersection(p2);
            var polygon = polygons.ToArray()[0];
            var vertices = new List<Vector3>(polygon.Vertices);

            Assert.True(vertices.Exists(vtx => vtx.X == 3 && vtx.Y == 1));
            Assert.True(vertices.Exists(vtx => vtx.X == 4 && vtx.Y == 1));
            Assert.True(vertices.Exists(vtx => vtx.X == 4 && vtx.Y == 4));
            Assert.True(vertices.Exists(vtx => vtx.X == 3 && vtx.Y == 4));
        }

        [Fact]
        public void Union()
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
            var polygon = p1.Union(p2);
            var vertices = new List<Vector3>(polygon.Vertices);

            Assert.True(vertices.Exists(vtx => vtx.X == 0 && vtx.Y == 0));
            Assert.True(vertices.Exists(vtx => vtx.X == 4 && vtx.Y == 0));
            Assert.True(vertices.Exists(vtx => vtx.X == 4 && vtx.Y == 1));
            Assert.True(vertices.Exists(vtx => vtx.X == 7 && vtx.Y == 1));
            Assert.True(vertices.Exists(vtx => vtx.X == 7 && vtx.Y == 5));
            Assert.True(vertices.Exists(vtx => vtx.X == 3 && vtx.Y == 5));
            Assert.True(vertices.Exists(vtx => vtx.X == 3 && vtx.Y == 4));
            Assert.True(vertices.Exists(vtx => vtx.X == 0 && vtx.Y == 4));
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
            var polygons = p1.XOR(p2);
            var polygon = polygons.ToArray()[0];
            var vertices = new List<Vector3>(polygon.Vertices);

            Assert.True(vertices.Exists(vtx => vtx.X == 4 && vtx.Y == 1));
            Assert.True(vertices.Exists(vtx => vtx.X == 7 && vtx.Y == 1));
            Assert.True(vertices.Exists(vtx => vtx.X == 7 && vtx.Y == 5));
            Assert.True(vertices.Exists(vtx => vtx.X == 3 && vtx.Y == 5));
            Assert.True(vertices.Exists(vtx => vtx.X == 3 && vtx.Y == 4));
            Assert.True(vertices.Exists(vtx => vtx.X == 0 && vtx.Y == 4));
            Assert.True(vertices.Exists(vtx => vtx.X == 0 && vtx.Y == 0));
            Assert.True(vertices.Exists(vtx => vtx.X == 4 && vtx.Y == 0));
        }

        [Fact]
        public void Offset()
        {
            var a = new Vector3();
            var b = new Vector3(2, 5);
            var c = new Vector3(-3, 5);

            var plinew = new Polygon(new[]{a,b,c});
            var offset = plinew.Offset(0.2);

            Assert.True(offset.Count == 1);
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
            var a = Profiles.Rectangular();
            Assert.Equal(1.0, a.Area);

            var b = Profiles.Rectangular(Vector3.Origin, 2.0,2.0);
            Assert.Equal(4.0, b.Area);

            var p1 = Vector3.Origin;
            var p2 = Vector3.XAxis;
            var p3 = new Vector3(1.0, 1.0);
            var p4 = new Vector3(0.0, 1.0);
            var pp = new Polygon(new[]{p1,p2,p3,p4});
            Assert.Equal(1.0, pp.Area);
        }

        [Fact]
        public void Length()
        {
            var a = new Vector3();
            var b = new Vector3(1,0);
            var c = new Vector3(1,1);
            var d = new Vector3(0,1);
            var p = new Polygon(new[]{a,b,c,d});
            Assert.Equal(4, p.Length);
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
            Assert.Throws<Exception>(()=>new Polygon(new[]{a,a,a}));
        }
    }
}