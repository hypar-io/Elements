using System;
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
        }

        [Fact, Trait("Category", "Example")]
        public void Example()
        {
            Name = "Elements_ModelText";

            // <example>
            var squareSize = 25.0;

            var texts = new List<(Vector3 location, Vector3 facingDirection, Vector3 lineDirection, string text, Color? color)>();
            var dir = Vector3.YAxis.Negate();

            for (var x = 0.0; x < squareSize; x += 1.0)
            {
                for (var y = 0.0; y < squareSize; y += 1.0)
                {
                    var c = new Color(x / squareSize, y / squareSize, 0.0, 1.0);
                    texts.Add((new Vector3(x, y), dir, Vector3.XAxis, $"[{x},{y}]", c));
                }
            }

            // Create a model text object.
            var modelText = new ModelText(texts, FontSize.PT36, 30);
            // </example>

            this.Model.AddElement(modelText);
        }
    }
}