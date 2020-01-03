using Elements;
using Elements.Geometry;
using Elements.Geometry.Profiles;
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

            var bezier1 = new Bezier(ctrlPts);
            var bezier2 = new Bezier(ctrlPts, FrameType.RoadLike);
            
            var profile = WideFlangeProfileServer.Instance.GetProfileByType(WideFlangeProfileType.W21x55);

            // Get transforms with Frenet frame.
            var beam = new Beam(bezier1, profile, material:BuiltInMaterials.Default);
            this.Model.AddElement(beam);

            // Get transforms with road-like frame.
            var beam1 = new Beam(bezier2, profile, material:BuiltInMaterials.Default, transform: new Transform(0, 0, 0));
            this.Model.AddElement(beam1);
        }
    }
}