using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Spatial;
using Elements.Spatial.CellComplex;
using Xunit;
using System.Linq;
using Xunit.Abstractions;

namespace Elements.Tests
{
    public class CellComplexTests : ModelTest
    {
        private static readonly Material DefaultPanelMaterial = new Material("Default", new Color(0.3, 0.3, 0.3, 0.5));
        private static readonly Material ZMaterial = new Material("Z", new Color(0, 0, 1, 0.5));
        private static readonly Material UMaterial = new Material("U", new Color(1, 0, 0, 0.5));
        private static readonly Material VMaterial = new Material("V", new Color(0, 1, 0, 0.5));
        private static readonly Material BaseMaterial = new Material("Base", new Color(0, 0, 0, 1));
        private static readonly Material LineMaterial = new Material("Line", new Color(1, 0, 1, 1));

        // Used to create target points when generally traversing in a direction, rather than toward a specific point
        private static readonly double BIG_NUMBER = 1000000;

        // Utility
        private static CellComplex MakeASimpleCellComplex(
            double uCellSize = 10,
            double vCellSize = 10,
            double uNumCells = 5,
            double vNumCells = 5,
            double cellHeight = 5,
            double numLevels = 3,
            Nullable<Vector3> origin = null,
            Nullable<Vector3> uDirection = null,
            Nullable<Vector3> vDirection = null,
            Polygon polygon = null
        )
        {
            var orig = origin == null ? new Vector3() : (Vector3)origin;
            var uDir = uDirection == null ? new Vector3(1, 0, 0) : ((Vector3)uDirection).Unitized();
            var vDir = vDirection == null ? new Vector3(0, 1, 0) : ((Vector3)vDirection).Unitized();

            var uLength = orig.X + uCellSize * uNumCells;
            var vLength = orig.Y + vCellSize * vNumCells;

            // Create Grid2d
            var boundary = polygon == null ? Polygon.Rectangle(orig, new Vector3(uLength, vLength)) : polygon;

            // Using constructor with origin
            var grid = new Grid2d(boundary, orig, uDir, vDir);
            for (var u = uCellSize; u < uLength; u += uCellSize)
            {
                grid.SplitAtPoint(orig + (uDir * u));
            }
            for (var v = vCellSize; v < vLength; v += vCellSize)
            {
                grid.SplitAtPoint(orig + (vDir * v));
            }

            var cellComplex = new CellComplex(Guid.NewGuid(), "Test");

            for (var i = 0; i < numLevels; i++)
            {
                foreach (var cell in grid.GetCells())
                {
                    foreach (var crv in cell.GetTrimmedCellGeometry())
                    {
                        cellComplex.AddCell((Polygon)crv, 5, cellHeight * i, grid.U, grid.V);
                    }
                }
            }
            return cellComplex;
        }

        private static void DrawCellComplexSkeleton(Model model, CellComplex cellComplex)
        {
            foreach (var edge in cellComplex.GetEdges())
            {
                model.AddElement(new ModelCurve(edge.GetGeometry(), DefaultPanelMaterial));
            }
        }

        private readonly ITestOutputHelper _output;

        public CellComplexTests(ITestOutputHelper output)
        {
            this._output = output;
            this.GenerateIfc = false;
        }

