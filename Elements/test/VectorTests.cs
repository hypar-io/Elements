using Elements.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Xunit;

namespace Elements.Tests
{
    public class VectorTests
    {
        [Fact]
        public void Vector3_TwoVectors_Equal()
        {
            var v1 = new Vector3(5, 5, 5);
            var v2 = new Vector3(5, 5, 5);
            Assert.True(v1.Equals(v2));

            var v3 = new Vector3();
            Assert.True(!v1.Equals(v3));
        }

        [Fact]
        public void Vector3_AnglesBetween_Success()
        {
            var a = Vector3.XAxis;
            var b = Vector3.YAxis;
            Assert.Equal(Math.PI / 2 * 180 / Math.PI, a.AngleTo(b), 5);

            var c = new Vector3(1, 1, 0);
            Assert.Equal(Math.PI / 4 * 180 / Math.PI, a.AngleTo(c), 5);

            Assert.Equal(0.0, a.AngleTo(a), 5);
        }

        [Fact]
        public void Vector3AngleBetweenSameAngleEqualsZero()
        {
            var a = new Vector3(0.9853, -0.1708);
            Assert.Equal(0.0, a.AngleTo(a));
        }

        [Fact]
        public void Vector3_PlaneAngles()
        {
            var a = Vector3.YAxis;
            var b = new Vector3(3, 7, 0);
            var z = new Vector3(0, 0, 9);

            Assert.Equal(336.801, a.PlaneAngleTo(b), 2);
            Assert.Equal(23.198, b.PlaneAngleTo(a), 2);
            Assert.True(Double.IsNaN(b.PlaneAngleTo(z)));

            // parallel
            var p1 = new Vector3(2, 3);
            var p2 = new Vector3(4, 6);
            Assert.Equal(0, p1.PlaneAngleTo(p2, new Vector3(8, 2, 12)));

            // anti-parallel
            Assert.Equal(180, p1.PlaneAngleTo(p2.Negate(), new Vector3(8, 2, 12)));

            // projected very small
            Assert.True(Double.IsNaN(a.PlaneAngleTo(b, b)));
            Assert.True(Double.IsNaN(a.PlaneAngleTo(b, a)));

            var angle = new Vector3(-1, -0.1, 0).PlaneAngleTo(Vector3.XAxis, Vector3.ZAxis);
            Assert.True(angle < 180);
        }

        [Fact]
        public void Vector3_Parallel_AngleBetween_Success()
        {
            var a = Vector3.XAxis;
            var b = Vector3.XAxis;
            Assert.True(a.IsParallelTo(b));

            var c = a.Negate();
            Assert.True(a.IsParallelTo(c));
        }

        [Fact]
        public void Vector3_Parallel_WithTolerance()
        {
            var a = new Vector3(12, 0, 0);
            var b = Vector3.XAxis;
            Assert.True(a.IsParallelTo(b));

            var c = a.Negate();
            var d = new Vector3(12, 0.0001, 0);
            Assert.True(d.IsParallelTo(c));
        }

        [Fact]
        public void Project()
        {
            var p = new Plane(new Vector3(0, 0, 5), Vector3.ZAxis);
            var v = new Vector3(5, 5, 0);
            var v1 = v.Project(p);
            Assert.Equal(v.X, v1.X);
            Assert.Equal(v.Y, v1.Y);
            Assert.Equal(5.0, v1.Z);
        }

        [Fact]
        public void CreateTransform()
        {
            // This list of vectors will make an invalid transform unless we
            // have the Vector3.Epsilon tolerance while doing the dot comparison
            // in the ToTransform() method.
            var points = new List<Vector3>{
                new Vector3(84.65819230015089, 45.936106099249145, 0),
                new Vector3(84.73003434911944, 45.864264050280596, 0),
                new Vector3(85.04991559960003, 45.544382799800005, 0),
                new Vector3(85.04991559960003, 45.544382799800005, 3.0480000000001444),
            };
            var t = points.ToTransform();
            Assert.True(t.XAxis.Length() > 0);
            Assert.True(t.YAxis.Length() > 0);
            Assert.True(t.ZAxis.Length() > 0);
        }

