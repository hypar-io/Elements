using Hypar.Elements;
using Hypar.GeoJSON;
using Hypar.Geometry;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Xunit;

namespace Hypar.Tests
{
    public class ExecutionTests
    {
        [Fact]
        public void Execution()
        {
            var str = File.ReadAllText("execution.json");
            var request = (JObject)JsonConvert.DeserializeObject(str);
            var features = ((JArray)request["location"]).ToObject<Feature[]>();

            var outline = (Polygon)features[0].Geometry;
            var origin = outline.Coordinates[0][0].ToVectorMeters();

            var plines = outline.ToPolylines();
            var transformed = plines.Select(pline=>new Polyline(pline.Vertices.Select(v=>new Vector3(v.X - origin.X, v.Y - origin.Y, v.Z))).Reversed()).ToArray();

            Console.WriteLine(transformed[0]);

            var mass = new Mass(transformed[0], 0.0, transformed[0], 5.0);
            var model = new Model();
            model.Origin = outline.Coordinates[0][0];

            model.AddElement(mass);
            model.SaveGlb("siteMass.glb");
            
            var json = JsonConvert.SerializeObject(model);
            Console.WriteLine(json);
        }
    }
}