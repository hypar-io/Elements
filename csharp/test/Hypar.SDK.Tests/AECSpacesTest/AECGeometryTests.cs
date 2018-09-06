using System.Collections.Generic;
using AECSpaces;
using Xunit;

namespace AECSpacesTest
{
    public class GeometryTests
    {
        const double dblVariance = 0.000000001;

        [Fact]
        public void AngleConvex()
        {
            AECPoint point0 = new AECPoint(0, 0, 0);
            AECPoint point1 = new AECPoint(3, 0, 0);
            AECPoint point2 = new AECPoint(0, 3, 0);
            Assert.True(AECGeometry.IsConvexAngle(point0, point2, point1));
        }

        [Fact]
        public void AngleConcave()
        {
            AECPoint point0 = new AECPoint(3, 0, 0);
            AECPoint point1 = new AECPoint(2, 1, 0);
            AECPoint point2 = new AECPoint(3, 3, 0);
            Assert.False(AECGeometry.IsConvexAngle(point1, point0, point2));
        }

        [Fact]
        public void AreColinear()
        {
            AECGeometry geometry = new AECGeometry();

            AECPoint thisPoint = new AECPoint(1, 1, 0);
            AECPoint thatPoint = new AECPoint(2, 1, 0);
            AECPoint othrPoint = new AECPoint(3, 1, 0);

            Assert.True(geometry.AreColinear(thisPoint, thatPoint, othrPoint));

            thisPoint = new AECPoint(1, 1, 0);
            thatPoint = new AECPoint(2, 5, 0);
            othrPoint = new AECPoint(3, 1, 0);

            Assert.False(geometry.AreColinear(thisPoint, thatPoint, othrPoint));

        }

        [Fact]
        public void AreOverlapping()
        {
            AECGeometry geometry = new AECGeometry();
            List<AECPoint> thesePoints = new List<AECPoint>
            {
                new AECPoint(1, 1, 0),
                new AECPoint(5, 0, 0),
                new AECPoint(5, 5, 0),
                new AECPoint(0, 5, 0)
            };
            List<AECPoint> thosePoints = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(3, 0, 0),
                new AECPoint(2, 1, 0),
                new AECPoint(3, 3, 0),
                new AECPoint(0, 3, 0)
            };
            Assert.True(geometry.AreOverlapping(thesePoints, thosePoints));
        }//method

        [Fact]
        public void Box()
        {
            AECPoint thisPoint = new AECPoint(0, 0, 0);
            AECPoint thatPoint = new AECPoint(1, 1, 0);
            AECBox box = AECGeometry.Box(thisPoint, thatPoint);

            Assert.Equal(0, box.NW.X, 0);
            Assert.Equal(1, box.NW.Y, 0);

            Assert.Equal(1, box.SE.X, 0);
            Assert.Equal(0, box.SE.Y, 0);

            thisPoint = new AECPoint(0, 0, 0);
            thatPoint = new AECPoint(-1, -1, 0);
            box = AECGeometry.Box(thisPoint, thatPoint);

            Assert.Equal(-1, box.NW.X, 0);
            Assert.Equal(0, box.NW.Y, 0);

            Assert.Equal(0, box.SE.X, 0);
            Assert.Equal(-1, box.SE.Y, 0);

            thisPoint = new AECPoint(0, 0, 0);
            thatPoint = new AECPoint(0, 1, 0);
            box = AECGeometry.Box(thisPoint, thatPoint);

            Assert.Null(box.SW);
            Assert.Null(box.SE);
            Assert.Null(box.NE);
            Assert.Null(box.NW);
        }//method

