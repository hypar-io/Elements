using System;
using Elements.Geometry;
using Elements.Spatial;
using Elements.Spatial.CellComplex;
using Xunit;
using System.Linq;
using System.Collections.Generic;

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
        public void CellComplexSerializesAndDeserializes()
        {
            this.Name = "Elements_CellComplex_Serialization";

            var uDirection = new Vector3(1, 1, 0).Unitized();
            var cellComplex = MakeASimpleCellComplex(uDirection: uDirection);
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
            var numNeighbors = 0;
            var lastNeighbor = curNeighbor;

            while (curNeighbor != null)
            {
                curNeighbor = curNeighbor.GetNeighbors(curNeighbor.GetTopFace());

                if (curNeighbor != null)
                {
                    var rep = new Representation(new[] { curNeighbor.GetGeometry() });
                    this.Model.AddElement(new GeometricElement(new Transform(), ZMaterial, rep, false, Guid.NewGuid(), "Test"));
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
                var pointFarU = curFaceNeighbor.GetGeometry().Centroid() + curFaceNeighbor.GetOrientation().U.GetGeometry() * 10000;
                curFaceNeighbor = curFaceNeighbor.GetClosestNeighbor(pointFarU, true, false);

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
                var pointFarV = curFaceNeighbor.GetGeometry().Centroid() + curFaceNeighbor.GetOrientation().V.GetGeometry() * 10000;
                curFaceNeighbor = curFaceNeighbor.GetClosestNeighbor(pointFarV, true, false);

                if (curFaceNeighbor != null)
                {
                    this.Model.AddElement(new Panel(curFaceNeighbor.GetGeometry(), VMaterial));
                    lastFaceNeighbor = curFaceNeighbor;
                    numVNeighbors += 1;
                }
            }

            Assert.True(numVNeighbors == 9);

            var origin = new Vector3(50, 50);

            var curEdge = lastFaceNeighbor.GetClosestEdge(origin);

            var count = 0;

            while (count < 1000 && curEdge != null)
            {
                this.Model.AddElement(new ModelCurve(curEdge.GetGeometry(), LineMaterial));
                curEdge = curEdge.GetClosestNeighbor(origin);
                count += 1;
            }

            Assert.False(count != 1000); // If it got this big, we are likely in an infinite traversal

        }

        [Fact]
        public void StarShapedCellComplex()
        {
            this.Name = "StarShapedCellComplex";
            var cc = new CellComplex(Guid.NewGuid(), "Hooray");
            var boundary = Polygon.Star(20, 10, 5);
            var grid = new Grid2d(boundary, Vector3.Origin, Vector3.XAxis, Vector3.YAxis);
            grid.U.DivideByFixedLength(5);
            grid.V.DivideByFixedLength(5);
            for (var i = 0; i < 5; i++)
            {
                foreach (var cell in grid.GetCells())
                {
                    foreach (var crv in cell.GetTrimmedCellGeometry())
                    {
                        cc.AddCell((Polygon)crv, 2, i, grid.U, grid.V);
                    }
                }
            }

            var facesToDraw = cc.GetFaces();

            var startPt = new Vector3(-20, -20, 0);
            var endPt = new Vector3(20, 20, 10);
            this.Model.AddElement(new Mass(Polygon.Rectangle(0.5, 0.5), 0.5, BuiltInMaterials.XAxis, transform: new Transform(startPt)));
            this.Model.AddElement(new Mass(Polygon.Rectangle(0.5, 0.5), 0.5, BuiltInMaterials.XAxis, transform: new Transform(endPt)));

            // A list of vertices to record the traversal path.
            var path = new List<Vector3>();
            path.Add(startPt);
            var start = cc.GetClosestVertex(startPt);

            // A list to contains previously visited sites
            // so that we don't backtrack.
            var visited = new List<Elements.Spatial.CellComplex.Vertex>();

            var stop = false;
            var count = 0;
            while (!stop && count < 500)
            {
                visited.Add(start);
                path.Add(start.Value);
                var neighbors = start.GetEdges();

                var minDist = double.MaxValue;
                Elements.Spatial.CellComplex.Vertex newStart = null;
                Elements.Spatial.CellComplex.Edge chosenEdge = null;
                foreach (var e in neighbors)
                {
                    var a = cc.GetVertex(e.StartVertexId);
                    var b = cc.GetVertex(e.EndVertexId);
                    var sample = start == a ? b : a;

                    var d = endPt.DistanceTo(sample.Value);
                    if (d < minDist && !visited.Contains(sample))
                    {
                        minDist = d;
                        newStart = sample;
                        chosenEdge = e;
                    }
                }

                foreach (var f in chosenEdge.GetFaces())
                {
                    facesToDraw.Remove(f);
                }

                // If the last connected segment points sufficiently away
                // from the target, then we assume there is no better
                // forward solution and we exit.
                var lastSegmentDirection = (newStart.Value - path[path.Count - 1]).Unitized();
                var targetSegmentDirection = (endPt - newStart.Value).Unitized();
                if (lastSegmentDirection.Dot(targetSegmentDirection) < -0.5)
                {
                    stop = true;
                    path.Add(endPt);
                }

                start = newStart;
                count++;
            }


            foreach (var face in facesToDraw)
            {
                var faceGeo1 = face.GetGeometry();
                this.Model.AddElement(new Panel(faceGeo1, BuiltInMaterials.Mass));
            }

            var pline = new Polyline(path);
            foreach (var s in pline.Segments())
            {
                this.Model.AddElement(new Beam(s, Polygon.Rectangle(0.2, 0.2), BuiltInMaterials.XAxis));
            }
        }
    }
}