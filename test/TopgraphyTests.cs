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

            // Read topo elevations
            var w = 512/8 - 1;
            var data = JsonConvert.DeserializeObject<Dictionary<string,double[]>>(File.ReadAllText("./elevations.json"));
            var elevations = data["points"];

            // Compute the mapbox tile side lenth.
            var d = (40075016.685578 / Math.Pow(2, 15))/w;

            Func<Triangle, Vector3,Color> colorizer = (tri, n) => {
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

            var ngon = Polygon.Ngon(5, 200);

            var t = new Transform();
            t.Move(new Vector3(700,0,topo.MinElevation));
            t.Rotate(Vector3.ZAxis, 33.0);
            var mass = new Mass(new Profile(ngon), 500, BuiltInMaterials.Mass, t);
            this.Model.AddElement(mass);
            topo.Subtract(mass);

            var t1 = new Transform();
            t1.Move(new Vector3(900,0,topo.MinElevation));
            var mass1 = new Mass(ngon, 500, BuiltInMaterials.Mass, t1);
            this.Model.AddElement(mass1);
            topo.Subtract(mass1);
        }
    }
}