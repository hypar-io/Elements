using System;
using System.IO;
using System.Linq;
using Elements.Spatial;
using Xunit;
using Elements.Geometry;
using System.Collections.Generic;
using System.Text.Json;

namespace Elements.Tests
{
    public class Grid2dTests : ModelTest
    {
        [Fact, Trait("Category", "Examples")]
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
            var modelCurves = grid.ToModelCurves();
            // </example>

            Model.AddElement(floor);
            Model.AddElements(modelCurves);
        }

        [Fact]
        public void GenerateAndSubdivide2d()
        {
            var grid = new Grid2d(10, 10);
            grid.U.SplitAtPosition(2);
            grid.U.SplitAtPosition(7);
            grid.V.SplitAtPosition(5);
            var subGrid = grid[1, 0];
            subGrid.U.DivideByCount(5);
            var subGrid2 = grid[1, 1];
            subGrid2.V.DivideByFixedLengthFromPosition(0.5, 8);

            Assert.Equal(6, grid.CellsFlat.Count);
            Assert.Equal(19, grid.GetCells().Count);
        }

        [Fact]
        public void TrimBehavior()
        {
            Name = "TrimBehavior";
            var polygonjson = "[{\"discriminator\":\"Elements.Geometry.Polygon\",\"Vertices\":[{\"X\":-14.371519985751306,\"Y\":-4.8816304299427005,\"Z\":0.0},{\"X\":-17.661873645682569,\"Y\":9.2555712951713573,\"Z\":0.0},{\"X\":12.965610421927806,\"Y\":9.2555712951713573,\"Z\":0.0},{\"X\":12.965610421927806,\"Y\":3.5538269529982784,\"Z\":0.0},{\"X\":6.4046991240848143,\"Y\":3.5538269529982784,\"Z\":0.0},{\"X\":1.3278034769444158,\"Y\":-4.8816304299427005,\"Z\":0.0}]},{\"discriminator\":\"Elements.Geometry.Polygon\",\"Vertices\":[{\"X\":-9.4508365123690652,\"Y\":0.20473478280229102,\"Z\":0.0},{\"X\":-1.8745460850979974,\"Y\":0.20473478280229102,\"Z\":0.0},{\"X\":-1.8745460850979974,\"Y\":5.4378426037008651,\"Z\":0.0},{\"X\":-9.4508365123690652,\"Y\":5.4378426037008651,\"Z\":0.0}]}]\r\n";
            var polygons = JsonSerializer.Deserialize<List<Polygon>>(polygonjson);
            var grid = new Grid2d(polygons);
            foreach (var pt in polygons[1].Vertices)
            {
                grid.SplitAtPoint(pt);
            }
            grid.CellsFlat.ForEach(c => c.U.DivideByApproximateLength(1.0, EvenDivisionMode.RoundDown));

            var trimmedCells = grid.GetCells().Select(c =>
                (TrimmedGeometry: c.GetTrimmedCellGeometry(),
                BaseRect: c.GetCellGeometry(),
                IsTrimmed: c.IsTrimmed(),
                IsTrimmedButNotOutside: c.IsTrimmed(false)));

            foreach (var trimGeometry in trimmedCells)
            {
                var trimGeo = trimGeometry.TrimmedGeometry.OfType<Polygon>();
                var material = trimGeometry.IsTrimmed ? (trimGeometry.IsTrimmedButNotOutside ? BuiltInMaterials.XAxis : BuiltInMaterials.YAxis) : BuiltInMaterials.ZAxis;
                foreach (var t in trimGeo)
                {
                    Model.AddElement(new ModelCurve(t));
                    Model.AddElement(new Mass(t, 0.1, material, new Transform(0, 0, -1.001)));
                }
                Model.AddElement(new ModelCurve(trimGeometry.BaseRect, material, new Transform(0, 0, 1)));
            }
            Assert.Equal(87, trimmedCells.Count());
            Assert.Equal(18, trimmedCells.Count(c => c.IsTrimmedButNotOutside));
            Assert.Equal(35, trimmedCells.Count(c => c.IsTrimmed));
        }

