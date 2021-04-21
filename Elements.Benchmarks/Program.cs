using System;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Elements.Geometry;
using Elements.Geometry.Profiles;
using Elements.Geometry.Solids;
using Elements.Serialization.glTF;

namespace Elements.Benchmarks
{
    [SimpleJob(launchCount: 1, warmupCount: 1, targetCount: 3)]
    public class Csg
    {
        private WideFlangeProfileFactory _profileFactory = new WideFlangeProfileFactory();

        [Params(1, 10, 20)]
        public int Samples { get; set; }

        [Benchmark(Description = "Compute csg of beam.")]
        public void CSG()
        {
            var profile = _profileFactory.GetProfileByType(WideFlangeProfileType.W10x100);

            var line = new Line(new Vector3(0, 0, 0), new Vector3(10, 0, 5));
            var beam = new Beam(line, profile, BuiltInMaterials.Steel);
            for (var i = 0.0; i <= 1.0; i += 1.0 / (double)Samples)
            {
                var t = line.TransformAt(i);
                var lt = new Transform(t.Origin, t.ZAxis, t.XAxis.Negate());
                lt.Move(lt.ZAxis * -0.5);
                var hole = new Extrude(Polygon.Rectangle(0.1, 0.1), 1.0, Vector3.ZAxis, true)
                {
                    LocalTransform = lt
                };
                beam.Representation.SolidOperations.Add(hole);
            }
            var model = new Model();
            model.AddElement(beam);
            GltfExtensions.InitializeGlTF(model, out var buffers, false);
        }
    }

    [MemoryDiagnoser]
    [SimpleJob(launchCount: 1, warmupCount: 3, targetCount: 10)]
    public class HSS
    {
        [Benchmark(Description = "Create all HSS beams.")]
        public void CreateAllHSSBeams()
        {
            var x = 0.0;
            var z = 0.0;
            var hssFactory = new HSSPipeProfileFactory();
            var profiles = hssFactory.AllProfiles().ToList();
            var model = new Model();
            foreach (var profile in profiles)
            {
                var color = new Color((float)(x / 20.0), (float)(z / profiles.Count), 0.0f, 1.0f);
                var line = new Line(new Vector3(x, 0, z), new Vector3(x, 3, z));
                var m = new Material(Guid.NewGuid().ToString(), color, 0.1f, 0.4f);
                model.AddElement(m, false);
                var beam = new Beam(line, profile, m);
                model.AddElement(beam, false);
                x += 2.0;
                if (x > 20.0)
                {
                    z += 2.0;
                    x = 0.0;
                }
            }
            model.ToGlTF(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "CreateAllHSSBeamsBenchmark.glb"));
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<HSS>();
        }
    }
}
