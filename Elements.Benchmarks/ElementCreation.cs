using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Elements.Geometry;
using Elements.Geometry.Profiles;
using Elements.Validators;

namespace Elements.Benchmarks
{
    [MemoryDiagnoser]
    public class ElementCreation
    {

        private List<HSSPipeProfile> _hssProfiles;

        [Params(true, false)]
        public bool SkipValidation { get; set; }

        [IterationSetup]
        public void IterSetup()
        {
            if (this.SkipValidation)
            {
                Validator.DisableValidationOnConstruction = true;
            }
        }

        [IterationCleanup]
        public void IterCleanup()
        {
            Validator.DisableValidationOnConstruction = false;
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            var factory = new HSSPipeProfileFactory();
            _hssProfiles = factory.AllProfiles().ToList();
        }

        public static Model DrawAllBeams(List<HSSPipeProfile> profiles)
        {
            var x = 0.0;
            var z = 0.0;
            var model = new Model();
            model.AddElement(BuiltInMaterials.Steel, false);
            foreach (var profile in profiles)
            {
                var color = new Color((float)(x / 20.0), (float)(z / profiles.Count), 0.0f, 1.0f);
                var line = new Line(new Vector3(x, 0, z), new Vector3(x, 3, z));
                var beam = new Beam(line, profile);
                beam.Representation.SkipCSGUnion = true;
                model.AddElement(profile, false);
                model.AddElement(beam, false);
                x += 2.0;
                if (x > 20.0)
                {
                    z += 2.0;
                    x = 0.0;
                }
            }
            return model;
        }

        [Benchmark(Description = "Draw all HSS beams.")]
        public void DrawAllBeams()
        {
            DrawAllBeams(_hssProfiles);
        }
    }
}