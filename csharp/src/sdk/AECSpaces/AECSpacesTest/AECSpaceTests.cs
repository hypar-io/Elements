using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using AECSpaces;

namespace AECSpacesTest
{
    [TestClass]
    public class SpaceTests
    {
        const double dblVariance = 0.000000001;

        /* -------- Begin Property Tests -------- */

        [TestMethod]
        public void Address()
        {
            AECSpace space = new AECSpace();
            AECAddress address = new AECAddress(1, 1, 1);
            space.Address = address;
            address = space.Address;

            Assert.AreEqual(1, address.x, 0);
            Assert.AreEqual(1, address.y, 0);
            Assert.AreEqual(1, address.z, 0);
        }//method

        [TestMethod]
        public void AxisMajor()
        {
            List<AECPoint> bndPoints = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(3, 0, 0),
                new AECPoint(2, 1, 0),
                new AECPoint(3, 3, 0),
                new AECPoint(0, 3, 0)
            };
            AECSpace space = new AECSpace(bndPoints);
            List<AECPoint> axis = space.AxisMajor;

            Assert.AreEqual(0, axis[0].X, 0);
            Assert.AreEqual(1.5, axis[0].Y, 0);
            Assert.AreEqual(0, axis[0].Z, 0);

            Assert.AreEqual(3, axis[1].X, 0);
            Assert.AreEqual(1.5, axis[1].Y, 0);
            Assert.AreEqual(0, axis[1].Z, 0);
        }//method

        [TestMethod]
        public void AxisMinor()
        {
            List<AECPoint> bndPoints = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(3, 0, 0),
                new AECPoint(2, 1, 0),
                new AECPoint(3, 3, 0),
                new AECPoint(0, 3, 0)
            };
            AECSpace space = new AECSpace(bndPoints);
            List<AECPoint> axis = space.AxisMinor;

            Assert.AreEqual(1.5, axis[0].X, 0);
            Assert.AreEqual(0, axis[0].Y, 0);
            Assert.AreEqual(0, axis[0].Z, 0);

