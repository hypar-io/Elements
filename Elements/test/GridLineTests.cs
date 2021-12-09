using System;
using System.Collections.Generic;
using Elements.Geometry;
using Xunit;

namespace Elements.Tests.Examples
{
    public class GridLinesTests : ModelTest
    {
        public GridLinesTests()
        {
            this.GenerateIfc = false;
        }

        [Fact, Trait("Category", "Example")]
        public void Example()
        {
            this.Name = "Elements_GridLines";

            var gridData = new List<(string name, Vector3 origin)>() {
                ("A", new Vector3()),
                ("B", new Vector3(10, 0, 0)),
                ("C", new Vector3(20, 0, 0)),
                ("D", new Vector3(30, 0, 0)),
            };

            var texts = new List<(Vector3 location, Vector3 facingDirection, Vector3 lineDirection, string text, Color? color)>();
            var radius = 1;
            var material = new Material("Red", new Color(1, 0, 0, 1));

            foreach (var (name, origin) in gridData)
            {
                var gridline = new GridLine();
                gridline.Name = name;
                gridline.Curve = new Line(origin, origin + new Vector3(25, 25, 0));
                gridline.Material = material;
                gridline.Radius = radius;

                var circleCenter = gridline.GetCircleTransform();
                texts.Add((circleCenter.Origin, circleCenter.ZAxis, circleCenter.XAxis, name, Colors.White));
                this.Model.AddElement(gridline);
            }

            this.Model.AddElement(new ModelText(texts, FontSize.PT72, 50));
        }

        [Fact]
        public void CurvedGridline()
        {
            this.Name = "Elements_GridLinesCurved";

            var size = 50;

            var gridline = new GridLine();
            gridline.Name = "A";
            gridline.Material = new Material("Red", new Color(1, 0, 0, 1));
            gridline.Curve = new Arc(new Vector3(), size, 0, 180);
            gridline.ExtensionBeginning = 100;
            gridline.ExtensionEnd = 100;

            this.Model.AddElement(gridline);

            var verticalGridline = new GridLine
            {
                Name = "B",
                Line = new Line(new Vector3(), new Vector3(0, 0, 25)),
                Material = new Material("Green", new Color(0, 1, 0, 1))
            };

            this.Model.AddElements(gridline, verticalGridline);
        }
    }
}