        [Fact]
        public void CompassLine()
        {
            AECBox box = new AECBox
            {
                SW = new AECPoint(0, 0, 0),
                SE = new AECPoint(8, 0, 0),
                NE = new AECPoint(8, 8, 0),
                NW = new AECPoint(0, 8, 0)
            };

            List<AECPoint> line = AECGeometry.CompassLine(box, AECGeometry.Compass.N);
            Assert.Equal(4, line[0].X, 0);
            Assert.Equal(4, line[0].Y, 0);
            AECPoint point = line[1];
            Assert.Equal(4, point.X, 0);
            Assert.Equal(8, point.Y, 0);

            line = AECGeometry.CompassLine(box, AECGeometry.Compass.E);
            point = line[1];
            Assert.Equal(8, point.X, 0);
            Assert.Equal(4, point.Y, 0);

            line = AECGeometry.CompassLine(box, AECGeometry.Compass.S);
            point = line[1];
            Assert.Equal(4, point.X, 0);
            Assert.Equal(0, point.Y, 0);

            line = AECGeometry.CompassLine(box, AECGeometry.Compass.W);
            point = line[1];
            Assert.Equal(0, point.X, 0);
            Assert.Equal(4, point.Y, 0);

            line = AECGeometry.CompassLine(box, AECGeometry.Compass.NNE);
            point = line[1];
            Assert.Equal(6, point.X, 0);
            Assert.Equal(8, point.Y, 0);

            line = AECGeometry.CompassLine(box, AECGeometry.Compass.ENE);
            point = line[1];
            Assert.Equal(8, point.X, 0);
            Assert.Equal(6, point.Y, 0);

            line = AECGeometry.CompassLine(box, AECGeometry.Compass.ESE);
            point = line[1];
            Assert.Equal(8, point.X, 0);
            Assert.Equal(2, point.Y, 0);

            line = AECGeometry.CompassLine(box, AECGeometry.Compass.SSE);
            point = line[1];
            Assert.Equal(6, point.X, 0);
            Assert.Equal(0, point.Y, 0);

            line = AECGeometry.CompassLine(box, AECGeometry.Compass.SSW);
            point = line[1];
            Assert.Equal(2, point.X, 0);
            Assert.Equal(0, point.Y, 0);

            line = AECGeometry.CompassLine(box, AECGeometry.Compass.WSW);
            point = line[1];
            Assert.Equal(0, point.X, 0);
            Assert.Equal(2, point.Y, 0);

            line = AECGeometry.CompassLine(box, AECGeometry.Compass.WNW);
            point = line[1];
            Assert.Equal(0, point.X, 0);
            Assert.Equal(6, point.Y, 0);

            line = AECGeometry.CompassLine(box, AECGeometry.Compass.NNW);
            point = line[1];
            Assert.Equal(2, point.X, 0);
            Assert.Equal(8, point.Y, 0);
        }//method

        [Fact]
        public void CompassPoint()
        {
            AECBox box = new AECBox
            {
                SW = new AECPoint(0, 0, 0),
                SE = new AECPoint(8, 0, 0),
                NE = new AECPoint(8, 8, 0),
                NW = new AECPoint(0, 8, 0)
            };

            AECPoint point = AECGeometry.CompassPoint(box, AECGeometry.Compass.N);
            Assert.Equal(4, point.X, 0);
            Assert.Equal(8, point.Y, 0);

            point = AECGeometry.CompassPoint(box, AECGeometry.Compass.E);
            Assert.Equal(8, point.X, 0);
            Assert.Equal(4, point.Y, 0);

            point = AECGeometry.CompassPoint(box, AECGeometry.Compass.S);
            Assert.Equal(4, point.X, 0);
            Assert.Equal(0, point.Y, 0);

            point = AECGeometry.CompassPoint(box, AECGeometry.Compass.W);
            Assert.Equal(0, point.X, 0);
            Assert.Equal(4, point.Y, 0);

            point = AECGeometry.CompassPoint(box, AECGeometry.Compass.NNE);
            Assert.Equal(6, point.X, 0);
            Assert.Equal(8, point.Y, 0);

            point = AECGeometry.CompassPoint(box, AECGeometry.Compass.ENE);
            Assert.Equal(8, point.X, 0);
            Assert.Equal(6, point.Y, 0);

            point = AECGeometry.CompassPoint(box, AECGeometry.Compass.ESE);
            Assert.Equal(8, point.X, 0);
            Assert.Equal(2, point.Y, 0);

            point = AECGeometry.CompassPoint(box, AECGeometry.Compass.SSE);
            Assert.Equal(6, point.X, 0);
            Assert.Equal(0, point.Y, 0);

            point = AECGeometry.CompassPoint(box, AECGeometry.Compass.SSW);
            Assert.Equal(2, point.X, 0);
            Assert.Equal(0, point.Y, 0);

            point = AECGeometry.CompassPoint(box, AECGeometry.Compass.WSW);
            Assert.Equal(0, point.X, 0);
            Assert.Equal(2, point.Y, 0);

            point = AECGeometry.CompassPoint(box, AECGeometry.Compass.WNW);
            Assert.Equal(0, point.X, 0);
            Assert.Equal(6, point.Y, 0);

            point = AECGeometry.CompassPoint(box, AECGeometry.Compass.NNW);
            Assert.Equal(2, point.X, 0);
            Assert.Equal(8, point.Y, 0);
        }//method

