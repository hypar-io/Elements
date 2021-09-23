using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using Elements.Geometry.Profiles;

namespace Elements.Benchmarks
{
    [EventPipeProfiler(EventPipeProfile.CpuSampling)]
    [SimpleJob(launchCount: 1, warmupCount: 0, targetCount: 1)]
    public class Trace
    {
        [Benchmark(Description = "Create all HSS beams and serialize to JSON.")]
        public void TraceModelCreation()
        {
            var factory = new HSSPipeProfileFactory();
            var hssProfiles = factory.AllProfiles().ToList();
            var model = ElementCreation.DrawAllBeams(hssProfiles);
            var json = model.ToJson();
        }
    }
}