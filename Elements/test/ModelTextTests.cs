using System.Collections.Generic;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class ModelTextTests : ModelTest
    {
        public ModelTextTests()
        {
            this.GenerateIfc = false;
            this.GenerateJson = false;
        }

        [Fact, Trait("Category", "Example")]
        public void Example()
        {
            this.Name = "Elements_ModelText";

            var squareSize = 25.0;

            // <example>
            var texts = new List<(Vector3, Vector3, string)>();
            var d = Vector3.YAxis.Negate();
            for (var x = 0.0; x < squareSize; x += 1.0)
            {
                for (var y = 0.0; y < squareSize; y += 1.0)
                {
                    texts.Add((new Vector3(x, y), d, $"[{x},{y}]"));
                }
            }

            // Create a model arrows object.
            var modelText = new ModelText(texts, FontSize.PT36, 5);
            // </example>

            this.Model.AddElement(modelText);
        }
    }
}