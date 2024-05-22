using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Elements.Geometry.Profiles;
using Elements.Serialization.glTF;

namespace Elements.Benchmarks
{
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
            _model = ElementCreation.DrawAllBeams(hssProfiles);
            _json = _model.ToJson();
        }

        [Params(true, false)]
        public bool Merge { get; set; }

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

        [Benchmark(Description = "Deserialize invalid Elements from JSON.")]
        public void DeserializeInvalidsFromJSON()
        {
            var guids = Enumerable.Range(0, 1000000).Select(i => Guid.NewGuid().ToString()).ToArray();
            var basElements = String.Join(",", guids.Select(g => $"'{g}':{{'discriminator':'Elements.Baz'}}").ToArray());
            var toDeserialize = _json.Replace("'Elements':{", $"'Elements':{{{basElements},");
            Model.FromJson(toDeserialize);
        }

        [Benchmark(Description = "Serialize to glTF.")]
        public void SerializeToGlTF()
        {
            _model.ToGlTF(false, this.Merge);
        }
    }
}