        [Fact, Trait("Category", "Examples")]
        public void CellComplexExample()
        {
            this.Name = "Elements_Spatial_CellComplex_CellComplex";

            // <example>

            // Assemble CellComplex from Grid2d
            var numLevels = 10;
            var levelHeight = 1;
            var cellSize = 2;
            var complex = new CellComplex();
            var boundary = new Circle(new Vector3(), 10).ToPolygon();
            var grid = new Grid2d(boundary, Vector3.Origin, Vector3.XAxis, Vector3.YAxis);
            var pathMaterial = new Material("Path", new Color(1, 0, 0, 0.75));

            grid.U.DivideByFixedLength(cellSize);
            grid.V.DivideByFixedLength(cellSize);


            for (var i = 0; i < numLevels; i++)
            {
                foreach (var cell in grid.GetCells())
                {
                    foreach (var crv in cell.GetTrimmedCellGeometry())
                    {
                        complex.AddCell((Polygon)crv, levelHeight, i * levelHeight, grid.U, grid.V);
                    }
                }
            }

            // Draw base CellComplex
            foreach (var face in complex.GetFaces())
            {
                this.Model.AddElement(new Panel(face.GetGeometry(), BuiltInMaterials.Mass));
            }

            // Traverse CellComplex
            var start = new Vector3(15, 15, 15);
            var end = new Vector3(-15, -15, -15);

            // Draw lines from start and end to closest points, for reference
            foreach (var pt in new List<Vector3>() { start, end })
            {
                var closest = complex.GetClosestVertex(pt).GetGeometry();
                this.Model.AddElement(new ModelCurve(new Line(pt, closest), pathMaterial));
            }

            var curCell = complex.GetClosestCell(start);
            var traversedCells = curCell.TraverseNeighbors(end);

            foreach (var cell in traversedCells)
            {
                var rep = new Representation(new[] { cell.GetGeometry() });
                this.Model.AddElement(new GeometricElement(new Transform(), pathMaterial, rep, false, Guid.NewGuid(), "Path"));
            }
            // </example>
        }

        [Fact]
        public void CellComplexSerializesAndDeserializes()
        {
            this.Name = "Elements_Spatial_CellComplex_Serialization";

            var uDirection = new Vector3(1, 1, 0).Unitized();
            var cellComplex = MakeASimpleCellComplex(uDirection: uDirection);
            var vertices = cellComplex.GetVertices();
            var bounds = new BBox3(vertices.Select(v => v.Value).ToList());
            cellComplex.Tolerance = 0.005;

            var i = 0;
            foreach (var vertex in vertices)
            {
                vertex.Name = $"Vertex-{i}";
                i++;
            }

            var model = new Model();
            model.AddElement(cellComplex);
            var json = model.ToJson();
            var modelFromDeserialization = Model.FromJson(json);
            var cellComplexDeserialized = modelFromDeserialization.GetElementOfType<CellComplex>(cellComplex.Id);

            var copyTransform = new Transform(new Vector3((bounds.Max.X - bounds.Min.X) * 1.5, 0));

            foreach (var cell in cellComplex.GetCells())
            {
                var dir = cell.GetBottomFace().GetOrientation().U.Value;
                Assert.True(dir.Equals(uDirection));
            }

            foreach (var edge in cellComplex.GetEdges())
            {
                var line1 = edge.GetGeometry();
                var line2 = cellComplexDeserialized.GetEdge(edge.Id).GetGeometry();
                Assert.True(line1.Start.Equals(line2.Start));
                Assert.True(line1.End.Equals(line2.End));
            }

            foreach (var face in cellComplex.GetFaces())
            {
                var faceGeo1 = face.GetGeometry();
                var faceGeo2 = cellComplexDeserialized.GetFace(face.Id).GetGeometry();
                this.Model.AddElement(new Panel(faceGeo1, DefaultPanelMaterial));
                this.Model.AddElement(new Panel(faceGeo2, UMaterial, copyTransform));
                Assert.Equal(Math.Abs(faceGeo1.Area()), Math.Abs(faceGeo2.Area()), 5);
            }

            foreach (var vertex in vertices)
            {
                var vertexCopy = cellComplexDeserialized.GetVertex(vertex.Id);
                Assert.Equal(vertex.Name, vertexCopy.Name);
            }
        }

        [Fact]
        public void CellComplexVertexLookup()
        {
            var cellComplex = MakeASimpleCellComplex(origin: new Vector3());
            var almostAtOrigin = new Vector3(0, Vector3.EPSILON / 2, 0);
            Assert.False(cellComplex.VertexExists(almostAtOrigin, out var nullVertex));
            Assert.True(cellComplex.VertexExists(almostAtOrigin, out var originVertex, Vector3.EPSILON));
            Assert.True(cellComplex.VertexExists(new Vector3(), out var originVertexAgain));
        }

