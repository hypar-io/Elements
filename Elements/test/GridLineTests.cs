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

            var gridline = new GridLine();
            gridline.Name = "A";
            gridline.Curve = new Line(new Vector3(), new Vector3(25, 25, 0));
            gridline.Material = new Material("Red", new Color(1, 0, 0, 1));

            this.Model.AddElement(gridline);
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
        }
    }
}