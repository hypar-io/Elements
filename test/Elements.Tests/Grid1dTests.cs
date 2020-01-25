using System;
using System.Linq;
using System.IO;
using Elements.MathUtils;
using Elements.Spatial;
using Newtonsoft.Json;
using Xunit;
using Elements.Geometry;
using System.Collections.Generic;

namespace Elements.Tests
{
    public class Grid1dTests : ModelTest
    {
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
            var cellJson = JsonConvert.SerializeObject(cellGeometry);
            File.WriteAllText("/Users/andrewheumann/Desktop/cell-test.json", cellJson);
            Assert.Equal(11, allCells.Count);
        }

        [Fact]
        public void GridOnCurves()
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
            //var cellJson = JsonConvert.SerializeObject(cellGeometry);
            //File.WriteAllText("/Users/andrewheumann/Desktop/bz-cell-test.json", cellJson);
            Assert.Equal(25, cellGeometry.Count());


            var r = 2.0;
            var a1 = new Arc(new Vector3(5, 0), r, -90.0, 90.0);

            var arcGrid = new Grid1d(a1);
            arcGrid.DivideByApproximateLength(1, EvenDivisionMode.RoundDown);
            arcGrid.Cells[1].DivideByCount(4);
            var arcCellGeometry = arcGrid.GetCells().Select(cl => cl.GetCellGeometry());
            //var arcCellJson = JsonConvert.SerializeObject(arcCellGeometry);
            //File.WriteAllText("/Users/andrewheumann/Desktop/arc-cell-test.json", arcCellJson);
            Assert.Equal(9, arcCellGeometry.Count());
        }


        [Fact]
        public void Grid1dFixedDivisions()
        {
            var length = 18.245354;
            var panelTarget = 1.5;
            var sacrificial = 1;
            var inMiddle = new Grid1d(new Line(new Vector3(0, 0, 0), new Vector3(length, 0, 0)));
            var atStart = new Grid1d(new Line(new Vector3(0, 1, 0), new Vector3(length, 1, 0)));
            var atEnd = new Grid1d(new Line(new Vector3(0, 2, 0), new Vector3(length, 2, 0)));
            var atBothEnds = new Grid1d(new Line(new Vector3(0, 3, 0), new Vector3(length, 3, 0)));
            inMiddle.DivideByFixedLength(panelTarget, FixedDivisionMode.RemainderNearMiddle,sacrificial);
            atStart.DivideByFixedLength(panelTarget, FixedDivisionMode.RemainderAtStart, sacrificial);
            atEnd.DivideByFixedLength(panelTarget, FixedDivisionMode.RemainderAtEnd, sacrificial);
            atBothEnds.DivideByFixedLength(panelTarget, FixedDivisionMode.RemainderAtBothEnds, sacrificial);
            var cellGeo = new List<IEnumerable<Curve>>();
            cellGeo.Add(inMiddle.GetCells().Select(cl => cl.GetCellGeometry()));
            cellGeo.Add(atStart.GetCells().Select(cl => cl.GetCellGeometry()));
            cellGeo.Add(atEnd.GetCells().Select(cl => cl.GetCellGeometry()));
            cellGeo.Add(atBothEnds.GetCells().Select(cl => cl.GetCellGeometry()));
            var json = JsonConvert.SerializeObject(cellGeo);
            File.WriteAllText("/Users/andrewheumann/Desktop/fixedDivision-test.json", json);

        }

        [Fact]
        public void DivideAGridThatsTooSmall()
        {
            var grid1 = new Grid1d(1);
            var grid2 = new Grid1d(1);
            var grid3 = new Grid1d(1);
            grid1.DivideByApproximateLength(6, EvenDivisionMode.RoundDown);
            grid2.DivideByFixedLength(2, FixedDivisionMode.RemainderAtBothEnds);
            grid3.DivideByFixedLength(1.5, FixedDivisionMode.RemainderAtStart);
        }

    }
}
