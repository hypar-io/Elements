using System;
using System.IO;
using Elements.Geometry;
using System.Linq;
using Elements.Serialization.IFC;
using Elements.Serialization.glTF;
using System.Reflection;

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

        public bool GenerateIfc { get; set; }

        public bool GenerateGlb { get; set; }

        public bool GenerateJson { get; set; }

        internal static Line TestLine = new Line(Vector3.Origin, new Vector3(5, 5, 5));
        internal static Arc TestArc = new Arc(Vector3.Origin, 2.0, 0.0, 90.0);
        internal static Polyline TestPolyline = new Polyline(new[] { new Vector3(0, 0), new Vector3(0, 2), new Vector3(0, 3, 1) });
        internal static Polygon TestPolygon = Polygon.Ngon(5, 2);
        internal static Circle TestCircle = new Circle(Vector3.Origin, 5);
        internal static Ellipse TestEllipse = new Ellipse(Vector3.Origin, 5, 2);
        internal static EllipticalArc TestEllipticalArc = new EllipticalArc(TestEllipse, Math.PI * 0.25, Math.PI * 0.75);

        public ModelTest()
        {
            this._model = new Model();
            this.GenerateGlb = true;
            this.GenerateIfc = true;
            this.GenerateJson = true;

            var modelsDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "models");
            if (!Directory.Exists(modelsDir))
            {
                Directory.CreateDirectory(modelsDir);
            }
        }

        public virtual void Dispose()
        {
            if (this._model.Elements.Any() && !String.IsNullOrEmpty(this._name))
            {
                if (this.GenerateGlb)
                {
                    var modelPath = $"models/{this._name}.glb";
                    this._model.ToGlTF(modelPath, true, false);
                }

                if (this.GenerateJson)
                {
                    var jsonPath = $"models/{this._name}.json";
                    var json = this._model.ToJson();
                    File.WriteAllText(jsonPath, json);

                    var newModel = Model.FromJson(File.ReadAllText(jsonPath));

                    var elements = this._model.AllElementsOfType<Element>();
                    foreach (var e in elements)
                    {
                        var newEl = newModel.GetElementOfType<Element>(e.Id);
                        if (newEl == null)
                        {
                            throw new Exception($"{this.Name}: An element with the id {e.Id}, could not be found in the new model.");
                        }
                    }
                }

                if (this.GenerateIfc)
                {
                    var ifcPath = $"models/{this._name}.ifc";
                    this._model.ToIFC(ifcPath);
                }
            }
        }
    }
}