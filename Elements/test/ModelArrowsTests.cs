using System;
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
            // Create some arrow locations.
            var vectors = new List<(Vector3, Vector3, double, Color?)>();

            var target = new Vector3(15, 20);
            var squareSize = 25.0;
            var maxDistance = Math.Sqrt(Math.Pow(squareSize, 2) + Math.Pow(squareSize, 2));
            for (var x = 0.0; x < squareSize; x += 1.0)
            {
                for (var y = 0.0; y < squareSize; y += 1.0)
                {
                    var l = new Vector3(x, y);
                    var d = (target - l).Unitized();
                    var distance = target.DistanceTo(l);
                    var r = distance / maxDistance;
                    var c = new Color(x / squareSize, y / squareSize, 0.0, 1.0);
                    vectors.Add((l, d, r, c));
                }
            }

            // Create a model arrows object.
            var modelArrows = new ModelArrows(vectors, false, true);
            // </example>

            this.Model.AddElement(modelArrows);
        }
    }
}