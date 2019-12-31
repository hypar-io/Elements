using Elements;
using Elements.Geometry;
using Elements.Geometry.Profiles;
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
            var b = new Vector3(5, 0, 1);
            var c = new Vector3(5, 5, 2);
            var d = new Vector3(0, 5, 3);
            var e = new Vector3(0, 0, 4);
            var f = new Vector3(5, 0, 5);
            var ctrlPts = new List<Vector3>{a,b,c,d,e,f};
            var bezier = new Bezier(ctrlPts);
            
            var mc = new ModelCurve(bezier);
            this.Model.AddElement(mc);

            for(var t=0.0; t<=1.0; t+=0.1)
            {
                var trans = bezier.TransformAt(t);
                this.Model.AddElements(trans.ToModelCurves());
            }

            var profile = WideFlangeProfileServer.Instance.GetProfileByType(WideFlangeProfileType.W21x55);
            var beam = new Beam(bezier, profile, material:BuiltInMaterials.Default, rotation: 45);
            this.Model.AddElement(beam);

            this.Model.ToGlTF("bezier.gltf", false);

            this._output.WriteLine($"The bezier is {bezier.Length()} units long.");
        }
    }
}