        [Fact]
        public void DistanceToPlane()
        {
            var v = new Vector3(0.5, 0.5, 1.0);
            var p = new Plane(Vector3.Origin, Vector3.ZAxis);
            Assert.Equal(1.0, v.DistanceTo(p));

            v = new Vector3(0.5, 0.5, -1.0);
            Assert.Equal(-1.0, v.DistanceTo(p));

            p = new Plane(Vector3.Origin, Vector3.YAxis);
            v = new Vector3(0.5, 1.0, 0.5);
            Assert.Equal(1.0, v.DistanceTo(p));

            v = Vector3.Origin;
            Assert.Equal(0.0, v.DistanceTo(p));

            p = new Plane(new Vector3(2, 3, 5), Vector3.XAxis);
            v = new Vector3(2, 1.0, 0.5);
            Assert.Equal(0.0, v.DistanceTo(p));

            v = new Vector3(2.5, 1.0, 0.5);
            Assert.Equal(0.5, v.DistanceTo(p));
        }

        [Fact]
        public void DistanceToPolyline()
        {
            var v = new Vector3();

            var pLine1 = new Polyline(new List<Vector3> {
                                            new Vector3(-1,-1),
                                            new Vector3(0,-1),
                                            new Vector3(1,-2)
                                        });
            var dist1 = v.DistanceTo(pLine1, out var closest1);
            Assert.Equal(1, dist1);
            Assert.Equal(new Vector3(0, -1), closest1);

            var pLine2 = new Polyline(new List<Vector3> {
                                            new Vector3(-2,-2),
                                            new Vector3(1,-2),
                                            new Vector3(2,-3)
                                        });
            var dist2 = v.DistanceTo(pLine2, out var closest2);
            Assert.Equal(2, dist2);
            Assert.Equal(new Vector3(0, -2), closest2);
        }

        [Fact]
        public void DistanceToPolygon()
        {
            var squareSize = 10;
            var rect = Polygon.Rectangle(squareSize, squareSize);
            Assert.Equal(0, new Vector3().DistanceTo(rect));
            Assert.Equal(0, new Vector3(-5, 0).DistanceTo(rect));
            Assert.Equal(5, new Vector3(10, 0).DistanceTo(rect));
            Assert.Equal(5, new Vector3(0, 0, 5).DistanceTo(rect));
        }

        [Fact]
        public void AreCoplanar()
        {
            var a = Vector3.Origin;
            var b = new Vector3(1, 0);
            var c = new Vector3(1, 1, 1);

            // Any three points are coplanar.
            Assert.True(new[] { a, b, c }.AreCoplanar());

            var d = new Vector3(5, 5);
            Assert.False(new[] { a, b, c, d }.AreCoplanar());

            var e = new Vector3(1, 0, 0);
            var f = new Vector3(1, 1, 0);
            var g = new Vector3(1, 1, 1);
            var h = new Vector3(1, 0, 1);
            Assert.True(new[] { e, f, g, h }.AreCoplanar());
        }

        [Fact]
        public void AreCoplanar_FourPoints()
        {
            var a = new Vector3(1, 1, 0);
            var b = new Vector3(3, 0, 2);
            var c = new Vector3(1, 5, 0);
            var cross = (b - a).Cross(c - a).Unitized();
            var expected = Math.Sqrt(2) / 2;
            Assert.Equal(cross, new Vector3(-expected, 0, expected));
            var d = new Vector3(-1 + 1e-7, -10, -2 - 1e-7);
            Assert.True(Vector3.AreCoplanar(a, b, c, d));
            b = c + cross.Cross(d - a) * 1.5;
            Assert.True(Vector3.AreCoplanar(a, b, c, d));
            d = new Vector3(4, 4, 4);
            Assert.False(Vector3.AreCoplanar(a, b, c, d));
        }

        [Fact]
        public void NormalFromPlanarWoundPoints()
        {
            var points = new List<Vector3>() {
                new Vector3(0,1,0),
                new Vector3(0,1,1),
                new Vector3(0,0,1),
                new Vector3(0,0,0),
            };
            points.Reverse();
            var normal = points.NormalFromPlanarWoundPoints();
            Assert.Equal(Vector3.XAxis.Negate(), normal);
        }

        [Fact]
        public void CCW()
        {
            var a = new Vector3();
            var b = new Vector3(5, 0);
            var c = new Vector3(5, 5);
            var d = new Vector3(10, 0);
            Assert.True(Vector3.CCW(a, b, c) > 0);
            Assert.True(Vector3.CCW(c, b, a) < 0);
            Assert.True(Vector3.CCW(a, b, d) == 0);
        }

