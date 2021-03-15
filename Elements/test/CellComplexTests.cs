using System.Collections.Generic;
using System;
using Elements.Geometry;
using Elements.Geometry.Profiles;
using Elements.Geometry.Solids;
using Elements.Spatial;
using Elements.Spatial.CellComplex;
using Newtonsoft.Json;
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
        public void CellComplexSerializesAndDeserializes()
        {
            this.Name = "Elements_CellComplex_Serialization";

            var cellComplex = MakeASimpleCellComplex();
            var vertices = cellComplex.GetVertices();
            var bounds = new BBox3(vertices.Select(v => v.Value).ToList());

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

            foreach (var segment in cellComplex.GetUniqueEdges())
            {
                var line1 = segment.GetGeometry();
                var line2 = cellComplexDeserialized.GetUniqueEdge(segment.Id).GetGeometry();
                Assert.True(line1.Start.Equals(line2.Start));
                Assert.True(line1.End.Equals(line2.End));
            }

            foreach (var face in cellComplex.GetFaces())
            {
                var faceGeo1 = face.GetGeometry();
                var faceGeo2 = cellComplexDeserialized.GetFace(face.Id).GetGeometry();
                Assert.True(faceGeo1.Area() == faceGeo2.Area());
                this.Model.AddElement(new Panel(faceGeo1, DefaultPanelMaterial));
                this.Model.AddElement(new Panel(faceGeo2, UMaterial, copyTransform));
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

            var cellComplex = MakeASimpleCellComplex(numLevels: 10, uNumCells: 5, vNumCells: 5);

            foreach (var segment in cellComplex.GetUniqueEdges())
            {
                this.Model.AddElement(new ModelCurve(segment.GetGeometry(), DefaultPanelMaterial));
            }

            var baseCell = cellComplex.GetCells().First();

            foreach (var face in baseCell.GetFaces())
            {
                this.Model.AddElement(new Panel(face.GetGeometry(), BaseMaterial));
            }

            // Traverse cells upward
            var curNeighbor = baseCell;
            var numNeighbors = 0;
            var lastNeighbor = curNeighbor;

            while (curNeighbor != null)
            {
                curNeighbor = curNeighbor.GetNeighbor(curNeighbor.GetTopFace());

                if (curNeighbor != null)
                {
                    foreach (var face in curNeighbor.GetFaces())
                    {
                        this.Model.AddElement(new Panel(face.GetGeometry(), ZMaterial));
                    }
                    lastNeighbor = curNeighbor;
                    numNeighbors += 1;
                }
            }

            Assert.True(numNeighbors == 9);

            // Traverse faces from top cell
            var baseFace = cellComplex.GetFace(lastNeighbor.TopFaceId);
            this.Model.AddElement(new Panel(baseFace.GetGeometry(), BaseMaterial));

            var curFaceNeighbor = baseFace;
            var lastFaceNeighbor = curFaceNeighbor;
            var numUNeighbors = 0;

            while (curFaceNeighbor != null && numUNeighbors < 30)
            {
                var pointFarU = curFaceNeighbor.GetGeometry().Centroid() + cellComplex.GetUV(curFaceNeighbor.UId).GetGeometry() * 10000;
                curFaceNeighbor = curFaceNeighbor.GetClosestAssociatedFace(pointFarU, true, false);

                if (curFaceNeighbor != null)
                {
                    this.Model.AddElement(new Panel(curFaceNeighbor.GetGeometry(), UMaterial));
                    lastFaceNeighbor = curFaceNeighbor;
                    numUNeighbors += 1;
                }
            }

            Assert.True(numUNeighbors == 4);

            var numVNeighbors = 0;
            curFaceNeighbor = lastFaceNeighbor;

            while (curFaceNeighbor != null)
            {
                var pointFarV = curFaceNeighbor.GetGeometry().Centroid() + cellComplex.GetUV(curFaceNeighbor.VId).GetGeometry() * 10000;
                curFaceNeighbor = curFaceNeighbor.GetClosestAssociatedFace(pointFarV, true, false);

                if (curFaceNeighbor != null)
                {
                    this.Model.AddElement(new Panel(curFaceNeighbor.GetGeometry(), VMaterial));
                    lastFaceNeighbor = curFaceNeighbor;
                    numVNeighbors += 1;
                }
            }

            Assert.True(numVNeighbors == 4);

        }
    }
}