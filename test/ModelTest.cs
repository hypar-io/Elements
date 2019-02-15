using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Elements;
using Xunit;
using Xunit.Sdk;
using Elements.Geometry;
using System.Linq;
using System.Diagnostics;

namespace Elements.Tests
{
    public class ModelTest : IDisposable
    {
        private Model _model;
        private string _name;

        public Model Model
        {
            get => _model;
            set => _model = value;
        }
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        internal static Line TestLine = new Line(Vector3.Origin, new Vector3(5,5,5));
        internal static Arc TestArc = new Arc(new Plane(Vector3.Origin, Vector3.ZAxis), 2.0, 0.0, 90.0);
        internal static Polyline TestPolyline = new Polyline(new []{new Vector3(0,0), new Vector3(0,2), new Vector3(0,3,1)});
        internal static Polygon TestPolygon = Polygon.Ngon(5, 2);

        public ModelTest()
        {
            this._model = new Model();
        }

        public void Dispose()
        {
            if(this._model.Elements.Any())
            {
                if(!Directory.Exists("models"))
                {
                    Directory.CreateDirectory("models");
                }
                var sw = new Stopwatch();
                sw.Start();

                // Write the model as a glb
                var modelPath = $"models/{this._name}.glb";
                this._model.SaveGlb($"models/{this._name}.glb");
                sw.Stop();
                Console.WriteLine($"Saved {this._name} to glb: {modelPath}.({sw.Elapsed.TotalMilliseconds}ms)");

                // Write the model as json
                var jsonPath = $"models/{this._name}.json";
                Console.WriteLine($"Serializing {this._name} to JSON: {jsonPath}");
                File.WriteAllText(jsonPath, this._model.ToJson());

                // Try deserializing JSON
                Console.WriteLine($"Deserializing {this._name} from JSON.");
                var newModel = Model.FromJson(File.ReadAllText(jsonPath));
            }
        }
    }
}