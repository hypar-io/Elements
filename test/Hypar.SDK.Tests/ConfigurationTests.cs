#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

using Xunit;
using Newtonsoft.Json;
using Hypar.Configuration;
using System;
using System.Collections.Generic;

namespace Hypar.Tests
{
    public class ConfigurationTests
    {
        private string configStr = @"{
  ""function"": ""box.box"",
  ""runtime"": ""python3.6"",
  ""parameters"": {
    ""height"": {
      ""description"": ""The height of the box."",
      ""max"": 11,
      ""min"": 1,
      ""step"": 5,
      ""type"": ""number""
    },
    ""length"": {
      ""description"": ""The length of the box."",
      ""max"": 11,
      ""min"": 1,
      ""step"": 5,
      ""type"": ""number""
    },
    ""width"": {
      ""description"": ""The width of the box."",
      ""max"": 11,
      ""min"": 1,
      ""step"": 5,
      ""type"": ""number""
    },
    ""point_1"":{
        ""point"":{
            ""x"": 1.0,
            ""y"": 2.0,
            ""z"": 3.0
        },
        ""type"":""point""
    }
  },
  ""returns"": {
    ""volume"": {
      ""description"": ""The volume of the box."",
      ""type"":""number""
    }
  },
  ""version"": ""0.0.1""
}";

        [Fact]
        public void Valid_Config_Deserialize()
        {
         
            var converters = new[]{new ParameterDataConverter()};
            var settings = new JsonSerializerSettings(){Converters = converters};
            var config = JsonConvert.DeserializeObject<HyparConfig>(configStr, settings);

            Assert.Equal("box.box", config.Function);
            Assert.Equal(4, config.Parameters.Values.Count);
            Assert.Equal(1, config.Returns.Count);
            Assert.NotNull(config.Returns["volume"]);
        }

        [Fact]
        public void Valid_Config_Serialize()
        {
            var converters = new[]{new ParameterDataConverter()};
            var settings = new JsonSerializerSettings(){Converters = converters};
            var config = JsonConvert.DeserializeObject<HyparConfig>(configStr, settings);
            JsonConvert.SerializeObject(config, Formatting.Indented, settings);
        }

        [Fact]
        public void Valid_JSON_Static_Construct_Success()
        {
            HyparConfig.FromJson(configStr);
        }
    }
}