        [Fact]
        public void TrimmedCellProfile()
        {
            var outer = new Polygon(new List<Vector3> { new Vector3(0, 0, 0), new Vector3(10, 0, 0), new Vector3(10, 0, 10), new Vector3(0, 0, 10) });
            var inner1 = new Polygon(new List<Vector3> { new Vector3(3, 0, 9), new Vector3(3, 0, 7), new Vector3(1, 0, 7), new Vector3(1, 0, 9) });
            var inner2 = new Polygon(new List<Vector3> { new Vector3(9, 0, 3), new Vector3(9, 0, 1), new Vector3(7, 0, 1), new Vector3(7, 0, 3) });
            var inner3 = new Polygon(new List<Vector3> { new Vector3(6, 0, 6), new Vector3(6, 0, 3), new Vector3(3, 0, 3), new Vector3(3, 0, 6) });

            var polygons = new List<Polygon>();
            polygons.Add(outer);
            polygons.Add(inner1);
            polygons.Add(inner2);
            polygons.Add(inner3);

            var grid = new Grid2d(polygons);
            var profiles = grid.GetTrimmedCellProfiles();

            Assert.Single(profiles);
            var profile = profiles.FirstOrDefault();
            Assert.Equal(3, profile.Voids.Count());
            Assert.Equal(100, profile.Perimeter.Area());
            Assert.Equal(4, profile.Voids[0].Area());
            Assert.Equal(9, profile.Voids[1].Area());
            Assert.Equal(4, profile.Voids[2].Area());
        }

        [Fact]
        public void TrimmedCellProfileAfterSplitting()
        {
            var outer = new Polygon(new List<Vector3> { new Vector3(0, 0, 0), new Vector3(10, 0, 0), new Vector3(10, 0, 10), new Vector3(0, 0, 10) });
            var inner1 = new Polygon(new List<Vector3> { new Vector3(3, 0, 9), new Vector3(3, 0, 7), new Vector3(1, 0, 7), new Vector3(1, 0, 9) });
            var inner2 = new Polygon(new List<Vector3> { new Vector3(9, 0, 3), new Vector3(9, 0, 1), new Vector3(7, 0, 1), new Vector3(7, 0, 3) });
            var inner3 = new Polygon(new List<Vector3> { new Vector3(6, 0, 6), new Vector3(6, 0, 3), new Vector3(3, 0, 3), new Vector3(3, 0, 6) });


            var polygons = new List<Polygon>();
            polygons.Add(outer);
            polygons.Add(inner1);
            polygons.Add(inner2);
            polygons.Add(inner3);
            var grid = new Grid2d(polygons);

            grid.SplitAtPoints(new List<Vector3> { new Vector3(4, 0, 0), new Vector3(6, 0, 0) });
            var cells = grid.GetCells();

            var cell = cells[0];
            var profiles = cell.GetTrimmedCellProfiles();
            Assert.Single(profiles);
            Assert.Single(profiles.First().Voids);

            cell = cells[1];
            profiles = cell.GetTrimmedCellProfiles();
            Assert.Equal(2, profiles.Count());
            Assert.Empty(profiles.First().Voids);
            Assert.Empty(profiles.Last().Voids);

            cell = cells[2];
            profiles = cell.GetTrimmedCellProfiles();
            Assert.Single(profiles);
            Assert.Single(profiles.First().Voids);
        }


        [Fact]
        public void ChildGridUpdatesParent()
        {
            var u = new Grid1d(10);
            var v = new Grid1d(5);
            var grid2d = new Grid2d(u, v);
            Assert.Single(grid2d.CellsFlat);
            grid2d.U.DivideByCount(10);
            grid2d.V.DivideByCount(5);
            Assert.Equal(50, grid2d.CellsFlat.Count);
        }

        [Fact]
        public void DisallowedGridEditingThrowsException()
        {
            var grid2d = new Grid2d(5, 5);
            grid2d.V.DivideByCount(5);
            grid2d.CellsFlat[2].U.DivideByCount(5);
            Assert.Throws<NotSupportedException>(() => grid2d.U.DivideByCount(2));
        }

        [Fact]
        public void Grid2dSerializes()
        {
            Name = "grid2d serializes";
            var polyline = new Polyline(new[] {
                new Vector3(0,0,0),
                new Vector3(10,2,0),
                new Vector3(30,4,0),
            });
            var uGrid = new Grid1d(polyline);
            var p2 = new Line(Vector3.Origin, new Vector3(0, 20, 0));
            var vGrid = new Grid1d(p2);
            var grid2d = new Grid2d(uGrid, vGrid);
            grid2d.U.DivideByCount(10);
            grid2d.V.DivideByCount(3);
            grid2d[2, 2].U.DivideByCount(4);
            var json = JsonSerializer.Serialize(grid2d);
            var deserialized = Element.Deserialize<Grid2d>(json);
            Assert.Equal(grid2d.GetCells().Count, deserialized.GetCells().Count);

            var grid2dElem = new Grid2dElement(grid2d, Guid.NewGuid(), "Grid");
            Model.AddElement(grid2dElem);
        }

