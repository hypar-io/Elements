using System.Collections.Generic;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests.Examples
{
    public class ModelArrowsTests : ModelTest
    {
        public ModelArrowsTests()
        {
            this.GenerateIfc = false;
        }

        [Fact, Trait("Category", "Example")]
        public void Example()
        {
            this.Name = "Elements_ModelArrows";

            // <example>
            // Create some point locations.
            var vectors = new List<(Vector3, Vector3, double)>();
            var target = new Vector3(15, 20);
            for (var x = 0; x < 25; x++)
            {
                for (var y = 0; y < 25; y++)
                {
                    var l = new Vector3(x, y);
                    var d = (target - l).Unitized();
                    var distance = target.DistanceTo(l);
                    vectors.Add((l, d, distance / 25));
                }
            }

            // Create a model arrows object.
            var modelArrows = new ModelArrows(vectors, true, true, material: BuiltInMaterials.ZAxis);
            // </example>

            this.Model.AddElement(modelArrows);
        }
    }
}