        [Fact]
        public void ToleranceInvariance()
        {
            var polygon = new Polygon(new[] {
                new Vector3(0,0),
                new Vector3(Vector3.EPSILON * 1.1, 0),
                new Vector3(4, 0),
                new Vector3(4,4),
                new Vector3(0,4)
            });
            var rotation = new Transform();
            rotation.Rotate(Vector3.ZAxis, 45);
            var rotatedPolygon = polygon.TransformedPolygon(rotation);
            // Should execute without exception.
        }

        [Fact]
        public void ProfileContains()
        {
            var perimeter = new Polygon(new[]
            {
                new Vector3(-3.6674344150115,-0.575821478244796,0),
                new Vector3(-1.00768758692839,2.61861672249419,0),
                new Vector3(-1.99481012106233,-0.877442252563498,0),
                new Vector3(1.85770976909928,3.12588802475747,0),
                new Vector3(3.72227455579672,2.49522640572745,0),
                new Vector3(1.99481012106233,0.562111443048493,0),
                new Vector3(-0.500416284665121,-0.411301055889139,0),
                new Vector3(2.99564269039257,0.534691372655883,0),
                new Vector3(2.88596240882213,1.1653529916859,0),
                new Vector3(3.79082473177824,1.42584366041569,0),
                new Vector3(4.33922613963043,-1.08309278050807,0),
                new Vector3(0.664936707020777,-2.82426725043876,0),
                new Vector3(3.16016311274823,-2.89281742642029,0),
                new Vector3(3.76340466138563,-2.13876549062353,0),
                new Vector3(4.13357561168586,-3.00249770799072,0),
                new Vector3(2.22788071939951,-3.72912957339487,0),
                new Vector3(-1.3093083612471,-3.11217798956116,0),
                new Vector3(1.65205924115471,-1.49439383639721,0),
                new Vector3(-2.43353124734408,-1.67262429394917,0),
                new Vector3(-3.24242332392606,-3.20814823593529,0),
                new Vector3(-3.98276522452651,-2.27586584258658,0),
                new Vector3(-2.85854233842953,-0.671791724618928,0)
            });
            var voids = new Polygon[]
            {
                new Polygon(new []
                {
                    new Vector3(2.95451258480366,-0.82260211177828,0),
                    new Vector3(2.83210175913191,-0.184080777868908,0),
                    new Vector3(0.18606496624511,-0.691352080132181,0),
                    new Vector3(0.308475791916855,-1.32987341404155,0)
                }),
                new Polygon(new []
                {
                    new Vector3(-2.44724128254038,-0.671791724618928,0),
                    new Vector3(-1.86692577850678,-0.130163920854226,0),
                    new Vector3(-2.25080676400331,0.281137135034914,0),
                    new Vector3(-2.83112226803692,-0.260490668729788,0)
                }),
                new Polygon(new []
                {
                    new Vector3(2.37869110655886,1.63149418836026,0),
                    new Vector3(2.37869110655886,2.6323267576905,0),
                    new Vector3(1.93996998027711,2.6323267576905,0),
                    new Vector3(1.93996998027711,1.63149418836026,0)
                }),
                new Polygon(new []
                {
                    new Vector3(-3.28355342951497,-2.6460367928868,0),
                    new Vector3(-2.87188183213404,-2.34663926751885,0),
                    new Vector3(-3.20092267684535,-1.8942081060408,0),
                    new Vector3(-3.61259427422628,-2.19360563140875,0)
                })
            };

            var profile = new Profile(perimeter, voids, default(Guid), "");
            var ptsToTest = new Dictionary<Vector3, bool>
            {
               {new Vector3(-1.91334741463405,-2.96997766092954,0), false},
                {new Vector3(4.32167548616329,2.2746423178971,0), false},
                {new Vector3(-2.52934817318559,3.08382557184115,0), false},
                {new Vector3(3.68051948709585,-3.34290823110025,0), false},
                {new Vector3(1.47363235412019,-0.318362797170227,0), true},
                {new Vector3(1.03151184379788,3.03068639229677,0), false},
                {new Vector3(-2.95289884475764,-0.0462495805211567,0), true},
                {new Vector3(-0.621995215911255,1.05705346447982,0), false},
                {new Vector3(4.10420657061144,-0.566629455682417,0), true},
                {new Vector3(0.330286098829276,-3.5901259123077,0), false},
                {new Vector3(-0.24932288983424,-1.64532269157363,0), false},
                {new Vector3(-3.82465396213172,-1.83361135499892,0), false},
                {new Vector3(2.56729062582401,1.02546126481966,0), false},
                {new Vector3(-3.80767261083786,1.65455831105201,0), false},
                {new Vector3(2.00514204041448,-2.92098965628603,0), true},
                {new Vector3(2.88074247023243,2.72485127897336,0), true},
                {new Vector3(-0.524706555697834,2.81824453284872,0), false},
                {new Vector3(-1.84098244224056,-1.3790147374684,0), true},
                {new Vector3(3.39908309297339,-1.84493503651402,0), false},
                {new Vector3(0.65205689391488,1.13625251347017,0), true},
                {new Vector3(-2.04425197929765,1.37541650435303,0), false},
                {new Vector3(-0.661902074460968,-0.504785285552072,0), true},
                {new Vector3(1.82278238764248,-1.59523972080554,0), true},
                {new Vector3(-3.620949136078,-3.63013797953193,0), false},
                {new Vector3(1.91472943578338,2.12193876071354,0), true},
                {new Vector3(3.63328005158634,0.635947130047641,0), true},
                {new Vector3(2.52386727577964,-0.823794004369836,0), false},
                {new Vector3(-3.77633611233577,3.09833381074556,0), false},
                {new Vector3(-2.8513912108739,-1.29825685358609,0), true},
                {new Vector3(-1.07527845574056,1.94299642234845,0), false},
                {new Vector3(3.35311117131022,1.57950690967399,0), false},
                {new Vector3(0.185824335404782,2.04076518348335,0), false},
                {new Vector3(-0.589561535387669,-2.60849165452688,0), false},
                {new Vector3(-2.12805663220433,-0.384342162467137,0), true},
                {new Vector3(0.273964292487538,-0.0382582098835089,0), true},
                {new Vector3(4.27599299761961,-1.53701107888109,0), false},
                {new Vector3(0.295742755147601,-0.929507978140383,0), false},
                {new Vector3(0.94757983923356,-2.19128236423104,0), true},
                {new Vector3(-3.21033981959072,-2.60966643313582,0), true},
                {new Vector3(-3.97378329188504,-0.600818882481563,0), false},
                {new Vector3(-1.31960908449334,0.135866691870123,0), false},
                {new Vector3(-2.99607308344248,0.975188260982761,0), false},
                {new Vector3(-1.25204077393947,-3.69544796442061,0), false},
                {new Vector3(1.52221861779236,-3.61031508082525,0), false},
                {new Vector3(4.12224219738895,-2.45367193809444,0), false},
                {new Vector3(1.60987927877555,0.748894350021771,0), true},
                {new Vector3(2.92062349650421,-0.0420986577974796,0), true},
                {new Vector3(3.75635173169776,3.12459364393421,0), false},
                {new Vector3(-1.72896438455763,2.77930858868642,0), false},
                {new Vector3(-3.69588313355008,0.353927908199615,0), false},
                {new Vector3(-1.05754567425085,-1.5659779352417,0), true},
                {new Vector3(-2.95449505167064,2.0902905298356,0), false},
                {new Vector3(-2.73666210483694,-3.39252895469605,0), false},
                {new Vector3(2.7012550831002,-2.14178129392341,0), false},
                {new Vector3(-0.437686977758653,-3.44893495246733,0), false},
                {new Vector3(-2.5070807224777,-2.01478737356734,0), false},
                {new Vector3(-2.22516037035492,2.18440936771125,0), false},
                {new Vector3(3.46734282579874,-2.6629153161418,0), true},
                {new Vector3(1.78543934198807,2.87785559416455,0), true},
                {new Vector3(2.71723380409013,-3.33835238203706,0), true},
                {new Vector3(-1.54071448801245,-2.30018636029404,0), false},
                {new Vector3(4.19964888561659,1.19884340579508,0), false},
                {new Vector3(1.02936055680281,2.18768247121518,0), true},
                {new Vector3(-2.21247938804578,0.511731988446303,0), true},
                {new Vector3(3.68066540497666,-1.23424440333159,0), true},
                {new Vector3(1.44320901462995,1.5089628180384,0), true},
                {new Vector3(1.06149262823672,-0.905494778763094,0), false},
                {new Vector3(0.0337533424069378,0.873451641803066,0), true},
                {new Vector3(-3.90766383652856,-2.84405525421944,0), false},
                {new Vector3(0.420348882644054,2.68914441583881,0), false},
                {new Vector3(0.918810842844982,-3.02808829988996,0), true},
                {new Vector3(2.56799067823095,1.93157714014166,0), true},
                {new Vector3(-0.358288665854758,0.342974920768614,0), true},
                {new Vector3(-2.5592891706461,-2.70213602580895,0), false},
                {new Vector3(-1.48740995451241,0.74399453446443,0), false},
                {new Vector3(1.77101999385746,-2.3560184666247,0), false},
                {new Vector3(2.73204263988329,-1.44748561978022,0), true},
                {new Vector3(2.35183555451272,0.429594969860303,0), false},
                {new Vector3(-3.36379789663229,-0.567811266523619,0), true},
                {new Vector3(-1.53359780782258,-0.625566192001836,0), true},
                {new Vector3(-1.13394743416365,-3.0069111075288,0), false},
                {new Vector3(0.274582837198435,-2.42718849077329,0), true},
                {new Vector3(3.31168354880983,-0.632021104482929,0), true},
                {new Vector3(4.28637395001078,0.220961787192477,0), false},
                {new Vector3(-0.407346765810272,1.70007411670455,0), false},
                {new Vector3(3.48461554007624,2.33450300909345,0), true},
                {new Vector3(-3.61678678725695,-1.18027072158594,0), false},
                {new Vector3(-0.100779719501275,-2.90429416331805,0), true},
                {new Vector3(1.93374194196669,-0.908690719239205,0), false},
                {new Vector3(-3.23747431017598,-2.05846009913801,0), false},
                {new Vector3(3.01025089444163,0.75774928707979,0), true},
                {new Vector3(0.841728085027284,-0.0584785781459851,0), true},
                {new Vector3(-2.54833104549111,1.59963309135231,0), false},
                {new Vector3(1.16594499067128,-1.58753660465589,0), false},
                {new Vector3(-2.29266516590198,-1.03134304218741,0), true},
                {new Vector3(0.974686075522627,0.642437127079215,0), true},
                {new Vector3(-0.0486675320265331,-0.440921485770396,0), true},
                {new Vector3(2.00274993117517,-0.328307525932269,0), true},
                {new Vector3(4.21286785824697,-3.72598025696974,0), false},
                {new Vector3(-3.58071192586354,1.0545253530434,0), false}
            };

            foreach (var pt in ptsToTest)
            {
                var includes = profile.Contains(pt.Key, out _);
                Assert.True(includes == pt.Value);
            }
        }