        [Fact]
        public void CellSeparatorsTrimmed()
        {
            Name = "CellSeparatorsTrimmed";
            var polygon = new Polygon(new[] {
                Vector3.Origin,
                new Vector3(4,2),
                new Vector3(5,3),
                new Vector3(7,7),
                new Vector3(3,6)
            });
            var polygon2 = new Polygon(new[] {
                new Vector3(1.1,1),
                new Vector3(1.5,1),
                new Vector3(1.5,2),
                new Vector3(1.1,2)
            });
            var polygons = new[] { polygon, polygon2 };
            var grid2d = new Grid2d(polygons);
            grid2d.U.DivideByCount(10);
            grid2d.V.DivideByFixedLength(1);
            var csu = grid2d.GetCellSeparators(GridDirection.U, true);
            var csv = grid2d.GetCellSeparators(GridDirection.V, true);
            Assert.Equal(10, csu.Count);
            Assert.Equal(10, csv.Count);
            Model.AddElements(polygons.Select(p => new ModelCurve(p, BuiltInMaterials.XAxis)));
            Model.AddElements(csu.Union(csv).Select(l => new ModelCurve(l as Line)));
        }

        [Fact]
        public void RotationOfTransform()
        {
            var rectangle = Polygon.Rectangle(10, 6);
            var rotation = new Transform(Vector3.Origin, 30); //30 degree rotation
            var rotatedRectangle = (Polygon)rectangle.Transformed(rotation);
            var grid = new Grid2d(rotatedRectangle, rotation);
            grid.U.DivideByCount(20);
            grid.V.DivideByCount(12);
            Assert.Equal(0.5, grid[5, 5].U.Domain.Length, 3);
            Assert.Equal(0.5, grid[5, 5].V.Domain.Length, 3);
        }

