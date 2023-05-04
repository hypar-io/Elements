using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using Elements.Tests;
using System.IO;
using System.Diagnostics;

namespace Elements.Geometry.Tests
{
    public class PolygonTests : ModelTest
    {
        private const string _bigPoly = "{\"discriminator\":\"Elements.Geometry.Polygon\",\"Vertices\":[{\"X\":-12.330319085473015,\"Y\":-12.608248581489981,\"Z\":0.0},{\"X\":19.35916170781505,\"Y\":-15.672958886892182,\"Z\":0.0},{\"X\":21.295342177562976,\"Y\":4.347384863139645,\"Z\":0.0},{\"X\":6.363400191236501,\"Y\":4.347384863139645,\"Z\":0.0},{\"X\":6.363400191236501,\"Y\":19.134320178583096,\"Z\":0.0},{\"X\":22.88039942030141,\"Y\":21.638839095895968,\"Z\":0.0},{\"X\":20.67861826061697,\"Y\":43.07898042004283,\"Z\":0.0},{\"X\":-6.526208021474245,\"Y\":35.050326680125984,\"Z\":0.0},{\"X\":-31.648970883272245,\"Y\":15.181167747161043,\"Z\":0.0}]}";
        private const string _splitters = "[{\"discriminator\":\"Elements.Geometry.Polygon\",\"Vertices\":[{\"X\":1.283510965111264,\"Y\":-16.672391732323188,\"Z\":-5.0},{\"X\":23.642056747347993,\"Y\":-2.1405882651190176,\"Z\":-5.0},{\"X\":23.642056747347993,\"Y\":-2.1405882651190176,\"Z\":5.0},{\"X\":1.283510965111264,\"Y\":-16.672391732323188,\"Z\":5.0}]}]";
        private Polygon _peaks = new Polygon(new List<Vector3>(){
                new Vector3(0,0,0),
                new Vector3(5,0,0),
                new Vector3(5,5,0),
                new Vector3(2.5, 2.5, 0),
                new Vector3(0,5,0)
            });
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

        public static IEnumerable<object[]> GetCenterTestPolygons()
        {
            // Square in Quadrant II
            var polygon = new Polygon(Vector3.Origin,
                                      (-6.0, 0.0),
                                      (-6.0, 6.0),
                                      (0.0, 6.0));
            yield return new object[] { polygon, new Vector3(-3.0, 3.0), new Vector3(-3.0, 3.0) };

            // Square in Quadrant IV
            polygon = new Polygon(Vector3.Origin,
                                  (6.0, 0.0),
                                  (6.0, -6.0),
                                  (0.0, -6.0));
            yield return new object[] { polygon, new Vector3(3.0, -3.0), new Vector3(3.0, -3.0) };

            // Bow Tie in Quadrant I
            polygon = new Polygon((1.0, 1.0),
                                  (4.0, 4.0),
                                  (7.0, 1.0),
                                  (7.0, 9.0),
                                  (4.0, 6.0),
                                  (1.0, 9.0));
            yield return new object[] { polygon, new Vector3(4.0, 5.0), new Vector3(4.0, 5.0) };

            // Bow Tie in Quadrant III
            polygon = new Polygon((-1.0, -1.0),
                                  (-4.0, -4.0),
                                  (-7.0, -1.0),
                                  (-7.0, -9.0),
                                  (-4.0, -6.0),
                                  (-1.0, -9.0));
            yield return new object[] { polygon, new Vector3(-4.0, -5.0), new Vector3(-4.0, -5.0) };

            polygon = new Polygon((20.710443, 4.926839, 0),
                                  (16.247129, 18.703601, 0),
                                  (20.769954, 26.440012, 0),
                                  (28.179055, 23.52398, 0),
                                  (25.571221, 16.897954, 0),
                                  (30.202424, 13.37738, 0),
                                  (28.565876, 9.003332, 0),
                                  (20.710443, 4.926839, 0));
            yield return new object[] { polygon, new Vector3(24.320872, 16.124728, 0), new Vector3(22.862188, 15.809823, 0) };

            polygon = Polygon.Rectangle(6, 4);
            polygon.Vertices.Insert(2, new Vector3(3.0, 1.0));

            yield return new object[] { polygon, new Vector3(0.6, 0.2), new Vector3(0, 0) };
        }

        [Theory]
        [MemberData(nameof(GetCenterTestPolygons))]
        public void CentersAndCentroids(Polygon polygon, Vector3 center, Vector3 centroid)
        {
            var foundCentroid = polygon.Centroid();
            var foundCenter = polygon.Center();
            Assert.True(center.IsAlmostEqualTo(foundCenter));
            Assert.True(centroid.IsAlmostEqualTo(foundCentroid));
        }

        [Fact]
        public void ThreeDCentroids()
        {
            var pgon = new Polygon((-4.800147, -9.701137, 0),
                                   (-4.800147, 6.574991, 0),
                                   (9.600293, 3.126146, 0),
                                   (-4.800147, -9.701137, 0));
            Assert.Equal((0, 0), pgon.Centroid());
            var pgon2 = new Polygon((-14.525461, -9.276887, 1),
                                    (-15.433052, -2.863245, 1),
                                    (-7.022711, 1.19066, 1),
                                    (-2.121721, 8.269868, 1),
                                    (4.654957, 22.428284, 1),
                                    (15.48554, 14.804521, 1),
                                    (3.807872, 0.948636, 1),
                                    (-3.694878, 0.948636, 1),
                                    (15.122504, -3.226282, 1),
                                    (9.011393, -13.088768, 1),
                                    (-0.894814, -6.950567, 1),
                                    (-0.894814, -13.572816, 1),
                                    (-8.958905, -13.572816, 1),
                                    (-14.525461, -9.276887, 1));
            Assert.Equal((1, 1, 1), pgon2.Centroid());
            var pgon3 = new Polygon((7.110806, -11.726081, -2.15209),
                                    (-11.130781, 9.469516, -2.833149),
                                    (-4.657744, 18.325267, 6.541436),
                                    (0.998623, 7.638583, 4.458195),
                                    (-4.011031, 5.850639, 0.027956),
                                    (13.805502, 3.394647, 10.868168),
                                    (6.806682, -2.3006, 2.895764),
                                    (17.41365, -4.20029, 9.105426),
                                    (12.630537, -8.092519, 3.656948),
                                    (7.110806, -11.726081, -2.15209));
            Assert.Equal((2, 2, 2), pgon3.Centroid());

            var pgon4 = new Polygon((38.337892, 0.04147, 27.352492),
                                    (55.553043, 1.557339, 17.913199),
                                    (70.414095, 0.745786, 15.341362),
                                    (60.364178, 10.513956, -7.169442),
                                    (47.355624, 13.559341, -11.060032),
                                    (18.808653, 18.030108, -13.77884),
                                    (17.047729, 13.175249, -0.451222),
                                    (13.719433, 9.651443, 9.871657),
                                    (23.803571, 1.111343, 29.141383),
                                    (38.337892, 0.04147, 27.352492));
            Assert.True(pgon4.Centroid().IsAlmostEqualTo(new Vector3(38.944684, 7.812381, 6.720177)));

            var pgon5 = new Polygon((13.367577, 37.637427, 19.913696),
                                    (-14.19544, 24.029118, 25.91016),
                                    (7.030176, -2.096658, 26.66828),
                                    (28.726802, -10.894791, 24.813338),
                                    (27.499415, 9.222174, 22.036983),
                                    (13.367577, 37.637427, 19.913696));
            Assert.True(pgon5.Centroid().IsAlmostEqualTo(new Vector3(10.523549, 14.044947, 23.791014)));

            var pgon6 = new Polygon((15.164654, 33.292556, 13.183789),
                                    (7.462208, 35.568885, 20.632929),
                                    (8.147323, 22.539536, 33.843797),
                                    (23.718464, 18.112903, 18.595288),
                                    (26.914692, 20.349983, 12.062899),
                                    (24.18474, 34.732703, 0.01945),
                                    (7.483351, 40.774657, 14.975199),
                                    (15.164654, 33.292556, 13.183789));
            Assert.True(pgon6.Centroid().IsAlmostEqualTo(new Vector3(16.95831, 28.253992, 16.325457)));

            var pgon7 = new Polygon((54.53824, 29.174668, -13.957174),
                                    (49.089342, 38.630962, -15.007422),
                                    (41.692673, 43.190619, -12.301108),
                                    (34.52574, 41.779419, -6.768785),
                                    (22.997622, 56.878945, -6.541126),
                                    (9.979076, 54.67304, 3.329722),
                                    (12.144266, 41.168415, 8.612961),
                                    (14.906397, 25.693682, 14.477601),
                                    (12.021633, 15.32467, 21.597258),
                                    (8.605531, 7.350425, 27.879325),
                                    (15.977353, 7.52054, 22.828556),
                                    (30.190687, -3.299195, 18.655516),
                                    (38.126281, 1.246412, 11.040648),
                                    (40.766884, -4.078799, 11.920318),
                                    (50.549067, -2.199374, 4.39255),
                                    (57.429011, 2.955815, -2.815521),
                                    (54.53824, 29.174668, -13.957174));
            Assert.True(pgon7.Centroid().IsAlmostEqualTo(new Vector3(32.623945, 23.645959, 3.564879)));

            var pgon8 = new Polygon((10.072801, 0.604668, 31.063089),
                                    (36.455088, 10.00387, 9.599957),
                                    (31.815341, 12.467897, 6.809542),
                                    (33.728473, 12.916257, 5.625037),
                                    (9.678915, 22.398682, -3.593184),
                                    (0.954908, 18.588346, 4.623971),
                                    (-12.930914, 18.147159, 8.735526),
                                    (5.138936, 2.18134, 29.759844),
                                    (10.072801, 0.604668, 31.063089));
            Assert.True(pgon8.Centroid().IsAlmostEqualTo(new Vector3(11.681926, 11.801531, 12.813532)));
        }


