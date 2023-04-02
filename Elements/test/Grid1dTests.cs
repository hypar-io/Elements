using System;
using System.Linq;
using Elements.Spatial;
using Xunit;
using Elements.Geometry;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Elements.Tests
{
    public class Grid1dTests : ModelTest
    {
        [Fact, Trait("Category", "Examples")]
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

            // Take the fourth grid segment and subdivide it by a repeating pattern
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
                walls.Add(new StandardWall(wallLine, 0.1, 3.0, new Material(color.ToString(), color, 0, 0, null, false, false)));
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

        [Fact]
        public void SplitAtOffset()
        {
            var grid = new Grid1d(30);
            grid.SplitAtOffset(5);
            grid.SplitAtOffset(5, true);
            Assert.Equal(5, grid.Cells[0].Domain.Length);
            Assert.Equal(20, grid.Cells[1].Domain.Length);
            Assert.Equal(5, grid.Cells[2].Domain.Length);
        }

        [Fact]
        public void SplitAtPositions()
        {
            var positions = new[] { 3.0, 8, 5, 4 };
            var grid = new Grid1d(10);
            grid.SplitAtPositions(positions);
            Assert.Equal(5, grid.Cells.Count);
            Assert.Equal(1, grid[1].Domain.Length);
            grid.SplitAtPosition(8); // should do nothing but not throw an error
            Assert.Equal(5, grid.Cells.Count);
        }

        [Fact]
        public void SplitAtPositionsInRightPlace()
        {
            var simpleLine = new Line(Vector3.Origin, new Vector3(10, 0, 0));
            var lineGrid = new Grid1d(simpleLine);
            var splitLocation = new Vector3(3, 0, 0);
            lineGrid.SplitAtPoint(splitLocation);
            Assert.True(lineGrid[0].GetCellGeometry().End.DistanceTo(splitLocation) < 0.01);

            var polyline = new Polyline(new[] { Vector3.Origin, new Vector3(3, 5), new Vector3(6, 2), new Vector3(10, -3) });
            var polylineGrid = new Grid1d(polyline);
            var polylineSplitLocation = new Vector3(3, 5);
            polylineGrid.SplitAtPoint(polylineSplitLocation);
            Assert.True(polylineGrid[0].GetCellGeometry().End.DistanceTo(polylineSplitLocation) < 0.01);

        }

        [Fact]
        public void AlreadySplitGridsThrowsExceptionWhenDividedByN()
        {
            var grid = new Grid1d();
            grid.SplitAtParameter(0.25);
            var ex = Assert.Throws<Exception>(() => grid.DivideByCount(10));
            Assert.Equal("This grid already has subdivisions. Maybe you meant to select a subgrid to divide?", ex.Message);
        }


        [Fact]
        public void DivideFromPosition()
        {
            var grid = new Grid1d(new Domain1d(-8, 8));
            grid.DivideByFixedLengthFromPosition(20, 10);
            Assert.Null(grid.Cells); // this should have been left undivided
            grid.DivideByFixedLengthFromPosition(4, 0);
            Assert.Equal(4, grid.Cells.Count);
        }

        [Fact]
        public void DivideFromPoint()
        {
            var grid = new Grid1d(new Line(new Vector3(-5, -5), new Vector3(5, 5)));
            grid.DivideByFixedLengthFromPoint(3, new Vector3(-4, 4));
            Assert.Equal(6, grid.Cells.Count);
        }

        [Fact]
        public void SplitGridAtParameters()
        {
            var grid = new Grid1d(new Domain1d(50, 100));

            grid.SplitAtParameter(0.25);
            var subCell = grid.Cells[1];
            subCell.DivideByCount(5);
            subCell.Cells[3].DivideByApproximateLength(1.2, EvenDivisionMode.Nearest);
            var allCells = grid.GetCells();
            var cellGeometry = allCells.Select(c => c.GetCellGeometry());
            Assert.Equal(11, allCells.Count);
        }

        [Fact]
        public void Grid1dSerializes()
        {
            var polyline = new Polyline(new[] {
                new Vector3(0,0,0),
                new Vector3(10,2,0),
                new Vector3(30,4,0),
            });
            var grid = new Grid1d(polyline);
            grid.DivideByCount(4);
            grid[3].DivideByFixedLength(0.4);
            var json = JsonConvert.SerializeObject(grid);
            var deserialized = JsonConvert.DeserializeObject<Grid1d>(json);
            Assert.Equal(grid.GetCells().Count, deserialized.GetCells().Count);
            Assert.Equal(0, (grid.Curve as Polyline).Start.DistanceTo((deserialized.Curve as Polyline).Start));
        }

        [Fact]
        public void GetSeparators()
        {
            var grid = new Grid1d(100);
            grid.DivideByCount(5);
            var pts = grid.GetCellSeparators();
            Assert.Equal(6, pts.Count);
            Assert.Equal(pts[0], new Vector3(0, 0, 0));
            Assert.Equal(pts[1], new Vector3(20, 0, 0));
            Assert.Equal(pts[5], new Vector3(100, 0, 0));

            grid[1].DivideByCount(3);
            var pts2 = grid.GetCellSeparators(true);
            Assert.Equal(8, pts2.Count);
        }

        [Fact]
        public void GridFromCurves()
        {
            var a = Vector3.Origin;
            var b = new Vector3(5, 0, 1);
            var c = new Vector3(5, 5, 2);
            var d = new Vector3(0, 5, 3);
            var e = new Vector3(0, 0, 4);
            var f = new Vector3(5, 0, 5);
            var ctrlPts = new List<Vector3> { a, b, c, d, e, f };

            var bezier1 = new Bezier(ctrlPts);

            var grid = new Grid1d(bezier1);
            grid.DivideByApproximateLength(0.5, EvenDivisionMode.RoundUp);
            var cellGeometry = grid.GetCells().Select(cl => cl.GetCellGeometry());
            Assert.Equal(26, cellGeometry.Count());

            var r = 2.0;
            var a1 = new Arc(new Vector3(5, 0), r, -90.0, 90.0);

            var arcGrid = new Grid1d(a1);
            arcGrid.DivideByApproximateLength(1, EvenDivisionMode.RoundDown);
            arcGrid.Cells[1].DivideByCount(4);
            var arcCellGeometry = arcGrid.GetCells().Select(cl => cl.GetCellGeometry());
            Assert.Equal(9, arcCellGeometry.Count());
        }


        [Fact]
        public void Grid1dFixedDivisions()
        {
            var length = 10;
            var panelTarget = 3;
            var sacrificial = 0;
            var inMiddle = new Grid1d(new Line(new Vector3(0, 0, 0), new Vector3(length, 0, 0)));
            var atStart = new Grid1d(new Line(new Vector3(0, 1, 0), new Vector3(length, 1, 0)));
            var atEnd = new Grid1d(new Line(new Vector3(0, 2, 0), new Vector3(length, 2, 0)));
            var atBothEnds = new Grid1d(new Line(new Vector3(0, 3, 0), new Vector3(length, 3, 0)));
            inMiddle.DivideByFixedLength(panelTarget, FixedDivisionMode.RemainderNearMiddle, sacrificial);
            atStart.DivideByFixedLength(panelTarget, FixedDivisionMode.RemainderAtStart, sacrificial);
            atEnd.DivideByFixedLength(panelTarget, FixedDivisionMode.RemainderAtEnd, sacrificial);
            atBothEnds.DivideByFixedLength(panelTarget, FixedDivisionMode.RemainderAtBothEnds, sacrificial);

            Assert.Equal(panelTarget, inMiddle.Cells.First().Domain.Length);
            Assert.Equal(panelTarget, inMiddle.Cells.Last().Domain.Length);

            Assert.NotEqual(panelTarget, atStart.Cells.First().Domain.Length);
            Assert.Equal(panelTarget, atStart.Cells.Last().Domain.Length);

            Assert.Equal(panelTarget, atEnd.Cells.First().Domain.Length);
            Assert.NotEqual(panelTarget, atEnd.Cells.Last().Domain.Length);

            Assert.NotEqual(panelTarget, atBothEnds.Cells.First().Domain.Length);
            Assert.NotEqual(panelTarget, atBothEnds.Cells.Last().Domain.Length);
        }

        [Fact]
        public void Grid1dApproximateLength()
        {
            var grid1 = new Grid1d(10.5);
            var grid2 = new Grid1d(10.5);
            var grid3 = new Grid1d(10.2);
            grid1.DivideByApproximateLength(6, EvenDivisionMode.Nearest);
            Assert.Equal(2, grid1.Cells.Count);

            grid2.DivideByApproximateLength(4, EvenDivisionMode.RoundUp);
            Assert.Equal(3, grid2.Cells.Count);

            grid3.DivideByApproximateLength(1, EvenDivisionMode.RoundDown);
            Assert.Equal(10, grid3.Cells.Count);
        }

        [Fact]
        public void DivideGridFromOrigin()
        {
            var grid1 = new Grid1d(10);
            grid1.DivideByFixedLengthFromPosition(3, 5);
            var cellGeo = grid1.GetCells().Select(c => c.GetCellGeometry());
            Assert.Equal(4, cellGeo.Count());
            Assert.Equal(2, cellGeo.Last().Length());
            Assert.Equal(3, cellGeo.ToArray()[1].Length());
        }

        [Fact]
        public void DivideByPattern()
        {
            var grid = new Grid1d(new Domain1d(60, 150));
            var pattern = new List<(string typename, double length)>
            {
                ("Solid", 1),
                ("Glazing", 3),
                ("Fin", 0.2)
            };
            grid.DivideByPattern(pattern, PatternMode.Cycle, FixedDivisionMode.RemainderAtBothEnds);
            var cells = grid.GetCells();
            var types = cells.Select(c => c.Type);

            for (int i = 0; i < 10; i++)
            {
                Assert.Equal(pattern[i % pattern.Count].length, cells[i + 1].Domain.Length, 3);
                Assert.Equal(pattern[i % pattern.Count].typename, cells[i + 1].Type);
            }
        }

        [Fact]
        public void DivideByPatternWithoutRemainder()
        {
            var grid = new Grid1d(6);
            var pattern = new List<(string typename, double length)>
            {
                ("A", 2),
                ("B", 1)
            };
            grid.DivideByPattern(pattern, PatternMode.Cycle, FixedDivisionMode.RemainderAtEnd);
            var cells = grid.GetCells();

            for (int i = 0; i < cells.Count; i++)
            {
                Assert.Equal(pattern[i % pattern.Count].typename, cells[i].Type);
                Assert.Equal(pattern[i % pattern.Count].length, cells[i].Domain.Length, 3);
            }
        }

        [Fact]
        public void PatternTooLongThrowsException()
        {
            var grid = new Grid1d(4);
            var pattern = new List<(string, double)>
            {
                ("Solid", 1),
                ("Glazing", 2),
                ("Fin", 1.1)
            };
            Exception ex = Assert.Throws<ArgumentException>(() => grid.DivideByPattern(pattern, PatternMode.None, FixedDivisionMode.RemainderAtBothEnds));

            Assert.Equal("The grid could not be constructed. Pattern length exceeds grid length.", ex.Message);
        }

        [Fact]
        public void PatternWithinEpsilonOfLengthDoesNotThrowException()
        {
            var grid = new Grid1d(4);
            var pattern = new List<(string, double)>
            {
                ("Solid", 1),
                ("Glazing", 2),
                ("Fin", 1.000001)
            };
            grid.DivideByPattern(pattern, PatternMode.None, FixedDivisionMode.RemainderAtBothEnds);
        }

        [Fact]
        public void TryToSplitButAlreadySplitAtLowerLevel()
        {
            //var grid = new Grid1d(100);
            //grid.DivideByCount(2); //now split at 50
            //grid[1].SplitAtParameter(0.5); // splitting child cell at halfway mark = 75 on the parent
            //grid.SplitAtPosition(75); // should silently do nothing.
            //Assert.Equal(3, grid.GetCells().Count);

            var grid2 = new Grid1d(256);
            grid2.DivideByCount(2); //split at 128
            grid2[0].DivideByCount(2); // split at 64
            grid2[0][0].DivideByCount(2); // split at 32
            grid2[0][0][0].DivideByCount(2); // split at 16
            grid2.SplitAtPosition(32);
            Assert.Equal(5, grid2.GetCells().Count);
            Assert.Equal(3, grid2.Cells.Count);
            Assert.Single(grid2[0].Cells);
            Assert.Equal(2, grid2[1].Cells.Count);
            Assert.Single(grid2[0][0].Cells);
            Assert.Equal(2, grid2[0][0][0].Cells.Count);
        }
    }
}
