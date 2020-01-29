using System;
using System.IO;
using System.Linq;
using Elements.Spatial;
using Xunit;
using Newtonsoft.Json;
using Elements.Geometry;
using System.Collections.Generic;
using Elements.MathUtils;

namespace Elements.Tests
{
    public class Grid2dTests : ModelTest
    {
        [Fact]
        public void GenerateAndSubdivide2d()
        {
            var grid = new Grid2d(10, 10);
            grid.U.SplitAtPosition(2);
            grid.U.SplitAtPosition(7);
            grid.V.SplitAtPosition(5);
            var col = grid[1];
            var subGrid = grid[1, 0];
            subGrid.U.DivideByCount(5);
            var subGrid2 = grid[1, 1];
            subGrid2.V.DivideByFixedLengthFromPosition(0.5, 8);

            var geo = grid.GetCells().Select(c => c.GetCellGeometry());
            var json = JsonConvert.SerializeObject(geo);
            File.WriteAllText("/Users/andrewheumann/Desktop/grid2dTest.json", json);
        }


        [Fact]
        public void GridFromPolygon()
        {
            var a = new Vector3(0.03, 5.08);
            var b = new Vector3(4.28, 9.80);
            var c = new Vector3(9.69, 9.50);
            var d = new Vector3(9.63, 2.43);
            var e = new Vector3(4.72, -0.86);
            var f = new Vector3(1.78, -0.75);

            var polygon = new Polygon(new[] { a, b, c, d, e, f });

            var g = new Vector3(7.735064, 5.746821);
            var h = new Vector3(6.233137, 7.248748);
            var i = new Vector3(3.660163, 4.675775);
            var j = new Vector3(5.162091, 3.173848);

            var polygon2 = new Polygon(new[] { g, h, i, j });



            var alignment = new Transform();
            alignment.Rotate(Vector3.ZAxis, 45);
            var grid = new Grid2d(new[] { polygon, polygon2 }, alignment);
            grid.U.DivideByCount(10);
            var panelA = ("A", 1.0);
            var panelB = ("B", 0.5);
            var panelC = ("C", 1.5);
            var pattern = new List<(string, double)> { panelA, panelB, panelC };
            var pattern2 = new List<(string, double)> { panelB, panelA };
            var patterns = new[] { pattern, pattern2 };

            for (int index = 0; index < grid.CellsFlat.Count; index++)
            {
                var vDomain = grid.CellsFlat[index].V.Domain;
                var start = 0.1.MapToDomain(vDomain);
                grid.CellsFlat[index].V.DivideByPattern(patterns[index % patterns.Count()], PatternMode.Cycle, FixedDivisionMode.RemainderAtBothEnds);
            }
            var cells = grid.GetCells();
            var geo = cells.Select(cl => cl.GetTrimmedCellGeometry());
            var types = cells.Select(cl => cl.Type);
            var trimmed = cells.Select(cl => cl.IsTrimmed());

            var resultDict = new Dictionary<string, object>();
            resultDict["Input Polygon"] = polygon;
            resultDict["Geometry"] = geo;
            resultDict["Types"] = types;
            resultDict["Trimmed"] = trimmed;

            var json = JsonConvert.SerializeObject(resultDict);
            File.WriteAllText($"/Users/andrewheumann/Desktop/grid2dRotatedTest.json", json);

        }
    }
}
