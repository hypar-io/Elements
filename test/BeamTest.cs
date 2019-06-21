using Elements.Geometry;
using Elements.Geometry.Profiles;
using Xunit;

namespace Elements.Tests
{
    public class BeamTest : ModelTest
    {
        [Fact]
        public void Beam()
        {
            this.Name = "Elements_Beam";

            var profile = WideFlangeProfileServer.Instance.GetProfileByName("W44x335");
            var framingType = new StructuralFramingType(profile.Name, profile, BuiltInMaterials.Steel);

            // A straight beam.
            var line = new Line(Vector3.Origin, new Vector3(5,0,5));
            var linearBeam = new Beam(line, framingType);

            // A polygon beam.
            var polygon = Polygon.Ngon(5, 2);
            var polygonBeam = new Beam(polygon, framingType, 0, 0, new Transform(6,0,0));

            this.Model.AddElement(linearBeam);
            this.Model.AddElement(polygonBeam);
        }
    }
}