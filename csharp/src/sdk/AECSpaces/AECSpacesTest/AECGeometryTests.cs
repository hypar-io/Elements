using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AECSpaces;

namespace AECSpacesTest
{
    [TestClass]
    public class GeometryTests
    {
        const double dblVariance = 0.000000001;

        [TestMethod]
        public void AngleConvex()
        {
            AECPoint point0 = new AECPoint(0, 0, 0);
            AECPoint point1 = new AECPoint(3, 0, 0);
            AECPoint point2 = new AECPoint(0, 3, 0);
             Assert.IsTrue(AECGeometry.IsConvexAngle(point0, point2, point1));
        }//method

        [TestMethod]
        public void AngleConcave()
        {
            AECPoint point0 = new AECPoint(3, 0, 0);
            AECPoint point1 = new AECPoint(2, 1, 0);
            AECPoint point2 = new AECPoint(3, 3, 0);
            Assert.IsFalse(AECGeometry.IsConvexAngle(point1, point0, point2));
        }//method

        [TestMethod]
        public void AreColinear()
        {
            AECGeometry geometry = new AECGeometry();

            AECPoint thisPoint = new AECPoint(1, 1, 0);
            AECPoint thatPoint = new AECPoint(2, 1, 0);
            AECPoint othrPoint = new AECPoint(3, 1, 0);

            Assert.IsTrue(geometry.AreColinear(thisPoint, thatPoint, othrPoint));

            thisPoint = new AECPoint(1, 1, 0);
            thatPoint = new AECPoint(2, 5, 0);
            othrPoint = new AECPoint(3, 1, 0);

            Assert.IsFalse(geometry.AreColinear(thisPoint, thatPoint, othrPoint));

        }//method

        [TestMethod]
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
            Assert.IsTrue(geometry.AreOverlapping(thesePoints, thosePoints));
        }//method

        [TestMethod]
        public void Box()
        {
            AECPoint thisPoint = new AECPoint(0, 0, 0);
            AECPoint thatPoint = new AECPoint(1, 1, 0);
            AECBox box = AECGeometry.Box(thisPoint, thatPoint);

            Assert.AreEqual(0, box.NW.X, 0);
            Assert.AreEqual(1, box.NW.Y, 0);

            Assert.AreEqual(1, box.SE.X, 0);
            Assert.AreEqual(0, box.SE.Y, 0);

            thisPoint = new AECPoint(0, 0, 0);
            thatPoint = new AECPoint(-1, -1, 0);
            box = AECGeometry.Box(thisPoint, thatPoint);

            Assert.AreEqual(-1, box.NW.X, 0);
            Assert.AreEqual(0, box.NW.Y, 0);

            Assert.AreEqual(0, box.SE.X, 0);
            Assert.AreEqual(-1, box.SE.Y, 0);

            thisPoint = new AECPoint(0, 0, 0);
            thatPoint = new AECPoint(0, 1, 0);
            box = AECGeometry.Box(thisPoint, thatPoint);

            Assert.IsNull(box.SW);
            Assert.IsNull(box.SE);
            Assert.IsNull(box.NE);
            Assert.IsNull(box.NW);
        }//method

        [TestMethod]
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
            Assert.AreEqual(4, line[0].X, 0);
            Assert.AreEqual(4, line[0].Y, 0);
            AECPoint point = line[1];
            Assert.AreEqual(4, point.X, 0);
            Assert.AreEqual(8, point.Y, 0);

            line = AECGeometry.CompassLine(box, AECGeometry.Compass.E);
            point = line[1];
            Assert.AreEqual(8, point.X, 0);
            Assert.AreEqual(4, point.Y, 0);

            line = AECGeometry.CompassLine(box, AECGeometry.Compass.S);
            point = line[1];
            Assert.AreEqual(4, point.X, 0);
            Assert.AreEqual(0, point.Y, 0);

            line = AECGeometry.CompassLine(box, AECGeometry.Compass.W);
            point = line[1];
            Assert.AreEqual(0, point.X, 0);
            Assert.AreEqual(4, point.Y, 0);

            line = AECGeometry.CompassLine(box, AECGeometry.Compass.NNE);
            point = line[1];
            Assert.AreEqual(6, point.X, 0);
            Assert.AreEqual(8, point.Y, 0);

