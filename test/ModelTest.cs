using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Elements;
using Xunit;
using Xunit.Sdk;
using Elements.Geometry;
using System.Linq;

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
        internal static Arc TestArc = new Arc(Vector3.Origin, 5.0, 0.0, 45.0);
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
                // Write the model as a glb
                this._model.SaveGlb($"models/{this._name}.glb");

                // Write the model as json
                File.WriteAllText($"models/{this._name}.json", this._model.ToJson());
            }
        }
    }
}