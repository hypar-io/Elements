using Hypar.Elements;
using Hypar.Geometry;
using System;
using System.IO;
using Xunit;

namespace Hypar.Tests
{
    public class SlabTests
    {
        [Fact]
        public void ValidHoles_Construct_Success()
        {
            var model = new Model();
            var poly = Profiles.Square(new Vector2(), 20, 20);
            var hole1 = Profiles.Square(new Vector2(-3,-3), 5, 5).Reversed();
            var hole2 = Profiles.Square(new Vector2(6,6), 3, 3).Reversed();
            var hole3 = Profiles.Square(new Vector2(5,1), 1, 1).Reversed();
            var slab = new Slab(poly, new[]{hole1, hole2}, 0.0, 0.5, model.Materials[BuiltInMaterials.CONCRETE]);
            var slab2 = new Slab(poly, new[]{hole1, hole3}, 10.0, 0.2, model.Materials[BuiltInMaterials.CONCRETE]);
            var slab3 = new Slab(poly, new[]{hole1, hole2, hole3}, 20.0, 0.2, model.Materials[BuiltInMaterials.CONCRETE]);
            var slab4 = new Slab(poly, new Polygon2[]{}, 30.0, 0.2, model.Materials[BuiltInMaterials.CONCRETE]);
            model.AddElement(slab);
            model.AddElement(slab2);
            model.AddElement(slab3);
            model.AddElement(slab4);
            model.SaveGlb("slabs.glb");
            Assert.True(File.Exists("slabs.glb"));
            Assert.Equal(4, model.Elements.Count);
        }

        [Fact]
        public void ZeroThickness_Construct_ThrowsException()
        {
            var model = new Model();
            var poly = Profiles.Square(new Vector2(), 20, 20);
            Assert.Throws<ArgumentOutOfRangeException>(()=>new Slab(poly, new Polygon2[]{}, 0.0, 0.0, model.Materials[BuiltInMaterials.DEFAULT]));
        }

        [Fact]
        public void HoleOnEdge_Construct_ThrowsException()
        {
            Assert.True(false, "Implement this test.");
        }

        [Fact]
        public void HoleOutsidePerimeter_Construct_ThrowsException()
        {
            Assert.True(false, "Implement this test.");
        }
    }
}