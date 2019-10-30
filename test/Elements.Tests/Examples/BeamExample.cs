using Elements.Geometry;
using Elements.Geometry.Profiles;
using Xunit;

namespace Elements.Tests.Examples
{
    public class BeamExample : ModelTest
    {
        [Fact]
        public void Example()
        {
            this.Name = "Elements_Beam";

            // <example>
            // Create a framing type.
            var profile = WideFlangeProfileServer.Instance.GetProfileByName("W44x335");

            // Create a straight beam.
            var line = new Line(Vector3.Origin, new Vector3(5,0,5));
            var linearBeam = new Beam(line, profile, BuiltInMaterials.Wood, 0, 0, 15);
            var lineT = line.TransformAt(0).ToModelCurves(linearBeam.Transform);

            // Create a polygon beam.
            var polygon = Polygon.Ngon(5, 2);
            var polygonBeam = new Beam(polygon, profile, BuiltInMaterials.Steel, 0, 0, 45.0, new Transform(6,0,0));
            var polyT = polygon.TransformAt(0).ToModelCurves(polygonBeam.Transform);

            // Create a curved beam.
            var arc = new Arc(Vector3.Origin, 5.0, 45.0, 135.0);
            var arcBeam = new Beam(arc, profile, BuiltInMaterials.Steel, 0, 0, 45.0, new Transform(12,0,0));
            var arcT = arc.TransformAt(0).ToModelCurves(arcBeam.Transform);
            // </example>
            
            this.Model.AddElement(linearBeam);
            this.Model.AddElements(lineT);
            this.Model.AddElement(polygonBeam);
            this.Model.AddElements(polyT);
            this.Model.AddElement(arcBeam);
            this.Model.AddElements(arcT);
        }
    }
}