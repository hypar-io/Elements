using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AECSpaces;

namespace AECSpacesTest
{
    [TestClass]
    public class SpaceGroupTests
    {
        /* -------- Begin Property Tests -------- */

        [TestMethod]
        public void Area()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(points);
            AECSpace thatSpace = new AECSpace(points);

            List<AECSpace> spaces = new List<AECSpace>();
            spaces.Add(thisSpace);
            spaces.Add(thatSpace);

            AECSpaceGroup spaceGroup = new AECSpaceGroup(spaces);
            Assert.AreEqual(32, spaceGroup.Area, 0);
        }//method

        [TestMethod]
        public void ByLevel()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(points);
            AECSpace thatSpace = new AECSpace(points);
            AECSpace othrSpace = new AECSpace(points);

            thisSpace.Level = 20;
            thatSpace.Level = 10;
            othrSpace.Level = 5;

            thisSpace.Name = "this";
            thatSpace.Name = "that";
            othrSpace.Name = "othr";

            List<AECSpace> spaces = new List<AECSpace>();
            spaces.Add(thisSpace);
            spaces.Add(thatSpace);
            spaces.Add(othrSpace);

            AECSpaceGroup spaceGroup = new AECSpaceGroup(spaces);
            spaces = spaceGroup.ByLevel;

