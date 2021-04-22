using System;
using System.Collections.Generic;
using Elements.Analysis;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class AnalysisImageTests : ModelTest
    {
        [Fact, Trait("Category", "Examples")]
        public void AnalysisImage()
        {
            this.Name = "Elements_Analysis_AnalysisImage_Example";

            var origin = new Vector3(0, 0);
            var width = 900;
            var height = 900;
            var rect = Polygon.Rectangle(new Vector3(origin.X - width / 2, origin.Y - height / 2), new Vector3(origin.X + width / 2, origin.Y + height / 2));

            Func<Vector3, double> analyze = (v) =>
            {
                return (Math.Abs(v.X / width) + Math.Abs(v.Y / height)) / 2;
            };

            var colorScale = new ColorScale(new List<Elements.Geometry.Color>() { Colors.Teal, Colors.Yellow, Colors.Coral });

            var analysisImage = new AnalysisImage(rect, 10, 10, colorScale, analyze);
            analysisImage.Analyze();
            this.Model.AddElement(analysisImage.GetMeshElement());
        }
    }
}