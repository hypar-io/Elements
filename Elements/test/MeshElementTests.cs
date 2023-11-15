using Elements.Geometry;
using System;
using System.Collections.Generic;
using Xunit;
using Vertex = Elements.Geometry.Vertex;

namespace Elements.Tests
{
    public class MeshElementTests : ModelTest
    {
        [Fact, Trait("Category", "Examples")]
        public void MeshElement()
        {
            this.Name = "Elements_MeshElement";
            // <example>
            var mesh = new Mesh();
            var gridSize = 10;
            for (var u = 0; u < gridSize; u += 1)
            {
                for (var v = 0; v < gridSize; v += 1)
                {
                    var sinu = Math.Sin(-Math.PI + 2 * ((double)u / (double)gridSize * Math.PI));
                    var sinv = Math.Sin(-Math.PI + 2 * ((double)v / (double)gridSize * Math.PI));
                    var z = sinu + sinv;
                    var vertex = new Vertex(new Vector3(u, v, z), color: Colors.Mint);
                    mesh.AddVertex(vertex);

                    if (u > 0 && v > 0)
                    {
                        var index = u * gridSize + v;
                        var a = mesh.Vertices[index];
                        var b = mesh.Vertices[index - gridSize];
                        var c = mesh.Vertices[index - 1];
                        var d = mesh.Vertices[index - gridSize - 1];
                        var tri1 = new Triangle(a, b, c);
                        var tri2 = new Triangle(c, b, d);

                        mesh.AddTriangle(tri1);
                        mesh.AddTriangle(tri2);
                    }
                }
            }
            mesh.ComputeNormals();
            var meshElement = new MeshElement(mesh, material: new Material("Lime", Colors.Lime));
            //</example>
            this.Model.AddElement(meshElement);
        }

        [Fact]
        public void AddInvalidTriangle()
        {
            var a = new Vertex(new Vector3(0, 0, 0));
            var b = new Vertex(new Vector3(1, 1, 1));

            var mesh = new Mesh();

            Assert.Throws<ArgumentException>(() => mesh.AddTriangle(a, b, b));
            Assert.Throws<ArgumentException>(() => mesh.AddTriangle(a, a, b));
            Assert.Throws<ArgumentException>(() => mesh.AddTriangle(new Triangle(new List<Vertex> { b, a, b }, Vector3.ZAxis)));
        }
    }
}