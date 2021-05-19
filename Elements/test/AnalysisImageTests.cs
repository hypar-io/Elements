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
            this.Name = "Elements_Analysis_AnalysisImage";

            // <example>
            var origin = new Vector3(0, 0);
            var size = 900;
            var shape = Polygon.Rectangle(900, 900);

            Func<Vector3, double> analyze = (v) =>
            {
                return Math.Min(v.DistanceTo(origin) / size * 2, 1);
            };

            var colorScale = new ColorScale(new List<Elements.Geometry.Color>() { Colors.Magenta, Colors.Yellow, Colors.Lime, Colors.Teal });

            var analysisImage = new AnalysisImage(shape, 10, 10, colorScale, analyze);
            analysisImage.Analyze();
            this.Model.AddElement(analysisImage);
            // </example>
        }
    }
}