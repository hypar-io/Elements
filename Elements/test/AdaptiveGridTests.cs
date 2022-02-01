using Elements.Geometry;
using Elements.Spatial.AdaptiveGrid;
using Elements.Tests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Vertex = Elements.Spatial.AdaptiveGrid.Vertex;

namespace Elements.Tests
{
    public class AdaptiveGridTests : ModelTest
    {      
        [Fact]
        public void AdaptiveGridPolygonKeyPointsExample()
        {
            this.Name = "Elements_Spatial_AdaptiveGrid_AdaptiveGridPolygonKeyPoints";
            var random = new Random();

            var adaptiveGrid = new AdaptiveGrid(new Transform());
            var points = new List<Vector3>()
            {
                new Vector3(-6, -4),
                new Vector3(-2, -4),
                new Vector3(3, -4),
                new Vector3(1, 4.5),
                new Vector3(6, 3),
            };
            adaptiveGrid.AddFromPolygon(Polygon.Rectangle(15, 10).TransformedPolygon(
                new Transform(new Vector3(), new Vector3(10, 0, 10))), points);

            foreach (var edge in adaptiveGrid.GetEdges())
            {
                Model.AddElement(new ModelCurve(edge.GetGeometry(), material: random.NextMaterial()));
            }
        }

        [Fact]
        public void AdaptiveGridBboxKeyPointsExample()
        {
            this.Name = "Elements_Spatial_AdaptiveGrid_AdaptiveGridBboxKeyPoints";
            var random = new Random();

            var adaptiveGrid = new AdaptiveGrid(new Transform());
            var points = new List<Vector3>()
            {
                new Vector3(-6, -4),
                new Vector3(-2, -4),
                new Vector3(3, -4),
                new Vector3(1, 4.5, 3),
                new Vector3(6, 3, -2),
            };
            adaptiveGrid.AddFromBbox(new BBox3(new Vector3(-7.5, -5, -3), new Vector3(10, 10, 3)), points);

            points = new List<Vector3>()
            {
                new Vector3(-6, -4, 3),
                new Vector3(-2, 0, 3),
                new Vector3(0, 4, 3),
                new Vector3(2, 6, 3)
            };
            var rectangle = Polygon.Rectangle(new Vector3(-10, -5), new Vector3(15, 10));
            adaptiveGrid.AddFromPolygon(rectangle.TransformedPolygon(new Transform(new Vector3(0, 0, 3))), points);
            points = new List<Vector3>()
            {
                new Vector3(-6, -4, 2),
                new Vector3(-2, 0, 2),
                new Vector3(0, 4, 2),
                new Vector3(2, 6, 2)
            };
            adaptiveGrid.AddFromPolygon(rectangle.TransformedPolygon(new Transform(new Vector3(0, 0, 2))), points);

            foreach (var edge in adaptiveGrid.GetEdges())
            {
                Model.AddElement(new ModelCurve(edge.GetGeometry(), material: random.NextMaterial()));
            }
        }

        [Fact]
        public void AdaptiveGridAddVertex()
        {
            var adaptiveGrid = new AdaptiveGrid(new Transform());
            var points = new List<Vector3>()
            {
                new Vector3(-6, -4),
                new Vector3(-2, -4),
                new Vector3(3, -4),
                new Vector3(1, 4.5, 3),
                new Vector3(6, 3, -2),
            };
            adaptiveGrid.AddFromPolygon(Polygon.Rectangle(15, 10), points);

            ulong id;
            Assert.True(adaptiveGrid.TryGetVertexIndex(new Vector3(-2, -4), out id));
            var oldV = adaptiveGrid.GetVertex(id);
            var edgesBefore = oldV.Edges.Count;

            var newV = adaptiveGrid.AddVertex(new Vector3(-2, -4, 2), new List<Vertex> { oldV });
            Assert.NotNull(newV);
            Assert.False(newV.Id == 0);
            Assert.Single(newV.Edges);
            Assert.True(newV.Edges.First().StartId == id || newV.Edges.First().EndId == id);
            Assert.Equal(edgesBefore + 1, oldV.Edges.Count());
            Assert.Contains(oldV.Edges, e => e.StartId == newV.Id || e.EndId == newV.Id);
        }

        [Fact]
        public void AdaptiveGridSubtractBoxKeepEdges()
        {
            var adaptiveGrid = new AdaptiveGrid(new Transform());
            var polygon = Polygon.Rectangle(new Vector3(0, 0), new Vector3(10, 10));

            var points = new List<Vector3>();
            for (int i = 1; i < 10; i++)
            {
                points.Add(new Vector3(i, i, 1));
            }
            adaptiveGrid.AddFromExtrude(polygon, Vector3.ZAxis, 2, points);
            Assert.True(adaptiveGrid.TryGetVertexIndex(new Vector3(5, 5, 1), out _));
            Assert.False(adaptiveGrid.TryGetVertexIndex(new Vector3(5, 4.9, 1), out _));
            var numVertices = adaptiveGrid.GetVertices().Count;

            var box = new BBox3(new Vector3(4.9, 4.9, 0), new Vector3(5.1, 5.1, 2));
            adaptiveGrid.SubtractBox(box, false);
            Assert.False(adaptiveGrid.TryGetVertexIndex(new Vector3(5, 5, 1), out _));
            Assert.True(adaptiveGrid.TryGetVertexIndex(new Vector3(5, 4.9, 1), out var cutEdgeId));

            var v = adaptiveGrid.GetVertex(cutEdgeId);
            Assert.Single(v.Edges);
            adaptiveGrid.TryGetVertexIndex(new Vector3(5, 4, 1), out var borderId);
            Assert.True(v.Edges.First().StartId == borderId || v.Edges.First().EndId == borderId);
            //On each elevation one vertex is removed but 4 added
            Assert.Equal(numVertices + (3 * (4 - 1)), adaptiveGrid.GetVertices().Count);
        }

