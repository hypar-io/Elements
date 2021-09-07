using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using Elements.Geometry;
using Elements.Geometry.Profiles;
using Elements.Geometry.Solids;
using Elements.Serialization.glTF;
using Elements.Validators;

namespace Elements.Benchmarks
{
    [MemoryDiagnoser]
    public class CsgBenchmarks
    {
        private Beam _beam;
        private Csg.Solid _csg;

        [Params(1, 10, 20, 50)]
        public int NumberOfHoles { get; set; }

        public CsgBenchmarks()
        {
            var line = new Line(new Vector3(0, 0, 0), new Vector3(10, 0, 5));
            var profile = Polygon.Rectangle(Units.InchesToMeters(10), Units.InchesToMeters(20));
            _beam = new Beam(line, profile, BuiltInMaterials.Steel);
            for (var i = 0.0; i <= 1.0; i += 1.0 / (double)NumberOfHoles)
            {
                var t = line.TransformAt(i);
                var lt = new Transform(t.Origin, t.ZAxis, t.XAxis.Negate());
                lt.Move(lt.ZAxis * -0.5);
                var hole = new Extrude(Polygon.Rectangle(0.1, 0.1), 1.0, Vector3.ZAxis, true)
                {
                    LocalTransform = lt
                };
                _beam.Representation.SolidOperations.Add(hole);
            }
            _csg = _beam.GetFinalCsgFromSolids();
        }

        [Benchmark(Description = "Tesselate CSG.")]
        public void CsgToGraphicsBuffers()
        {
            _csg.Tessellate();
        }
    }

    [EventPipeProfiler(EventPipeProfile.CpuSampling)]
    [SimpleJob(launchCount: 1, warmupCount: 0, targetCount: 1)]
    public class Trace
    {
        [Benchmark(Description = "Create all HSS beams and serialize to JSON.")]
        public void TraceModelCreation()
        {
            var factory = new HSSPipeProfileFactory();
            var hssProfiles = factory.AllProfiles().ToList();
            var model = HSS.DrawAllBeams(hssProfiles);
            var json = model.ToJson();
        }
    }

    [MemoryDiagnoser]
    public class Serialization
    {
        private Model _model;
        private string _json;

        [GlobalSetup]
        public void Setup()
        {
            var factory = new HSSPipeProfileFactory();
            var hssProfiles = factory.AllProfiles().ToList();
            _model = HSS.DrawAllBeams(hssProfiles);
            _json = _model.ToJson();
        }

        [Benchmark(Description = "Serialize")]
        public void SerializeToJSON()
        {
            _model.ToJson();
        }

        [Benchmark(Description = "Deserialize from JSON.")]
        public void DeserializeFromJSON()
        {
            Model.FromJson(_json);
        }

        [Benchmark(Description = "Serialize to glTF.")]
        public void SerializeToGlTF()
        {
            _model.ToGlTF(false);
        }
    }

    [MemoryDiagnoser]
    public class ElementCreation
    {
        Model _model;
        string _json;

        private List<HSSPipeProfile> _hssProfiles;

        [Params(true, false)]
        public bool SkipValidation { get; set; }

        [Params(true, false)]
        public bool Merge { get; set; }

        [IterationSetup]
        public void Setup()
        {
            if (this.SkipValidation)
            {
                Validator.DisableValidationOnConstruction = true;
            }
        }

        [IterationCleanup]
        public void Cleanup()
        {
            Validator.DisableValidationOnConstruction = false;
        }

        public static Model DrawAllBeams(List<HSSPipeProfile> profiles)
        {
            var x = 0.0;
            var z = 0.0;
            var model = new Model();
            foreach (var profile in profiles)
            {
                var color = new Color((float)(x / 20.0), (float)(z / profiles.Count), 0.0f, 1.0f);
                var line = new Line(new Vector3(x, 0, z), new Vector3(x, 3, z));
                var beam = new Beam(line, profile);
                model.AddElement(beam);
                x += 2.0;
                if (x > 20.0)
                {
                    z += 2.0;
                    x = 0.0;
                }
            }
            return model;
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            var factory = new HSSPipeProfileFactory();
            _hssProfiles = factory.AllProfiles().ToList();
        }

        [Benchmark(Description = "Draw all HSS beams.")]
        public void DrawAllBeams()
        {
            DrawAllBeams(_hssProfiles);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}
