using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Elements.Geometry;

namespace Elements.Benchmarks
{
    public class MeshRayIntersection
    {
        private Mesh _mesh;
        private List<Ray> _rays;

        public MeshRayIntersection()
        {
            var random = new Random(10);
            _mesh = new Mesh();
            _rays = new List<Ray>();
            var xCount = 100;
            var yCount = 300;
            MeshConstruction.BuildRandomMesh(_mesh, random, xCount, yCount);

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
    public class MeshConstruction
    {
        public static void BuildRandomMesh(Mesh m, Random random, int xCount, int yCount)
        {
            for (int i = 0; i < xCount; i++)
            {
                for (int j = 0; j < yCount; j++)
                {
                    var point = new Vector3(i, j, random.NextDouble() * 2);
                    var c = m.AddVertex(point);
                    if (i != 0 && j != 0)
                    {
                        // add faces
                        var d = m.Vertices[i * yCount + j - 1];
                        var a = m.Vertices[(i - 1) * yCount + j - 1];
                        var b = m.Vertices[(i - 1) * yCount + j];
                        m.AddTriangle(a, b, c);
                        m.AddTriangle(c, d, a);
                    }
                }
            }
        }

        [Params(1000, 5000, 10000, 30000)]
        public int VertexCount { get; set; }

        [Benchmark(Description = "Construct Mesh")]
        public void ConstructMesh()
        {
            var mesh = new Mesh();
            BuildRandomMesh(mesh, new Random(10), 100, VertexCount / 100);
        }

    }
}