        [Fact]
        public void CellComplexTraverseCells()
        {
            this.Name = "Elements_Spatial_CellComplex_TraverseCells";

            var cellComplex = MakeASimpleCellComplex(numLevels: 10, uNumCells: 5, vNumCells: 10);

            DrawCellComplexSkeleton(this.Model, cellComplex);

            var baseCell = cellComplex.GetCells().First();

            foreach (var face in baseCell.GetFaces())
            {
                this.Model.AddElement(new Panel(face.GetGeometry(), BaseMaterial));
            }

            // Traverse cells upward
            var curNeighbor = baseCell;
            var curTarget = baseCell.GetBottomFace().GetGeometry().Centroid() + Vector3.ZAxis * BIG_NUMBER;
            var traversedCells = baseCell.TraverseNeighbors(curTarget);

            foreach (var cell in traversedCells)
            {
                var rep = new Representation(new[] { cell.GetGeometry() });
                this.Model.AddElement(new GeometricElement(new Transform(), ZMaterial, rep, false, Guid.NewGuid(), "Test"));
            }

            Assert.True(traversedCells.Count == 10);
        }


        [Fact]
        public void CellComplexTraverseFaces()
        {
            this.Name = "Elements_Spatial_CellComplex_TraverseFaces";

            var cellComplex = MakeASimpleCellComplex(numLevels: 10, uNumCells: 5, vNumCells: 10);

            DrawCellComplexSkeleton(this.Model, cellComplex);

            // Traverse faces from top corner
            var baseFace = cellComplex.GetClosestFace(new Vector3(-BIG_NUMBER, -BIG_NUMBER, BIG_NUMBER));
            this.Model.AddElement(new Panel(baseFace.GetGeometry(), BaseMaterial));

            var curFaceNeighbor = baseFace;
            var curFaceTarget = curFaceNeighbor.GetGeometry().Centroid() + curFaceNeighbor.GetOrientation().U.GetGeometry() * BIG_NUMBER;
            var curFaceTraversals = baseFace.TraverseNeighbors(curFaceTarget, true);

            Assert.Equal(5, curFaceTraversals.Count);

            foreach (var face in curFaceTraversals)
            {
                this.Model.AddElement(new Panel(face.GetGeometry(), UMaterial));
            }

            curFaceNeighbor = baseFace;
            curFaceTarget = curFaceNeighbor.GetGeometry().Centroid() + curFaceNeighbor.GetOrientation().V.GetGeometry() * BIG_NUMBER;
            curFaceTraversals = curFaceNeighbor.TraverseNeighbors(curFaceTarget, true);

            Assert.Equal(10, curFaceTraversals.Count);

            foreach (var face in curFaceTraversals)
            {
                this.Model.AddElement(new Panel(face.GetGeometry(), VMaterial));
            }
        }

        [Fact]
        public void CellComplexTraverseEdges()
        {
            this.Name = "Elements_Spatial_CellComplex_TraverseEdges";

            var cellComplex = MakeASimpleCellComplex(numLevels: 10, uNumCells: 5, vNumCells: 10);

            DrawCellComplexSkeleton(this.Model, cellComplex);

            var origin = new Vector3(-BIG_NUMBER, -BIG_NUMBER, -BIG_NUMBER);
            var target = new Vector3(BIG_NUMBER, BIG_NUMBER, BIG_NUMBER);

            var curEdge = cellComplex.GetClosestEdge(origin);

            var traversedEdges = curEdge.TraverseNeighbors(target);

            foreach (var edge in traversedEdges)
            {
                this.Model.AddElement(new ModelCurve(edge.GetGeometry(), LineMaterial));
            }
        }

        [Fact]
        public void SplitEdgesMakesCorrectEdgesAndVertices()
        {
            this.Name = nameof(SplitEdgesMakesCorrectEdgesAndVertices);
            var cp = new CellComplex(Guid.NewGuid(), "SplitComplex");
            var rect = Polygon.Rectangle(10, 10);
            cp.AddCell(rect, 10, 0.0);

            Assert.Equal(8, cp.GetVertices().Count);
            Assert.Equal(12, cp.GetEdges().Count);
            Assert.Equal(6, cp.GetFaces().Count);

            foreach (var e in cp.GetEdges())
            {
                var edge = cp.GetEdge(e.Id);
                var a = cp.GetVertex(edge.StartVertexId).Value;
                var b = cp.GetVertex(edge.EndVertexId).Value;

                if (!cp.TrySplitEdge(e, a.Average(b), out Elements.Spatial.CellComplex.Vertex result))
                {
                    throw new Exception("Could not split.");
                }

                Assert.Equal(2, e.GetFaces().Count);
            }

            Assert.Equal(24, cp.GetEdges().Count);

            this.Model.AddElements(cp.ToModelElements(true));
        }

