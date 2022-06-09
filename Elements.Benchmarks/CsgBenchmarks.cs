using BenchmarkDotNet.Attributes;
using Elements.Geometry;
using Elements.Geometry.Solids;

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
            _beam = new Beam(line, profile, material: BuiltInMaterials.Steel);
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

        [Benchmark(Description = "Perform CSG ops.")]
        public void CsgOperations()
        {
            _beam.GetFinalCsgFromSolids();
        }
    }
}