        [Fact]
        public void CollinearByDistance()
        {
            Vector3 p0 = new Vector3(0, 0, 0);
            Vector3 p1 = new Vector3(10, 10, 10);
            Vector3 p2 = new Vector3(20, 20, 20);
            Vector3 p3 = new Vector3(15, 5, 20);

            Vector3 p4 = new Vector3(0, -118.7170, 13.8152);
            Vector3 p5 = new Vector3(0, -80.4465, 13.8152);
            Vector3 p6 = new Vector3(0, -118.7170, 13.8173);
            Vector3 p7 = new Vector3(0, 33.5632, 13.8173);

            Assert.True(Vector3.AreCollinearByDistance(p0, p1, p2));
            Assert.False(Vector3.AreCollinearByDistance(p0, p1, p3));

            var zeroPlane = new Plane(Vector3.Origin, Vector3.YAxis);
            Assert.False(p4.Project(zeroPlane).IsAlmostEqualTo(p6.Project(zeroPlane)));
            Assert.False(Vector3.AreCollinearByDistance(p4, p5, p6));
            Assert.False(Vector3.AreCollinearByDistance(p4, p5, p7));
        }

        [Fact]
        public void CollinearByAngle()
        {
            Vector3 p0 = new Vector3(0, 0, 0);
            Vector3 p1 = new Vector3(10, 10, 10);
            Vector3 p2 = new Vector3(20, 20, 20);
            Vector3 p3 = new Vector3(15, 5, 20);

            Assert.True(Vector3.AreCollinearByDistance(p0, p1, p2));
            Assert.False(Vector3.AreCollinearByDistance(p0, p1, p3));

            // Small angle delta can accumulate significant distance delta
            Vector3 p4 = new Vector3(10000, 0);
            Vector3 p5 = new Vector3(20000, 0.1);
            Assert.True(Vector3.AreCollinearByAngle(p0, p4, p5));
            Assert.False(Vector3.AreCollinearByAngle(p0, p4, p5, Math.Cos(Units.DegreesToRadians(0.0001))));

            // Order is important
            Vector3 p6 = new Vector3(10, 0);
            Vector3 p7 = new Vector3(10, 0.0001);
            Assert.False(Vector3.AreCollinearByAngle(p0, p6, p7));
            Assert.True(Vector3.AreCollinearByAngle(p6, p0, p7));
        }

