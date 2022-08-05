using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Elements.Geometry;
using Elements.Serialization.glTF;

namespace Elements.Benchmarks
{
    public class Mesh
    {
        private Geometry.Mesh _mesh;
        private List<Ray> _rays;
        public Mesh()
        {
            var random = new Random(10);
            _mesh = new Geometry.Mesh();
            _rays = new List<Ray>();
            var xCount = 100;
            var yCount = 300;
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
                var ray = new Ray(new Vector3(random.NextDouble() * xCount, random.NextDouble() * yCount, 2.1), new Vector3(random.NextDouble() * 2 - 1, random.NextDouble() * 2 - 1, -1));
                _rays.Add(ray);
            }
        }

        [Benchmark(Description = "Intersect 1000 rays with mesh.")]
        public void IntersectRays()
        {
            foreach (var ray in _rays)
            {
                ray.Intersects(_mesh, out var _);
            }
        }
    }
}