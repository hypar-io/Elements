using Elements;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Profiles;
using System;
using Xunit;

namespace Elements.Tests
{
    public class BeamTest : ModelTest
    {
        [Fact]
        public void Beam()
        {
            this.Name = "Elements_Beam";

            var line = new Line(Vector3.Origin, new Vector3(5,0,0));
            var profile = WideFlangeProfileServer.Instance.GetProfileByName("W44x335");
            var framingType = new StructuralFramingType(profile.Name, profile, BuiltInMaterials.Steel);
            var beam = new Beam(line, framingType);

            this.Model.AddElement(beam);
        }
    }
}