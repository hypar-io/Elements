using Elements;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using System;
using System.Linq;
using Xunit;

namespace Elements.Tests
{
    public class StructuralFramingTests : ModelTest
    {
        public enum BeamType
        {
            Line, Polyline, Polygon, Arc
        }

        private Profile _testProfile = WideFlangeProfileServer.Instance.GetProfileByName("W44x335");

        [Theory]
        [InlineData("LinearBeam", BeamType.Line, 0.25, 0.25)]
        [InlineData("PolylineBeam", BeamType.Polyline, 0.25, 0.25)]
        [InlineData("PolygonBeam", BeamType.Polygon, 0.25, 0.25)]
        [InlineData("ArcBeam", BeamType.Arc, 0.25, 0.25)]
        public void Beam(string testName, BeamType beamType, double startSetback, double endSetback)
        {
            this.Name = testName;

            ICurve cl = null;
            switch(beamType)
            {
                case BeamType.Line:
                    cl = ModelTest.TestLine;
                    break;
                case BeamType.Arc:
                    cl = ModelTest.TestArc;
                    break;
                case BeamType.Polygon:
                    cl = ModelTest.TestPolygon;
                    break;
                case BeamType.Polyline:
                    cl = ModelTest.TestPolyline;
                    break;
            }

            var beam = new Beam(cl, this._testProfile, BuiltInMaterials.Steel, null, startSetback, endSetback);
            Assert.Equal(BuiltInMaterials.Steel, beam.Material);
            Assert.Equal(cl, beam.Curve);

            this.Model.AddElement(beam);
        }

        [Fact]
        public void Example2()
        {
            var x = 0.0;
            var z = 0.0;
            var model = new Model();
            var profiles = WideFlangeProfileServer.Instance.AllProfiles().ToList();
            foreach(var profile in profiles)
            {
                var color = new Color((float)(x/20.0), (float)(z/profiles.Count), 0.0f, 1.0f);
                var line = new Line(new Vector3(x, 0, z), new Vector3(x,3,z));
                var beam = new Beam(line, profile, new Material(Guid.NewGuid().ToString(), color, 0.0f, 0.0f));
                model.AddElement(beam);
                x += 2.0;
                if (x > 20.0)
                {
                    z += 2.0;
                    x = 0.0;
                }
            }
            model.SaveGlb("wide_flange.glb");
        }

        [Fact]
        public void Example3()
        {
            var x = 0.0;
            var z = 0.0;
            var model = new Model();
            var profiles = HSSPipeProfileServer.Instance.AllProfiles().ToList();
            foreach(var profile in profiles)
            {
                var color = new Color((float)(x/20.0), (float)(z/profiles.Count), 0.0f, 1.0f);
                var line = new Line(new Vector3(x, 0, z), new Vector3(x,3,z));
                var beam = new Beam(line, profile, new Material(Guid.NewGuid().ToString(), color, 0.0f, 0.0f));
                model.AddElement(beam);
                x += 2.0;
                if (x > 20.0)
                {
                    z += 2.0;
                    x = 0.0;
                }
            }
            model.SaveGlb("hss_pipe.glb");
        }


        [Fact]
        public void Column()
        {
            this.Name = "Column";
            var column = new Column(Vector3.Origin, 3.0, this._testProfile);
            Assert.Equal(BuiltInMaterials.Steel, column.Material);
            Assert.Equal(3.0, column.Curve.Length());
            this.Model.AddElement(column);
        }

        [Fact]
        public void Brace()
        {
            this.Name = "Brace";
            var line = new Line(Vector3.Origin, new Vector3(3,3,3));
            var brace = new Brace(line, this._testProfile);
            Assert.Equal(BuiltInMaterials.Steel, brace.Material);
            Assert.Equal(line, brace.Curve);
            this.Model.AddElement(brace);
        }
    }
}