            line = AECGeometry.CompassLine(box, AECGeometry.Compass.ENE);
            point = line[1];
            Assert.AreEqual(8, point.X, 0);
            Assert.AreEqual(6, point.Y, 0);

            line = AECGeometry.CompassLine(box, AECGeometry.Compass.ESE);
            point = line[1];
            Assert.AreEqual(8, point.X, 0);
            Assert.AreEqual(2, point.Y, 0);

            line = AECGeometry.CompassLine(box, AECGeometry.Compass.SSE);
            point = line[1];
            Assert.AreEqual(6, point.X, 0);
            Assert.AreEqual(0, point.Y, 0);

            line = AECGeometry.CompassLine(box, AECGeometry.Compass.SSW);
            point = line[1];
            Assert.AreEqual(2, point.X, 0);
            Assert.AreEqual(0, point.Y, 0);

            line = AECGeometry.CompassLine(box, AECGeometry.Compass.WSW);
            point = line[1];
            Assert.AreEqual(0, point.X, 0);
            Assert.AreEqual(2, point.Y, 0);

            line = AECGeometry.CompassLine(box, AECGeometry.Compass.WNW);
            point = line[1];
            Assert.AreEqual(0, point.X, 0);
            Assert.AreEqual(6, point.Y, 0);

            line = AECGeometry.CompassLine(box, AECGeometry.Compass.NNW);
            point = line[1];
            Assert.AreEqual(2, point.X, 0);
            Assert.AreEqual(8, point.Y, 0);
        }//method

        [TestMethod]
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
            Assert.AreEqual(4, point.X, 0);
            Assert.AreEqual(8, point.Y, 0);

            point = AECGeometry.CompassPoint(box, AECGeometry.Compass.E);
            Assert.AreEqual(8, point.X, 0);
            Assert.AreEqual(4, point.Y, 0);

            point = AECGeometry.CompassPoint(box, AECGeometry.Compass.S);
            Assert.AreEqual(4, point.X, 0);
            Assert.AreEqual(0, point.Y, 0);

            point = AECGeometry.CompassPoint(box, AECGeometry.Compass.W);
            Assert.AreEqual(0, point.X, 0);
            Assert.AreEqual(4, point.Y, 0);

            point = AECGeometry.CompassPoint(box, AECGeometry.Compass.NNE);
            Assert.AreEqual(6, point.X, 0);
            Assert.AreEqual(8, point.Y, 0);

            point = AECGeometry.CompassPoint(box, AECGeometry.Compass.ENE);
            Assert.AreEqual(8, point.X, 0);
            Assert.AreEqual(6, point.Y, 0);

            point = AECGeometry.CompassPoint(box, AECGeometry.Compass.ESE);
            Assert.AreEqual(8, point.X, 0);
            Assert.AreEqual(2, point.Y, 0);

            point = AECGeometry.CompassPoint(box, AECGeometry.Compass.SSE);
            Assert.AreEqual(6, point.X, 0);
            Assert.AreEqual(0, point.Y, 0);

            point = AECGeometry.CompassPoint(box, AECGeometry.Compass.SSW);
            Assert.AreEqual(2, point.X, 0);
            Assert.AreEqual(0, point.Y, 0);

            point = AECGeometry.CompassPoint(box, AECGeometry.Compass.WSW);
            Assert.AreEqual(0, point.X, 0);
            Assert.AreEqual(2, point.Y, 0);

            point = AECGeometry.CompassPoint(box, AECGeometry.Compass.WNW);
            Assert.AreEqual(0, point.X, 0);
            Assert.AreEqual(6, point.Y, 0);

            point = AECGeometry.CompassPoint(box, AECGeometry.Compass.NNW);
            Assert.AreEqual(2, point.X, 0);
            Assert.AreEqual(8, point.Y, 0);
        }//method

        [TestMethod]
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
            Assert.IsTrue(geometry.CoversPoint(shape, point));
        }//method

        [TestMethod]
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
            Assert.IsTrue(geometry.CoversShape(thesePoints, thosePoints));
        }//method

        [TestMethod]
        public void Midpoint()
        {
            AECPoint start = new AECPoint(0, 0, 0);
            AECPoint end = new AECPoint(3, 3, 3);
            AECPoint mid = AECGeometry.MidPoint(start, end);

            Assert.AreEqual(1.5, mid.X, 0);
            Assert.AreEqual(1.5, mid.Y, 0);
            Assert.AreEqual(1.5, mid.Z, 0);
        }//method

        [TestMethod]
        public void Normal()
        {
            AECBox box = new AECBox
            {
                SE = new AECPoint(1, 0, 0),
                SW = new AECPoint(0, 0, 0),
                NW = new AECPoint(0, 0, 1)
            };
            AECVector normal = AECGeometry.Normal(box);

            Assert.AreEqual(0, normal.X, 0);
            Assert.AreEqual(-1, normal.Y, 0);
            Assert.AreEqual(0, normal.Z, 0);

            box.SE = new AECPoint(1, 1, 0);
            box.SW = new AECPoint(1, 0, 0);
            box.NW = new AECPoint(1, 0, 1);
            normal = AECGeometry.Normal(box);

            Assert.AreEqual(1, normal.X, 0);
            Assert.AreEqual(0, normal.Y, 0);
            Assert.AreEqual(0, normal.Z, 0);

            box.SE = new AECPoint(0, 1, 0);
            box.SW = new AECPoint(1, 1, 0);
            box.NW = new AECPoint(1, 1, 1);
            normal = AECGeometry.Normal(box);

            Assert.AreEqual(0, normal.X, 0);
            Assert.AreEqual(1, normal.Y, 0);
            Assert.AreEqual(0, normal.Z, 0);

            box.SE = new AECPoint(0, 0, 0);
            box.SW = new AECPoint(0, 1, 0);
            box.NW = new AECPoint(0, 1, 1);
            normal = AECGeometry.Normal(box);

            Assert.AreEqual(-1, normal.X, 0);
            Assert.AreEqual(0, normal.Y, 0);
            Assert.AreEqual(0, normal.Z, 0);

            box.SE = new AECPoint(0, 1, 0);
            box.SW = new AECPoint(0, 0, 0);
            box.NW = new AECPoint(1, 0, 0);
            normal = AECGeometry.Normal(box);

            Assert.AreEqual(0, normal.X, 0);
            Assert.AreEqual(0, normal.Y, 0);
            Assert.AreEqual(-1, normal.Z, 0);

            box.SE = new AECPoint(1, 0, 1);
            box.SW = new AECPoint(0, 0, 1);
            box.NW = new AECPoint(0, 1, 1);
            normal = AECGeometry.Normal(box);

            Assert.AreEqual(0, normal.X, 0);
            Assert.AreEqual(0, normal.Y, 0);
            Assert.AreEqual(1, normal.Z, 0);
        }//method

        [TestMethod]
        public void PointAlong()
        {
            AECPoint thisPoint = new AECPoint(0, 0, 0);
            AECPoint thatPoint = new AECPoint(0, 20, 0);
            double fraction = AECGeometry.RandomDouble(0, 1);
            AECPoint along = AECGeometry.PointAlong(thisPoint, thatPoint, fraction);

            Assert.AreEqual(0, along.X, 0);
            Assert.IsTrue((along.Y >= 0 && along.Y <= 20));
            Assert.AreEqual(0, along.Z, 0);
        }//method

        [TestMethod]
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
            Assert.IsFalse(AECGeometry.IsConvexPolygon(bndPoints));
        }//method

        [TestMethod]
        public void PolygonConvex()
        {
            List<AECPoint> bndPoints = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(3, 0, 0),
                new AECPoint(3, 3, 0),
                new AECPoint(0, 3, 0)
            };
            Assert.IsTrue(AECGeometry.IsConvexPolygon(bndPoints));
        }//method

        [TestMethod]
        public void RemoveColinear()
        {
            AECGeometry geometry = new AECGeometry();
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(1, 0, 0),
                new AECPoint(3, 0, 0)
            };
            Assert.AreEqual(2, geometry.RemoveColinear(points).Count, 0); 

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
            Assert.AreEqual(4, cleanPoints.Count, 0);

            points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(1, 0, 0),
                new AECPoint(1, 1, 0),
                new AECPoint(0, 1, 0)
            };
            cleanPoints = geometry.RemoveColinear(points);
            Assert.AreEqual(4, cleanPoints.Count, 0);
        }//method
    }//class
}//namespace
