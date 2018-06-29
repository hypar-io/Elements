using Hypar.Elements;
using Hypar.Geometry;
using System;
using System.Linq;
using System.IO;
using Xunit;

namespace Hypar.Tests
{
    public class SlabTests
    {
        [Fact]
        public void Single_WithinPerimeters_Valid()
        {
            var p = Profiles.Rectangular();
            var slab = Slab.WithinPerimeter(p);
            Assert.Equal(0.0, slab.Elevation);
            Assert.Equal(0.2, slab.Thickness);
            Assert.Equal(p, slab.Perimeter);
        }

        [Fact]
        public void Collection_WithinPerimeters_Valid()
        {
            var p1 = Profiles.Rectangular(width:5.0, height:10.0);
            var p2 = Profiles.Rectangular(width:1.0, height:2.0);
            var slabs = new[]{p1,p2}.WithinEachCreate<Slab>(p=>{
                return Slab.WithinPerimeter(p);
            });
            Assert.Equal(2, slabs.Count());
        }

        [Fact]
        public void Params_WithinPerimeters_Valid()
        {
            var p1 = Profiles.Rectangular(width:5.0, height:10.0);
            var p2 = Profiles.Rectangular(width:1.0, height:2.0);
            var slabs = new[]{p1,p2}.WithinEachCreate<Slab>(p => {
                return Slab.WithinPerimeter(p);
            });
            Assert.Equal(2, slabs.Count());
        }

        [Fact]
        public void ZeroThickness_WithThickness_ThrowsException()
        {
            var model = new Model();
            var poly = Profiles.Rectangular(width:20, height:20);
            Assert.Throws<ArgumentOutOfRangeException>(()=>Slab.WithinPerimeter(poly)
                                                                .WithThickness(0.0));
        }
    }
}