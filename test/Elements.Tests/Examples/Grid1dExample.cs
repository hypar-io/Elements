using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Spatial;
using System.Linq;
using Xunit;

namespace Elements.Tests.Examples
{
    public class Grid1dExample : ModelTest
    {
        [Fact]
        public void Grid1d()
        {
            this.Name = "Elements_Spatial_Grid1d";

            // <example>            
            // Create a 1d Grid from a line
            var line = new Line(new Vector3(5, 0, 0), new Vector3(60, 0, 0));
            var grid = new Grid1d(line);

            // Divide the grid into sections of length 10, and leave remainders
            // at both ends
            grid.DivideByFixedLength(10, FixedDivisionMode.RemainderAtBothEnds);

            // Take the second grid segment and subdivide it
            // into 5 equal length segments
            grid[1].DivideByCount(5);

            // Take the third grid segment and subdivide it into
            // segments of approximate length 3
            grid[2].DivideByApproximateLength(3);

            //Take the fourth grid segment and subdivide it by a repeating pattern
            var pattern = new[] { 1.0, 1.5 };
            grid[3].DivideByPattern(pattern);

            // Retrieve all bottom-level cells.
            // Note that grid.Cells gets the top-level cells only, and
            // grid.GetCells() recursively gets the bottom-level individual cells. 
            var cells = grid.GetCells();

            // Get lines representing each cell
            var lines = cells.Select(c => c.GetCellGeometry()).OfType<Line>();

            // Create walls from lines, and assign a random color material
            List<Wall> walls = new List<Wall>();
            var rand = new Random();
            foreach (var wallLine in lines)
            {
                var color = new Color(rand.NextDouble(), rand.NextDouble(), rand.NextDouble(), 1.0);
                walls.Add(new StandardWall(wallLine, 0.1, 3.0, new Material(color, 0, 0, Guid.NewGuid(), color.ToString())));
            }

            // Create rectangles from top-level grid cells
            var topLevelCells = grid.Cells.Select(c => c.GetCellGeometry()).OfType<Line>();
            var cellRects = new List<ModelCurve>();
            foreach (var topLevelCell in topLevelCells)
            {
                var rect = Polygon.Rectangle(topLevelCell.Start - new Vector3(0, 2, 0), topLevelCell.End + new Vector3(0, 2, 0));
                cellRects.Add(new ModelCurve(rect));
            }
            // </example>

            this.Model.AddElements(cellRects);
            this.Model.AddElements(walls);
        }
    }
}