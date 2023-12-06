using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Serialization.JSON;
using System.Text.Json.Serialization;
using Xunit;
using System.Text.Json;

namespace Elements.Tests
{
    public class MeshTests : ModelTest
    {
        [Fact]
        public void Volume()
        {
            // A simple extrusion.
            var extrude = new Extrude(Polygon.Rectangle(1, 1), 1, Vector3.ZAxis, false);
            var mesh = new Mesh();
            extrude.Solid.Tessellate(ref mesh);
            Assert.Equal(1.0, mesh.Volume(), 5);

            // A more complicated extrusion.
            var l = Polygon.L(20, 10, 5);
            var lMesh = new Mesh();
            var lExtrude = new Extrude(l, 5, Vector3.ZAxis, false);
            lExtrude.Solid.Tessellate(ref lMesh);
            Assert.Equal(l.Area() * 5, lMesh.Volume(), 5);

            // A boolean.
            var l1 = l.Offset(-1)[0].Reversed();
            var l1Mesh = new Mesh();
            var l1Extrude = new Extrude(new Profile(l, l1), 5, Vector3.ZAxis, false);
            l1Extrude.Solid.Tessellate(ref l1Mesh);
            var a = l.Area();
            var a1 = l1.Area();
            var v = l1Mesh.Volume();
            Assert.Equal((l.Area() - l1.Area()) * 5, l1Mesh.Volume(), 5);
        }

        [Fact]
        public void ReadMeshSerializedAsNull()
        {
            var json = @"
            {
  ""Mesh"": null,
}
            ";
            var options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true
            };

            JsonSerializer.Deserialize<InputsWithMesh>(json, options);
        }

        [Fact]
        public void IntersectsRays()
        {
            Name = nameof(IntersectsRays);
            var random = new Random(10);
            var _mesh = new Mesh();
            var _rays = new List<Ray>();
            var xCount = 100;
            var yCount = 100;
            for (int i = 0; i < xCount; i++)
            {
                for (int j = 0; j < yCount; j++)
                {
                    var point = new Vector3(i, j, random.NextDouble() * 2);
                    var c = _mesh.AddVertex(point);
                    if (i != 0 && j != 0)
                    {
                        // add faces
                        var d = _mesh.Vertices[i * yCount + j - 1];
                        var a = _mesh.Vertices[(i - 1) * yCount + j - 1];
                        var b = _mesh.Vertices[(i - 1) * yCount + j];
                        _mesh.AddTriangle(a, b, c);
                        _mesh.AddTriangle(c, d, a);
                    }
                }
            }

            // create 1000 random rays
            for (int i = 0; i < 1000; i++)
            {
                var ray = new Ray(new Vector3(random.NextDouble() * (xCount - 1), random.NextDouble() * (yCount - 1), 5), new Vector3(0, 0, -1));
                _rays.Add(ray);
                Model.AddElement(new ModelCurve(new Line(ray.Origin, ray.Origin + ray.Direction * 0.1), BuiltInMaterials.XAxis));
            }
            _mesh.ComputeNormals();
            Model.AddElement(new MeshElement(_mesh) { Material = new Material("b") { Color = (0.6, 0.6, 0.6, 1), DoubleSided = true } });

            var pts = new List<Vector3>();

            foreach (var ray in _rays)
            {
                if (ray.Intersects(_mesh, out var p))
                {
                    pts.Add(p);
                    var l = new Line(p, ray.Origin);
                    Model.AddElement(l);
                }
            }

            Assert.Equal(_rays.Count, pts.Count);
        }

        [Fact]
        public void MergeOnFirstVertexAdd()
        {
            var mesh = new Mesh();
            mesh.AddVertex(new Vector3(0, 0, 0), merge: true);
            // no execptions
        }

        public class InputsWithMesh
        {
            [JsonConstructor]
            public InputsWithMesh(Mesh @mesh)
            {
                this.Mesh = @mesh;
            }

            [JsonConverter(typeof(MeshConverter))]
            public Mesh Mesh { get; set; }
        }
    }

}