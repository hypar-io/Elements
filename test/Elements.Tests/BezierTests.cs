using Elements;
using Elements.Geometry;
using Elements.Serialization.glTF;
using Elements.Tests;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Hypar.Tests
{
    public class BezierTests : ModelTest
    {
        ITestOutputHelper _output;

        public BezierTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Fact]
        public void Bezier()
        {
            this.Name = "Bezier";
            
            var a = Vector3.Origin;
            var b = new Vector3(5, 5, 1);
            var c = new Vector3(10, 2, 5);
            var d = new Vector3(20, -5, 30);
            var e = new Vector3(30, 0, 0);
            var ctrlPts = new List<Vector3>{a,b,c,d,e};
            var bezier = new Bezier(ctrlPts);
            var mc = new ModelCurve(bezier);
            this.Model.AddElement(mc);

            var bounds = bezier.Bounds();
            this._output.WriteLine($"Min: {bounds.Min}");
            this._output.WriteLine($"Max: {bounds.Max}");

            for(var t=0.0; t<=1.0; t+=0.1)
            {
                var trans = bezier.TransformAt(t);
                this.Model.AddElements(trans.ToModelCurves());
            }

            var beam = new Beam(bezier, Polygon.Rectangle(1,2), material:BuiltInMaterials.Mass);
            this.Model.AddElement(beam);

            this.Model.ToGlTF("bezier.gltf", false);

            this._output.WriteLine($"The bezier is {bezier.Length()} units long.");
        }
    }
}