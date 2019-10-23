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
        public void Simple()
        {
            this.Name = "TopographySimple";
            var elevations = new double[]{0.2, 1.0, 0.5, 0.25, 0.1, 0.2, 2.0, 0.05, 0.05, 0.2, 0.5, 0.6};
            var colorizer = new Func<Triangle,Color>((t)=>{
                return Colors.Green;
            });
            var topo = new Topography(Vector3.Origin, 1.0, 1.0, elevations, 3, colorizer);
            this.Model.AddElement(topo);

            // var mass = new Mass(Polygon.Rectangle(0.75,1.0, new Vector3(2,1)), 3);
            // topo.Subtract(mass);
            // this.Model.AddElement(mass);
        }

        [Fact]
        public void Topography()
        {
            this.Name = "Topography";

            // Read topo elevations
            var w = 512/8 - 1;
            var data = JsonConvert.DeserializeObject<Dictionary<string,double[]>>(File.ReadAllText("./elevations.json"));
            var elevations = data["points"];

            // Compute the mapbox tile side lenth.
            var d = (40075016.685578 / Math.Pow(2, 15))/w;

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

            var topo = new Topography(Vector3.Origin, d, d, elevations, w, colorizer);
            this.Model.AddElement(topo);
        }
    }
}