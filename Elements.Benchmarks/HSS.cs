using System.Linq;
using BenchmarkDotNet.Attributes;
using Elements.Geometry;
using Elements.Geometry.Profiles;
using Elements.Serialization.glTF;

namespace Elements.Benchmarks
{
    [MemoryDiagnoser]
    public class HSS
    {
        readonly Model _model;

        public HSS()
        {
            var x = 0.0;
            var z = 0.0;
            var hssFactory = new HSSPipeProfileFactory();
            var profiles = hssFactory.AllProfiles().ToList();
            _model = new Model();
            foreach (var profile in profiles)
            {
                var line = new Line(new Vector3(x, 0, z), new Vector3(x, 3, z));
                var beam = new Beam(line, profile);
                _model.AddElement(beam);
                x += 2.0;
                if (x > 20.0)
                {
                    z += 2.0;
                    x = 0.0;
                }
            }
        }

        [Benchmark(Description = "Draw all beams without merge.")]
        public void DrawAllBeamsWithoutMerge()
        {
            _model.ToGlTF(false, false);
        }

        [Benchmark(Description = "Draw all beams with merge.")]
        public void DrawAllBeamsWithMerge()
        {
            _model.ToGlTF(false, true);
        }
    }
}