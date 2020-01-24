using System.Collections.Generic;
using System.IO;
using Elements.Geometry;
using Newtonsoft.Json;
using Xunit;
using Elements.Spatial;

namespace Elements.Tests.Examples
{
    public class TopographyExample : ModelTest
    {
        [Fact]
        public void Example()
        {
            this.Name = "Elements_Topography";
            
            // <example>
            // Read topo elevations from a file.
            var data = JsonConvert.DeserializeObject<Dictionary<string,double[]>>(File.ReadAllText("./elevations.json"));
            var elevations = data["points"];
            var tileSize = WebMercatorProjection.GetTileSizeMeters(15);

            // Create a topography.
            var topo = new Topography(Vector3.Origin, tileSize, elevations);
            // </example>

            this.Model.AddElement(topo);
        }
    }
}