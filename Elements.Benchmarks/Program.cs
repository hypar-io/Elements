using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Elements.Geometry;
using Elements.Geometry.Profiles;
using Elements.Geometry.Solids;
using Elements.Serialization.glTF;

namespace Elements.Benchmarks
{
    [SimpleJob(launchCount: 1, warmupCount: 10, targetCount: 30)]
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

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Csg>();
        }
    }
}
