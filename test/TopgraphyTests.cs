using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var colorizer = new Func<Triangle,Vector3,Color>((t,v)=>{
                return Colors.Green;
            });
            var topo = new Topography(Vector3.Origin, 1.0, 1.0, elevations, 3, colorizer);
            this.Model.AddElement(topo);

            var mass = new Mass(Polygon.Rectangle(0.75,1.0, new Vector3(2,1)), 3);
            topo.Subtract(mass);
            this.Model.AddElement(mass);
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

            var sw = new Stopwatch();
            sw.Start();

            var topo = new Topography(Vector3.Origin, d, d, elevations, w, colorizer);

            sw.Stop();
            Console.WriteLine($"{sw.Elapsed.TotalMilliseconds}ms to create topography.");
            sw.Reset();

            this.Model.AddElement(topo);

            // var ngon = Polygon.Ngon(5, 100);
            // var rand = new Random();

            // for(var i=0.0; i<1500; i+= 200.0)
            // {
            //     for(var j=0.0; j<1500; j+= 200.0)
            //     {
            //         sw.Start();
            //         var t = new Transform();
            //         t.Rotate(Vector3.ZAxis, rand.NextDouble()*360.0);
            //         t.Move(new Vector3(i,j,topo.MinElevation));
            //         var mass = new Mass(new Profile(ngon), 500, BuiltInMaterials.Mass, t);
            //         // this.Model.AddElement(mass);
            //         topo.Subtract(mass);
            //         sw.Stop();
            //         Console.WriteLine($"{sw.Elapsed.TotalMilliseconds}ms to subtract.");
            //         sw.Reset();
            //     }
            // }
        }
    }
}