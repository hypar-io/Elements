using Hypar.Elements;
using Hypar.Geometry;
using System;
using System.Linq;
using System.IO;
using Xunit;

namespace Hypar.Tests
{
    public class FloorTests
    {
        [Fact]
        public void Example()
        {
            var p = Profiles.Rectangular(Vector3.Origin, 5, 5);
            var p1 = Profiles.Rectangular(new Vector3(3,2,0), 3, 1).Reversed();
            var floor = new Floor(p, 0.0, 0.2, new[]{p1});
            var model = new Model();
            model.AddElement(floor);
            model.SaveGlb("floor.glb");
        }

        [Fact]
        public void Construct()
        {
            var p = Profiles.Rectangular();
            var floor = new Floor(p, 0.0, 0.2);
            Assert.Equal(0.0, floor.Elevation);
            Assert.Equal(0.2, floor.Thickness);
            Assert.Equal(p, floor.Perimeter);
        }

        [Fact]
        public void ZeroThickness_ThrowsException()
        {
            var model = new Model();
            var poly = Profiles.Rectangular(width:20, height:20);
            Assert.Throws<ArgumentOutOfRangeException>(()=> new Floor(poly, 0.0, 0.0));
        }

        [Fact]
        public void Area()
        {
            // A floor with two holes punched in it.
            var p1 = Profiles.Rectangular(new Vector3(1,1,0), 1, 1).Reversed();
            var p2 = Profiles.Rectangular(new Vector3(3,3,0), 1, 1).Reversed();
            var floor = new Floor(Profiles.Rectangular(Vector3.Origin, 10, 10), 0.0, 0.2, new []{p1, p2}, BuiltInMaterials.Concrete);
            Assert.Equal(100.0-2.0, floor.Area());
        }
    }
}