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
            var p = Profiles.Square();
            var slab = Slab.WithinPerimeter(p);
            Assert.Equal(0.0, slab.Elevation);
            Assert.Equal(0.2, slab.Thickness);
            Assert.Equal(p, slab.Perimeter);
        }

        [Fact]
        public void Collection_WithinPerimeters_Valid()
        {
            var p1 = Profiles.Square(width:5.0, height:10.0);
            var p2 = Profiles.Square(width:1.0, height:2.0);
            var slabs = Slab.WithinPerimeters(new[]{p1,p2});
            Assert.Equal(2, slabs.Count());
        }

        [Fact]
        public void Params_WithinPerimeters_Valid()
        {
            var p1 = Profiles.Square(width:5.0, height:10.0);
            var p2 = Profiles.Square(width:1.0, height:2.0);
            var slabs = Slab.WithinPerimeters(p1,p2);
            Assert.Equal(2, slabs.Count());
        }

        // [Fact]
        // public void ValidHoles_Construct_Success()
        // {
        //     var model = new Model();

        //     var poly = Profiles.Square(Vector3.Origin(), 20, 20);
        //     var hole1 = Profiles.Square(Vector3.ByXY(-3,-3), 5, 5).Reversed();
        //     var hole2 = Profiles.Square(Vector3.ByXY(6,6), 3, 3).Reversed();
        //     var hole3 = Profiles.Square(Vector3.ByXY(5,1), 1, 1).Reversed();

        //     var slab = Slab.WithinPerimeter(poly)
        //                     .WithHoles(new[]{hole1, hole2})
        //                     .WithThickness(0.03)
        //                     .AtElevation(0.0)
        //                     .OfMaterial(BuiltIntMaterials.Concrete);
            
        //     var slab2 = Slab.WithinPerimeter(poly)
        //                     .WithHoles(new[]{hole1, hole3})
        //                     .WithThickness(0.03)
        //                     .AtElevation(10.0)
        //                     .OfMaterial(BuiltIntMaterials.Concrete);

        //     var slab3 = Slab.WithinPerimeter(poly)
        //                     .WithHoles(new[]{hole1, hole2, hole3})
        //                     .WithThickness(0.03)
        //                     .AtElevation(30.0)
        //                     .OfMaterial(BuiltIntMaterials.Concrete);

        //     var slab4 = Slab.WithinPerimeter(poly)
        //                     .WithHoles(new Polyline[]{})
        //                     .WithThickness(0.03)
        //                     .AtElevation(40.0)
        //                     .OfMaterial(BuiltIntMaterials.Concrete);

        //     model.AddElements(new[]{slab,slab2,slab3,slab4});

        //     model.SaveGlb("slabs.glb");
        //     Assert.True(File.Exists("slabs.glb"));
        //     Assert.Equal(4, model.Elements.Count);
        // }

        [Fact]
        public void ZeroThickness_WithThickness_ThrowsException()
        {
            var model = new Model();
            var poly = Profiles.Square(width:20, height:20);
            Assert.Throws<ArgumentOutOfRangeException>(()=>Slab.WithinPerimeter(poly)
                                                                .WithThickness(0.0));
        }
    }
}