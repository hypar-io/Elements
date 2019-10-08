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

            // Create a colorizer which colors a triangle
            // according to its slope.
            Func<Triangle,Color> colorizer = (tri) => {
                var slope = tri.Normal.AngleTo(Vector3.ZAxis);
                if(slope >=0.0 && slope < 15.0)
                {
                    return Colors.Green;
                }
                else if(slope >= 15.0 && slope < 30.0)
                {
                    return Colors.Yellow;
                }
                else if(slope >= 30.0 && slope < 45.0)
                {
                    return Colors.Orange;
                }
                else if(slope >= 45.0)
                {
                    return Colors.Red;
                }
                return Colors.Red;
            };

            // Create a topography.
            var topo = new Topography(Vector3.Origin, cellSize, cellSize, elevations, w, colorizer);
            // </example>

            this.Model.AddElement(topo);
        }
    }
}