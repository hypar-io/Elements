using System;
using System.Collections.Generic;
using Elements.Analysis;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests
{
    public class AnalysisMeshTests : ModelTest
    {
        [Fact, Trait("Category", "Examples")]
        public void AnalysisMesh()
        {
            this.Name = "Elements_Analysis_AnalysisMesh";

            // <example>
            var perimeter1 = Polygon.L(10, 10, 3);
            var perimeter2 = Polygon.Ngon(5, 5);
            var move = new Transform(3, 7, 0);
            var perimeter = perimeter1.Union(move.OfPolygon(perimeter2));
            var mc = new ModelCurve(perimeter);
            this.Model.AddElement(mc);

            // Construct a mass from which we will measure
            // distance to the analysis mesh's cells.
            var center = perimeter.Centroid();
            var mass = new Mass(Polygon.Rectangle(1, 1));
            mass.Transform.Move(center);
            this.Model.AddElement(mass);

            // The analyze function computes the distance
            // to the attractor.
            var analyze = new Func<Vector3, double>((v) =>
            {
                return center.DistanceTo(v);
            });

            // Construct a color scale from a small number
            // of colors.
            var colorScale = new ColorScale(new List<Color>() { Colors.Cyan, Colors.Purple, Colors.Orange }, 10);

            var analysisMesh = new AnalysisMesh(perimeter, 0.2, 0.2, colorScale, analyze);
            analysisMesh.Analyze();
            // </example>

            this.Model.AddElement(analysisMesh);
        }
    }
}