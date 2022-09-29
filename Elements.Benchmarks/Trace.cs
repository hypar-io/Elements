using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using Elements.Geometry.Profiles;
using Elements.Serialization.glTF;

namespace Elements.Benchmarks
{
    [EventPipeProfiler(EventPipeProfile.CpuSampling)]
    [SimpleJob]
    public class TraceJsonSerialization
    {
        [Benchmark(Description = "Create all HSS beams and serialize to JSON.")]
        public void TraceModelCreation()
        {
            Validators.Validator.DisableValidationOnConstruction = true;
            var factory = new HSSPipeProfileFactory();
            var hssProfiles = factory.AllProfiles().ToList();
            var model = ElementCreation.DrawAllBeams(hssProfiles);
            model.ToJson(gatherSubElements: false);
        }
    }

    [EventPipeProfiler(EventPipeProfile.CpuSampling)]
    [SimpleJob]
    public class TraceGltfSerialization
    {
        [Benchmark(Description = "Create all HSS beams and serialize to glTF.")]
        public void TraceModelCreation()
        {
            var factory = new HSSPipeProfileFactory();
            var hssProfiles = factory.AllProfiles().ToList();
            var model = ElementCreation.DrawAllBeams(hssProfiles);
            model.ToGlTF();
        }
    }
}