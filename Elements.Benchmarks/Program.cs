using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Elements.Geometry;
using Elements.Geometry.Profiles;
using Elements.Geometry.Solids;
using Elements.Serialization.glTF;

namespace Elements.Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(launchCount: 1, warmupCount: 1, targetCount: 3)]
    public class CsgBenchmarks
    {
        private WideFlangeProfileFactory _profileFactory = new WideFlangeProfileFactory();
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

    [MemoryDiagnoser]
    [SimpleJob(launchCount: 1, warmupCount: 3, targetCount: 10)]
    public class HSS
    {
        Model _model;

        public HSS()
        {
            var x = 0.0;
            var z = 0.0;
            var hssFactory = new HSSPipeProfileFactory();
            var profiles = hssFactory.AllProfiles().ToList();
            _model = new Model();
            foreach (var profile in profiles)
            {
                var color = new Color((float)(x / 20.0), (float)(z / profiles.Count), 0.0f, 1.0f);
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
    
    [MemoryDiagnoser]
    [SimpleJob(launchCount: 1, warmupCount: 3, targetCount: 10)]
    public class WasmComparison
    {
        private Model _model;
        public WasmComparison()
        {
            var total = 10;
            _model = new Model();
            for(var x=0; x<10; x++)
            {
                for(var y=0; y<10; y++)
                {
                    for(var z=0;z<10; z++)
                    {
                        var mass = new Mass(Polygon.Rectangle(0.01 + x/total, 0.01 + y/total), 0.01 + z/total);
                        _model.AddElement(mass);
                    }
                }
            }
        }
        
        [Benchmark(Description = "Write all cubes to glb.")]
        public void WriteModelToGlb()
        {
            _model.ToGlTF();
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<HSS>();
            BenchmarkRunner.Run<CsgBenchmarks>();
            BenchmarkRunner.Run<WasmComparison>();
        }
    }
}
