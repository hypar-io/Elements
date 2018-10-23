#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

using Xunit;
using Newtonsoft.Json;
using Hypar.Functions;
using System;
using System.IO;
using System.Collections.Generic;

namespace Hypar.Tests
{
    public class ConfigurationTests
    {
        private string configStr = @"{
  ""description"": ""A test function."",
  ""function_id"": ""box"",
  ""name"":""Box Function"",
  ""inputs"": {
    ""height"": {
      ""description"": ""The height of the box."",
      ""max"": 11,
      ""min"": 1,
      ""step"": 5,
      ""type"": ""range""
    },
    ""length"": {
      ""description"": ""The length of the box."",
      ""max"": 11,
      ""min"": 1,
      ""step"": 5,
      ""type"": ""range""
    },
    ""width"": {
      ""description"": ""The width of the box."",
      ""max"": 11,
      ""min"": 1,
      ""step"": 5,
      ""type"": ""range""
    },
    ""location"":{
        ""description"": ""The location."",
        ""type"":""location""
    }
  },
  ""outputs"": {
    ""volume"": {
      ""description"": ""The volume of the box."",
      ""type"":""number""
    }
  },
  ""repository_url"": ""https://github.com/foo/bar"",
}";

        [Fact]
        public void Valid_Config_Deserialize()
        {
            var config = HyparConfig.FromJson(configStr);
            Assert.Equal(4, config.Inputs.Values.Count);
            Assert.Equal(1, config.Outputs.Count);
            Assert.NotNull(config.Outputs["volume"]);
            Assert.Equal("Box Function", config.Name);
        }

        [Fact]
        public void Valid_Config_Serialize()
        {
            var converters = new[]{new InputOutputConverter()};
            var settings = new JsonSerializerSettings(){Converters = converters};
            var config = JsonConvert.DeserializeObject<HyparConfig>(configStr, settings);
            var result = JsonConvert.SerializeObject(config, Formatting.Indented, settings);
        }

        [Fact]
        public void Valid_JSON_Static_Construct_Success()
        {
            HyparConfig.FromJson(configStr);
        }

        [Fact]
        public void Emit()
        {
          var config = HyparConfig.FromJson(configStr);
          var codeGen = new CodeGen(config);
          var tmp = Path.GetTempPath();
          Console.WriteLine(tmp);
          codeGen.EmitCSharp(tmp);
          Assert.True(File.Exists(Path.Combine(tmp, "Box.cs")));
          Assert.True(File.Exists(Path.Combine(tmp, "Input.g.cs")));
          Assert.True(File.Exists(Path.Combine(tmp, "Output.g.cs")));
          Assert.True(File.Exists(Path.Combine(tmp, "Function.g.cs")));
        }
    }
}