        [Fact]
        public void SplitFaceMakesCorrectFacesVerticesAndEdges()
        {
            this.Name = nameof(SplitFaceMakesCorrectFacesVerticesAndEdges);
            var cp = new CellComplex(Guid.NewGuid(), "SplitComplex");
            var rect = Polygon.Rectangle(10, 10);
            var rect1 = Polygon.Rectangle(new Vector3(5, -5), new Vector3(10, 5));
            var rect2 = Polygon.Rectangle(new Vector3(-5, -15), new Vector3(5, -5));
            var rect3 = Polygon.Rectangle(new Vector3(5, -15), new Vector3(10, -5));
            var rect4 = Polygon.Rectangle(new Vector3(10, -15), new Vector3(20, -5));
            var face = cp.AddFace(rect);
            var face1 = cp.AddFace(rect1);
            var face2 = cp.AddFace(rect2);
            var face3 = cp.AddFace(rect3);
            var face4 = cp.AddFace(rect4);

            Assert.Equal(11, cp.GetVertices().Count);
            Assert.Equal(15, cp.GetEdges().Count);
            Assert.Equal(5, cp.GetFaces().Count);

            // var ngon = Polygon.Ngon(5, 4).TransformedPolygon(new Transform((3, -3)));
            var ngon = Polygon.Ngon(5, 4).TransformedPolyline(new Transform((3, -3)));
            ngon.Vertices.Add(ngon.Vertices.First());

            foreach (var f in cp.GetFaces())
            {
                cp.TrySplitFace(f, ngon, out var newFaces, out var newExternalVertices, out var newInternalVertices);
            }

            Assert.False(cp.HasDuplicateEdges());

            Assert.Equal(20, cp.GetVertices().Count);
            Assert.Equal(9, cp.GetFaces().Count);
            Assert.Equal(28, cp.GetEdges().Count);

            this.Model.AddElement(new ModelCurve(new Polyline(ngon.Vertices)));
            this.Model.AddElements(cp.ToModelElements(true));
        }

        [Fact]
        public void ResplittingFaceResultsInVertices()
        {
            this.Name = nameof(ResplittingFaceResultsInVertices);
            var cp = new CellComplex(Guid.NewGuid(), "SplitComplex");
            var rect = Polygon.Rectangle(10, 10);
            var f = cp.AddFace(rect);

            var ngon = Polygon.Ngon(5, 4).TransformedPolygon(new Transform((3, -3)));
            cp.TrySplitFace(f, ngon, out var newFaces, out _, out _);

            Assert.Equal(2, newFaces.Count);

            foreach (var face in newFaces)
            {
                cp.TrySplitFace(face, ngon, out var moreNewFaces, out var newExternalVertices, out var newInternalVertices);
                Assert.NotEmpty(newExternalVertices);
                Assert.NotEmpty(newInternalVertices);
            }

            Assert.False(cp.HasDuplicateEdges());

            this.Model.AddElement(new ModelCurve(new Polyline(ngon.Vertices)));
            this.Model.AddElements(cp.ToModelElements(true));
        }

        [Fact]
        public void SplittingAlmostOnEdgeResultsInThreeCells()
        {
            this.Name = nameof(SplittingAlmostOnEdgeResultsInThreeCells);
            var cp = new CellComplex(Guid.NewGuid(), "SplitComplex");
            var rect = Polygon.Rectangle(10, 10);
            var f = cp.AddFace(rect);

            var pline = new Polyline(new[]{
                new Vector3(-6, 4),
                new Vector3(0,-5.00001),
                new Vector3(6,4)
            });

            // This test is only to ensure that we don't throw.
            // It creates a "bad" face that can't be visualized.
            cp.TrySplitFace(f, pline, out var moreNewFaces, out var newExternalVertices, out var newInternalVertices);
            Assert.Equal(3, cp.GetFaces().Count);
            this.Model.AddElements(cp.ToModelElements(true));
        }

