using System;
using System.Collections.Generic;
using System.IO;
using Elements.Geometry;
using Newtonsoft.Json;
using Xunit;

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
            var w = 512/8 - 1;
            var data = JsonConvert.DeserializeObject<Dictionary<string,double[]>>(File.ReadAllText("./elevations.json"));
            var elevations = data["points"];

            // Compute the mapbox tile size.
            var cellSize = (40075016.685578 / Math.Pow(2, 15))/w;

            // Create a topography.
            var topo = new Topography(Vector3.Origin, cellSize, cellSize, elevations, w);
            // </example>

            this.Model.AddElement(topo);
        }
    }
}