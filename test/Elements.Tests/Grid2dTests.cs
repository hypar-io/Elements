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

            var subGrid = grid.FindCellAtPosition(5, 3);
            subGrid.U.DivideByCount(5);
            var subGrid2 = grid.FindCellAtPosition(5, 8);
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

            var alignment = new Transform();
            alignment.Rotate(Vector3.ZAxis, 30);
            var grid = new Grid2d(polygon, alignment);
            grid.U.DivideByCount(10);
            var pattern = new[] { 2, 3 };

            for (int i = 0; i < grid.Cells.Count; i++)
            {
                grid.Cells[i].V.DivideByFixedLengthFromPosition(2, 2 + pattern[i % pattern.Length]);
            }

            var geo = grid.GetCells().Select(cl => cl.GetCellGeometry());

            var resultDict = new Dictionary<string, object>();
            resultDict["Input Polygon"] = polygon;
            resultDict["Geometry"] = geo;
            

            var json = JsonConvert.SerializeObject(resultDict);
            File.WriteAllText("/Users/andrewheumann/Desktop/grid2dRotatedTest.json", json);
        }
    }
}
