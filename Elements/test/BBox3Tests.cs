using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Profiles;
using Elements.Geometry.Solids;
using Newtonsoft.Json;
using Xunit;

namespace Elements.Tests
{
    public class Bbox3Tests : ModelTest
    {
        [Fact]
        public void Bbox3Calculates()
        {
            var polygon = JsonConvert.DeserializeObject<Polygon>("{\"discriminator\":\"Elements.Geometry.Polygon\",\"Vertices\":[{\"X\":-30.52885,\"Y\":-5.09393,\"Z\":0.0},{\"X\":-48.41906,\"Y\":111.94723,\"Z\":0.0},{\"X\":-95.02982,\"Y\":70.39512,\"Z\":0.0},{\"X\":-72.98057,\"Y\":-34.12159,\"Z\":0.0}]}");
            var bbox = new BBox3(polygon);
        }
    }
}