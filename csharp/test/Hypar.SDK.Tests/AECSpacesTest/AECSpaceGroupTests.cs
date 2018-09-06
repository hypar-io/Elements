using System.Collections.Generic;
using AECSpaces;
using Xunit;

namespace AECSpacesTest
{
    public class SpaceGroupTests
    {
        [Fact]
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
            Assert.Equal(32, spaceGroup.Area, 0);
        }

        [Fact]
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

            Assert.Equal("othr", spaces[0].Name);
            Assert.Equal("that", spaces[1].Name);
            Assert.Equal("this", spaces[2].Name);
        }

        [Fact]
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

            Assert.Equal(3, spaceGroup.Count);
        }

        [Fact]
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

            Assert.Equal(32, spaceGroup.Volume, 0);
        }

        [Fact]
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

            Assert.Equal(32, othrSpace.Area, 0);
        }

        /* -------- Begin Method Tests -------- */

        [Fact]
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

            Assert.Equal(0, spaceGroup.Count);
        }

        [Fact]
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

            Assert.Equal(2, spaceGroup.Count);
        }

        [Fact]
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

            Assert.Equal("that", space.Name);
        }

        [Fact]
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

            Assert.Equal("that", space.Name);
        }

        [Fact]
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

            Assert.Equal("that", space.Name);
        }

        [Fact]
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

            Assert.Equal(2, spacesByName.Count);
        }

        [Fact]
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

            Assert.Equal(1, space.PointsBox.SW.X);
            Assert.Equal(1, space.PointsBox.SW.Y);
            Assert.Equal(1, space.PointsBox.SW.Z);

            Assert.Equal(5, space.PointsBox.NE.X);
            Assert.Equal(5, space.PointsBox.NE.Y);
            Assert.Equal(1, space.PointsBox.NE.Z);

            space = spaceGroup.GetSpace(1);

            Assert.Equal(1, space.PointsBox.SW.X);
            Assert.Equal(1, space.PointsBox.SW.Y);
            Assert.Equal(1, space.PointsBox.SW.Z);

            Assert.Equal(5, space.PointsBox.NE.X);
            Assert.Equal(5, space.PointsBox.NE.Y);
            Assert.Equal(1, space.PointsBox.NE.Z);

            space = spaceGroup.GetSpace(2);

            Assert.Equal(1, space.PointsBox.SW.X);
            Assert.Equal(1, space.PointsBox.SW.Y);
            Assert.Equal(1, space.PointsBox.SW.Z);

            Assert.Equal(5, space.PointsBox.NE.X);
            Assert.Equal(5, space.PointsBox.NE.Y);
            Assert.Equal(1, space.PointsBox.NE.Z);
        }

        [Fact]
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

            Assert.Equal(1, space.PointsBox.SW.X);
            Assert.Equal(1, space.PointsBox.SW.Y);
            Assert.Equal(1, space.PointsBox.SW.Z);

            Assert.Equal(5, space.PointsBox.NE.X);
            Assert.Equal(5, space.PointsBox.NE.Y);
            Assert.Equal(1, space.PointsBox.NE.Z);

            space = spaceGroup.GetSpace(1);

            Assert.Equal(1, space.PointsBox.SW.X);
            Assert.Equal(1, space.PointsBox.SW.Y);
            Assert.Equal(1, space.PointsBox.SW.Z);

            Assert.Equal(5, space.PointsBox.NE.X);
            Assert.Equal(5, space.PointsBox.NE.Y);
            Assert.Equal(1, space.PointsBox.NE.Z);

            space = spaceGroup.GetSpace(2);

            Assert.Equal(1, space.PointsBox.SW.X);
            Assert.Equal(1, space.PointsBox.SW.Y);
            Assert.Equal(1, space.PointsBox.SW.Z);

            Assert.Equal(5, space.PointsBox.NE.X);
            Assert.Equal(5, space.PointsBox.NE.Y);
            Assert.Equal(1, space.PointsBox.NE.Z);
        }

        [Fact]
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

            Assert.Equal(-4, space.PointsBox.SW.X);
            Assert.Equal(0, space.PointsBox.SW.Y);

            Assert.Equal(0, space.PointsBox.NE.X);
            Assert.Equal(4, space.PointsBox.NE.Y);

            space = spaceGroup.GetSpace(1);

            Assert.Equal(-4, space.PointsBox.SW.X);
            Assert.Equal(0, space.PointsBox.SW.Y);

            Assert.Equal(0, space.PointsBox.NE.X);
            Assert.Equal(4, space.PointsBox.NE.Y);

            space = spaceGroup.GetSpace(2);

            Assert.Equal(-4, space.PointsBox.SW.X);
            Assert.Equal(0, space.PointsBox.SW.Y);

            Assert.Equal(0, space.PointsBox.NE.X);
            Assert.Equal(4, space.PointsBox.NE.Y);
        }

        [Fact]
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
            Assert.Equal(64, space.Area);

            space = spaceGroup.GetSpace(1);
            Assert.Equal(64, space.Area);

            space = spaceGroup.GetSpace(2);
            Assert.Equal(64, space.Area);
        }

        [Fact]
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
            Assert.Equal(12, space.Height);

            space = spaceGroup.GetSpace(1);
            Assert.Equal(12, space.Height);

            space = spaceGroup.GetSpace(2);
            Assert.Equal(12, space.Height);
        }
    }
}