        [Fact]
        public void CollinearWithCoincidentPoints()
        {
            Vector3 p0 = new Vector3(13.770340049720485, 2.8262770874797756);
            Vector3 p1 = new Vector3(14.68227083781888, -2.387949898514293);
            Vector3 p2 = new Vector3(14.682270837818884, -2.3879498985142926);
            Vector3 p3 = new Vector3(11.069392253545676, 18.26972418936262);

            Vector3 p4 = new Vector3(0, 0);
            Vector3 p5 = new Vector3(1, 0);
            Vector3 p6 = new Vector3(1, 0.000000000001);
            Vector3 p7 = new Vector3(2, 0);

            Assert.True(p1.IsAlmostEqualTo(p2));
            Assert.True(new[] { p0, p1, p2, p3 }.AreCollinearByDistance());
            Assert.True(p5.IsAlmostEqualTo(p6));
            Assert.True(new[] { p4, p5, p6, p7 }.AreCollinearByDistance());
        }

        [Fact]
        public void TupleSyntax()
        {
            // Integer Tuples + params constructor
            var polygon = new Polygon((0, 0), (10, 0), (10, 10));
            Assert.Equal(50, polygon.Area());

            // Mixed tuples + params constructor
            var polyline = new Polyline((0, 0, 0), (0.5, 0), (1, 0.5));
        }

