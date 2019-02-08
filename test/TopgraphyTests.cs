using System;
using System.Collections.Generic;
using System.IO;
using Elements.Geometry;
using Newtonsoft.Json;
using Xunit;

namespace Elements.Tests
{
    public class TopographyTests: ModelTest
    {
        [Fact]
        public void Topography()
        {
            this.Name = "Topography";

            // Create random elevations
            // var w = 100;
            // var elevations = new double[(int)Math.Pow(w+1,2)];
            // var r = new Random();
            // for(var i=0; i < elevations.Length; i++)
            // {
            //     elevations[i] = r.NextDouble() * r.NextDouble();
            // }

            // Read topo elevations
            var w = 512/8 - 1;
            var data = JsonConvert.DeserializeObject<Dictionary<string,double[]>>(File.ReadAllText("./elevations.json"));
            var elevations = data["points"];
            var d = (40075016.685578 / Math.Pow(2, 15))/w;

            Func<Vector3,Color> colorizer = n => {
                var slope = n.AngleTo(Vector3.ZAxis);
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

            var topo = new Topography(Vector3.Origin, d, d, elevations, w, colorizer);
            this.Model.AddElement(topo);
        }
    }
}