        [Fact]
        public void DoesNotContainPointNotInPlane()
        {
            var rect = Polygon.Rectangle(5, 5);
            var point = new Vector3(0, 0, 2);
            Assert.False(rect.Contains(point));
        }

        [Fact]
        public void Contains()
        {
            var v1 = new Vector3();
            var v2 = new Vector3(7.5, 7.5);
            var p1 = new Polygon
            (
                    new Vector3(0.0, 0.0),
                    new Vector3(20.0, 0.0),
                    new Vector3(20.0, 20.0),
                    new Vector3(0.0, 20.0)
            );
            var p2 = new Polygon
            (
                    new Vector3(0.0, 0.0),
                    new Vector3(10.0, 5.0),
                    new Vector3(10.0, 10.0),
                    new Vector3(5.0, 10.0)
            );
            var p3 = new Polygon
            (
                    new Vector3(5.0, 5.0),
                    new Vector3(10.0, 5.0),
                    new Vector3(10.0, 10.0),
                    new Vector3(5.0, 10.0)
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

            // A big square
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

            // A smaller shape inside p1 that shares a corner with it.
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

            // A smaller square in the center of p1.
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

            // A shape that fully matches p1, but has one extra point inside p1. Covers() needs special code for this case).
            var p4 = new Polygon
            (
                new[]
                {
                new Vector3(0.0, 0.0),
                new Vector3(20.0, 0.0),
                new Vector3(20.0, 20.0),
                new Vector3(10.0, 10),
                new Vector3(0.0, 20.0)
                }
            );

            Assert.True(p1.Covers(v1));
            Assert.True(p1.Covers(p2.Reversed()));
            Assert.True(p1.Covers(p1));
            Assert.True(p1.Covers(p2));
            Assert.True(p1.Covers(p3));
            Assert.True(p3.Covers(v2));
            Assert.False(p3.Covers(v1));
            Assert.False(p3.Covers(p1));
            Assert.False(p4.Covers(p1));
            Assert.True(p1.Covers(p4));

            var t = new Transform(Vector3.Origin, Vector3.YAxis);
            var tp1 = p1.TransformedPolygon(t);
            var tp2 = p2.TransformedPolygon(t);
            var tp3 = p3.TransformedPolygon(t);

            Assert.True(tp1.Contains3D(tp1, out var c1));
            Assert.True(tp1.Contains3D(tp2, out var c2));
            Assert.True(tp1.Contains3D(tp3, out var c3));
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
        public void UnionAll()
        {
            var result = Polygon.UnionAll(
                Polygon.Rectangle(10, 10),
                Polygon.Rectangle(10, 10).TransformedPolygon(new Transform(5, 5, 0))).First();
            Assert.Equal(175, result.Area());
        }

        [Fact]
        public void Union()
        {
            var p1 = new Polygon
            (
                (0, 0),
                (4.0, 0.0),
                (4.0, 4.0),
                (0.0, 4.0)
            );
            var p2 = new Polygon
            (
                (3.0, 1.0),
                (7.0, 1.0),
                (7.0, 5.0),
                (3.0, 5.0)
            );
            var p3 = new Polygon
            (
                (3.0, 2.0),
                (8.0, 2.0),
                (8.0, 3.0),
                (3.0, 3.0)
            );

            var vertices = p1.Union(p2).Vertices;

            Assert.Contains(vertices, p => p.IsAlmostEqualTo(0.0, 0.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(4.0, 0.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(4.0, 1.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(7.0, 1.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(7.0, 5.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(3.0, 5.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(3.0, 4.0));
            Assert.Contains(vertices, p => p.IsAlmostEqualTo(0.0, 4.0));

            vertices = p1.Union(p2, p3).Vertices;

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

            var t = new Transform(Vector3.Origin, Vector3.XAxis);
            var ta = a.TransformedPolygon(t);
            Assert.Equal(1.0, ta.Area());
            var tb = b.TransformedPolygon(t);
            Assert.Equal(4.0, tb.Area());

            var concave = new Polygon(new[] {
                new Vector3(5, 0, 0),
                new Vector3(5, 1, 0),
                new Vector3(3, 1, 0),
                new Vector3(3, 4, 0),
                new Vector3(5, 4, 0),
                new Vector3(5, 5, 0),
                new Vector3(0, 5, 0),
                new Vector3(0, 0, 0),
            });
            Assert.Equal(19, concave.Area());
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
            Assert.Equal(new Vector3(1.0, 1.0), p.Mid());

            var r = Polygon.Rectangle(2, 2);
            Assert.Equal(new Vector3(1, 1, 0), r.Mid());
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
        public void SameVertices_RemovesDuplicates()
        {
            var a = new Vector3();
            var b = new Vector3(10, 0, 0);
            var c = new Vector3(10, 3, 0);
            var polygon = new Polygon(new[] { a, a, a, b, c });
            Assert.Equal(3, polygon.Segments().Count());
        }

        [Fact]
        public void UnionAllSequential()
        {
            Name = "UnionAllSequential";
            // sample data contributed by Marco Juliani
            var polygonsA = JsonConvert.DeserializeObject<List<Polygon>>(File.ReadAllText("../../../models/Geometry/testUnionAll.json"));
            var polygonsB = JsonConvert.DeserializeObject<List<Polygon>>(File.ReadAllText("../../../models/Geometry/testUnionAll_2.json"));
            var unionA = Polygon.UnionAll(polygonsA);
            var unionB = Polygon.UnionAll(polygonsB);
            Model.AddElements(unionA.Select(u => new ModelCurve(u)));
            Model.AddElements(unionB.Select(u => new ModelCurve(u)));
        }

        [Fact]
        public void ConvexHullAndBoundingRect()
        {
            Name = "Convex Hull";
            var rand = new Random();
            // fuzz test
            for (int test = 0; test < 50; test++)
            {
                var basePt = new Vector3((test % 5) * 12, (test - (test % 5)) / 5 * 12);
                var pts = new List<Vector3>();
                for (int i = 0; i < 20; i++)
                {
                    pts.Add(basePt + new Vector3(rand.NextDouble() * 10, rand.NextDouble() * 10));
                }
                var modelPts = pts.Select(p => new ModelCurve(new Circle(p, 0.2)));
                var hull = ConvexHull.FromPoints(pts);
                var boundingRect = Polygon.FromAlignedBoundingBox2d(pts);
                Model.AddElements(modelPts);
                Model.AddElements(boundingRect);
                Model.AddElement(hull);
            }

            // handle collinear pts test
            var coPts = new List<Vector3> {
                new Vector3(0,0),
                new Vector3(1,0),
                new Vector3(2,0),
                new Vector3(4,0),
                new Vector3(10,0),
                new Vector3(10,5),
                new Vector3(10,10)
            };
            var coHull = ConvexHull.FromPoints(coPts);
            Assert.Equal(50, coHull.Area());
        }

        [Fact]
        public void FromAlignedBoundingBox2dAlongAxis()
        {
            Name = nameof(FromAlignedBoundingBox2dAlongAxis);
            // handle random points test
            var pts = new List<Vector3> {
                new Vector3(2,1),
                new Vector3(1,3),
                new Vector3(5,5),
                new Vector3(6,3),
                new Vector3(2,2),
                new Vector3(2.5,3),
                new Vector3(4,3)
            };
            var boundingRect = Polygon.FromAlignedBoundingBox2d(pts, pts[1] - pts[0]);
            Assert.True(boundingRect.Area().ApproximatelyEquals(10));
            Model.AddElements(pts.Select(p => new ModelCurve(new Circle(p, 0.2))));
            Model.AddElements(new ModelCurve(boundingRect));

            // handle collinear points test
            var coPts = new List<Vector3> {
                new Vector3(0,0),
                new Vector3(1,0),
                new Vector3(2,0),
                new Vector3(4,0),
                new Vector3(10,0)
            };
            var coBoundingRect = Polygon.FromAlignedBoundingBox2d(coPts, new Vector3(1, 0));
            Assert.Equal(1, coBoundingRect.Area());
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
        public void PointInternal()
        {
            Name = "PointInternal";
            var extremelyConcavePolygon = new Polygon(new[] {
                new Vector3(
                        5.894565217391305,
                        0.0,
                        0.0
                        ),
                        new Vector3(
                        5.894565217391305,
                        0.69347826086956488,
                        0.0
                        ),
                        new Vector3(
                        0.19082958701549974,
                        0.13222235119778919,
                        0.0
                        ),
                        new Vector3(
                        0.0,
                        2.3964022904166224,
                        0.0
                        ),
                        new Vector3(
                        6.2310064053894925,
                        3.9731847432442322,
                        0.0
                        ),
                        new Vector3(
                        5.9682093299182242,
                        4.5513383092810225,
                        0.0
                        ),
                        new Vector3(
                        -0.085624933213594254,
                        2.4045397339325851,
                        0.0
                        ),
                        new Vector3(
                        0.0,
                        0.0,
                        0.0
                        )
            });
            var pointInternal = extremelyConcavePolygon.PointInternal();
            Assert.True(extremelyConcavePolygon.Contains(pointInternal));
            Model.AddElement(extremelyConcavePolygon);
            Curve.MinimumChordLength = 0.001;
            Model.AddElement(new Circle(pointInternal, 0.02));
            Curve.MinimumChordLength = 0.1;
        }

        [Fact]
        public void PolygonSplitWithPolyline()
        {
            Name = "PolygonSplitWithPolyline";
            var random = new Random(23);

            // Simple Split
            var polygon = Polygon.Rectangle(5, 5);
            var polyline = new Polyline((-3, 0), (0, 1), (3, 0));
            var splitResults = polygon.Split(polyline);
            Assert.Equal(2, splitResults.Count);

            // Convex shape split
            var convexPolygon = new Polygon(
                (-2.5, -2.5),
                (2.5, -2.5),
                (2.5, -1),
                (1, -1),
                (1, 1),
                (2.5, 1),
                (2.5, 2.5),
                (-2.5, 2.5)
            );
            var convexSplitPolyline = new Polyline(
                (1.5, -3),
                (1.5, 3)
            );

            var splitResults2 = convexPolygon.Split(convexSplitPolyline);
            Model.AddElements(splitResults2.Select(s => new Panel(s, random.NextMaterial())));
            Assert.True(splitResults2.Count == 3);

            // doesn't intersect, no change
            var shiftedPolygon = convexPolygon.TransformedPolygon(new Transform(6, 0, 0));
            var splitResults3 = shiftedPolygon.Split(convexSplitPolyline);
            Assert.True(splitResults3.Count == 1);
            Model.AddElements(splitResults3.Select(s => new Panel(s, random.NextMaterial())));

            // totally contained, no change
            var internalPl = new Polyline(new[] { new Vector3(6 - 2.5 + 0.5, -2), new Vector3(6 - 2.5 + 0.5, 2) });
            Model.AddElement(internalPl);
            var splitResults4 = shiftedPolygon.Split(internalPl);
            Assert.True(splitResults4.Count == 1);

            // split with pass through vertex
            var cornerPg = new Polygon(new[] { new Vector3(0, 10), new Vector3(3, 10), new Vector3(3, 13), new Vector3(0, 13) });
            var cornerPl = new Polyline(new[] {
                new Vector3(-1, 9),
                new Vector3(3, 13)
            });
            Model.AddElements(cornerPg, cornerPl);
            var splitResults5 = cornerPg.Split(cornerPl);
            Assert.True(splitResults5.Count == 2);
            Model.AddElements(splitResults5.Select(s => new Panel(s, random.NextMaterial())));

            // pass through incompletely, no change
            var cornerPl2 = new Polyline(new[] {
                new Vector3(-1, 9),
                new Vector3(2, 11)
            });

            var splitResults6 = cornerPg.Split(cornerPl2);
            Assert.True(splitResults6.Count == 1);

            // overlap at edge, no change.ioU
            var rect2 = Polygon.Ngon(5, 5).TransformedPolygon(new Transform(-6, -8, 0));
            var splitCrv = rect2.Segments()[3];
            var splitResults7 = rect2.Split(splitCrv.ToPolyline(1));
            Assert.True(splitResults7.Count == 1);
            Model.AddElements(splitResults7.Select(s => new Panel(s, random.NextMaterial())));


            // fuzz test
            var shifted = convexPolygon.TransformedPolygon(new Transform(12, 0, 0));
            var collection = new List<Polygon> { shifted };
            var bbox = new BBox3(shifted);
            var rect = Polygon.Rectangle(bbox.Min, bbox.Max);
            for (int i = 0; i < 20; i++)
            {
                var randomLine = new Polyline(new[] {
                    rect.PointAt(random.NextDouble()),
                    bbox.Min + new Vector3((bbox.Max.X - bbox.Min.X) * random.NextDouble(), (bbox.Max.Y - bbox.Min.Y) * random.NextDouble()),
                    rect.PointAt(random.NextDouble()) });
                collection = collection.SelectMany(c => c.Split(randomLine)).ToList();
            }
            Model.AddElements(collection.Select(c => new Panel(c, random.NextMaterial())));

        }

        [Fact]
        public void NonXYSplit()
        {
            Name = nameof(NonXYSplit);
            var shape = new Polygon(new[] {
                        new Vector3(3,0,1),
                        new Vector3(1,0,10),
                        new Vector3(10,0,17),
                        new Vector3(21,0,14),
                        new Vector3(12,0,11),
                        new Vector3(15,0,5),
                        new Vector3(22,0,8),
                        new Vector3(22,0,2),
                        new Vector3(13,0,2),
                        new Vector3(13,0,1)
                        });
            var polylines = new List<Polyline> {
                        new Polyline(new [] {
                        new Vector3(1,0,16),
                        new Vector3(7,0,9),
                        new Vector3(7,0,-1)
                        }),
                        new Polyline(new [] {
                        new Vector3(-2,0,5),
                        new Vector3(10,0,5),
                        new Vector3(17,0,9),
                        new Vector3(14,0,18)
                        })
                        };

            var results = shape.Split(polylines);
            Assert.True(results.Count == 5);
            Assert.Equal(Math.Abs(shape.Area()), results.Sum(r => Math.Abs(r.Area())), 5);
            var rand = new Random(4);
            Model.AddElements(results.Select(r => new Panel(r, rand.NextMaterial())));
        }

        [Fact]
        public void ToTransformOrientation()
        {
            var polygon = new Polygon(new[] {
                        new Vector3(3,0,1),
                        new Vector3(1,0,10),
                        new Vector3(10,0,17),
                        new Vector3(21,0,14),
                        new Vector3(12,0,11),
                        new Vector3(15,0,5),
                        new Vector3(22,0,8),
                        new Vector3(22,0,2),
                        new Vector3(13,0,2),
                        new Vector3(13,0,1)
                        });
            var polygonNormal = polygon.Normal();
            var transform = polygon.ToTransform();
            Assert.Equal(1, transform.ZAxis.Dot(polygonNormal));
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
            Assert.True(polyCircle.PointAt(polyCircle.Domain.Max).IsAlmostEqualTo(polyCircle.Start));
            Assert.True(polyline.PointAt(polyline.Domain.Max).IsAlmostEqualTo(polyline.Vertices[polyline.Vertices.Count - 1]));
            // Test value close to u=0.0 within tolerance
            Assert.True(polyCircle.PointAt(-1e-15).IsAlmostEqualTo(polyCircle.End));
            Assert.True(polyline.PointAt(-1e-15).IsAlmostEqualTo(polyline.Vertices[polyline.Vertices.Count - 1]));

            this.Model.AddElement(new ModelCurve(polyCircle));

            var circle = new Circle(Vector3.Origin, 0.1).ToPolygon();
            for (var u = 0.0; u <= 1.0; u += 0.05)
            {
                var pt = polyCircle.PointAt(u);
                this.Model.AddElement(new ModelCurve(circle, BuiltInMaterials.XAxis));
            }
        }

        [Fact]
        public void RemoveVerticesNearCurve()
        {
            this.Name = nameof(RemoveVerticesNearCurve);

            var square = Polygon.Rectangle(1, 1);
            var upperLeftLine = new Line(new Vector3(-1, 0), new Vector3(0, 1));
            var remainderPoly1 = square.RemoveVerticesNearCurve(upperLeftLine, out var removed);

            Assert.Equal(new[] {
                new Vector3(-0.5, -0.5),
                new Vector3(0.5, -0.5),
                new Vector3(0.5, 0.5)
            }, remainderPoly1.Vertices);
            Assert.Equal(new[] { new Vector3(-0.5, 0.5) }, removed);

            var lowerRightLine = new Line(new Vector3(0, -1), new Vector3(1, 0));

            var house = new Polygon(new[] {
                new Vector3(0,0),
                new Vector3(0,2),
                new Vector3(2,2),
                new Vector3(2,0),
                new Vector3(1.5,0),
                new Vector3(1.5,1),
                new Vector3(0.5,1),
                new Vector3(0.5,0)
            });

            var doorOutline = Polygon.Rectangle(new Vector3(0.5, 0), new Vector3(1.5, 1));

            var houseOutline = house.RemoveVerticesNearCurve(doorOutline, out var removedDoor);
            Assert.Equal(new List<Vector3> {( X:1.5000, Y:0.0000, Z:0.0000 ), ( X:1.5000, Y:1.0000, Z:0.0000 ),
                                            ( X:0.5000, Y:1.0000, Z:0.0000 ), ( X:0.5000, Y:0.0000, Z:0.0000 )},
                             removedDoor);

            var expectedhouseOutline = Polygon.Rectangle(new Vector3(), new Vector3(2, 2));
            Assert.True(expectedhouseOutline.IsAlmostEqualTo(houseOutline, ignoreWinding: true));
        }

        [Fact]
        public void SharedSegments_ConcentricCircles_NoResults()
        {
            var a = new Circle(new Vector3(), 1).ToPolygon();
            var b = new Circle(new Vector3(), 2).ToPolygon();

            var results = Polygon.SharedSegments(a, b, true);

            Assert.Empty(results);
        }

        [Fact]
        public void SharedSegments_IntersectingCircles_NoResults()
        {
            var a = new Circle(new Vector3(1, 0, 0), 2).ToPolygon();
            var b = new Circle(new Vector3(-1, 0, 0), 2).ToPolygon();

            var results = Polygon.SharedSegments(a, b, true);

            Assert.Empty(results);
        }

        [Fact]
        public void SharedSegments_DuplicateCircles_TenResults()
        {
            var a = new Circle(new Vector3(), 1).ToPolygon();
            var b = new Circle(new Vector3(), 1).ToPolygon();

            var results = Polygon.SharedSegments(a, b, true);

            Assert.Equal(10, results.Count);
        }

        [Fact]
        public void SharedSegments_DuplicateCircles_AccurateResults()
        {
            var a = new Circle(new Vector3(), 1).ToPolygon();
            var b = new Circle(new Vector3(), 1).ToPolygon();

            var matches = Polygon.SharedSegments(a, b, true);

            var result = matches.Select(match =>
            {
                var (t, u) = match;

                var segmentA = a.Segments()[t];
                var segmentB = b.Segments()[u];

                // Reverse segment b if necessary
                if (!segmentA.Start.IsAlmostEqualTo(segmentB.Start))
                {
                    segmentB = segmentB.Reversed();
                }

                var sa = segmentA.Start;
                var sb = segmentB.Start;
                var ea = segmentA.End;
                var eb = segmentB.End;

                var startMatches = sa.IsAlmostEqualTo(sb);
                var endMatches = ea.IsAlmostEqualTo(eb);

                return startMatches && endMatches;
            });

            Assert.DoesNotContain(false, result);
        }

        [Fact]
        public void SharedSegments_DuplicateReversedCircles_AccurateResults()
        {
            var a = new Circle(new Vector3(), 1).ToPolygon();
            var b = new Circle(new Vector3(), 1).ToPolygon().Reversed();

            var matches = Polygon.SharedSegments(a, b, true);

            var result = matches.Select(match =>
            {
                var (t, u) = match;

                var segmentA = a.Segments()[t];
                var segmentB = b.Segments()[u];

                // Reverse segment b if necessary
                if (!segmentA.Start.IsAlmostEqualTo(segmentB.Start))
                {
                    segmentB = segmentB.Reversed();
                }

                var sa = segmentA.Start;
                var sb = segmentB.Start;
                var ea = segmentA.End;
                var eb = segmentB.End;

                var startMatches = sa.IsAlmostEqualTo(sb);
                var endMatches = ea.IsAlmostEqualTo(eb);

                return startMatches && endMatches;
            });

            Assert.DoesNotContain(false, result);
        }

        [Fact]
        public void SharedSegments_MirroredSquares_OneResult()
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

            var matches = Polygon.SharedSegments(a, b, true);

            Assert.Single(matches);
        }

        [Fact]
        public void SharedSegments_TouchingShiftedSquares_NoResults()
        {
            var s = 1;

            var a = new Polygon(new List<Vector3>(){
                new Vector3(0, s, 0),
                new Vector3(-s, s, 0),
                new Vector3(-s, 0, 0),
                new Vector3(0, 0, 0),
            });
            var b = new Polygon(new List<Vector3>(){
                new Vector3(0, s + 0.5, 0),
                new Vector3(s, s + 0.5, 0),
                new Vector3(s, 0.5, 0),
                new Vector3(0, 0.5, 0),
            });

            var matches = Polygon.SharedSegments(a, b, true);

            Assert.Empty(matches);
        }

        [Fact]
        public void TransformSegment_UnitSquare_Outwards()
        {
            var s = 0.5;

            var square = new Polygon(new List<Vector3>()
            {
                new Vector3(s, s, 0),
                new Vector3(-s, s, 0),
                new Vector3(-s, -s, 0),
                new Vector3(s, -s, 0)
            });

            var t = new Transform(0, 1, 0);

            square.TransformSegment(t, 0);

            var segment = square.Segments()[0];

            var start = square.Vertices[0];
            var end = square.Vertices[1];

            // Confirm vertices are correctly moved
            Assert.Equal(s, segment.Start.X);
            Assert.Equal(s + 1, segment.Start.Y);
            Assert.Equal(-s, segment.End.X);
            Assert.Equal(s + 1, segment.End.Y);

            // Confirm area has been correctly modified
            Assert.True(square.Area().ApproximatelyEquals(2));
        }

        [Fact]
        public void TransformSegment_UnitSquare_Inwards()
        {
            var s = 0.5;

            var square = new Polygon(new List<Vector3>()
            {
                new Vector3(s, s, 0),
                new Vector3(-s, s, 0),
                new Vector3(-s, -s, 0),
                new Vector3(s, -s, 0)
            });

            var t = new Transform(0, -0.5, 0);

            square.TransformSegment(t, 0);

            var segment = square.Segments()[0];

            // Confirm vertices are correctly moved
            Assert.Equal(s, segment.Start.X);
            Assert.Equal(s - 0.5, segment.Start.Y);
            Assert.Equal(-s, segment.End.X);
            Assert.Equal(s - 0.5, segment.End.Y);

            // Confirm area has been correctly modified
            Assert.True(square.Area().ApproximatelyEquals(0.5));
        }

        [Fact]
        public void TransformSegment_UnitSquare_LastSegment()
        {
            var s = 0.5;

            var square = new Polygon(new List<Vector3>()
            {
                new Vector3(s, s, 0),
                new Vector3(-s, s, 0),
                new Vector3(-s, -s, 0),
                new Vector3(s, -s, 0)
            });

            var t = new Transform(1, 0, 0);

            square.TransformSegment(t, 3);

            var segment = square.Segments()[3];

            // Confirm vertices are correctly moved
            Assert.Equal(s + 1, segment.Start.X);
            Assert.Equal(-s, segment.Start.Y);
            Assert.Equal(s + 1, segment.End.X);
            Assert.Equal(s, segment.End.Y);

            // Confirm area has been correctly modified
            Assert.True(square.Area().ApproximatelyEquals(2));
        }

        [Fact]
        public void TransformSegment_UnitSquare_OutOfRange()
        {
            var s = 0.5;

            var square = new Polygon(new List<Vector3>()
            {
                new Vector3(s, s, 0),
                new Vector3(-s, s, 0),
                new Vector3(-s, -s, 0),
                new Vector3(s, -s, 0)
            });

            var t = new Transform(1, 1, 0);

            square.TransformSegment(t, 100);

            // Confirm area has remained the same
            Assert.True(square.Area().ApproximatelyEquals(1));
        }

        [Fact]
        public void TransformSegment_UnitSquare_AllowsValidNonPlanar()
        {
            var s = 0.5;

            var square = new Polygon(new List<Vector3>()
            {
                new Vector3(s, s, 0),
                new Vector3(-s, s, 0),
                new Vector3(-s, -s, 0),
                new Vector3(s, -s, 0)
            });

            var t = new Transform(1, 1, 1);

            square.TransformSegment(t, 0);
        }

        [Fact]
        public void TransformSegment_RotatedSquare_AllowsPlanarMotion()
        {
            var s = 0.5;

            var square = new Polygon(new List<Vector3>()
            {
                new Vector3(s, s, s),
                new Vector3(-s, s, s),
                new Vector3(-s, -s, -s),
                new Vector3(s, -s, -s)
            });

            var t = new Transform(0, 2, 2);

            square.TransformSegment(t, 0);

            var segment = square.Segments()[0];

            var start = square.Vertices[0];
            var end = square.Vertices[1];

            // Confirm vertices are correctly moved
            Assert.Equal(s + 2, segment.Start.Y);
            Assert.Equal(s + 2, segment.Start.Z);
            Assert.Equal(s + 2, segment.End.Y);
            Assert.Equal(s + 2, segment.End.Z);
        }

        [Fact]
        public void TransformSegment_Circle_ThrowsOnNonPlanar()
        {
            var circle = new Circle(new Vector3(), 1).ToPolygon();

            var t = new Transform(2, 2, 2);

            Assert.Throws<Exception>(() => circle.TransformSegment(t, 0));
        }

        [Fact]
        public void VerticalContainment()
        {
            var point = new Vector3(8.874555, 6.112945, 30);
            var polygon = new Polygon(new List<Vector3>() {
                new Vector3(11.37475, 8.56224, -3),
                new Vector3(6.37436, 3.66365, -3),
                new Vector3(6.37436, 3.66365, 0),
                new Vector3(11.37475, 8.56224, 0)
            });
            var contains = polygon.Contains(point, out var type);
            Assert.False(contains);
        }

        [Fact]
        public void PolygonIsTrimmedAbovePlane()
        {
            this.Name = nameof(PolygonIsTrimmedAbovePlane);

            var r = new Random();

            // Trim above
            var t = new Transform(Vector3.Origin, Vector3.XAxis, Vector3.YAxis.Negate());
            var polygon = Polygon.Star(5, 2, 5).TransformedPolygon(t);
            var plane = new Plane(new Vector3(0, 0, -2.5), Vector3.ZAxis);
            var trimmed = polygon.Trimmed(plane);
            Assert.Single(trimmed);
            var panels = trimmed.Select(t => new Panel(t, r.NextMaterial()));
            this.Model.AddElement(new ModelCurve(polygon));
            Model.AddElement(new Panel(Polygon.Rectangle(100, 100).TransformedPolygon(new Transform(plane.Origin, plane.Normal)), BuiltInMaterials.Glass));
            this.Model.AddElements(trimmed.Select(t => new ModelCurve(t)));
            this.Model.AddElements(panels);
        }

        [Fact]
        public void PolygonIsTrimmedBelowPlane()
        {
            this.Name = nameof(PolygonIsTrimmedBelowPlane);
            var r = new Random();

            // Trim below
            var t = new Transform(Vector3.Origin, Vector3.XAxis, Vector3.YAxis.Negate());
            var polygon = Polygon.Star(5, 2, 5).TransformedPolygon(t);
            var plane = new Plane(new Vector3(0, 0, -2.5), Vector3.ZAxis);
            var trimmedReverse = polygon.Trimmed(plane, true);
            Assert.Equal<int>(2, trimmedReverse.Count);
            var move = new Transform(0, 0, 0);
            move.Rotate(Vector3.ZAxis, 15);
            move.Move(new Vector3(8, 0, 0));
            var panel2 = trimmedReverse.Select(t => new Panel(t, r.NextMaterial(), transform: move));
            this.Model.AddElement(new ModelCurve(polygon, transform: move));
            this.Model.AddElements(trimmedReverse.Select(t => new ModelCurve(t, transform: move)));
            this.Model.AddElements(panel2);
        }

        [Fact]
        public void TrimPolygonThroughVertex()
        {
            this.Name = nameof(TrimPolygonThroughVertex);
            var r = new Random();

            // Trim through vertex
            var t = new Transform(Vector3.Origin, Vector3.XAxis, Vector3.YAxis.Negate());
            var polygon = Polygon.Star(5, 2, 5).TransformedPolygon(t);
            var vertexTrimPlane = new Plane(new Vector3(0, 0, polygon.Vertices[7].Z), Vector3.ZAxis);
            var trimmedAtVertex = polygon.Trimmed(vertexTrimPlane, true);
            Assert.Equal<int>(2, trimmedAtVertex.Count);
            var move2 = new Transform(16, 0, 0);
            var panel3 = trimmedAtVertex.Select(t => new Panel(t, r.NextMaterial(), transform: move2));
            this.Model.AddElements(panel3);
            this.Model.AddElements(trimmedAtVertex.Select(t => new ModelCurve(t, transform: move2)));
            this.Model.AddElement(new ModelCurve(polygon, transform: move2));
        }

        [Fact]
        public void TrimPolygonThroughTwoVertices()
        {
            Name = nameof(TrimPolygonThroughTwoVertices);
            var shape = Polygon.Ngon(4);
            var plane = new Plane(Vector3.Origin, Vector3.YAxis);
            var trimmed = shape.Trimmed(plane);
            Assert.Single(trimmed);
            if (trimmed != null)
            {
                Model.AddElement(new Panel(trimmed[0]));
            }
            else
            {
                Model.AddElement(new Panel(shape));
            }

            Model.AddElement(new Panel(Polygon.Rectangle(5, 5).TransformedPolygon(new Transform(plane.Origin, plane.Normal)), BuiltInMaterials.Glass));
            Model.AddElement(new ModelCurve(shape));
        }

        [Fact]
        public void TrimPolygonThroughVertexAndEdge()
        {
            Name = nameof(TrimPolygonThroughVertexAndEdge);
            var shape = Polygon.Ngon(4);
            var plane = new Plane(shape.Vertices[0], new Vector3(0.01, 1, 0).Unitized());
            var trimmed = shape.Trimmed(plane);
            Assert.Single(trimmed);
            if (trimmed != null)
            {
                Model.AddElement(new Panel(trimmed[0]));
            }
            else
            {
                Model.AddElement(new Panel(shape));
            }

            Model.AddElement(new Panel(Polygon.Rectangle(5, 5).TransformedPolygon(new Transform(plane.Origin, plane.Normal)), BuiltInMaterials.Glass));
            Model.AddElement(new ModelCurve(shape));
        }

        [Fact]
        public void CorrectWindingForTrims()
        {
            this.Name = nameof(CorrectWindingForTrims);

            var r = new Random();

            var bigPoly = JsonConvert.DeserializeObject<Polygon>(_bigPoly);
            var splitters = JsonConvert.DeserializeObject<List<Polygon>>(_splitters);

            foreach (var splitter in splitters)
            {
                this.Model.AddElement(new Panel(splitter, BuiltInMaterials.Mass));
                this.Model.AddElement(new ModelCurve(new Line(splitter.Centroid(), splitter.Centroid() + splitter.Normal() * 1)));
            }

            var result1 = bigPoly.TrimmedTo(splitters);
            foreach (var p in result1)
            {
                this.Model.AddElement(new Panel(p, r.NextMaterial()));
            }

            var result2 = bigPoly.TrimmedTo(splitters.Select(s => s.Reversed()).ToList());
            foreach (var p in result2)
            {
                this.Model.AddElement(new Panel(p, r.NextMaterial()));
            }
        }

        [Fact]
        public void TrimsTwoPolygonsAcrossOnePeak()
        {
            this.Name = nameof(TrimsTwoPolygonsAcrossOnePeak);

            var random = new Random();
            var p1 = new Polygon(new List<Vector3>(){
                new Vector3(2.5,3,-1),
                new Vector3(4.5, 3.5, -1),
                new Vector3(4.5, 3.5, 1),
                new Vector3(2.5,3,1),
            });
            var p2 = new Polygon(new List<Vector3>(){
                new Vector3(4.5,3.5,-1),
                new Vector3(6, 3, -1),
                new Vector3(6, 3, 1),
                new Vector3(4.5,3.5,1),
            });

            this.Model.AddElement(new ModelCurve(p1));
            this.Model.AddElement(new ModelCurve(p2));

            var trims = _peaks.TrimmedTo(new[] { p1, p2 });
            foreach (var l in trims)
            {
                this.Model.AddElement(new Panel(l, random.NextMaterial()));
                this.Model.AddElement(new ModelCurve(l, random.NextMaterial()));
            }
            Assert.Single(trims);

            var trims1 = _peaks.TrimmedTo(new[] { p1.Reversed() });
            Assert.Single(trims1);
        }

        [Fact]
        public void TrimsAcrossBothPeaks()
        {
            this.Name = nameof(TrimsAcrossBothPeaks);

            var random = new Random();
            var p1 = new Polygon(new List<Vector3>(){
                new Vector3(-1,3,-1),
                new Vector3(6, 3, -1),
                new Vector3(6, 3, 1),
                new Vector3(-1,3,1),
            });

            var trims = _peaks.TrimmedTo(new[] { p1 });
            foreach (var l in trims)
            {
                this.Model.AddElement(new Panel(l, random.NextMaterial()));
                this.Model.AddElement(new ModelCurve(l, random.NextMaterial()));
            }
            Assert.Single(trims);

            var trims1 = _peaks.TrimmedTo(new[] { p1.Reversed() });
            Assert.Equal(2, trims1.Count());
        }

        [Fact]
        public void TrimsAcrossOnePeak()
        {
            this.Name = nameof(TrimsAcrossOnePeak);

            var random = new Random();
            var p1 = new Polygon(new List<Vector3>(){
                new Vector3(-1,3,-1),
                new Vector3(2.5, 3, -1),
                new Vector3(2.5, 3, 1),
                new Vector3(-1,3,1),
            });

            var trims = _peaks.TrimmedTo(new[] { p1 });
            foreach (var l in trims)
            {
                this.Model.AddElement(new Panel(l, random.NextMaterial()));
                this.Model.AddElement(new ModelCurve(l, random.NextMaterial()));
            }
            Assert.Single(trims);

            var trims1 = _peaks.TrimmedTo(new[] { p1.Reversed() });
            Assert.Single(trims1);
        }

        [Fact]
        public void TrimsAtValley()
        {
            this.Name = nameof(TrimsAtValley);

            var random = new Random();
            var p1 = new Polygon(new List<Vector3>(){
                new Vector3(-1,2.5,-1),
                new Vector3(6, 2.5, -1),
                new Vector3(6, 2.5, 1),
                new Vector3(-1,2.5,1),
            });

            var trims = _peaks.TrimmedTo(new[] { p1 });
            foreach (var l in trims)
            {
                this.Model.AddElement(new Panel(l, random.NextMaterial()));
                this.Model.AddElement(new ModelCurve(l, random.NextMaterial()));
            }
            Assert.Single(trims);

            var trims1 = _peaks.TrimmedTo(new[] { p1.Reversed() });
            Assert.Equal(2, trims1.Count());
        }

        [Fact]
        public void DoesNotTrimAtTopOfPeak()
        {
            this.Name = nameof(DoesNotTrimAtTopOfPeak);

            var random = new Random();
            var p1 = new Polygon(new List<Vector3>(){
                new Vector3(-1,5,-1),
                new Vector3(6, 5, -1),
                new Vector3(6, 5, 1),
                new Vector3(-1,5,1),
            });

            var trims = _peaks.TrimmedTo(new[] { p1 });
            foreach (var l in trims)
            {
                this.Model.AddElement(new Panel(l, random.NextMaterial()));
                this.Model.AddElement(new ModelCurve(l, random.NextMaterial()));
            }
            Assert.Single(trims);

            // This will return the entire original polygon.
            var trims1 = _peaks.TrimmedTo(new[] { p1.Reversed() });
            Assert.Single(trims1);
        }

        [Fact]
        public void PlaneIntersectsThroughEdges()
        {
            var t = new Transform(Vector3.Origin, Vector3.XAxis, Vector3.YAxis.Negate());
            var polygon = Polygon.Star(5, 2, 5).TransformedPolygon(t);
            var plane = new Plane(new Vector3(0, 0, -2.5), Vector3.ZAxis);
            var intersects = polygon.Intersects(plane, out List<Vector3> results);
            Assert.True(intersects);
            Assert.Equal<int>(4, results.Count);
        }

        [Fact]
        public void PlaneIntersectsAtCoincidentPoint()
        {
            // A plane coincident with the right-most point of the star.
            var t = new Transform(Vector3.Origin, Vector3.XAxis, Vector3.YAxis.Negate());
            var polygon = Polygon.Star(5, 2, 5).TransformedPolygon(t);
            var verticalRightPlane = new Plane(new Vector3(5.0, 0, 0), Vector3.XAxis);
            var intersectsRight = polygon.Intersects(verticalRightPlane, out List<Vector3> resultsRight);
            Assert.True(intersectsRight);
            Assert.Single(resultsRight);
        }

        [Fact]
        public void IntersectionResultsAreOrderedAlongPlane()
        {
            var t = new Transform(Vector3.Origin, Vector3.XAxis, Vector3.YAxis.Negate());
            var polygon = Polygon.Star(5, 2, 5).TransformedPolygon(t);
            var plane = new Plane(new Vector3(0, 0, -2.5), Vector3.ZAxis);
            var intersects = polygon.Intersects(plane, out List<Vector3> results);
            // Assert that results are ordered along the plane
            for (var i = 1; i < results.Count; i++)
            {
                Assert.True(results[i].X > results[i - 1].X);
            }
        }

        [Fact]
        public void PolygonContains3D()
        {
            Name = nameof(PolygonContains3D);
            var t = new Transform(new Vector3(0, 0, 1), new Vector3(0.1, 0.1, 1.0).Unitized());
            var rect = Polygon.Rectangle(5, 5).TransformedPolygon(t);
            Assert.True(rect.Contains(rect.Centroid()));

            var v1 = new Vector3(2.5, -2.5);
            Assert.True(rect.Contains(t.OfPoint(v1), out _));
            Model.AddElement(new ModelCurve(rect));
            var arc = new Circle(v1, 0.1);
            Model.AddElement(new ModelCurve(arc.ToPolygon().TransformedPolygon(t)));

            var star = Polygon.Star(5, 2, 5).TransformedPolygon(t);
            Model.AddElement(new ModelCurve(star));
            var centroid = star.Centroid();
            Assert.True(star.Contains(centroid));
            var arc2 = new Circle(centroid, 0.1);
            Model.AddElement(new ModelCurve(arc2.ToPolygon()));
        }

        [Fact]
        public void PointAtLowerRightVertexIsContained()
        {
            var rect = Polygon.Rectangle(5, 5);
            Assert.True(rect.Contains(new Vector3(2.5, -2.5), out _));
        }

        [Fact]
        public void PointAtUpperRightVertexIsContained()
        {
            var rect = Polygon.Rectangle(5, 5);
            Assert.True(rect.Contains(new Vector3(2.5, 2.5), out _));
        }

        [Fact]
        public void PointOnEdgeIsContained()
        {
            var rect = Polygon.Rectangle(5, 5);
            Assert.True(rect.Contains(new Vector3(0, -2.5), out _));
            Assert.True(rect.Contains(new Vector3(2.5, 0), out _));
            Assert.True(rect.Contains(new Vector3(0, 2.5), out _));
        }

        [Fact]
        public void PointInCenterIsContained()
        {
            var rect = Polygon.Rectangle(5, 5);
            Assert.True(rect.Contains(new Vector3()));
        }

        [Fact]
        public void EnclosingPlanesTrimToFormVolume()
        {
            this.Name = nameof(EnclosingPlanesTrimToFormVolume);

            var bottomT = new Transform(new Vector3(0, 0, 1), Vector3.ZAxis);
            var topT = new Transform(Vector3.Origin, Vector3.ZAxis.Negate());
            topT.Rotate(Vector3.XAxis, 15.0);
            topT.Move(new Vector3(0, 0, 3));
            var t1 = new Transform(new Vector3(3, 0), Vector3.XAxis.Negate());
            var t2 = new Transform(new Vector3(0, 3), Vector3.YAxis.Negate());
            var t3 = new Transform(new Vector3(-3, 0), Vector3.XAxis);
            var t4 = new Transform(new Vector3(0, -3), Vector3.YAxis);
            var transforms = new[] { t1, t2, t3, t4, bottomT, topT };
            var polys = new List<Polygon>();
            foreach (var t in transforms)
            {
                var p = Polygon.Rectangle(10, 10).TransformedPolygon(t);
                polys.Add(p);
                this.Model.AddElement(new ModelCurve(p));
            }
            var r = new Random();
            foreach (var p in polys)
            {
                var other = polys.Where(pp => pp != p).ToList();
                var trimmed = p.TrimmedTo(other, LocalClassification.Outside);

                Assert.Single(trimmed);

                var a = p.Area();
                foreach (var t in trimmed)
                {
                    // These are naive checks to ensure that the trimmed
                    // polygons don't look like the originals.
                    Assert.NotEqual(t.Vertices, p.Vertices);
                    Assert.NotEqual(a, t.Area());
                    this.Model.AddElement(new Panel(t, r.NextMaterial()));
                }
            }
        }

        [Fact]
        public void PolygonIsTrimmedByPolygons()
        {
            this.Name = nameof(PolygonIsTrimmedByPolygons);
            var random = new Random();

            var hex = Polygon.Ngon(6, 3);
            var star = Polygon.Star(5, 2, 5).TransformedPolygon(new Transform(new Vector3(0, 0, 1), new Vector3(0.1, 0.1, 1.0).Unitized()));
            var segs = hex.Segments();
            var trimPolys = new List<Polygon>();

            for (var i = 0; i < segs.Count(); i++)
            {
                var s = segs[i];
                var p = new Polygon(new[]{
                    s.Start,
                    s.End,
                    s.End + new Vector3(0,0,2),
                    s.Start + new Vector3(0,0,2)
                });

                this.Model.AddElement(new Panel(p, BuiltInMaterials.Mass));
                trimPolys.Add(p);
            }

            var sw = new Stopwatch();
            sw.Start();
            var trim1 = star.TrimmedTo(trimPolys);
            var trim2 = star.TrimmedTo(trimPolys.Select(tp => tp.Reversed()).ToList());
            sw.Stop();
            _output.WriteLine($"{sw.Elapsed.TotalMilliseconds}ms for trimming.");

            Assert.Equal(5, trim1.Count());
            Assert.Single(trim2);

            foreach (var l in trim1)
            {
                this.Model.AddElement(new Panel(l, random.NextMaterial()));
            }
            foreach (var l in trim2)
            {
                this.Model.AddElement(new Panel(l, random.NextMaterial()));
                this.Model.AddElement(new ModelCurve(l, random.NextMaterial()));
            }
        }

        [Fact]
        public void PolygonSplitsAtNewPoint()
        {
            var rect = Polygon.Rectangle(5, 5);
            rect.Split(new[] { new Vector3(2.5, 0) });
            Assert.Equal(5, rect.Vertices.Count());
        }

        [Fact]
        public void PolygonDoesNotSplitAtExistingPoint()
        {
            var rect = Polygon.Rectangle(5, 5);
            rect.Split(new[] { new Vector3(2.5, 2.5) });
            Assert.Equal(4, rect.Vertices.Count());
        }

        [Fact]
        public void PolygonDoesNotSplitAtNonIntersectingPoint()
        {
            var rect = Polygon.Rectangle(5, 5);
            rect.Split(new[] { new Vector3(1, 1) });
            Assert.Equal(4, rect.Vertices.Count());
        }

        [Fact]
        public void CollinearPointCanBeRemoved()
        {
            var points = new Vector3[6]{
                new Vector3( 0, 0, 0),
                new Vector3( 10,0,0),
                new Vector3(20,0,0),
                new Vector3( 20, 20, 0),
                new Vector3( 10, 20, 0),
                new Vector3( 0, 20 , 0)
                };

            var polygon = new Polygon(points);
            polygon = polygon.CollinearPointsRemoved();
            Assert.Equal(4, polygon.Vertices.Count());

            var points2 = new Vector3[] {
                (0, 0, 0),
                (10, 0.0001, 0),
                (20, 0, 0),
                (20, 20, 0),
                (10, 20.0001, 0),
                (0, 20, 0)
            };
            var polygon2 = new Polygon(points2);
            Assert.NotEqual(4, polygon2.CollinearPointsRemoved().Vertices.Count());
            Assert.Equal(4, polygon2.CollinearPointsRemoved(0.001).Vertices.Count());
        }

        [Fact]
        public void OverlappedPolygonTrimsCorrectly()
        {
            this.Name = nameof(OverlappedPolygonTrimsCorrectly);
            var r = new Transform();
            r.Rotate(Vector3.XAxis, 90);
            var p = Polygon.Rectangle(5, 5).TransformedPolygon(r);
            var p1 = Polygon.Rectangle(5, 5).TransformedPolygon(r).TransformedPolygon(new Transform(2.5, 2.5, 2.5));
            var polys = new List<Polygon>();
            for (var i = 0; i < p1.Vertices.Count; i++)
            {
                var a = p1.Vertices[i];
                var b = i == p1.Vertices.Count - 1 ? p1.Vertices[0] : p1.Vertices[i + 1];
                var newP = new Polygon(new List<Vector3>(){
                    a + new Vector3(0,-5,0), b + new Vector3(0,-5,0), b + new Vector3(0,5,0), a + new Vector3(0,5,0)
                });
                polys.Add(newP);
            }
            var trims = p.IntersectAndClassify(polys,
                                               polys,
                                               out _,
                                               out _,
                                               SetClassification.AOutsideB,
                                               SetClassification.AInsideB);
            Assert.Equal(2, trims.Count);
        }

        [Fact]
        public void VerticalPolygonIntersectsHorizontalRoundPolygon()
        {
            // Test the intersection of of perpendicular planes
            // where the plane of intersection of the first, cuts
            // exactly through a point on the other.

            this.Name = nameof(VerticalPolygonIntersectsHorizontalRoundPolygon);
            var sqPoly = new Polygon(new[]{
                new Vector3(-1, 1, -1), new Vector3(1, 1, -1), new Vector3(1, 1, 1), new Vector3(-1,1,1)});
            var circlePoly = new Circle(new Vector3(1, 1), 2).ToPolygon();
            sqPoly.Intersects3d(circlePoly, out List<Vector3> results);
            foreach (var r in results)
            {
                this.Model.AddElement(new ModelCurve(new Circle(new Transform(r), 0.05), BuiltInMaterials.YAxis));
            }
            Assert.Equal(2, results.Count);
            this.Model.AddElement(new Panel(sqPoly));
            this.Model.AddElement(new Panel(circlePoly));
        }

        [Fact]
        public void ExtendWhereSomeSegmentsAreAtOrigin()
        {
            var pgon = new Polygon((8.01, 6, 0),
                                    (8, 6, 0),
                                    (8, 20.90062, 0),
                                    (8.01, 20.90062, 0),
                                    (8.01, 26.90062, 0),
                                    (0, 26.90062, 0),
                                    (0, 0, 0),
                                    (8.01, 0, 0));
            var line = new Line((2.930087, 15.546126, 0), (2.830087, 15.546126, 0));
            var extension = line.ExtendTo(pgon.Segments());
            Assert.True(extension.Direction() == line.Direction());
        }

        [Fact]
        public void LineTrim()
        {
            Name = nameof(LineTrim);
            var boundary = JsonConvert.DeserializeObject<Polygon>("{\"discriminator\":\"Elements.Geometry.Polygon\",\"Vertices\":[{\"X\":27.25008,\"Y\":19.98296,\"Z\":0.0},{\"X\":-14.78244,\"Y\":19.98296,\"Z\":0.0},{\"X\":-14.78244,\"Y\":16.4675,\"Z\":0.0},{\"X\":27.25008,\"Y\":16.4675,\"Z\":0.0}]}");
            var line = JsonConvert.DeserializeObject<Line>("{\"discriminator\": \"Elements.Geometry.Line\",\"Start\": {\"X\": -0.771609999999999,\"Y\": 16.46749,\"Z\": 0.0},\"End\": {\"X\": -0.771609999999999,\"Y\": 19.98295,\"Z\": 0.0}\n}");

            var trimmed = line.Trim(boundary, out var remainder);
            Assert.True(trimmed.Sum(l => l.Length()) > 0);

        }

        [Fact]
        public void PolygonInsideAnotherPolygonTrimsAHole()
        {
            this.Name = nameof(PolygonInsideAnotherPolygonTrimsAHole);

            var p1 = Polygon.Rectangle(2, 2);
            var trims = Polygon.Star(0.75, 0.5, 5).Segments().Select(s => new Polygon(new[] {
                s.Start - new Vector3(0,0,0.5),
                s.End - new Vector3(0,0,0.5),
                s.End + new Vector3(0,0,0.5),
                s.Start + new Vector3(0,0,0.5)
            })).ToList();
            var polys = p1.IntersectOneToMany(trims, out _, out var trimEdges);

            // In the disjoint scenario we expect one outer, and two inner
            // wound in opposite directions.
            Assert.Equal(2, polys.Count);
            var r = new Random();
            foreach (var p in polys)
            {
                Model.AddElement(new Panel(p, r.NextMaterial()));
            }
        }

        [Fact]
        public void PolygonAcrossPolygonTrimsIntoThree()
        {
            this.Name = nameof(PolygonAcrossPolygonTrimsIntoThree);

            var p1 = Polygon.Rectangle(2, 2);
            var p2 = Polygon.Rectangle(0.5, 4);
            var p3 = Polygon.Rectangle(4.0, 0.5);
            var trims = p2.Segments().Select(s => new Polygon(new[] {
                s.Start - new Vector3(0,0,0.5),
                s.End - new Vector3(0,0,0.5),
                s.End + new Vector3(0,0,0.5),
                s.Start + new Vector3(0,0,0.5)
            })).ToList();

            var polys = p1.IntersectAndClassify(trims, trims, out _, out var trimEdges);
            var r = new Random();

            var aInB = r.NextMaterial();
            var aOutB = r.NextMaterial();
            var bInA = r.NextMaterial();
            var bOutA = r.NextMaterial();
            foreach (var p in polys)
            {
                var m = BuiltInMaterials.Default;
                switch (p.Item2)
                {
                    case SetClassification.AInsideB:
                        m = aInB;
                        break;
                    case SetClassification.AOutsideB:
                        m = aOutB;
                        break;
                    case SetClassification.BInsideA:
                        m = bInA;
                        break;
                    case SetClassification.BOutsideA:
                        m = bOutA;
                        break;
                }
                Model.AddElement(new Panel(p.Item1, m));
            }
            // In the disjoint scenario we expect one outer, and two inner
            // wound in opposite directions.
            Assert.Equal(3, polys.Count);
            Assert.Equal(2, polys.Where(p => p.Item2 == SetClassification.AOutsideB).Count());
            Assert.Single(polys.Where(p => p.Item2 == SetClassification.AInsideB));
        }

        [Fact]
        public void ConstructWithSequentialDuplicates()
        {
            var polygon = new Polygon(new List<Vector3>()
            {
                Vector3.Origin,
                new Vector3(-6.0, 0.0),
                new Vector3(-6.0, -6.0),
                new Vector3(0.0, -6.0),
                Vector3.Origin,
            });
        }


        [Fact]
        public void CoplanarWithinTolerance()
        {
            // this polygon has some small amount of deviation from planar (6.3e-7)
            var json = "{\n            \"discriminator\": \"Elements.Geometry.Polygon\",\n            \"Vertices\": [\n              {\n                \"X\": 30.00108,\n                \"Y\": 0.17123,\n                \"Z\": 24.666666666666668\n              },\n              {\n                \"X\": -2.5323,\n                \"Y\": 0.17123,\n                \"Z\": 24.666666666666668\n              },\n              {\n                \"X\": -2.5322999954223633,\n                \"Y\": -8.758851356437756,\n                \"Z\": 24.66666603088379\n              },\n              {\n                \"X\": -2.5323,\n                \"Y\": -21.3088,\n                \"Z\": 24.666666666666668\n              },\n              {\n                \"X\": 7.8653690051598115,\n                \"Y\": -21.308799743652344,\n                \"Z\": 24.66666603088379\n              },\n              {\n                \"X\": 14.06867950383497,\n                \"Y\": -21.308799743652344,\n                \"Z\": 24.66666603088379\n              },\n              {\n                \"X\": 21.64777460957137,\n                \"Y\": -21.308799743652344,\n                \"Z\": 24.66666603088379\n              },\n              {\n                \"X\": 30.00108,\n                \"Y\": -21.3088,\n                \"Z\": 24.666666666666668\n              }\n            ]\n          }";
            // verify does not throw
            var polygon = JsonConvert.DeserializeObject<Polygon>(json);
            Assert.NotNull(polygon);
        }

        [Fact]
        public void U()
        {
            var u = Polygon.U(10, 20, 2);
        }

        [Fact]
        public void UThicknessGreaterThanWidthOverTwoThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Polygon.U(10, 10, 6));
        }

        [Fact]
        public void TrimSkyPlanePolygon1()
        {
            Name = nameof(TrimSkyPlanePolygon1);
            var json = "{\"Item1\":{\"discriminator\":\"Elements.Geometry.Polygon\",\"Vertices\":[{\"X\":5.456602446962343,\"Y\":-0.9024812458451947,\"Z\":60.0},{\"X\":22.56585182362492,\"Y\":32.63913758323735,\"Z\":60.0},{\"X\":12.232537580045737,\"Y\":37.91005939478421,\"Z\":31.000000000000313},{\"X\":-4.876711796617052,\"Y\":4.3684405657018885,\"Z\":30.9999999999996}]},\"Item2\":{\"Origin\":{\"X\":18.54496511754798,\"Y\":34.69015228590052,\"Z\":31.000000000000266},\"Normal\":{\"X\":0.4218903484051781,\"Y\":0.8270897771341386,\"Z\":0.3713906763541038}}}";
            var items = JsonConvert.DeserializeObject<(Polygon, Plane)>(json);
            var trimmed = items.Item1.Trimmed(items.Item2, true);
            Assert.Single(trimmed);
            Model.AddElement(new Panel(trimmed[0]));
            Model.AddElement(new Panel(Polygon.Rectangle(100, 100).TransformedPolygon(new Transform(items.Item2.Origin, items.Item2.Normal)), BuiltInMaterials.Glass));
            Model.AddElement(new ModelCurve(items.Item1));
        }

        [Fact]
        public void TrimSkyPlanePolygon2()
        {
            Name = nameof(TrimSkyPlanePolygon2);
            var json = "{\"Item1\":{\"discriminator\":\"Elements.Geometry.Polygon\",\"Vertices\":[{\"X\":14.11044881100593,\"Y\":-5.316723034955274,\"Z\":60.0},{\"X\":24.445333749146343,\"Y\":-10.58844604232641,\"Z\":30.999999999999876},{\"X\":40.96973706172442,\"Y\":23.25149700325788,\"Z\":30.999999999999172},{\"X\":30.63485212358372,\"Y\":28.523220010629068,\"Z\":60.0}]},\"Item2\":{\"Origin\":{\"X\":18.54496511754798,\"Y\":34.69015228590052,\"Z\":31.000000000000266},\"Normal\":{\"X\":0.4218903484051781,\"Y\":0.8270897771341386,\"Z\":0.3713906763541038}}}";
            var items = JsonConvert.DeserializeObject<(Polygon, Plane)>(json);
            var trimmed = items.Item1.Trimmed(items.Item2, true);
            Assert.Single(trimmed);
            Model.AddElement(new Panel(trimmed[0]));
            Model.AddElement(new Panel(Polygon.Rectangle(100, 100).TransformedPolygon(new Transform(items.Item2.Origin, items.Item2.Normal)), BuiltInMaterials.Glass));
            Model.AddElement(new ModelCurve(items.Item1));
        }

        [Fact]
        public void TrimSkyPlanePolygon3()
        {
            Name = nameof(TrimSkyPlanePolygon3);
            var json = "{\"Item1\":{\"discriminator\":\"Elements.Geometry.Polygon\",\"Vertices\":[{\"X\":23.128761583107114,\"Y\":-13.28462370583442,\"Z\":30.99999999999992},{\"X\":23.12876158310712,\"Y\":-13.284623705834392,\"Z\":-7.588341022412967E-15},{\"X\":40.969737061724466,\"Y\":23.251497003257874,\"Z\":-4.6993153936072985E-15},{\"X\":40.96973706172446,\"Y\":23.251497003257846,\"Z\":30.99999999999918}]},\"Item2\":{\"Origin\":{\"X\":18.54496511754798,\"Y\":34.69015228590052,\"Z\":31.000000000000266},\"Normal\":{\"X\":0.4218903484051781,\"Y\":0.8270897771341386,\"Z\":0.3713906763541038}}}";
            var items = JsonConvert.DeserializeObject<(Polygon, Plane)>(json);
            var trimmed = items.Item1.Trimmed(items.Item2, true);
            Assert.Null(trimmed);
            Model.AddElement(new Panel(Polygon.Rectangle(100, 100).TransformedPolygon(new Transform(items.Item2.Origin, items.Item2.Normal)), BuiltInMaterials.Glass));
            Model.AddElement(new ModelCurve(items.Item1));
        }

        [Fact]
        public void BigRectangleContainsSmallRectangle()
        {
            var r1 = Polygon.Rectangle(2, 2);
            var r2 = Polygon.Rectangle(1, 1).TransformedPolygon(new Transform(new Vector3(0.5, 0.5), Vector3.ZAxis));
            Assert.True(r1.Contains3D(r2));
        }
    }
}