            Assert.AreEqual("othr", spaces[0].Name);
            Assert.AreEqual("that", spaces[1].Name);
            Assert.AreEqual("this", spaces[2].Name);
        }//method

        [TestMethod]
        public void Count()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(points);
            AECSpace thatSpace = new AECSpace(points);
            AECSpace othrSpace = new AECSpace(points);

            List<AECSpace> spaces = new List<AECSpace>();
            spaces.Add(thisSpace);
            spaces.Add(thatSpace);
            spaces.Add(othrSpace);
            AECSpaceGroup spaceGroup = new AECSpaceGroup(spaces);

            Assert.AreEqual(3, spaceGroup.Count, 0);
        }//method

        [TestMethod]
        public void Volume()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(points);
            AECSpace thatSpace = new AECSpace(points);

            List<AECSpace> spaces = new List<AECSpace>();
            spaces.Add(thisSpace);
            spaces.Add(thatSpace);
            AECSpaceGroup spaceGroup = new AECSpaceGroup(spaces);

            Assert.AreEqual(32, spaceGroup.Volume, 0);
        }//method

        [TestMethod]
        public void Wrap()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(points);
            AECSpace thatSpace = new AECSpace(points);
            thatSpace.MoveBy(xDelta : 4);

            List<AECSpace> spaces = new List<AECSpace>();
            spaces.Add(thisSpace);
            spaces.Add(thatSpace);
            AECSpaceGroup spaceGroup = new AECSpaceGroup(spaces);

            List<AECPoint> tstPoints = spaceGroup.Wrap;
            AECSpace othrSpace = new AECSpace(tstPoints);

            Assert.AreEqual(32, othrSpace.Area, 0);
        }//method

        /* -------- Begin Method Tests -------- */

        [TestMethod]
        public void Clear()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(points);
            AECSpace thatSpace = new AECSpace(points);
            AECSpace othrSpace = new AECSpace(points);

            List<AECSpace> spaces = new List<AECSpace>();
            spaces.Add(thisSpace);
            spaces.Add(thatSpace);
            spaces.Add(othrSpace);
            AECSpaceGroup spaceGroup = new AECSpaceGroup(spaces);
            spaceGroup.Clear();

            Assert.AreEqual(0, spaceGroup.Count, 0);
        }//method

        [TestMethod]
        public void Delete()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(points);
            AECSpace thatSpace = new AECSpace(points);
            AECSpace othrSpace = new AECSpace(points);

            List<AECSpace> spaces = new List<AECSpace>();
            spaces.Add(thisSpace);
            spaces.Add(thatSpace);
            spaces.Add(othrSpace);
            AECSpaceGroup spaceGroup = new AECSpaceGroup(spaces);
            spaceGroup.Delete(1);

            Assert.AreEqual(2, spaceGroup.Count, 0);
        }//method

        [TestMethod]
        public void GetSpace()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(points);
            AECSpace thatSpace = new AECSpace(points);
            AECSpace othrSpace = new AECSpace(points);

            thisSpace.Name = "this";
            thatSpace.Name = "that";
            othrSpace.Name = "othr";

            List<AECSpace> spaces = new List<AECSpace>();
            spaces.Add(thisSpace);
            spaces.Add(thatSpace);
            spaces.Add(othrSpace);

            AECSpaceGroup spaceGroup = new AECSpaceGroup(spaces);
            AECSpace space = spaceGroup.GetSpace(1);

            Assert.AreEqual("that", space.Name);
        }//method

        [TestMethod]
        public void GetSpaceByID()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(points);
            AECSpace thatSpace = new AECSpace(points);
            AECSpace othrSpace = new AECSpace(points);
            string thatID = thatSpace.ID;

            thisSpace.Name = "this";
            thatSpace.Name = "that";
            othrSpace.Name = "othr";

            List<AECSpace> spaces = new List<AECSpace>();
            spaces.Add(thisSpace);
            spaces.Add(thatSpace);
            spaces.Add(othrSpace);

            AECSpaceGroup spaceGroup = new AECSpaceGroup(spaces);
            AECSpace space = spaceGroup.GetSpaceByID(thatID);

            Assert.AreEqual("that", space.Name);
        }//method

        [TestMethod]
        public void GetSpaceByName()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(points);
            AECSpace thatSpace = new AECSpace(points);
            AECSpace othrSpace = new AECSpace(points);
            string thatID = thatSpace.ID;

            thisSpace.Name = "this";
            thatSpace.Name = "that";
            othrSpace.Name = "othr";

            List<AECSpace> spaces = new List<AECSpace>();
            spaces.Add(thisSpace);
            spaces.Add(thatSpace);
            spaces.Add(othrSpace);

            AECSpaceGroup spaceGroup = new AECSpaceGroup(spaces);
            AECSpace space = spaceGroup.GetSpaceByName("that");

            Assert.AreEqual("that", space.Name);
        }//method

        [TestMethod]
        public void GetSpacesByName()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(points);
            AECSpace thatSpace = new AECSpace(points);
            AECSpace othrSpace = new AECSpace(points);
            string thatID = thatSpace.ID;

            thisSpace.Name = "that";
            thatSpace.Name = "that";
            othrSpace.Name = "othr";

            List<AECSpace> spaces = new List<AECSpace>();
            spaces.Add(thisSpace);
            spaces.Add(thatSpace);
            spaces.Add(othrSpace);

            AECSpaceGroup spaceGroup = new AECSpaceGroup(spaces);
            List<AECSpace> spacesByName = spaceGroup.GetSpacesByName("that");

            Assert.AreEqual(2, spacesByName.Count);
        }//method

        [TestMethod]
        public void MoveBy()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(points);
            AECSpace thatSpace = new AECSpace(points);
            AECSpace othrSpace = new AECSpace(points);

            List<AECSpace> spaces = new List<AECSpace>();
            spaces.Add(thisSpace);
            spaces.Add(thatSpace);
            spaces.Add(othrSpace);

            AECSpaceGroup spaceGroup = new AECSpaceGroup(spaces);
            spaceGroup.MoveBy(1, 1, 1);

            AECSpace space = spaceGroup.GetSpace(0);

            Assert.AreEqual(1, space.PointsBox.SW.X);
            Assert.AreEqual(1, space.PointsBox.SW.Y);
            Assert.AreEqual(1, space.PointsBox.SW.Z);

            Assert.AreEqual(5, space.PointsBox.NE.X);
            Assert.AreEqual(5, space.PointsBox.NE.Y);
            Assert.AreEqual(1, space.PointsBox.NE.Z);

            space = spaceGroup.GetSpace(1);

            Assert.AreEqual(1, space.PointsBox.SW.X);
            Assert.AreEqual(1, space.PointsBox.SW.Y);
            Assert.AreEqual(1, space.PointsBox.SW.Z);

            Assert.AreEqual(5, space.PointsBox.NE.X);
            Assert.AreEqual(5, space.PointsBox.NE.Y);
            Assert.AreEqual(1, space.PointsBox.NE.Z);

            space = spaceGroup.GetSpace(2);

            Assert.AreEqual(1, space.PointsBox.SW.X);
            Assert.AreEqual(1, space.PointsBox.SW.Y);
            Assert.AreEqual(1, space.PointsBox.SW.Z);

            Assert.AreEqual(5, space.PointsBox.NE.X);
            Assert.AreEqual(5, space.PointsBox.NE.Y);
            Assert.AreEqual(1, space.PointsBox.NE.Z);
        }//method

        [TestMethod]
        public void MoveTo()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(points);
            AECSpace thatSpace = new AECSpace(points);
            AECSpace othrSpace = new AECSpace(points);

            List<AECSpace> spaces = new List<AECSpace>();
            spaces.Add(thisSpace);
            spaces.Add(thatSpace);
            spaces.Add(othrSpace);

            AECSpaceGroup spaceGroup = new AECSpaceGroup(spaces);
            spaceGroup.MoveTo(new AECPoint(0, 0, 0), new AECPoint(1, 1, 1));

            AECSpace space = spaceGroup.GetSpace(0);

            Assert.AreEqual(1, space.PointsBox.SW.X);
            Assert.AreEqual(1, space.PointsBox.SW.Y);
            Assert.AreEqual(1, space.PointsBox.SW.Z);

            Assert.AreEqual(5, space.PointsBox.NE.X);
            Assert.AreEqual(5, space.PointsBox.NE.Y);
            Assert.AreEqual(1, space.PointsBox.NE.Z);

            space = spaceGroup.GetSpace(1);

            Assert.AreEqual(1, space.PointsBox.SW.X);
            Assert.AreEqual(1, space.PointsBox.SW.Y);
            Assert.AreEqual(1, space.PointsBox.SW.Z);

            Assert.AreEqual(5, space.PointsBox.NE.X);
            Assert.AreEqual(5, space.PointsBox.NE.Y);
            Assert.AreEqual(1, space.PointsBox.NE.Z);

            space = spaceGroup.GetSpace(2);

            Assert.AreEqual(1, space.PointsBox.SW.X);
            Assert.AreEqual(1, space.PointsBox.SW.Y);
            Assert.AreEqual(1, space.PointsBox.SW.Z);

            Assert.AreEqual(5, space.PointsBox.NE.X);
            Assert.AreEqual(5, space.PointsBox.NE.Y);
            Assert.AreEqual(1, space.PointsBox.NE.Z);
        }//method

        [TestMethod]
        public void Rotate()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(points);
            AECSpace thatSpace = new AECSpace(points);
            AECSpace othrSpace = new AECSpace(points);

            List<AECSpace> spaces = new List<AECSpace>();
            spaces.Add(thisSpace);
            spaces.Add(thatSpace);
            spaces.Add(othrSpace);

            AECSpaceGroup spaceGroup = new AECSpaceGroup(spaces);
            spaceGroup.Rotate(90, points[0]);

            AECSpace space = spaceGroup.GetSpace(0);

            Assert.AreEqual(-4, space.PointsBox.SW.X);
            Assert.AreEqual(0, space.PointsBox.SW.Y);

            Assert.AreEqual(0, space.PointsBox.NE.X);
            Assert.AreEqual(4, space.PointsBox.NE.Y);

            space = spaceGroup.GetSpace(1);

            Assert.AreEqual(-4, space.PointsBox.SW.X);
            Assert.AreEqual(0, space.PointsBox.SW.Y);

            Assert.AreEqual(0, space.PointsBox.NE.X);
            Assert.AreEqual(4, space.PointsBox.NE.Y);

            space = spaceGroup.GetSpace(2);

            Assert.AreEqual(-4, space.PointsBox.SW.X);
            Assert.AreEqual(0, space.PointsBox.SW.Y);

            Assert.AreEqual(0, space.PointsBox.NE.X);
            Assert.AreEqual(4, space.PointsBox.NE.Y);
        }//method

        [TestMethod]
        public void Scale()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(points);
            AECSpace thatSpace = new AECSpace(points);
            AECSpace othrSpace = new AECSpace(points);

            List<AECSpace> spaces = new List<AECSpace>();
            spaces.Add(thisSpace);
            spaces.Add(thatSpace);
            spaces.Add(othrSpace);

            AECSpaceGroup spaceGroup = new AECSpaceGroup(spaces);
            spaceGroup.Scale(xScale: 2, yScale: 2);

            AECSpace space = spaceGroup.GetSpace(0);
            Assert.AreEqual(64, space.Area);

            space = spaceGroup.GetSpace(1);
            Assert.AreEqual(64, space.Area);

            space = spaceGroup.GetSpace(2);
            Assert.AreEqual(64, space.Area);
        }//method

        [TestMethod]
        public void SetHeight()
        {
            List<AECPoint> points = new List<AECPoint>
            {
                new AECPoint(0, 0, 0),
                new AECPoint(4, 0, 0),
                new AECPoint(4, 4, 0),
                new AECPoint(0, 4, 0)
            };
            AECSpace thisSpace = new AECSpace(points);
            AECSpace thatSpace = new AECSpace(points);
            AECSpace othrSpace = new AECSpace(points);

            List<AECSpace> spaces = new List<AECSpace>();
            spaces.Add(thisSpace);
            spaces.Add(thatSpace);
            spaces.Add(othrSpace);

            AECSpaceGroup spaceGroup = new AECSpaceGroup(spaces);
            spaceGroup.SetHeight(12);

            AECSpace space = spaceGroup.GetSpace(0);
            Assert.AreEqual(12, space.Height);

            space = spaceGroup.GetSpace(1);
            Assert.AreEqual(12, space.Height);

            space = spaceGroup.GetSpace(2);
            Assert.AreEqual(12, space.Height);
        }//method
    }//class
}//namespace