        [Fact]
        public void AdaptiveGridSubtractBoxCutEdges()
        {
            var adaptiveGrid = new AdaptiveGrid(new Transform());
            var polygon = Polygon.Rectangle(new Vector3(0, 0), new Vector3(10, 10));

            var points = new List<Vector3>();
            for (int i = 1; i < 10; i++)
            {
                points.Add(new Vector3(i, i, 1));
            }

            adaptiveGrid.AddFromExtrude(polygon, Vector3.ZAxis, 2, points);
            Assert.True(adaptiveGrid.TryGetVertexIndex(new Vector3(5, 5, 1), out _));
            Assert.False(adaptiveGrid.TryGetVertexIndex(new Vector3(5, 4.9, 1), out _));

            adaptiveGrid.TryGetVertexIndex(new Vector3(5, 4, 1), out var borderId);
            var borderV = adaptiveGrid.GetVertex(borderId);
            var numEdges = borderV.Edges.Count;
            var numVertices = adaptiveGrid.GetVertices().Count;

            var box = new BBox3(new Vector3(4.9, 4.9, 0), new Vector3(5.1, 5.1, 2));
            adaptiveGrid.SubtractBox(box, true);
            Assert.False(adaptiveGrid.TryGetVertexIndex(new Vector3(5, 5, 1), out _));
            Assert.False(adaptiveGrid.TryGetVertexIndex(new Vector3(5, 4.9, 1), out _));

            Assert.Equal(numEdges - 1, borderV.Edges.Count);
            //On each elevation one vertex is removed and no added
            Assert.Equal(numVertices - (3 * 1), adaptiveGrid.GetVertices().Count);
        }

        [Fact]
        public void AdaptiveGridSubtractBoxSmallDifference()
        {
            var adaptiveGrid = new AdaptiveGrid(new Transform());
            var polygon = Polygon.Rectangle(new Vector3(-41, -51), new Vector3(-39, -49));

            var points = new List<Vector3>();
            points.Add(new Vector3(-40, -49.9, 1));
            points.Add(new Vector3(-40, -49.80979, 1));

            adaptiveGrid.AddFromExtrude(polygon, Vector3.ZAxis, 2, points);
            Assert.True(adaptiveGrid.TryGetVertexIndex(new Vector3(-40, -49.9, 1), out _));
            Assert.True(adaptiveGrid.TryGetVertexIndex(new Vector3(-40, -49.9, 2), out _));

            var box = new BBox3(new Vector3(-40.2, -50.190211303259034, 0),
                                new Vector3(-39.8, -49.809788696740966, 2));
            adaptiveGrid.SubtractBox(box);
            Assert.False(adaptiveGrid.TryGetVertexIndex(new Vector3(-40, -49.9, 1), out _));
            Assert.False(adaptiveGrid.TryGetVertexIndex(new Vector3(-40, -49.9, 2), out _));
        }

        [Fact]
        public void AdaptiveGridLongSectionDoNowThrow()
        {
            var adaptiveGrid = new AdaptiveGrid(new Transform());
            var polygon = Polygon.Rectangle(new Vector3(0, 0), new Vector3(200000, 10));

            var points = new List<Vector3>();
            points.Add(new Vector3(1, 5));
            points.Add(new Vector3(1999, 5));
 
            adaptiveGrid.AddFromExtrude(polygon, Vector3.ZAxis, 2, points);
        }

        [Fact]
        public void AdaptiveGridTwoAlignedSections()
        {
            var adaptiveGrid = new AdaptiveGrid(new Transform());
            var polygon1 = Polygon.Rectangle(new Vector3(0, 0), new Vector3(10, 10));
            var polygon2 = Polygon.Rectangle(new Vector3(10, 2), new Vector3(20, 12));

            var points = new List<Vector3>();
            points.AddRange(polygon1.Vertices);
            points.AddRange(polygon2.Vertices);

            adaptiveGrid.AddFromExtrude(polygon1, Vector3.ZAxis, 2, points);
            adaptiveGrid.AddFromExtrude(polygon2, Vector3.ZAxis, 2, points);

            ulong id;
            Assert.True(adaptiveGrid.TryGetVertexIndex(new Vector3(10, 2), out id));
            var vertex = adaptiveGrid.GetVertex(id);
            //Up, North, South, East, West
            Assert.Equal(5, vertex.Edges.Count);
            Assert.True(adaptiveGrid.TryGetVertexIndex(new Vector3(10, 10), out id));
            vertex = adaptiveGrid.GetVertex(id);
            Assert.Equal(5, vertex.Edges.Count);
        }
    }
}
