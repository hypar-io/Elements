using Elements.Geometry;
using Elements.Geometry.Profiles;
using System;
using System.Linq;
using Xunit;

namespace Elements.Tests
{
    public class BeamSystemTests : ModelTest
    {
        [Fact]
        public void BeamSystem()
        {
            this.Name = "BeamSystem";
            var profile = new WideFlangeProfile("test", 1.0, 2.0, 0.1, 0.1);
            var a = new Vector3(0,0,0);
            var b = new Vector3(20,0,0);
            var d = new Vector3(20,20,10);
            var c = new Vector3(0,20,0);
            var polygon = new Polygon(new[]{a,b,c,d});
            var framingType = new StructuralFramingType(Guid.NewGuid().ToString(), profile, BuiltInMaterials.Steel);
            var beam = new Beam(new Line(a,b), framingType);
            var system = new BeamSystem(5, framingType, new Line(a,b), new Line(c,d));
            Assert.True(system.Elements.Count() == 5);
            this.Model.AddElements(system.Elements);
            this.Model.AddElement(beam);
        }
    }
}