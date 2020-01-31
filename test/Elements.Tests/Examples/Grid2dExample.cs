using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Spatial;
using System.Linq;
using Xunit;

namespace Elements.Tests.Examples
{
    public class Grid2dExample : ModelTest
    {
        [Fact]
        public void Grid2d()
        {
            this.Name = "Elements_Spatial_Grid2d";

            // <example>
            // Create a 2d grid that's 40 x 30 in size
            var grid = new Grid2d(40, 30);

            // Access the U and V axes directly and use 1d subdivision methods on them
            grid.U.DivideByFixedLength(7, FixedDivisionMode.RemainderAtBothEnds);
            grid.V.DivideByPattern(new[] { 2.0, 5.0 });

            // Get a row by index
            var fifthRow = grid.GetRowAtIndex(4);
            // Divide U axis of all cells in row into panels of approximate width 1
            fifthRow.ForEach(c => c.U.DivideByApproximateLength(1));

            // Get a cell by u, v indices
            var cell = grid[1, 1];
            // Divide the cell in the V direction
            cell.V.DivideByCount(4);

            // Create a floor from the entire grid's boundary
            var floor = new Floor(new Profile((Polygon)grid.GetCellGeometry()), 0.5, new Transform(0, 0, -0.51));

            // Create model curves from all subdivided cells of the grid
            var modelCurves = grid.GetCells().Select(c => new ModelCurve(c.GetCellGeometry()));

            // </example>

            Model.AddElement(floor);
            Model.AddElements(modelCurves);
        }
    }
}