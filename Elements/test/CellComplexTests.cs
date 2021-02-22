using System.Collections.Generic;
using System;
using Elements.Geometry;
using Elements.Geometry.Profiles;
using Elements.Geometry.Solids;
using Elements.Spatial;
using Newtonsoft.Json;
using Xunit;

namespace Elements.Tests
{
    public class CellComplexTests : ModelTest
    {
        [Fact]
        public void CellComplexSerializes()
        {
            // Create Grid2d
            var squareSize = 10;
            var rect = Polygon.Rectangle(squareSize, squareSize);

            // Using constructor with origin
            var origin = new Vector3();
            var uDirection = new Vector3(1, 0, 0);
            var vDirection = new Vector3(-1, 1, 0); // 45 degrees up to the left

            var grid = new Grid2d(rect, origin, uDirection, vDirection);
            grid.SplitAtPoint(origin);

            var cellComplex = new CellComplex(Guid.NewGuid(), "Test");
            var height = 5;

            var i = 0;

            for (i = 0; i < 3; i++)
            {
                foreach (var cell in grid.GetCells())
                {
                    foreach (var crv in cell.GetTrimmedCellGeometry())
                    {
                        cellComplex.AddCell((Polygon)crv, 5, height * i);
                    }
                }
            }

            i = 0;
            foreach (var vertex in cellComplex.Vertices.Values)
            {
                vertex.Name = $"Vertex-{i}";
                i++;
            }

            var model = new Model();
            model.AddElement(cellComplex);
            var json = model.ToJson();
            var modelFromDeserialization = Model.FromJson(json);
            var cellComplexDeserialized = modelFromDeserialization.GetElementOfType<CellComplex>(cellComplex.Id);
            var vertexExists = cellComplexDeserialized.VertexExists(new Vector3(0, Vector3.EPSILON / 2, 0), out var vertexId, Vector3.EPSILON);

        }
    }
}