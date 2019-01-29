using Elements;
using Elements.Geometry;
using System;
using Xunit;

namespace Elements.Tests
{
    public class FloorTests : ModelTest
    {
        [Fact]
        public void Floor()
        {
            this.Name = "Floor";
            var p = Polygon.Rectangle(Vector3.Origin, 5, 5);
            var p1 = Polygon.Rectangle(new Vector3(3,2,0), 3, 1);
            var profile = new Profile(p, p1);
            var floorType = new FloorType("test", 0.2);
            var floor = new Floor(profile, floorType, 0.0);
            var model = new Model();
            this.Model.AddElement(floor);
        }

        [Fact]
        public void Construct()
        {
            var p = Polygon.Rectangle();
            var profile = new Profile(p);
            var floorType = new FloorType("test", 0.2);
            var floor = new Floor(profile, floorType, 1.0);
            Assert.Equal(1.0, floor.Elevation);
            Assert.Equal(0.2, floor.ElementType.Thickness);
            Assert.Equal(1.0, floor.Transform.Origin.Z);
        }

        [Fact]
        public void ZeroThickness()
        {
            var model = new Model();
            var poly = Polygon.Rectangle(width:20, height:20);
            Assert.Throws<ArgumentOutOfRangeException>(()=> {var floorType = new FloorType("test", 0.0);});
        }

        [Fact]
        public void Area()
        {
            // A floor with two holes punched in it.
            var p1 = Polygon.Rectangle(new Vector3(1,1,0), 1, 1);
            var p2 = Polygon.Rectangle(new Vector3(3,3,0), 1, 1);
            var profile = new Profile(Polygon.Rectangle(Vector3.Origin, 10, 10), new[]{p1,p2});
            var floorType = new FloorType("test", 0.2);
            var floor = new Floor(profile, floorType, 0.0, BuiltInMaterials.Concrete);
            Assert.Equal(100.0-2.0, floor.Area());
        }
    }
}