using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using Elements.Geometry.Profiles;
using Elements.Serialization.glTF;

namespace Elements.Benchmarks
{
    [EventPipeProfiler(EventPipeProfile.CpuSampling)]
    [SimpleJob]
    [MemoryDiagnoser]
    public class TraceJsonSerialization
    {
        private Model _model;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var factory = new HSSPipeProfileFactory();
            var hssProfiles = factory.AllProfiles().ToList();
            _model = ElementCreation.DrawAllBeams(hssProfiles);
        }

        [Benchmark(Description = "Create all HSS beams and serialize to JSON.")]
        public void TraceModelCreation()
        {
            Validators.Validator.DisableValidationOnConstruction = true;
            _model.ToJson(gatherSubElements: false);
        }
    }

    [EventPipeProfiler(EventPipeProfile.CpuSampling)]
    [MemoryDiagnoser]
    [SimpleJob]
    public class TraceGltfSerialization
    {
        private Model _model;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var factory = new HSSPipeProfileFactory();
            var hssProfiles = factory.AllProfiles().ToList();
            _model = ElementCreation.DrawAllBeams(hssProfiles);
        }

        [Benchmark(Description = "Create all HSS beams and serialize to glTF.")]
        public void TraceModelCreation()
        {
            _model.ToGlTF();
        }
    }
}