        [Fact]
        public void HashCodesForDifferentComponentsAreNotEqual()
        {
            var a = new Vector3(1, 2, 3);
            var b = new Vector3(3, 2, 1);
            Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void ClosestPointOnInfiniteLine()
        {
            var line = new Line(Vector3.Origin, new Vector3(10, 10));

            Assert.True(new Vector3(2, 8).ClosestPointOn(line, true).IsAlmostEqualTo(new Vector3(5, 5)));

            var vector = new Vector3(-2, -8);
            var closestPointSegment = vector.ClosestPointOn(line);
            var closestPointInfinite = vector.ClosestPointOn(line, true);
            Assert.True(closestPointSegment.IsAlmostEqualTo(new Vector3(0, 0)));
            Assert.True(closestPointInfinite.IsAlmostEqualTo(new Vector3(-5, -5)));
        }

        [Fact]
        public void UniqueWithinToleranceReturnsNewCollection()
        {
            var vectorsList = new List<Vector3>
            {
                Vector3.Origin,
                new Vector3(0.000009, 0, 0),
                new Vector3(0, -0.000009, 0),
                new Vector3(5, 5),
                new Vector3(5, 5, 0.000009),
                Vector3.Origin,
                new Vector3(5,5)
            };

            var result = vectorsList.UniqueWithinTolerance();

            Assert.Collection(result,
                x => x.IsAlmostEqualTo(Vector3.Origin),
                x => x.IsAlmostEqualTo(new Vector3(5, 5)));
        }

        [Fact]
        public void UniqueWithinToleranceReturnsNewCollectionWithTolerance()
        {
            var tolerance = 0.2;

            var vectorsList = new List<Vector3>
            {
                new Vector3(0.1, 0, 0),
                Vector3.Origin,
                new Vector3(0, -0.1, 0),
                new Vector3(5, 5, 0.1),
                new Vector3(5, 5),
                Vector3.Origin,
                new Vector3(5,5)
            };

            var result = vectorsList.UniqueWithinTolerance(tolerance);

            Assert.Collection(result,
                x => x.IsAlmostEqualTo(Vector3.Origin, tolerance),
                x => x.IsAlmostEqualTo(new Vector3(5, 5), tolerance));
        }

        [Fact]
        public void PointsAreCoplanarWithinTolerance()
        {
            var ptsSerialized = "[{\"X\":0.0,\"Y\":0.0,\"Z\":0.0},{\"X\":20.0,\"Y\":0.0,\"Z\":-8.43769498715119E-15},{\"X\":19.999999999999996,\"Y\":20.0,\"Z\":-1.021405182655144E-14},{\"X\":9.999999999999995,\"Y\":20.0,\"Z\":-3.096967127191874E-13},{\"X\":10.0,\"Y\":10.0,\"Z\":-4.218847493575595E-15},{\"X\":1.7763568394002505E-15,\"Y\":9.999999999999998,\"Z\":0.0}]";
            var pts = JsonConvert.DeserializeObject<List<Vector3>>(ptsSerialized);
            Assert.True(pts.AreCoplanar());
        }
    }
}