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
        public void Single_WithinPerimeters_Valid()
        {
            var p = Profiles.Rectangular();
            var floor = new Floor(p, 0.2);
            Assert.Equal(0.0, floor.Elevation);
            Assert.Equal(0.2, floor.Thickness);
            Assert.Equal(p, floor.Location);
        }

        [Fact]
        public void Collection_WithinPerimeters_Valid()
        {
            var p1 = Profiles.Rectangular(width:5.0, height:10.0);
            var p2 = Profiles.Rectangular(width:1.0, height:2.0);
            var slabs = new[]{p1,p2}.Select(p=>{
                return new Floor(p, 0.1);
            });
            Assert.Equal(2, slabs.Count());
            var model = new Model();
            model.AddElements(slabs);
            model.SaveGlb("slabTests.glb");
        }

        [Fact]
        public void Params_WithinPerimeters_Valid()
        {
            var p1 = Profiles.Rectangular(width:5.0, height:10.0);
            var p2 = Profiles.Rectangular(width:1.0, height:2.0);
            var slabs = new[]{p1,p2}.Select(p => {
                return new Floor(p, 0.1);
            });
            Assert.Equal(2, slabs.Count());
        }

        [Fact]
        public void ZeroThickness_WithThickness_ThrowsException()
        {
            var model = new Model();
            var poly = Profiles.Rectangular(width:20, height:20);
            Assert.Throws<ArgumentOutOfRangeException>(()=> new Floor(poly, 0.0));
        }

        [Fact]
        public void Floor_Area()
        {
            var p1 = Profiles.Rectangular(new Vector3(1,1,0), 1, 1).Reversed();
            var p2 = Profiles.Rectangular(new Vector3(2,2,0), 1, 1).Reversed();
            var floor = new Floor(Profiles.Rectangular(Vector3.Origin(), 10, 10), new []{p1, p2}, 0.0, 0.2, BuiltInMaterials.Concrete);
            Assert.Equal(100.0-2.0, floor.Area);
        }
    }
}