using System;
using System.IO;
using Elements.Geometry;
using System.Linq;
using System.Diagnostics;
using Elements.Serialization.glTF;
using Elements.Serialization.JSON;
using Elements.Serialization.IFC;

namespace Elements.Tests
{
    /// <summary>
    /// ModelTest is the base class for all tests which
    /// create models. After a test is complete, the model
    /// is serialized to JSON, glTF, and IFC
    /// </summary>
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

        public bool GenerateIfc{get;set;}

        public bool GenerateGlb{get;set;}

        public bool GenerateJson{get;set;}

        internal static Line TestLine = new Line(Vector3.Origin, new Vector3(5,5,5));
        internal static Arc TestArc = new Arc(new Plane(Vector3.Origin, Vector3.ZAxis), 2.0, 0.0, 90.0);
        internal static Polyline TestPolyline = new Polyline(new []{new Vector3(0,0), new Vector3(0,2), new Vector3(0,3,1)});
        internal static Polygon TestPolygon = Polygon.Ngon(5, 2);

        public ModelTest()
        {
            this._model = new Model();
            this.GenerateGlb = true;
            this.GenerateIfc = true;
            this.GenerateJson = true;
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

                if(this.GenerateGlb)
                {
                    // Write the model as a glb
                    var modelPath = $"models/{this._name}.glb";
                    this._model.ToGlTF($"models/{this._name}.glb");
                    sw.Stop();
                    // Console.WriteLine($"Saved {this._name} to glb: {modelPath}.({sw.Elapsed.TotalMilliseconds}ms)");
                }

                if(this.GenerateJson)
                {
                    // Write the model as json
                    var jsonPath = $"models/{this._name}.json";
                    // Console.WriteLine($"Serializing {this._name} to JSON: {jsonPath}");
                    File.WriteAllText(jsonPath, this._model.ToJson());

                    // Try deserializing JSON
                    // Console.WriteLine($"Deserializing {this._name} from JSON.");
                    var newModel = Model.FromJson(File.ReadAllText(jsonPath));
                }

                if(this.GenerateIfc)
                {
                    // Write the model as an IFC
                    var ifcPath = $"models/{this._name}.ifc";
                    // Console.WriteLine($"Serializing {this._name} to IFC: {ifcPath}");
                    this._model.ToIFC(ifcPath);
                }
            }
        }
    }
}