        [Fact]
        public void NoExceptionsThrownWithAnyRotation()
        {
            for (int rotation = 0; rotation < 360; rotation += 10)
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
                var pattern = new[] { panelA, panelB, panelC };
                var pattern2 = new[] { panelB, panelA };
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
            }
            //Test verifies no exceptions are thrown at any rotation
        }

        [Fact]
        public void SeparatorsFromNestedGridFromPolygons()
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

            var orientation = new Transform();
            orientation.Rotate(Vector3.ZAxis, 15);

            var grid = new Grid2d(new[] { polygon, polygon2 }, orientation);
            grid.U.DivideByCount(3);
            grid.V.SplitAtParameter(0.5);
            grid[1, 0].V.DivideByCount(5);
            var cells = grid.GetCells();
            var geo = cells.Select(cl => cl.GetTrimmedCellGeometry());
            var types = cells.Select(cl => cl.Type);
            var trimmed = cells.Select(cl => cl.IsTrimmed());
            var uLines = grid.GetCellSeparators(GridDirection.U);
            var vLines = grid.GetCellSeparators(GridDirection.V);
            var dict = new Dictionary<string, object>
            {
                {"Cell Geometry", geo },
                {"U Lines", uLines },
                {"V Lines", vLines }
            };

        }

        [Fact]
        public void GridInheritsNamesFromBothDirections()
        {
            var grid = new Grid2d(20, 20);
            var uPattern = new[] { 1.0, 2.0, 3.0 };
            var vPattern = new[] { ("Large", 5.0), ("Small", 1.0) };
            grid.U.DivideByPattern(uPattern);
            grid.V.DivideByPattern(vPattern);
            Assert.Equal("B / Large", grid[1, 0].Type);
        }

        [Fact]
        public void InvalidSourceDomain()
        {
            var ex = Assert.ThrowsAny<Exception>(() => new Grid2d(0, 5));
        }

        [Fact]
        public void GetSeparators()
        {
            var grid = new Grid2d(20, 50);
            grid.U.DivideByCount(5);
            grid.V.DivideByFixedLength(8);
            var uLines = grid.GetCellSeparators(GridDirection.U);
            var vLines = grid.GetCellSeparators(GridDirection.V);
            var dict = new Dictionary<string, object>
            {
                { "U", uLines },
                { "V", vLines }
            };
        }

        [Fact]
        public void NonXYOrientedBoundary()
        {
            var polygon = new Polygon(new[]
            {
                new Vector3(-0.00143854692902412, 4.30437683003289, 18.5932250285742),
                new Vector3(4.74720486586016, -0.330526623597087, 18.5932250285742),
                new Vector3(5.51987513308118, 0.461104876693076, 15.6842815666606),
                new Vector3(3.74403947664555, 2.19440554840181, 15.6842815666606),
                new Vector3(4.16271103113095, 2.6233512512022, 14.1080699110863),
                new Vector3(1.28521062051391, 5.6226002093707, 13.7492575016872),
            });
            var grid = new Grid2d(polygon);
            grid.U.DivideByPattern(new double[] { 1, 2 });
            grid.V.DivideByCount(10);
            Assert.Equal(50, grid.GetCells().Count());
        }

        [Fact]
        public void SplitSubCellAtPoint()
        {
            Name = "Split Subcell at point";
            var u = new Grid1d(new Line(new Vector3(-2, 0), new Vector3(-4, 5)));
            var v = new Grid1d(new Line(new Vector3(0, -2), new Vector3(7, -2)));
            var grid = new Grid2d(u, v);
            grid.SplitAtPoint(new Vector3(2, 2));
            grid[1, 1].SplitAtPoint(new Vector3(3, 3));
            Model.AddElement(new Circle(new Vector3(2, 2), 0.1));
            Model.AddElement(new Circle(new Vector3(3, 3), 0.1));
            var subcell = grid[1, 1];
            Model.AddElements(grid.GetCells().Select(c => new ModelCurve(c.GetCellGeometry(), BuiltInMaterials.XAxis)));
            Assert.Equal(7, grid.GetCells().Count);

            var shape = new Polygon(new[] {
                new Vector3(10,10),
                new Vector3(20,10),
                new Vector3(24, 20),
                new Vector3(15, 16),
                new Vector3(10, 16)
            });
            var grid2 = new Grid2d(shape);
            grid2.SplitAtPoint(new Vector3(12, 13));
            Model.AddElements(new Circle(new Vector3(12, 13), 0.2));
            Model.AddElements(new Circle(new Vector3(17, 15), 0.2));
            grid2[1, 1].U.DivideByCount(4);
            grid2[1, 1].SplitAtPoint(new Vector3(17, 15));
            Model.AddElements(grid2.ToModelCurves(material: BuiltInMaterials.XAxis));
            Model.AddElement(shape);
            Assert.Equal(13, grid2.GetCells().Count);
        }

        [Fact]
        public void XYParallelNonOrthogonalBoundary()
        {
            var polygon = new Polygon(new[]
            {
                new Vector3(16.076011004152, -0.0286165078903409, 5.30811736183681),
                new Vector3(20.9095067384118, 5.97574627670645, 5.30811736183681),
                new Vector3(13.4808532467711, 11.9557921618838, 5.30811736183681),
                new Vector3(10.0455739172429, 3.54936529736341, 5.30811736183681),
            });
            var grid = new Grid2d(polygon);
            grid.U.DivideByPattern(new double[] { 1, 2 });
            grid.V.DivideByCount(10);
            Assert.Equal(80, grid.GetCells().Count());
        }

        [Fact]
        public void SkewedGridsFillBoundary()
        {
            var squareSize = 10;
            var rect = Polygon.Rectangle(squareSize, squareSize);

            // Using constructor with origin
            var origin = new Vector3();
            var uDirection = new Vector3(1, 0, 0);
            var vDirection = new Vector3(-1, 1, 0); // 45 degrees up to the left

            var grid = new Grid2d(rect, origin, uDirection, vDirection);

            // Making a grid from the origin doesn't subdivide it for you,
            // just makes sure the UV axes are big enough to fill the bounds
            Assert.Single(grid.GetCells());

            // Split grid at the origin
            grid.SplitAtPoint(origin);
            Assert.Equal(4, grid.GetCells().Count);
            Assert.Equal(squareSize * 2, grid.U.Curve.Length()); // Expanding to 45 degree square should give us 2x square size (half size extension each direction)
            Assert.Equal(Math.Sqrt(2 * Math.Pow(squareSize, 2)), grid.V.Curve.Length()); // Skewed side should be hypotenuse with other legs at square size.
        }

        [Fact]
        public void SkewedGridsWithNearlyParallelBoundary()
        {
            var transformStr = @"{
                ""Matrix"": {
                ""Components"": [
                    0.9999947633971941,
                    -0.009614940148020392,
                    0,
                    -10.992100150908389,
                    -0.003236229007759388,
                    -0.9999537753946179,
                    0,
                    17.971426034076142,
                    0,
                    0,
                    1.0000000000000004,
                    0
                ]
                }
            }";
            var transform = JsonSerializer.Deserialize<Transform>(transformStr);
            var origin = transform.Origin;
            var uDirection = transform.XAxis;
            var vDirection = transform.YAxis;
            var boundary = new Polygon(new List<Vector3>()
            {
                new Vector3(-12.0362, -34.1879, 0.0000),
                new Vector3(28.8939, -34.3204, 0.0000),
                new Vector3(29.0811, 23.5186, 0.0000),
                new Vector3(-11.8491, 23.6511, 0.0000)
            });
            var grid = new Grid2d(boundary, origin, uDirection, vDirection);
            Assert.True(uDirection.Unitized().Equals((grid.U.Curve.PointAt(1) - grid.U.Curve.PointAt(0)).Unitized()));
            Assert.True(vDirection.Unitized().Equals((grid.V.Curve.PointAt(1) - grid.V.Curve.PointAt(0)).Unitized()));
        }

        [Fact]
        public void CustomUVAndBounds()
        {
            var boundary = Polygon.Rectangle(new Vector3(), new Vector3(1, 1));

            var u = new Grid1d(new Line(new Vector3(5, 0), new Vector3(10, 0)));
            var v = new Grid1d(new Line(new Vector3(0, 5), new Vector3(0, 10)));

            var grid = new Grid2d(boundary, u, v);

            Assert.Equal(5, grid.U.Curve.Length());
            Assert.Equal(5, grid.V.Curve.Length());

            var count = grid.GetCells().Count;

            foreach (var cell in grid.GetCells())
            {
                // We gave it U and V coordinates that do not intersect with the bounds.
                // We expect empty trimmed cells back.
                var trimmed = cell.GetTrimmedCellGeometry();
                Assert.True(trimmed.Count() == 0);
            }
        }

        [Fact]
        public void RotateGridWithClosePointDoNotThrow()
        {
            var boundary = new Polygon
            (
                new[]
                {
                    new Vector3(),
                    new Vector3(3.998, 0.0),
                    new Vector3(4 , 2),
                    new Vector3(0.0, 1.998)
                }
            );

            Transform t = new Transform(new Vector3(2, 0));
            var grid = new Grid2d(boundary, t);
            grid.SplitAtPoint(new Vector3(3.998, 0.01));

            List<Curve[]> cellBoundaries = new List<Curve[]>();
            foreach (var cell in grid.GetCells())
            {
                cellBoundaries.Add(cell.GetTrimmedCellGeometry());
            }
            Assert.Equal(3, cellBoundaries.Where(cb => cb.Any()).Count());
        }

        [Fact]
        public void SeparatorsFromBadPolygon()
        {
            var json = File.ReadAllText("../../../models/Geometry/bad_grid.json");

            var grid = Element.Deserialize<Grid2d>(json);
            var cellSeparators = grid.GetCellSeparators(GridDirection.V, true);
        }

        [Fact]
        public void NoTrimmedCells()
        {
            var json = File.ReadAllText("../../../models/Geometry/badGridTrimmed.json");
            var grid = Element.Deserialize<Grid2d>(json);
            foreach (var c in grid.GetCells())
            {
                Assert.False(c.IsTrimmed());
            }
        }

        [Fact]
        public void GetCellsDoesntChangeCells()
        {
            var grid1 = new Grid2d(Polygon.Rectangle(4, 20));
            for (int i = 0; i < 10; i++)
            {
                grid1.V.SplitAtOffset(i);
            }

            var grid2 = new Grid2d(Polygon.Rectangle(4, 20));
            for (int i = 0; i < 10; i++)
            {
                grid2.V.SplitAtOffset(i);
                // it used to be that calling `GetCells` would cause the `Cells`
                // to be cached, and then it would fail to update after that.
                // This test ensures that doesn't happen.
                var cells = grid2.GetCells();
            }
            Assert.True(grid1.GetCells().Count() == grid2.GetCells().Count());
        }
    }
}