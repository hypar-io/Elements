using Elements.Geometry;
using Elements.Geometry.Profiles;
using System;
using System.Linq;
using Xunit;
using System.Diagnostics;

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

            Curve cl = null;
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

            var beam = new Beam(cl, this._testProfile, BuiltInMaterials.Steel, startSetback, endSetback);
            Assert.Equal(BuiltInMaterials.Steel, beam.Material);
            Assert.Equal(cl, beam.Curve);

            this.Model.AddElement(beam);
        }

        [Fact]
        public void NonLinearVolumeException()
        {
            Curve cl = ModelTest.TestArc;
            var beam = new Beam(cl, this._testProfile, BuiltInMaterials.Steel);
            Assert.Throws<InvalidOperationException>(()=>beam.Volume());
        }

        [Fact]
        public void WideFlange()
        {
            var sw = new Stopwatch();
            sw.Start();

            this.Name = "WideFlange";

            var x = 0.0;
            var z = 0.0;
            var profiles = WideFlangeProfileServer.Instance.AllProfiles().ToList();
            foreach(var profile in profiles)
            {
                var color = new Color((float)(x/20.0), (float)(z/profiles.Count), 0.0f, 1.0f);
                var line = new Line(new Vector3(x, 0, z), new Vector3(x,3,z));
                var beam = new Beam(line, profile, new Material(Guid.NewGuid().ToString(), color, 0.0f, 0.0f));
                this.Model.AddElement(beam);
                x += 2.0;
                if (x > 20.0)
                {
                    z += 2.0;
                    x = 0.0;
                }
            }

            sw.Stop();
            Console.WriteLine($"{sw.ElapsedMilliseconds}ms for creating beams.");
            Console.WriteLine($"{GC.GetTotalMemory(false)}bytes allocated.");
        }

        [Fact]
        public void HSS()
        {
            this.Name = "HSS";
            var x = 0.0;
            var z = 0.0;
            var profiles = HSSPipeProfileServer.Instance.AllProfiles().ToList();
            foreach(var profile in profiles)
            {
                var color = new Color((float)(x/20.0), (float)(z/profiles.Count), 0.0f, 1.0f);
                var line = new Line(new Vector3(x, 0, z), new Vector3(x,3,z));
                var beam = new Beam(line, profile, new Material(Guid.NewGuid().ToString(), color, 0.0f, 0.0f));
                this.Model.AddElement(beam);
                x += 2.0;
                if (x > 20.0)
                {
                    z += 2.0;
                    x = 0.0;
                }
            }
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
            var brace = new Brace(line, this._testProfile, BuiltInMaterials.Steel);
            Assert.Equal(BuiltInMaterials.Steel, brace.Material);
            Assert.Equal(line, brace.Curve);
            this.Model.AddElement(brace);
        }

        [Fact]
        public void Setbacks()
        {
            this.Name = "BeamSetbacks";
            var line = new Line(Vector3.Origin, new Vector3(3,3,0));
            var mc = new ModelCurve(line, BuiltInMaterials.XAxis);
            this.Model.AddElement(mc);
            // Normal setbacks
            var beam = new Beam(line, this._testProfile, BuiltInMaterials.Steel, 2.0, 2.0);
            this.Model.AddElement(beam);

            var line1 = new Line(new Vector3(2,0,0), new Vector3(5,3,0));
            var mc1 = new ModelCurve(line1, BuiltInMaterials.XAxis);
            this.Model.AddElement(mc1);

            var sb = line1.Length()/2;
            // Setbacks longer in total than the beam.
            // We are testing to ensure that the beam gets created
            // without throwing. It will not have setbacks.
            var beam1 = new Beam(line1, this._testProfile, BuiltInMaterials.Steel, sb, sb);
            this.Model.AddElement(beam1);
        }

        [Fact]
        public void Benchmark()
        {
            var sw = new Stopwatch();
            sw.Start();

            var x = 0.0;
            var z = 0.0;
            var profile = WideFlangeProfileServer.Instance.AllProfiles().First();
            var n = 100000;
            var mesh = new Mesh();
            for(var i=0; i<n; i++)
            {
                var line = new Line(new Vector3(x, 0, z), new Vector3(x,3,z));
                var beam = new Beam(line, profile, BuiltInMaterials.Steel);
                beam.UpdateRepresentations();
                beam.Representation.SolidOperations.First().GetSolid().Tessellate(ref mesh);
                x += 2.0;
                if (x > 20.0)
                {
                    z += 2.0;
                    x = 0.0;
                }
            }

            sw.Stop();
            Console.WriteLine($"{sw.Elapsed.TotalMilliseconds}ms for creating {n} beams.");
            Console.WriteLine($"{GC.GetTotalMemory(true)}bytes allocated.");
        }
    }
}