        [Fact]
        public void CoversPoint()
        {
            AECGeometry geometry = new AECGeometry();
            List<AECPoint> shape = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(3, 0, 0),
                new AECPoint(2, 1, 0),
                new AECPoint(3, 3, 0),
                new AECPoint(0, 3, 0)
            };
            AECPoint point = new AECPoint(2, 1, 0);
            Assert.True(geometry.CoversPoint(shape, point));
        }//method

        [Fact]
        public void CoversShape()
        {
            AECGeometry geometry = new AECGeometry();
            List<AECPoint> thesePoints = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(3, 0, 0),
                new AECPoint(2, 1, 0),
                new AECPoint(3, 3, 0),
                new AECPoint(0, 3, 0)
            };
            List<AECPoint> thosePoints = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(3, 0, 0),
                new AECPoint(2, 1, 0),
            };
            Assert.True(geometry.CoversShape(thesePoints, thosePoints));
        }//method

        [Fact]
        public void Midpoint()
        {
            AECPoint start = new AECPoint(0, 0, 0);
            AECPoint end = new AECPoint(3, 3, 3);
            AECPoint mid = AECGeometry.MidPoint(start, end);

            Assert.Equal(1.5, mid.X, 0);
            Assert.Equal(1.5, mid.Y, 0);
            Assert.Equal(1.5, mid.Z, 0);
        }//method

        [Fact]
        public void Normal()
        {
            AECBox box = new AECBox
            {
                SE = new AECPoint(1, 0, 0),
                SW = new AECPoint(0, 0, 0),
                NW = new AECPoint(0, 0, 1)
            };
            AECVector normal = AECGeometry.Normal(box);

            Assert.Equal(0, normal.X, 0);
            Assert.Equal(-1, normal.Y, 0);
            Assert.Equal(0, normal.Z, 0);

            box.SE = new AECPoint(1, 1, 0);
            box.SW = new AECPoint(1, 0, 0);
            box.NW = new AECPoint(1, 0, 1);
            normal = AECGeometry.Normal(box);

            Assert.Equal(1, normal.X, 0);
            Assert.Equal(0, normal.Y, 0);
            Assert.Equal(0, normal.Z, 0);

            box.SE = new AECPoint(0, 1, 0);
            box.SW = new AECPoint(1, 1, 0);
            box.NW = new AECPoint(1, 1, 1);
            normal = AECGeometry.Normal(box);

            Assert.Equal(0, normal.X, 0);
            Assert.Equal(1, normal.Y, 0);
            Assert.Equal(0, normal.Z, 0);

            box.SE = new AECPoint(0, 0, 0);
            box.SW = new AECPoint(0, 1, 0);
            box.NW = new AECPoint(0, 1, 1);
            normal = AECGeometry.Normal(box);

            Assert.Equal(-1, normal.X, 0);
            Assert.Equal(0, normal.Y, 0);
            Assert.Equal(0, normal.Z, 0);

            box.SE = new AECPoint(0, 1, 0);
            box.SW = new AECPoint(0, 0, 0);
            box.NW = new AECPoint(1, 0, 0);
            normal = AECGeometry.Normal(box);

            Assert.Equal(0, normal.X, 0);
            Assert.Equal(0, normal.Y, 0);
            Assert.Equal(-1, normal.Z, 0);

            box.SE = new AECPoint(1, 0, 1);
            box.SW = new AECPoint(0, 0, 1);
            box.NW = new AECPoint(0, 1, 1);
            normal = AECGeometry.Normal(box);

            Assert.Equal(0, normal.X, 0);
            Assert.Equal(0, normal.Y, 0);
            Assert.Equal(1, normal.Z, 0);
        }//method

        [Fact]
        public void PointAlong()
        {
            AECPoint thisPoint = new AECPoint(0, 0, 0);
            AECPoint thatPoint = new AECPoint(0, 20, 0);
            double fraction = AECGeometry.RandomDouble(0, 1);
            AECPoint along = AECGeometry.PointAlong(thisPoint, thatPoint, fraction);

            Assert.Equal(0, along.X, 0);
            Assert.True((along.Y >= 0 && along.Y <= 20));
            Assert.Equal(0, along.Z, 0);
        }//method

        [Fact]
        public void PolygonConcave()
        {
            List<AECPoint> bndPoints = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(3, 0, 0),
                new AECPoint(2, 1, 0),
                new AECPoint(3, 3, 0),
                new AECPoint(0, 3, 0)
            };
            Assert.False(AECGeometry.IsConvexPolygon(bndPoints));
        }

        [Fact]
        public void PolygonConvex()
        {
            List<AECPoint> bndPoints = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(3, 0, 0),
                new AECPoint(3, 3, 0),
                new AECPoint(0, 3, 0)
            };
            Assert.True(AECGeometry.IsConvexPolygon(bndPoints));
        }

        [Fact]
        public void RemoveColinear()
        {
            AECGeometry geometry = new AECGeometry();
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(1, 0, 0),
                new AECPoint(3, 0, 0)
            };
            Assert.Equal(2, geometry.RemoveColinear(points).Count); 

            points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(1, 0, 0),
                new AECPoint(3, 0, 0),
                new AECPoint(3, 3, 0),
                new AECPoint(2, 3, 0),
                new AECPoint(0, 3, 0)
            };
            List<AECPoint> cleanPoints = geometry.RemoveColinear(points);
            Assert.Equal(4, cleanPoints.Count);

            points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(1, 0, 0),
                new AECPoint(1, 1, 0),
                new AECPoint(0, 1, 0)
            };
            cleanPoints = geometry.RemoveColinear(points);
            Assert.Equal(4, cleanPoints.Count);
        }
    }
}
