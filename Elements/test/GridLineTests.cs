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
            gridline.Line = new Line(new Vector3(), new Vector3(25, 25, 0));
            gridline.Material = new Material("Red", new Color(1, 0, 0, 1));

            this.Model.AddElement(gridline);
        }
    }
}