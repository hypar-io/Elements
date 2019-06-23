using System.Collections.Generic;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests.Examples
{
    public class FloorExample : ModelTest
    {
        [Fact]
        public void Floor()
        {
            this.Name = "Elements_Floor";

            // <example>
            // Create a floor type.
            var floorType = new FloorType("test", new List<MaterialLayer> { new MaterialLayer(BuiltInMaterials.Concrete, 0.1) });
            
            // Create some openings.
            var openings = new List<Opening>(){
                new Opening(1, 1, 1, 1),
                new Opening(3, 3, 1, 3),
            };

            // Create two floors.
            var p = Polygon.L(10, 20, 5);
            var floor1 = new Floor(p, floorType, 0, null, openings);
            var floor2 = new Floor(p, floorType, 3, null, openings);
            // </example>

            Assert.Equal(2, floor1.Openings.Count);
            Assert.Equal(0.0, floor1.Elevation);
            Assert.Equal(0.1, floor1.ElementType.Thickness());
            Assert.Equal(0.0, floor1.Transform.Origin.Z);

            this.Model.AddElement(floor1);
            this.Model.AddElement(floor2);
        }
    }
}