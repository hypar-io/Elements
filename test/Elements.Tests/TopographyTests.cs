using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Elements.Geometry;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Elements.Tests
{
    public class TopographyTests: ModelTest
    {
        private ITestOutputHelper _output;
        public TopographyTests(ITestOutputHelper output)
        {
            this._output = output;
        }

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
        }

        [Fact]
        public void ConstructTopography()
        {
            this.Name = "Topography";
            var topo = CreateTopoFromMapboxElevations();
            this.Model.AddElement(topo);
        }

        [Fact]
        public void TopographyHasTextureApplied()
        {
            this.Name = "TexturedTopography";
            var m = new Material("texture",Colors.Gray, 0.0f, 0.0f, "UV.jpg");
            var topo = CreateTopoFromMapboxElevations(null, m);
            this.Model.AddElement(topo);
        }

        [Fact]
        public void TopographySerializesQuickly()
        {
            this.Name = "TopographySerializationPerfomance";
            var sw = new Stopwatch();
            var topo = CreateTopoFromMapboxElevations();
            sw.Start();
            this.Model.AddElement(topo);
            sw.Stop();
            _output.WriteLine($"Serialization of topography: {sw.ElapsedMilliseconds.ToString()}ms");
            this.Model.Elements.Clear();
            sw.Reset();
            sw.Start();
            this.Model.AddElement(BuiltInMaterials.Topography);
            topo.Material = BuiltInMaterials.Topography;
            this.Model.AddElement(topo, false);
            sw.Stop();
            _output.WriteLine($"Serialization of topography w/out recursive gather: {sw.ElapsedMilliseconds.ToString()}ms");
        }

        [Fact]
        public void RaysIntersectTopography()
        {
            this.Name = "RayTopographyIntersection";
            var topo = CreateTopoFromMapboxElevations(new Vector3(10000000,10000000));
            var mp = new ModelPoints(new List<Vector3>(), new Material("xsect", Colors.Black));
            foreach(var t in topo.Mesh.Triangles)
            {
                var o = t.Vertices[0].Position;
                var c = new[]{t.Vertices[0].Position, t.Vertices[1].Position, t.Vertices[2].Position}.Average();
                var r = new Ray(new Vector3(c.X, c.Y), Vector3.ZAxis);
                if(r.Intersects(t, out Vector3 result))
                {
                    mp.Locations.Add(result);
                }
            }
            this.Model.AddElement(topo);
            if(mp.Locations.Count > 0)
            {
                this.Model.AddElement(mp);
            }
        }

        private static Topography CreateTopoFromMapboxElevations(Vector3 origin = null, Material material = null)
        {
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

            return new Topography(origin != null ? origin : Vector3.Origin, d, d, elevations, w, colorizer, material);
        }
    }
}