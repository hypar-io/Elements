using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Spatial;
using Elements.Spatial.CellComplex;
using Xunit;
using System.Linq;

namespace Elements.Tests
{
    public class CellComplexTests : ModelTest
    {
        private static Material DefaultPanelMaterial = new Material("Default", new Color(0.3, 0.3, 0.3, 0.5));
        private static Material ZMaterial = new Material("Z", new Color(0, 0, 1, 0.5));
        private static Material UMaterial = new Material("U", new Color(1, 0, 0, 0.5));
        private static Material VMaterial = new Material("V", new Color(0, 1, 0, 0.5));
        private static Material BaseMaterial = new Material("Base", new Color(0, 0, 0, 1));
        private static Material LineMaterial = new Material("Line", new Color(1, 0, 1, 1));

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

        [Fact]
        public void CellComplexExample()
        {
            this.Name = "Elements_CellComplex_Example";

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
            var traversedCells = curCell.TraversedNeighbors(end);

            foreach (var cell in traversedCells)
            {
                var rep = new Representation(new[] { cell.GetGeometry() });
                this.Model.AddElement(new GeometricElement(new Transform(), pathMaterial, rep, false, Guid.NewGuid(), "Path"));
            }
        }

        [Fact]
        public void CellComplexSerializesAndDeserializes()
        {
            this.Name = "Elements_CellComplex_Serialization";

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
                Assert.True(faceGeo1.Area() == faceGeo2.Area());
            }

            foreach (var vertex in vertices)
            {
                var vertexCopy = cellComplexDeserialized.GetVertex(vertex.Id);
                Assert.True(vertex.Name == vertexCopy.Name);
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
        public void CellComplexTraversal()
        {
            this.Name = "Elements_CellComplex_Traversal";

            var cellComplex = MakeASimpleCellComplex(numLevels: 10, uNumCells: 5, vNumCells: 10);

            var bigNumber = 100000;

            foreach (var edge in cellComplex.GetEdges())
            {
                this.Model.AddElement(new ModelCurve(edge.GetGeometry(), DefaultPanelMaterial));
            }

            var baseCell = cellComplex.GetCells().First();

            foreach (var face in baseCell.GetFaces())
            {
                this.Model.AddElement(new Panel(face.GetGeometry(), BaseMaterial));
            }

            // Traverse cells upward
            var curNeighbor = baseCell;
            var curTarget = baseCell.GetBottomFace().GetGeometry().Centroid() + Vector3.ZAxis * bigNumber;
            var traversedCells = baseCell.TraversedNeighbors(curTarget);

            foreach (var cell in traversedCells)
            {
                var rep = new Representation(new[] { cell.GetGeometry() });
                this.Model.AddElement(new GeometricElement(new Transform(), ZMaterial, rep, false, Guid.NewGuid(), "Test"));
            }

            Assert.True(traversedCells.Count == 10);

            // Traverse faces from top cell
            var baseFace = cellComplex.GetFace(traversedCells.Last().TopFaceId);
            this.Model.AddElement(new Panel(baseFace.GetGeometry(), BaseMaterial));

            var curFaceNeighbor = baseFace;
            var curFaceTarget = curFaceNeighbor.GetGeometry().Centroid() + curFaceNeighbor.GetOrientation().U.GetGeometry() * bigNumber;
            var curFaceTraversals = baseFace.TraversedNeighbors(curFaceTarget, true);

            foreach (var face in curFaceTraversals)
            {
                this.Model.AddElement(new Panel(face.GetGeometry(), UMaterial));
            }

            Assert.True(curFaceTraversals.Count == 5);

            curFaceNeighbor = baseFace;
            curFaceTarget = curFaceNeighbor.GetGeometry().Centroid() + curFaceNeighbor.GetOrientation().V.GetGeometry() * bigNumber;
            curFaceTraversals = curFaceNeighbor.TraversedNeighbors(curFaceTarget, true);

            foreach (var face in curFaceTraversals)
            {
                this.Model.AddElement(new Panel(face.GetGeometry(), VMaterial));
            }

            Assert.True(curFaceTraversals.Count == 10);

            var origin = new Vector3(50, 50);

            var curEdge = curFaceTraversals.Last().GetClosestEdge(origin);
            var traversedEdges = curEdge.TraversedNeighbors(origin);

            foreach (var edge in traversedEdges)
            {
                this.Model.AddElement(new ModelCurve(curEdge.GetGeometry(), LineMaterial));
            }
        }
    }
}