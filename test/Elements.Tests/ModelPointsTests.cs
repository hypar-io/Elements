using System.Collections.Generic;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests.Examples
{
    public class ModelPointsTests : ModelTest
    {
        public ModelPointsTests()
        {
            this.GenerateIfc = false;
        }

        [Fact, Trait("Category", "Example")]
        public void Example()
        {
            this.Name = "Elements_ModelPoints";

            // <example>
            // Create some point locations.
            var pts = new List<Vector3>();
            for (var x = 0; x < 25; x++)
            {
                for (var y = 0; y < 25; y++)
                {
                    for (var z = 0; z < 25; z++)
                    {
                        // Add points to the object.
                        pts.Add(new Vector3(x, y, z));
                    }
                }
            }

            // Create a model points object.
            var pink = new Material("pink", Colors.Pink);
            var modelPoints = new ModelPoints(pts, pink);
            // </example>

            this.Model.AddElement(modelPoints);
        }
    }
}