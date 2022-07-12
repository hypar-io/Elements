using System;
using BenchmarkDotNet.Attributes;
using Elements.Geometry;
using Elements.Serialization.glTF;

namespace Elements.Benchmarks
{
    [MemoryDiagnoser]
    public class API
    {
        [Params(5, 50, 200, 500)]
        public int Value { get; set; }

        [Benchmark(Description = "API Test Serialization")]
        public void SerializeElementsToGlTF()
        {
            var model = new Model();
            var r = new Random();
            var size = 10;
            var profile = new Profile(Polygon.L(0.1, 0.1, 0.05));
            model.AddElement(profile);

            for (var i = 0; i < this.Value; i++)
            {
                var start = new Vector3(r.NextDouble() * size, r.NextDouble() * size, r.NextDouble() * size);
                var end = new Vector3(r.NextDouble() * size, r.NextDouble() * size, r.NextDouble() * size);
                var line = new Line(start, end);
                // var c = new Color(r.NextDouble(), r.NextDouble(), r.NextDouble(), 1.0);
                // var m = new Material(Guid.NewGuid().ToString(), c);
                var beam = new Beam(line, profile, null, BuiltInMaterials.Steel);
                model.AddElement(beam);
            }

            var json = model.ToJson();

            var newModel = Model.FromJson(json);

            var result = model.ToGlTF();
        }
    }
}