        [Fact]
        public void TrimmingPolyAlignedAlongFaceEdgeSplitsCorrectly()
        {
            this.Name = nameof(TrimmingPolyAlignedAlongFaceEdgeSplitsCorrectly);
            var cp = new CellComplex(Guid.NewGuid(), "SplitComplex");
            var rect = Polygon.Rectangle(10, 10);
            var f = cp.AddFace(rect);

            var split = Polygon.Rectangle(5, 5).TransformedPolygon(new Transform(new Vector3(-2.5, 0)));
            cp.TrySplitFace(f, split, out var moreNewFaces, out var newExternalVertices, out var newInternalVertices);
            Assert.Equal(2, cp.GetFaces().Count);
            Assert.Equal(9, cp.GetEdges().Count);
            Assert.Equal(8, cp.GetVertices().Count);
            this.Model.AddElements(cp.ToModelElements(true));
        }

        [Fact]
        public void TrimmingPolyAlignedToCornerSplitsCorrectly()
        {
            this.Name = nameof(TrimmingPolyAlignedToCornerSplitsCorrectly);
            var cp = new CellComplex(Guid.NewGuid(), "SplitComplex");
            var rect = Polygon.Rectangle(10, 10);
            var f = cp.AddFace(rect);

            var split = Polygon.Rectangle(5, 5).TransformedPolygon(new Transform(new Vector3(-2.5, -2.5)));
            cp.TrySplitFace(f, split, out var moreNewFaces, out var newExternalVertices, out var newInternalVertices);
            Assert.Equal(2, cp.GetFaces().Count);
            Assert.Equal(8, cp.GetEdges().Count);
            Assert.Equal(7, cp.GetVertices().Count);
            this.Model.AddElements(cp.ToModelElements(true));
        }

        [Fact]
        public void TrimmingPolyCrossingTrimsCorrectly()
        {
            this.Name = nameof(TrimmingPolyCrossingTrimsCorrectly);
            var cp = new CellComplex(Guid.NewGuid(), "SplitComplex");
            var rect = Polygon.Rectangle(10, 10);
            var f = cp.AddFace(rect);

            var split = Polygon.Rectangle(5, 20);
            cp.TrySplitFace(f, split, out var moreNewFaces, out var newExternalVertices, out var newInternalVertices);
            // TODO: Update this test to get 3 faces when we build 
            // the half edge graph manually.
            Assert.Equal(2, cp.GetFaces().Count);
            this.Model.AddElement(new ModelCurve(split));
            this.Model.AddElements(cp.ToModelElements(true));
        }

        [Fact]
        public void SplitCellMakesCorrectNumberOfCells()
        {
            this.Name = nameof(SplitCellMakesCorrectNumberOfCells);
            var cp = new CellComplex(Guid.NewGuid(), "SplitComplex");
            var rect = Polygon.Rectangle(10, 10);
            var cell = cp.AddCell(rect, 10, 0.0);
            Assert.Equal(1, cp.GetCells().Count);

            var ngon = Polygon.Ngon(5, 4).TransformedPolygon(new Transform((3, -3)));
            cp.TrySplitCell(cell, ngon, out var newCells);

            Assert.False(cp.HasDuplicateEdges());

            Assert.Equal(2, cp.GetCells().Count);

            this.Model.AddElements(cp.ToModelElements(true));
        }

        [Fact]
        public void SplitCellWorksAcrossALargerCellComplex()
        {
            this.Name = nameof(SplitCellWorksAcrossALargerCellComplex);

            var cp = MakeASimpleCellComplex(numLevels: 3, uNumCells: 2, vNumCells: 2);

            var ngon = Polygon.Ngon(8, 15).TransformedPolygon(new Transform(new Vector3(16, 16)));
            foreach (var c in cp.GetCells())
            {
                // _output.WriteLine($"Attempting split of cell {c.Id}");

                if (cp.TrySplitCell(c, ngon, out List<Cell> newCells))
                {
                    // _output.WriteLine($"\tNew Cells:");
                    // foreach (var newCell in newCells)
                    // {
                    //     _output.WriteLine($"\t\t{newCell.Id}");
                    // }
                }
            }
            this.Model.AddElement(new ModelCurve(new Polyline(ngon.Vertices)));

            this.Model.AddElements(cp.ToModelElements(true));
        }
    }
}