            Assert.AreEqual(1.5, axis[1].X, 0);
            Assert.AreEqual(3, axis[1].Y, 0);
            Assert.AreEqual(0, axis[1].Z, 0);
        }//method

        [TestMethod]
        public void AxisX()
        {
            List<AECPoint> bndPoints = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(3, 0, 0),
                new AECPoint(2, 1, 0),
                new AECPoint(3, 3, 0),
                new AECPoint(0, 3, 0)
            };
            AECSpace space = new AECSpace(bndPoints);
            List<AECPoint> axis = space.AxisX;

            Assert.AreEqual(0, axis[0].X, 0);
            Assert.AreEqual(1.5, axis[0].Y, 0);
            Assert.AreEqual(0, axis[0].Z, 0);

            Assert.AreEqual(3, axis[1].X, 0);
            Assert.AreEqual(1.5, axis[1].Y, 0);
            Assert.AreEqual(0, axis[1].Z, 0);
        }//method

        [TestMethod]
        public void AxisY()
        {
            List<AECPoint> bndPoints = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(3, 0, 0),
                new AECPoint(2, 1, 0),
                new AECPoint(3, 3, 0),
                new AECPoint(0, 3, 0)
            };
            AECSpace space = new AECSpace(bndPoints);
            List<AECPoint> axis = space.AxisY;

            Assert.AreEqual(1.5, axis[0].X, 0);
            Assert.AreEqual(0, axis[0].Y, 0);
            Assert.AreEqual(0, axis[0].Z, 0);

            Assert.AreEqual(1.5, axis[1].X, 0);
            Assert.AreEqual(3, axis[1].Y, 0);
            Assert.AreEqual(0, axis[1].Z, 0);
        }//method

        [TestMethod]
        public void CenterCentroid()
        {
            List<AECPoint> bndPoints = new List<AECPoint>
            {
                new AECPoint(0, 0),
                new AECPoint(2, 0),
                new AECPoint(2, 2),
                new AECPoint(0, 2)
            };
            AECSpace space = new AECSpace(bndPoints)
            {
                Level = 2,
                Height = 2
            };

            //Centers
            AECPoint center = space.CenterCeiling;
            Assert.AreEqual(1, center.X, 0);
            Assert.AreEqual(1, center.Y, 0);
            Assert.AreEqual(4, center.Z, 0);

            center = space.CenterFloor;
            Assert.AreEqual(1, center.X, 0);
            Assert.AreEqual(1, center.Y, 0);
            Assert.AreEqual(2, center.Z, 0);

            center = space.CenterSpace;
            Assert.AreEqual(1, center.X, 0);
            Assert.AreEqual(1, center.Y, 0);
            Assert.AreEqual(3, center.Z, 0);

            //Centroids
            center = space.CentroidCeiling;
            Assert.AreEqual(1, center.X, 0);
            Assert.AreEqual(1, center.Y, 0);
            Assert.AreEqual(4, center.Z, 0);

            center = space.CentroidFloor;
            Assert.AreEqual(1, center.X, 0);
            Assert.AreEqual(1, center.Y, 0);
            Assert.AreEqual(2, center.Z, 0);

            center = space.CentroidSpace;
            Assert.AreEqual(1, center.X, 0);
            Assert.AreEqual(1, center.Y, 0);
            Assert.AreEqual(3, center.Z, 0);
        }//method

        [TestMethod]
        public void Mesh()
        {
            List<AECPoint> pointsFlr = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(1, 0, 0),
                new AECPoint(1, 1, 0),
                new AECPoint(0, 1, 0)
            };
            AECSpace space = new AECSpace(pointsFlr)
            {
                Height = 1
            };
            AECMesh mesh = space.Mesh;
        }//method

        [TestMethod]
        public void Mesh2D()
        {
            List<AECPoint> pointsClg = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(3, 0, 0),
                new AECPoint(2, 1, 0),
                new AECPoint(3, 3, 0),
                new AECPoint(0, 3, 0)
            };
            AECSpace spaceClg = new AECSpace(pointsClg);
            AECMesh2D meshClg = spaceClg.MeshCeiling;

            Assert.AreEqual(1, meshClg.indices[0].x);
            Assert.AreEqual(2, meshClg.indices[0].y);
            Assert.AreEqual(0, meshClg.indices[0].z);

            Assert.AreEqual(2, meshClg.indices[1].x);
            Assert.AreEqual(3, meshClg.indices[1].y);
            Assert.AreEqual(0, meshClg.indices[1].z);

            Assert.AreEqual(3, meshClg.indices[2].x);
            Assert.AreEqual(4, meshClg.indices[2].y);
            Assert.AreEqual(0, meshClg.indices[2].z);

            List<AECPoint> pointsFlr = new List<AECPoint>
            {
                new AECPoint(0, 2, 0),
                new AECPoint(3, 0, 0),
                new AECPoint(2, 1, 0),
                new AECPoint(3, 3, 0)
            };
            AECSpace spaceFlr = new AECSpace(pointsFlr);
            AECMesh2D meshFlr = spaceFlr.MeshFloor;

            Assert.AreEqual(1, meshFlr.indices[0].x);
            Assert.AreEqual(2, meshFlr.indices[0].y);
            Assert.AreEqual(0, meshFlr.indices[0].z);

            Assert.AreEqual(2, meshFlr.indices[1].x);
            Assert.AreEqual(3, meshFlr.indices[1].y);
            Assert.AreEqual(0, meshFlr.indices[1].z);
        }//method

        [TestMethod]
        public void MeshGraphic()
        {
            List<AECPoint> pointsFlr = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(1, 0, 0),
                new AECPoint(1, 1, 0),
                new AECPoint(0, 1, 0)
            };
            AECSpace space = new AECSpace(pointsFlr)
            {
                Height = 1
            };
            AECMeshGraphic mesh = space.MeshGraphic;
            Assert.AreEqual(36, mesh.indices.Count, 0);
            Assert.AreEqual(72, mesh.normals.Count, 0);
            Assert.AreEqual(72, mesh.vertices.Count, 0);
            foreach (double value in mesh.vertices)
            {
                Assert.AreEqual(0, value, 1);
            }//foreach
            foreach (double value in mesh.normals)
            {
                Assert.AreEqual(0, value, 1);
            }//foreach
            foreach (double value in mesh.indices)
            {
                Assert.IsTrue(value >= 0 && value < 24);
            }//foreach
        }//method

        [TestMethod]
        public void Normals()
        {
            List<AECPoint> pointsFlr = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(1, 0, 0),
                new AECPoint(1, 1, 0),
                new AECPoint(0, 1, 0)
            };
            AECSpace space = new AECSpace(pointsFlr)
            {
                Height = 1
            };

            AECVector clgNormal = space.NormalCeiling;
            AECVector flrNormal = space.NormalFloor;
            List<AECSpaceSide> sidNormals = space.NormalSides;

            Assert.AreEqual(0, clgNormal.X, 0);
            Assert.AreEqual(0, clgNormal.Y, 0);
            Assert.AreEqual(1, clgNormal.Z, 0);

            Assert.AreEqual(0, flrNormal.X, 0);
            Assert.AreEqual(0, flrNormal.Y, 0);
            Assert.AreEqual(-1, flrNormal.Z, 0);

            AECSpaceSide side = sidNormals[0];
            Assert.AreEqual(0, side.normal.X, 0);
            Assert.AreEqual(-1, side.normal.Y, 0);
            Assert.AreEqual(0, side.normal.Z, 0);

            side = sidNormals[1];
            Assert.AreEqual(1, side.normal.X, 0);
            Assert.AreEqual(0, side.normal.Y, 0);
            Assert.AreEqual(0, side.normal.Z, 0);

            side = sidNormals[2];
            Assert.AreEqual(0, side.normal.X, 0);
            Assert.AreEqual(1, side.normal.Y, 0);
            Assert.AreEqual(0, side.normal.Z, 0);

            side = sidNormals[3];
            Assert.AreEqual(-1, side.normal.X, 0);
            Assert.AreEqual(0, side.normal.Y, 0);
            Assert.AreEqual(0, side.normal.Z, 0);
        }//method

        [TestMethod]
        public void PointsBox()
        {
            List<AECPoint> bndPoints = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(3, 0, 0),
                new AECPoint(2, 1, 0),
                new AECPoint(3, 3, 0),
                new AECPoint(0, 3, 0)
            };
            AECSpace space = new AECSpace(bndPoints);
            AECBox box = space.PointsBox;

            //Southwest Point
            Assert.AreEqual(0, box.SW.X, 0);
            Assert.AreEqual(0, box.SW.Y, 0);
            Assert.AreEqual(0, box.SW.Z, 0);

            //Southeast Point
            Assert.AreEqual(3, box.SE.X, 0);
            Assert.AreEqual(0, box.SE.Y, 0);
            Assert.AreEqual(0, box.SE.Z, 0);

            //Northeast Point
            Assert.AreEqual(3, box.NE.X, 0);
            Assert.AreEqual(3, box.NE.Y, 0);
            Assert.AreEqual(0, box.NE.Z, 0);

            //Northwest Point
            Assert.AreEqual(0, box.NW.X, 0);
            Assert.AreEqual(3, box.NW.Y, 0);
            Assert.AreEqual(0, box.NW.Z, 0);
        }//method

        [TestMethod]
        public void PointsCeiling()
        {
            List<AECPoint> bndPoints = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(3, 0, 0),
                new AECPoint(2, 1, 0),
                new AECPoint(3, 3, 0),
                new AECPoint(0, 3, 0)
            };
            AECSpace space = new AECSpace(bndPoints)
            {
                Height = 2.2,
                Level = 1.1
            };

            List<AECPoint> points = space.PointsCeiling;

            Assert.AreEqual(0, points[0].X, 0);
            Assert.AreEqual(0, points[0].Y, 0);
            Assert.AreEqual(3.3, points[0].Z, 0.0000000000001);

            Assert.AreEqual(3, points[1].X, 0);
            Assert.AreEqual(0, points[1].Y, 0);
            Assert.AreEqual(3.3, points[1].Z, 0.0000000000001);

            Assert.AreEqual(2, points[2].X, 0);
            Assert.AreEqual(1, points[2].Y, 0);
            Assert.AreEqual(3.3, points[2].Z, 0.0000000000001);

            Assert.AreEqual(3, points[3].X, 0);
            Assert.AreEqual(3, points[3].Y, 0);
            Assert.AreEqual(3.3, points[3].Z, 0.0000000000001);

            Assert.AreEqual(0, points[4].X, 0);
            Assert.AreEqual(3, points[4].Y, 0);
            Assert.AreEqual(3.3, points[4].Z, 0.0000000000001);
        }//method

        [TestMethod]
        public void PointsFloor()
        {
            List<AECPoint> bndPoints = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(3, 0, 0),
                new AECPoint(2, 1, 0),
                new AECPoint(3, 3, 0),
                new AECPoint(0, 3, 0)
            };
            AECSpace space = new AECSpace(bndPoints)
            {
                Height = 2.2,
                Level = 1.1
            };
            List<AECPoint> points = space.PointsFloor;

            Assert.AreEqual(0, points[0].X, 0);
            Assert.AreEqual(0, points[0].Y, 0);
            Assert.AreEqual(1.1, points[0].Z, 0);

            Assert.AreEqual(3, points[1].X, 0);
            Assert.AreEqual(0, points[1].Y, 0);
            Assert.AreEqual(1.1, points[1].Z, 0);

            Assert.AreEqual(2, points[2].X, 0);
            Assert.AreEqual(1, points[2].Y, 0);
            Assert.AreEqual(1.1, points[2].Z, 0);

            Assert.AreEqual(3, points[3].X, 0);
            Assert.AreEqual(3, points[3].Y, 0);
            Assert.AreEqual(1.1, points[3].Z, 0);

            Assert.AreEqual(0, points[4].X, 0);
            Assert.AreEqual(3, points[4].Y, 0);
            Assert.AreEqual(1.1, points[4].Z, 0);
        }//method

        [TestMethod]
        public void PointsSides()
        {
            List<AECPoint> bndPoints = new List<AECPoint>
            {
                new AECPoint(0, 0),
                new AECPoint(2, 0),
                new AECPoint(2, 2),
                new AECPoint(0, 2)
            };
            AECSpace space = new AECSpace(bndPoints)
            {
                Level = 2,
                Height = 2
            };
            List<AECBox> sides = space.PointsSides;

            //South
            Assert.AreEqual(0, sides[0].SW.X, 0);
            Assert.AreEqual(0, sides[0].SW.Y, 0);
            Assert.AreEqual(2, sides[0].SW.Z, 0);

            Assert.AreEqual(2, sides[0].SE.X, 0);
            Assert.AreEqual(0, sides[0].SE.Y, 0);
            Assert.AreEqual(2, sides[0].SE.Z, 0);

            Assert.AreEqual(2, sides[0].NE.X, 0);
            Assert.AreEqual(0, sides[0].NE.Y, 0);
            Assert.AreEqual(4, sides[0].NE.Z, 0);

            Assert.AreEqual(0, sides[0].NW.X, 0);
            Assert.AreEqual(0, sides[0].NW.Y, 0);
            Assert.AreEqual(4, sides[0].NW.Z, 0);

            //East
            Assert.AreEqual(2, sides[1].SW.X, 0);
            Assert.AreEqual(0, sides[1].SW.Y, 0);
            Assert.AreEqual(2, sides[1].SW.Z, 0);

            Assert.AreEqual(2, sides[1].SE.X, 0);
            Assert.AreEqual(2, sides[1].SE.Y, 0);
            Assert.AreEqual(2, sides[0].SE.Z, 0);

            Assert.AreEqual(2, sides[1].NE.X, 0);
            Assert.AreEqual(2, sides[1].NE.Y, 0);
            Assert.AreEqual(4, sides[1].NE.Z, 0);

            Assert.AreEqual(2, sides[1].NW.X, 0);
            Assert.AreEqual(0, sides[1].NW.Y, 0);
            Assert.AreEqual(4, sides[1].NW.Z, 0);

            ////North
            Assert.AreEqual(2, sides[2].SW.X, 0);
            Assert.AreEqual(2, sides[2].SW.Y, 0);
            Assert.AreEqual(2, sides[2].SW.Z, 0);

            Assert.AreEqual(0, sides[2].SE.X, 0);
            Assert.AreEqual(2, sides[2].SE.Y, 0);
            Assert.AreEqual(2, sides[2].SE.Z, 0);

            Assert.AreEqual(0, sides[2].NE.X, 0);
            Assert.AreEqual(2, sides[2].NE.Y, 0);
            Assert.AreEqual(4, sides[2].NE.Z, 0);

            Assert.AreEqual(2, sides[2].NW.X, 0);
            Assert.AreEqual(2, sides[2].NW.Y, 0);
            Assert.AreEqual(4, sides[2].NW.Z, 0);

            ////West
            Assert.AreEqual(0, sides[3].SW.X, 0);
            Assert.AreEqual(2, sides[3].SW.Y, 0);
            Assert.AreEqual(2, sides[3].SW.Z, 0);

            Assert.AreEqual(0, sides[3].SE.X, 0);
            Assert.AreEqual(0, sides[3].SE.Y, 0);
            Assert.AreEqual(2, sides[2].SE.Z, 0);

            Assert.AreEqual(0, sides[3].NE.X, 0);
            Assert.AreEqual(0, sides[3].NE.Y, 0);
            Assert.AreEqual(4, sides[3].NE.Z, 0);

            Assert.AreEqual(0, sides[3].NW.X, 0);
            Assert.AreEqual(2, sides[3].NW.Y, 0);
            Assert.AreEqual(4, sides[3].NW.Z, 0);
        }//method

        [TestMethod]
        public void Properties()
        {
            //Concave Space
            List<AECPoint> bndPoints = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(3, 0, 0),
                new AECPoint(2, 1, 0),
                new AECPoint(3, 3, 0),
                new AECPoint(0, 3, 0)
            };
            AECSpace space = new AECSpace(bndPoints);

            //Boundary 
            Assert.IsTrue(space.Boundary is Polygon);

            //BoundingBox  
            Assert.IsTrue(space.BoundingBox is Polygon);

            //Color
            //Assert.AreEqual(255, space.AECColor.Alpha);
            //Assert.AreEqual(255, space.Color.Red);
            //Assert.AreEqual(255, space.Color.Green);
            //Assert.AreEqual(255, space.Color.Blue);

            //Convex
            Assert.IsFalse(space.Convex);

            //Height
            space.Height = 2.2;
            Assert.AreEqual(2.2, space.Height, 0);

            //Level
            space.Level = 1.1;
            Assert.AreEqual(1.1, space.Level, 0);

            //Elevation (placed here for new value calculated from level + height)
            Assert.AreEqual(3.3, space.Elevation, 0.0000000000001);

            //Name
            space.Name = "TestSpace";
            Assert.AreEqual("TestSpace", space.Name);

            //Point
            Assert.IsTrue(space.Contains(space.PointCeiling));
            Assert.IsTrue(space.Contains(space.PointFloor));
            Assert.IsTrue(space.Contains(space.PointSpace));

            //Sizes
            Assert.AreEqual(3, space.SizeX, 0);
            Assert.AreEqual(3, space.SizeY, 0);

            //Convex Space
            bndPoints.Clear();
            bndPoints.Add(new AECPoint(0, 0));
            bndPoints.Add(new AECPoint(2, 0));
            bndPoints.Add(new AECPoint(2, 2));
            bndPoints.Add(new AECPoint(0, 2));
            space.PointsFloor = bndPoints;
            space.Level = 2;
            space.Height = 2;

            //Area
            Assert.AreEqual(4, space.Area);

            //Circumference
            Assert.AreEqual(8, space.Circumference, 0);

            //Volume
            Assert.AreEqual(8, space.Volume);
        }//method

        /* -------- Begin Method Tests -------- */

        [TestMethod]
        public void Add()
        {
            List<AECPoint> pointsFirst = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace space = new AECSpace(pointsFirst);
            List<AECPoint> pointsSecond = new List<AECPoint>
            {
                new AECPoint(3, 3, 0),
                new AECPoint(7, 3, 0),
                new AECPoint(7, 7, 0),
                new AECPoint(3, 7, 0)
            };
            Assert.IsTrue(space.Add(pointsSecond));
            List<AECPoint> points = space.PointsFloor;

            Assert.AreEqual(8, points.Count, 0);

            Assert.AreEqual(4, points[0].X, 0);
            Assert.AreEqual(0, points[0].Y, 0);
            Assert.AreEqual(0, points[0].Z, 0);

            Assert.AreEqual(3, points[4].X, 0);
            Assert.AreEqual(7, points[4].Y, 0);
            Assert.AreEqual(0, points[4].Z, 0);
        }//method

        [TestMethod]
        public void Contain()
        {
            List<AECPoint> pointsFirst = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace space = new AECSpace(pointsFirst);

            List<AECPoint> pointsSecond = new List<AECPoint>
            {
                new AECPoint(3, 3, 0),
                new AECPoint(7, 3, 0),
                new AECPoint(7, 7, 0),
                new AECPoint(3, 7, 0)
            };
            AECSpace container = new AECSpace(pointsSecond);

            space.Contain(container);
            Assert.AreEqual(1, space.Area, 0);
            List<AECPoint> points = space.PointsFloor;

            Assert.AreEqual(3, points[0].X, 0);
            Assert.AreEqual(4, points[0].Y, 0);
            Assert.AreEqual(0, points[0].Z, 0);

            Assert.AreEqual(4, points[1].X, 0);
            Assert.AreEqual(4, points[1].Y, 0);
            Assert.AreEqual(0, points[1].Z, 0);

            Assert.AreEqual(4, points[2].X, 0);
            Assert.AreEqual(3, points[2].Y, 0);
            Assert.AreEqual(0, points[2].Z, 0);

            Assert.AreEqual(3, points[3].X, 0);
            Assert.AreEqual(3, points[3].Y, 0);
            Assert.AreEqual(0, points[3].Z, 0);
        }//method

        [TestMethod]
        public void ContainsEnclosesPoint()
        {
            //Contains
            List<AECPoint> bndPoints = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace space = new AECSpace(bndPoints);
            AECPoint point = new AECPoint(2, 2, 0);
            Assert.IsTrue(space.Contains(point));
            point.X = 10;
            Assert.IsFalse(space.Contains(point));

            //Encloses
            space.Level = 10;
            space.Height = 10;
            point.X = 2;
            Assert.IsFalse(space.Encloses(point));
            point.Z = 12;
            Assert.IsTrue(space.Encloses(point));
        }//method

        [TestMethod]
        public void ContainsEnclosesSpace()
        {
            //Contains
            List<AECPoint> bndPoints = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace space = new AECSpace(bndPoints);

            List<AECPoint> shpPoints = new List<AECPoint>
            {
                new AECPoint(2, 2, 0),
                new AECPoint(3, 0, 0),
                new AECPoint(3, 3, 0),
                new AECPoint(0, 2, 0)
            };
            AECSpace shape = new AECSpace(shpPoints);
            Assert.IsTrue(space.Contains(shape));

            //Encloses
            space.Level = 10;
            space.Height = 10;
            shape.Level = 0;
            shape.Height = 9;
            Assert.IsFalse(space.Encloses(shape));

            AECPoint point = new AECPoint(2, 2, 11);
            Assert.IsTrue(space.Encloses(point));
        }//method

        [TestMethod]
        public void Copy()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(points)
            {
                //Color = Color.Aqua,
                Height = 5,
                Level = 10,
                Name = "Tester"
            };
            AECSpace thatSpace = thisSpace.Copy();
            points = thatSpace.PointsFloor;

            //Points
            Assert.AreEqual(0, points[0].X, 0);
            Assert.AreEqual(0, points[0].Y, 0);

            Assert.AreEqual(4, points[1].X, 0);
            Assert.AreEqual(0, points[1].Y, 0);

            Assert.AreEqual(4, points[2].X, 0);
            Assert.AreEqual(4, points[2].Y, 0);

            Assert.AreEqual(0, points[3].X, 0);
            Assert.AreEqual(4, points[3].Y, 0);

            Assert.AreEqual(thisSpace.Color, thatSpace.Color);
            Assert.AreEqual(thisSpace.Height, thatSpace.Height);
            Assert.AreEqual(thisSpace.Level, thatSpace.Level);
            Assert.AreEqual(thisSpace.Name, thatSpace.Name);
        }//method

        [TestMethod]
        public void CopyPlace()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(points);

            AECSpaceGroup thoseSpaces = thisSpace.CopyPlace(copies: 3, xDelta: 4);

            AECBox box = thoseSpaces.GetSpace(1).PointsBox;
            Assert.AreEqual(4, box.SW.X);
            Assert.AreEqual(0, box.SW.Y);
            Assert.AreEqual(0, box.SW.Z);

            box = thoseSpaces.GetSpace(2).PointsBox;
            Assert.AreEqual(8, box.SW.X);
            Assert.AreEqual(0, box.SW.Y);
            Assert.AreEqual(0, box.SW.Z);

            box = thoseSpaces.GetSpace(3).PointsBox;
            Assert.AreEqual(12, box.SW.X);
            Assert.AreEqual(0, box.SW.Y);
            Assert.AreEqual(0, box.SW.Z);
        }//method

        [TestMethod]
        public void CopyRow()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(points);

            AECSpaceGroup thoseSpaces = thisSpace.CopyRow(copies: 3, gap: 1);

            AECBox box = thoseSpaces.GetSpace(1).PointsBox;
            Assert.AreEqual(5, box.SW.X);
            Assert.AreEqual(0, box.SW.Y);
            Assert.AreEqual(0, box.SW.Z);

            box = thoseSpaces.GetSpace(2).PointsBox;
            Assert.AreEqual(10, box.SW.X);
            Assert.AreEqual(0, box.SW.Y);
            Assert.AreEqual(0, box.SW.Z);

            box = thoseSpaces.GetSpace(3).PointsBox;
            Assert.AreEqual(15, box.SW.X);
            Assert.AreEqual(0, box.SW.Y);
            Assert.AreEqual(0, box.SW.Z);

            thoseSpaces = thisSpace.CopyRow(copies: 3, gap: 1, xAxis: false);

            box = thoseSpaces.GetSpace(1).PointsBox;
            Assert.AreEqual(0, box.SW.X);
            Assert.AreEqual(5, box.SW.Y);
            Assert.AreEqual(0, box.SW.Z);

            box = thoseSpaces.GetSpace(2).PointsBox;
            Assert.AreEqual(0, box.SW.X);
            Assert.AreEqual(10, box.SW.Y);
            Assert.AreEqual(0, box.SW.Z);

            box = thoseSpaces.GetSpace(3).PointsBox;
            Assert.AreEqual(0, box.SW.X);
            Assert.AreEqual(15, box.SW.Y);
            Assert.AreEqual(0, box.SW.Z);
        }//method


        [TestMethod]
        public void CopyRowToArea()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(points);
            AECSpaceGroup thoseSpaces = thisSpace.CopyRowToArea(targetArea: 64);
            Assert.AreEqual(4, thoseSpaces.Count, 0);

            thoseSpaces = thisSpace.CopyRowToArea(targetArea: 55);
            Assert.AreEqual(4, thoseSpaces.Count, 0);

            thoseSpaces = thisSpace.CopyRowToArea(targetArea: 70);
            Assert.AreEqual(5, thoseSpaces.Count, 0);
        }//method

        [TestMethod]
        public void CopyStack()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(points)
            {
                Height = 4
            };

            AECSpaceGroup thoseSpaces = thisSpace.CopyStack(copies: 3, gap: 1);

            thisSpace = thoseSpaces.GetSpace(1);
            Assert.AreEqual(5, thisSpace.Level);

            AECBox box = thisSpace.PointsBox;
            Assert.AreEqual(0, box.SW.X);
            Assert.AreEqual(0, box.SW.Y);
            Assert.AreEqual(5, box.SW.Z);

            thisSpace = thoseSpaces.GetSpace(2);
            Assert.AreEqual(10, thisSpace.Level);

            box = thoseSpaces.GetSpace(2).PointsBox;
            Assert.AreEqual(0, box.SW.X);
            Assert.AreEqual(0, box.SW.Y);
            Assert.AreEqual(10, box.SW.Z);

            thisSpace = thoseSpaces.GetSpace(3);
            Assert.AreEqual(15, thisSpace.Level);

            box = thoseSpaces.GetSpace(3).PointsBox;
            Assert.AreEqual(0, box.SW.X);
            Assert.AreEqual(0, box.SW.Y);
            Assert.AreEqual(15, box.SW.Z);
        }//method

        [TestMethod]
        public void CopyStackToArea()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };


            AECSpace thisSpace = new AECSpace(points);
            thisSpace.Height = 10;
            AECSpaceGroup thoseSpaces = thisSpace.CopyStackToArea(targetArea: 64);
            Assert.AreEqual(4, thoseSpaces.Count, 0);

            thisSpace = thoseSpaces.GetSpace(1);
            Assert.AreEqual(10, thisSpace.Level, 0);

            thoseSpaces = thisSpace.CopyStackToArea(targetArea: 55);
            Assert.AreEqual(4, thoseSpaces.Count, 0);

            thoseSpaces = thisSpace.CopyStackToArea(targetArea: 70, gap: 5);
            Assert.AreEqual(5, thoseSpaces.Count, 0);

            List<Hypar.Geometry.Vector3> vertices = new List<Hypar.Geometry.Vector3>();
            Hypar.Elements.Model model = new Hypar.Elements.Model();
            Hypar.Geometry.Polyline boundary;
            foreach (AECSpace space in thoseSpaces.Spaces)
            {
                points = space.PointsFloor;
                foreach (AECPoint point in points)
                {
                    vertices.Add(new Hypar.Geometry.Vector3(point.X, point.Y, point.Z));
                }//foreach
                boundary = new Hypar.Geometry.Polyline(vertices);
                vertices.Clear();
                Hypar.Elements.Mass mass = Hypar.Elements.Mass.WithBottomProfile(boundary)
                                                              .WithTopAtElevation(space.Elevation)
                                                              .WithTopProfile(boundary)
                                                              .WithBottomAtElevation(space.Level);
                                                              
                                                              
                model.AddElement(mass);               
            }//foreach
            model.SaveGlb("C:\\Users\\Anthony\\Dropbox\\Business\\Hypar\\GitHub\\AECSpaces\\AECSpacesTest\\CopyStackToArea.glb");
        }//method

        [TestMethod]
        public void DataSetGet()
        {
            AECSpace space = new AECSpace();
            space.DataSet("Testing", "250");

            Assert.IsTrue(space.DataGet("Testing") == "250");
            Assert.IsTrue(space.DataGet("NonExistent") == "");
        }//method

        [TestMethod]
        public void Difference()
        {
            List<AECPoint> pointsFirst = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(pointsFirst);

            List<AECPoint> pointsSecond = new List<AECPoint>
            {
                new AECPoint(1, -2, 0),
                new AECPoint(3, -2, 0),
                new AECPoint(3, 5, 0),
                new AECPoint(1, 5, 0)
            };
            AECSpace thatSpace = new AECSpace(pointsSecond);
            AECSpaceGroup theseSpaces = thisSpace.Difference(thatSpace);

            Assert.AreEqual(2, theseSpaces.Count, 0);
            Assert.AreEqual(4, theseSpaces.GetSpace(0).Area, 0);
            Assert.AreEqual(4, theseSpaces.GetSpace(1).Area, 0);
        }//method

        [TestMethod]
        public void Intersection()
        {
            List<AECPoint> pointsFirst = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(pointsFirst);

            List<AECPoint> pointsSecond = new List<AECPoint>
            {
                new AECPoint(1, -2, 0),
                new AECPoint(3, -2, 0),
                new AECPoint(3, 5, 0),
                new AECPoint(1, 5, 0)
            };
            AECSpace thatSpace = new AECSpace(pointsSecond);
            AECSpaceGroup theseSpaces = thisSpace.Intersection(thatSpace);


            Assert.AreEqual(8, theseSpaces.GetSpace(0).Area, 0);

            pointsFirst = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(3, 0, 0),
                new AECPoint(2, 1, 0),
                new AECPoint(3, 3, 0),
                new AECPoint(0, 3, 0)
            };
            thisSpace = new AECSpace(pointsFirst);

            pointsSecond = new List<AECPoint>
            {
                new AECPoint(2, -2, 0),
                new AECPoint(4, -2, 0),
                new AECPoint(4, 5, 0),
                new AECPoint(2, 5, 0)
            };
            thatSpace = new AECSpace(pointsSecond);
            theseSpaces = thisSpace.Intersection(thatSpace);

            Assert.AreEqual(2, theseSpaces.Count, 0);
        }//method

        [TestMethod]
        public void IsAdjacentTo()
        {
            List<AECPoint> pointsThis = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(pointsThis);

            List<AECPoint> pointsThat = new List<AECPoint>
            {
                new AECPoint(4, 0, 0),
                new AECPoint(8, 0, 0),
                new AECPoint(8, 8, 0),
                new AECPoint(4, 4, 0)
            };
            AECSpace thatSpace = new AECSpace(pointsThat);

            Assert.IsTrue(thisSpace.IsAdjacentTo(thatSpace));
        }//method

        [TestMethod]
        public void IsNearTo()
        {
            List<AECPoint> pointsThis = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(pointsThis);

            List<AECPoint> pointsThat = new List<AECPoint>
            {
                new AECPoint(4, 0, 0),
                new AECPoint(8, 0, 0),
                new AECPoint(8, 8, 0),
                new AECPoint(4, 4, 0)
            };
            AECSpace thatSpace = new AECSpace(pointsThat);

            Assert.IsTrue(thisSpace.IsNearTo(thatSpace, 8));
            Assert.IsFalse(thisSpace.IsNearTo(thatSpace, 2));

            AECPoint thatPoint = new AECPoint(20, 20, 0);

            Assert.IsTrue(thisSpace.IsNearTo(thatPoint, 50));
            Assert.IsFalse(thisSpace.IsNearTo(thatPoint, 2));

        }//method

        [TestMethod]
        public void Mirror()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(1, 0, 0),
                new AECPoint(1, 1, 0),
                new AECPoint(0, 1, 0)
            };
            AECSpace space = new AECSpace(points);
            space.Mirror(thisPoint: new AECPoint(0, 0, 0), thatPoint: new AECPoint(0, 1, 0));

            List<AECPoint> tstPoints = space.PointsFloor;
            Assert.IsTrue(points[0].IsColocated(tstPoints[0]));

            Assert.AreEqual(-1, tstPoints[1].X, 0);
            Assert.AreEqual(0, tstPoints[1].Y, 0);
            Assert.AreEqual(0, tstPoints[1].Z, 0);

            Assert.AreEqual(-1, tstPoints[2].X, 0);
            Assert.AreEqual(1, tstPoints[2].Y, 0);
            Assert.AreEqual(0, tstPoints[2].Z, 0);

            Assert.AreEqual(0, tstPoints[3].X, 0);
            Assert.AreEqual(1, tstPoints[3].Y, 0);
            Assert.AreEqual(0, tstPoints[3].Z, 0);
        }//method

        [TestMethod]
        public void MoveBy()
        {
            //Contains
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(1, 0, 0),
                new AECPoint(1, 1, 0),
                new AECPoint(0, 1, 0)
            };
            AECSpace space = new AECSpace(points);
            space.MoveBy(20, 20, 20);
            points = space.PointsFloor;

            Assert.AreEqual(20, points[0].X, 0);
            Assert.AreEqual(20, points[0].Y, 0);
            Assert.AreEqual(20, points[0].Z, 0);
            Assert.AreEqual(20, space.Level);
        }//method

        [TestMethod]
        public void MoveTo()
        {
            //Contains
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(1, 0, 0),
                new AECPoint(1, 1, 0),
                new AECPoint(0, 1, 0)
            };
            AECSpace space = new AECSpace(points);
            space.MoveTo(from: new AECPoint(0, 0, 0), to: new AECPoint(1, 1, 1));
            int index = 0;
            foreach (AECPoint point in points)
            {
                point.MoveBy(1, 1, 1);
                Assert.IsTrue(point.IsColocated(points[index]));
                index++;
            }//foreach
        }//method

        [TestMethod]
        public void PlaceWithin()
        {
            List<AECPoint> pointsFirst = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace space = new AECSpace(pointsFirst);

            List<AECPoint> pointsSecond = new List<AECPoint>
            {
                new AECPoint(10, 10, 0),
                new AECPoint(20, 15, 0),
                new AECPoint(35, 50, 0),
                new AECPoint(10, 45, 0)
            };
            AECSpace container = new AECSpace(pointsSecond);
            Assert.IsTrue(space.PlaceWithin(container));
        }//method

        [TestMethod]
        public void PlaceWithinCompass()
        {
            List<AECPoint> pointsFirst = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace space = new AECSpace(pointsFirst);

            List<AECPoint> pointsSecond = new List<AECPoint>
            {
                new AECPoint(10, 10, 0),
                new AECPoint(30, 10, 0),
                new AECPoint(30, 30, 0),
                new AECPoint(10, 30, 0)
            };
            AECSpace container = new AECSpace(pointsSecond);
            AECGeometry.Compass orient = AECGeometry.Compass.N;
            Assert.IsTrue(space.PlaceWithinCompass(container, orient));
            Assert.IsTrue((space.CentroidFloor.Y >= 20 && space.CentroidFloor.Y <= 30));
        }//method

        [TestMethod]
        public void PlanBox()
        {
            AECSpace space = new AECSpace();
            space.PlanBox(7, 8);

            List<AECPoint> points = space.PointsFloor;

            Assert.AreEqual(0, points[0].X, 0);
            Assert.AreEqual(0, points[0].Y, 0);

            Assert.AreEqual(0, points[1].X, 7);
            Assert.AreEqual(0, points[1].Y, 0);

            Assert.AreEqual(0, points[2].X, 7);
            Assert.AreEqual(0, points[2].Y, 8);

            Assert.AreEqual(0, points[3].X, 0);
            Assert.AreEqual(0, points[3].Y, 8);
        }//method

        [TestMethod]
        public void PlanCircle()
        {
            AECSpace space = new AECSpace();
            space.PlanCircle(radius: 10);
            List<AECPoint> points = space.PointsFloor;
            Assert.AreEqual(72, points.Count, 0);
        }//method

        [TestMethod]
        public void PlanCross()
        {
            AECSpace space = new AECSpace();
            space.PlanX(9, 12);

            List<AECPoint> points = space.PointsFloor;

            Assert.AreEqual(3, points[0].X, dblVariance);
            Assert.AreEqual(4, points[0].Y, dblVariance);

            Assert.AreEqual(3, points[1].X, 0);
            Assert.AreEqual(0, points[1].Y, 0);

            Assert.AreEqual(6, points[2].X, dblVariance);
            Assert.AreEqual(0, points[2].Y, dblVariance);

            Assert.AreEqual(6, points[3].X, dblVariance);
            Assert.AreEqual(4, points[3].Y, dblVariance);

            Assert.AreEqual(9, points[4].X, dblVariance);
            Assert.AreEqual(4, points[4].Y, dblVariance);

            Assert.AreEqual(9, points[5].X, dblVariance);
            Assert.AreEqual(8, points[5].Y, dblVariance);

            Assert.AreEqual(6, points[6].X, dblVariance);
            Assert.AreEqual(8, points[6].Y, dblVariance);

            Assert.AreEqual(6, points[7].X, dblVariance);
            Assert.AreEqual(12, points[7].Y, dblVariance);

            Assert.AreEqual(3, points[8].X, dblVariance);
            Assert.AreEqual(12, points[8].Y, dblVariance);

            Assert.AreEqual(3, points[9].X, dblVariance);
            Assert.AreEqual(8, points[9].Y, dblVariance);

            Assert.AreEqual(0, points[10].X, dblVariance);
            Assert.AreEqual(8, points[10].Y, dblVariance);

            Assert.AreEqual(0, points[11].X, dblVariance);
            Assert.AreEqual(4, points[11].Y, dblVariance);
        }//method

        [TestMethod]
        public void PlanE()
        {
            AECSpace space = new AECSpace();
            space.PlanE(xSize: 9, ySize: 12, yWidthN: 3, yWidthM: 3, yWidthS: 3);
            List<AECPoint> points = space.PointsFloor;

            Assert.AreEqual(12, points.Count, 0);
        }//method

        [TestMethod]
        public void PlanF()
        {
            AECSpace space = new AECSpace();
            space.PlanF(xSize: 9, ySize: 12, yWidthN: 3, yWidthM: 3);
            List<AECPoint> points = space.PointsFloor;

            Assert.AreEqual(10, points.Count, 0);
        }//method

        [TestMethod]
        public void PlanH()
        {
            AECSpace space = new AECSpace();
            space.PlanH(xSize: 9, ySize: 9, xWidthW: 3, xWidthE: 3, yWidth: 3);

            List<AECPoint> points = space.PointsFloor;

            Assert.AreEqual(12, points.Count, 0);

            Assert.AreEqual(3, points[0].X, dblVariance);
            Assert.AreEqual(0, points[0].Y, dblVariance);

            Assert.AreEqual(3, points[1].X, dblVariance);
            Assert.AreEqual(3, points[1].Y, dblVariance);

            Assert.AreEqual(6, points[2].X, dblVariance);
            Assert.AreEqual(3, points[2].Y, dblVariance);

            Assert.AreEqual(6, points[3].X, dblVariance);
            Assert.AreEqual(0, points[3].Y, dblVariance);

            Assert.AreEqual(9, points[4].X, dblVariance);
            Assert.AreEqual(0, points[4].Y, dblVariance);

            Assert.AreEqual(9, points[5].X, dblVariance);
            Assert.AreEqual(9, points[5].Y, dblVariance);

            Assert.AreEqual(6, points[6].X, dblVariance);
            Assert.AreEqual(9, points[6].Y, dblVariance);

            Assert.AreEqual(6, points[7].X, dblVariance);
            Assert.AreEqual(6, points[7].Y, dblVariance);

            Assert.AreEqual(3, points[8].X, dblVariance);
            Assert.AreEqual(6, points[8].Y, dblVariance);

            Assert.AreEqual(3, points[9].X, dblVariance);
            Assert.AreEqual(9, points[9].Y, dblVariance);

            Assert.AreEqual(0, points[10].X, dblVariance);
            Assert.AreEqual(9, points[10].Y, dblVariance);

            Assert.AreEqual(0, points[11].X, dblVariance);
            Assert.AreEqual(0, points[11].Y, dblVariance);
        }//method

        [TestMethod]
        public void PlanL()
        {
            AECSpace space = new AECSpace();
            space.PlanL(xSize: 9, ySize: 12, xWidth: 3, yWidth: 4);
            List<AECPoint> points = space.PointsFloor;

            Assert.AreEqual(6, points.Count, 0);

            Assert.AreEqual(9, points[0].X, dblVariance);
            Assert.AreEqual(0, points[0].Y, dblVariance);

            Assert.AreEqual(9, points[1].X, dblVariance);
            Assert.AreEqual(4, points[1].Y, dblVariance);

            Assert.AreEqual(3, points[2].X, dblVariance);
            Assert.AreEqual(4, points[2].Y, dblVariance);

            Assert.AreEqual(3, points[3].X, 0);
            Assert.AreEqual(12, points[3].Y, 0);

            Assert.AreEqual(0, points[4].X, 0);
            Assert.AreEqual(12, points[4].Y, 0);

            Assert.AreEqual(0, points[5].X, 0);
            Assert.AreEqual(0, points[5].Y, 0);
        }//method

        [TestMethod]
        public void PlanPolygon()
        {
            AECSpace space = new AECSpace();
            space.PlanPolygon(radius: 10, sides: 3);
            List<AECPoint> points = space.PointsFloor;
            Assert.AreEqual(3, points.Count, 0);

            space.PlanPolygon(radius: 10, sides: 4);
            points = space.PointsFloor;
            Assert.AreEqual(4, points.Count, 0);

            space.PlanPolygon(radius: 10, sides: 5);
            points = space.PointsFloor;
            Assert.AreEqual(5, points.Count, 0);

            space.PlanPolygon(radius: 10, sides: 10);
            points = space.PointsFloor;
            Assert.AreEqual(10, points.Count, 0);

            space.PlanPolygon(radius: 10, sides: 12);
            points = space.PointsFloor;
            Assert.AreEqual(12, points.Count, 0);
        }//method

        [TestMethod]
        public void PlanT()
        {
            AECSpace space = new AECSpace();
            space.PlanT(xSize: 9, ySize: 12, xWidth: 3, yWidth: 4);

            List<AECPoint> points = space.PointsFloor;

            Assert.AreEqual(8, points.Count, 0);

            Assert.AreEqual(3, points[0].X, dblVariance);
            Assert.AreEqual(8, points[0].Y, dblVariance);

            Assert.AreEqual(3, points[1].X, dblVariance);
            Assert.AreEqual(0, points[1].Y, dblVariance);

            Assert.AreEqual(6, points[2].X, dblVariance);
            Assert.AreEqual(0, points[2].Y, dblVariance);

            Assert.AreEqual(6, points[3].X, dblVariance);
            Assert.AreEqual(8, points[3].Y, dblVariance);

            Assert.AreEqual(9, points[4].X, dblVariance);
            Assert.AreEqual(8, points[4].Y, dblVariance);

            Assert.AreEqual(9, points[5].X, dblVariance);
            Assert.AreEqual(12, points[5].Y, dblVariance);

            Assert.AreEqual(0, points[6].X, dblVariance);
            Assert.AreEqual(12, points[6].Y, dblVariance);

            Assert.AreEqual(0, points[7].X, dblVariance);
            Assert.AreEqual(8, points[7].Y, dblVariance);
        }//method

        [TestMethod]
        public void PlanU()
        {
            AECSpace space = new AECSpace();
            space.PlanU(xSize: 9, ySize: 9, xWidthW: 3, xWidthE: 3, yWidth: 3);

            List<AECPoint> points = space.PointsFloor;

            Assert.AreEqual(8, points.Count, 0);

            Assert.AreEqual(9, points[0].X, dblVariance);
            Assert.AreEqual(0, points[0].Y, dblVariance);

            Assert.AreEqual(9, points[1].X, dblVariance);
            Assert.AreEqual(9, points[1].Y, dblVariance);

            Assert.AreEqual(6, points[2].X, dblVariance);
            Assert.AreEqual(9, points[2].Y, dblVariance);

            Assert.AreEqual(6, points[3].X, dblVariance);
            Assert.AreEqual(3, points[3].Y, dblVariance);

            Assert.AreEqual(3, points[4].X, dblVariance);
            Assert.AreEqual(3, points[4].Y, dblVariance);

            Assert.AreEqual(3, points[5].X, dblVariance);
            Assert.AreEqual(9, points[5].Y, dblVariance);

            Assert.AreEqual(0, points[6].X, dblVariance);
            Assert.AreEqual(9, points[6].Y, dblVariance);

            Assert.AreEqual(0, points[7].X, dblVariance);
            Assert.AreEqual(0, points[7].Y, dblVariance);
        }//method

        [TestMethod]
        public void Rotate()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(1, 0, 0),
                new AECPoint(1, 1, 0),
                new AECPoint(0, 1, 0)
            };
            AECSpace space = new AECSpace(points);
            space.Rotate(180, points[0]);

            AECBox box = space.PointsBox;

            Assert.AreEqual(-1, box.SW.X, 0);
            Assert.AreEqual(-1, box.SW.Y, 0);

            Assert.AreEqual(0, box.NE.X, 0);
            Assert.AreEqual(0, box.NE.Y, 0);

        }//method

        [TestMethod]
        public void SaveAsGLB()
        {
            AECSpace space = new AECSpace();
            space.PlanU(xSize: 9, ySize: 9, xWidthW: 3, xWidthE: 3, yWidth: 3);
            space.Height = 3;

            List<AECPoint> points = space.PointsFloor;
            List<Hypar.Geometry.Vector3> vertices = new List<Hypar.Geometry.Vector3>();
            foreach (AECPoint point in points)
            {
                vertices.Add(new Hypar.Geometry.Vector3(point.X, point.Y, point.Z));
            }//foreach
            Hypar.Geometry.Polyline boundary = new Hypar.Geometry.Polyline(vertices);
            Hypar.Elements.Mass mass = Hypar.Elements.Mass.WithBottomProfile(boundary)
                                                          .WithTopProfile(boundary)
                                                          .WithBottomAtElevation(0)
                                                          .WithTopAtElevation(space.Height);
            Hypar.Elements.Model model = new Hypar.Elements.Model();
            model.AddElement(mass);
            model.SaveGlb("C:\\Users\\Anthony\\Dropbox\\Business\\Hypar\\GitHub\\AECSpaces\\AECSpacesTest\\model.glb");
        }//method

        [TestMethod]
        public void Scale()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(1, 0, 0),
                new AECPoint(1, 1, 0),
                new AECPoint(0, 1, 0)
            };
            AECSpace space = new AECSpace(points);
            space.Scale(xScale: 2, yScale: 2, point: points[0]);

            List<AECPoint> tstPoints = space.PointsFloor;
            Assert.IsTrue(points[0].IsColocated(tstPoints[0]));

            Assert.AreEqual(2, tstPoints[1].X, 0);
            Assert.AreEqual(0, tstPoints[1].Y, 0);
            Assert.AreEqual(0, tstPoints[1].Z, 0);

            Assert.AreEqual(2, tstPoints[2].X, 0);
            Assert.AreEqual(2, tstPoints[2].Y, 0);
            Assert.AreEqual(0, tstPoints[2].Z, 0);

            Assert.AreEqual(0, tstPoints[3].X, 0);
            Assert.AreEqual(2, tstPoints[3].Y, 0);
            Assert.AreEqual(0, tstPoints[3].Z, 0);
        }//method

        [TestMethod]
        public void Wrap()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(3, 0, 0),
                new AECPoint(2, 1, 0),
                new AECPoint(3, 3, 0),
                new AECPoint(0, 3, 0)
            };
            AECSpace space = new AECSpace(points);

            space.Wrap();
            List<AECPoint> tstPoints = space.PointsFloor;

            Assert.IsTrue(points[0].IsColocated(tstPoints[0]));
            Assert.IsTrue(points[1].IsColocated(tstPoints[1]));
            Assert.IsTrue(points[3].IsColocated(tstPoints[2]));
            Assert.IsTrue(points[4].IsColocated(tstPoints[3]));
        }//method
    }//class
}//namespace