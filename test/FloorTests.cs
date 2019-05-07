using Elements;
using Elements.Geometry;
using System;
using System.Collections.Generic;
using Xunit;

namespace Elements.Tests
{
    public class FloorTests : ModelTest
    {
        [Fact]
        public void Floor()
        {
            this.Name = "Floor";
            var p = Polygon.Rectangle(10, 10);
            var floorType = new FloorType("test", new List<MaterialLayer>{new MaterialLayer(new Material("green", Colors.Green, 0.0f,0.0f),0.1)});
            var openings = new Opening[]{
                new Opening(1, 1, 1, 1),
                new Opening(-2, 3, 3, 1),
                new Opening(2, -2, 1, 4),
            };
            var floor = new Floor(p, floorType, 0.5, BuiltInMaterials.Concrete, null, openings);
            var model = new Model();
            
            var s = new Space(Polygon.Rectangle(1,1), 1, 0.5, BuiltInMaterials.Mass);
            this.Model.AddElement(s);

            Assert.Equal(3, floor.Openings.Length);
            Assert.Equal(0.5, floor.Elevation);
            Assert.Equal(0.1, floor.ElementType.Thickness());
            Assert.Equal(0.4, floor.Transform.Origin.Z);
            this.Model.AddElement(floor);
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
            var p1 = Polygon.Rectangle(1, 1, new Vector3(1,1,0));
            var p2 = Polygon.Rectangle(1, 1, new Vector3(3,3,0));
            var profile = new Profile(Polygon.Rectangle(10, 10), new[]{p1,p2});
            var floorType = new FloorType("test", 0.2);
            var floor = new Floor(profile, floorType, 0.0, BuiltInMaterials.Concrete);
            Assert.Equal(100.0-2.0, floor.Area());
        }
    }
}