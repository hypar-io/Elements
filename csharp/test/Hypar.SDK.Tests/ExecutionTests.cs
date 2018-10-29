using Hypar.Elements;
using Hypar.GeoJSON;
using Hypar.Geometry;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using Hypar.Functions;
using System.Linq;

using Xunit;

namespace Hypar.Tests
{
    internal class TestFunction : IHyparFunction
    {
        public Model Execute(Model model, Dictionary<string, object> parameters, Dictionary<string, object> returns)
        {
            var profile = (Hypar.Geometry.Polygon)parameters["location"];
            var origin = (Position)parameters["origin"];

            var mass = new Mass(profile, 0.0, 5.0);
            model.AddElement(mass);
            model.Origin = origin;
            return model;
        }
    }

    public class ExecutionTests
    {
        // [Fact]
        // public void Execution()
        // {
        //     var str = File.ReadAllText("execution.json");
        //     var request = (JObject)JsonConvert.DeserializeObject(str);
        //     var features = ((JArray)request["location"]).ToObject<Feature[]>();

        //     var outline = (Hypar.GeoJSON.Polygon)features[0].Geometry;
        //     var origin = outline.Coordinates[0][0].ToVectorMeters();

        //     var plines = outline.ToPolygons();
        //     var transformed = plines.Select(pline=>new Hypar.Geometry.Polygon(pline.Vertices.Select(v=>new Vector3(v.X - origin.X, v.Y - origin.Y, v.Z)).ToArray()).Reversed()).ToArray();

        //     // Compare the request to the 
        //     // Execute the function
        //     var func = new TestFunction();
        //     var model = func.Execute(new Model(), )

        //     model.SaveGlb("siteMass.glb");
            
        //     var json = JsonConvert.SerializeObject(model);
        // }
    }
}