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
            this.GenerateJson = false;
        }

        [Fact, Trait("Category", "Example")]
        public void Example()
        {
            this.Name = "Elements_ModelText";

            // <example>
            var target = new Vector3(15, 20);
            var squareSize = 25.0;
            var maxDistance = Math.Sqrt(Math.Pow(squareSize, 2) + Math.Pow(squareSize, 2));

            var texts = new List<(Vector3 location, Vector3 direction, string text, Color? color)>();
            var dir = Vector3.YAxis.Negate();

            for (var x = 0.0; x < squareSize; x += 1.0)
            {
                for (var y = 0.0; y < squareSize; y += 1.0)
                {
                    var c = new Color(x / squareSize, y / squareSize, 0.0, 1.0);
                    texts.Add((new Vector3(x, y), dir, $"[{x},{y}]", c));
                }
            }

            // Create a model text object.
            var modelText = new ModelText(texts, FontSize.PT36, 5);
            // </example>

            this.Model.AddElement(modelText);
        }
    }
}