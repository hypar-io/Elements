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
            Line, Polyline, Polygon, Arc, Circle
        }

        private WideFlangeProfileFactory _wideFlangeFactory = new WideFlangeProfileFactory();
        private HSSPipeProfileFactory _hssFactory = new HSSPipeProfileFactory();
        private RHSProfileFactory _rhsFactory = new RHSProfileFactory();
        private SHSProfileFactory _shsFactory = new SHSProfileFactory();

        private WideFlangeProfile _testProfile;

        public StructuralFramingTests()
        {
            _testProfile = _wideFlangeFactory.GetProfileByType(WideFlangeProfileType.W10x100);
        }

        [Fact, Trait("Category", "Examples")]
        public void BeamExample()
        {
            this.Name = "Elements_Beam";

            // <example>
            // Create a framing type.
            var profile = _wideFlangeFactory.GetProfileByType(WideFlangeProfileType.W10x100);

            // Create a straight beam.
            var line = new Line(Vector3.Origin, new Vector3(5, 0, 5));
            var linearBeam = new Beam(line, profile, BuiltInMaterials.Wood, 0, 0, 15);
            var lineT = line.TransformAt(0).ToModelCurves(linearBeam.Transform);

            // Create a polygon beam.
            var polygon = Polygon.Ngon(5, 2);
            var polygonBeam = new Beam(polygon, profile, BuiltInMaterials.Steel, 0, 0, 45.0, new Transform(6, 0, 0));
            var polyT = polygon.TransformAt(0).ToModelCurves(polygonBeam.Transform);

            // Create a curved beam.
            var arc = new Arc(Vector3.Origin, 5.0, 45.0, 135.0);
            var arcBeam = new Beam(arc, profile, BuiltInMaterials.Steel, 0, 0, 45.0, new Transform(12, 0, 0));
            var arcT = arc.TransformAt(0).ToModelCurves(arcBeam.Transform);
            // </example>

            this.Model.AddElement(linearBeam);
            this.Model.AddElements(lineT);
            this.Model.AddElement(polygonBeam);
            this.Model.AddElements(polyT);
            this.Model.AddElement(arcBeam);
            this.Model.AddElements(arcT);
        }

        [Theory]
        [InlineData("LinearBeam", BeamType.Line, 0.25, 0.25)]
        [InlineData("PolylineBeam", BeamType.Polyline, 0.25, 0.25)]
        [InlineData("PolygonBeam", BeamType.Polygon, 0.25, 0.25)]
        [InlineData("ArcBeam", BeamType.Arc, 0.25, 0.25)]
        [InlineData("CircleBeam", BeamType.Circle, 3.0, 3.0)]
        public void Beam(string testName, BeamType beamType, double startSetback, double endSetback)
        {
            this.Name = testName;

            Curve cl = null;
            switch (beamType)
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
                case BeamType.Circle:
                    cl = ModelTest.TestCircle;
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
            Assert.Throws<InvalidOperationException>(() => beam.Volume());
        }

        [Fact]
        public void WideFlange()
        {
            var sw = new Stopwatch();
            sw.Start();

            this.Name = "WideFlange";

            var x = 0.0;
            var z = 0.0;
            var profiles = _wideFlangeFactory.AllProfiles().ToList();
            foreach (var profile in profiles)
            {
                var color = new Color((float)(x / 20.0), (float)(z / profiles.Count), 0.0f, 1.0f);
                var line = new Line(new Vector3(x, 0, z), new Vector3(x + 1, 3, z));
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
            var profiles = _hssFactory.AllProfiles().ToList();
            foreach (var profile in profiles)
            {
                var color = new Color((float)(x / 20.0), (float)(z / profiles.Count), 0.0f, 1.0f);
                var line = new Line(new Vector3(x, 0, z), new Vector3(x, 3, z));
                var m = new Material(Guid.NewGuid().ToString(), color, 0.1f, 0.5f);
                this.Model.AddElement(m);
                var beam = new Beam(line, profile, m);
                this.Model.AddElement(beam, false);
                x += 2.0;
                if (x > 20.0)
                {
                    z += 2.0;
                    x = 0.0;
                }
            }
        }

        [Fact]
        public void RHS()
        {
            this.Name = "RHS";
            var x = 0.0;
            var z = 0.0;
            var profiles = _rhsFactory.AllProfiles().ToList();
            foreach (var profile in profiles)
            {
                var color = new Color((float)(x / 20.0), (float)(z / profiles.Count), 0.0f, 1.0f);
                var line = new Line(new Vector3(x, 0, z), new Vector3(x, 3, z));
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
        public void SHS()
        {
            this.Name = "SHS";
            var x = 0.0;
            var z = 0.0;
            var profiles = _shsFactory.AllProfiles().ToList();
            foreach (var profile in profiles)
            {
                var color = new Color((float)(x / 20.0), (float)(z / profiles.Count), 0.0f, 1.0f);
                var line = new Line(new Vector3(x, 0, z), new Vector3(x, 3, z));
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
        public void GettingProfileTypeThatDoesntExistThrowsException()
        {
            Assert.Throws<ArgumentException>(() => _rhsFactory.GetProfileByName("foo"));
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
            var line = new Line(Vector3.Origin, new Vector3(3, 3, 3));
            var brace = new Brace(line, this._testProfile, BuiltInMaterials.Steel);
            Assert.Equal(BuiltInMaterials.Steel, brace.Material);
            Assert.Equal(line, brace.Curve);
            this.Model.AddElement(brace);
        }

        [Fact]
        public void Setbacks()
        {
            this.Name = "BeamSetbacks";
            var line = new Line(Vector3.Origin, new Vector3(3, 3, 0));
            var mc = new ModelCurve(line, BuiltInMaterials.XAxis);
            this.Model.AddElement(mc);
            // Normal setbacks
            var beam = new Beam(line, this._testProfile, BuiltInMaterials.Steel, 2.0, 2.0);
            this.Model.AddElement(beam);

            var line1 = new Line(new Vector3(2, 0, 0), new Vector3(5, 3, 0));
            var mc1 = new ModelCurve(line1, BuiltInMaterials.XAxis);
            this.Model.AddElement(mc1);

            var sb = line1.Length() / 2;
            // Setbacks longer in total than the beam.
            // We are testing to ensure that the beam gets created
            // without throwing. It will not have setbacks.
            var beam1 = new Beam(line1, this._testProfile, BuiltInMaterials.Steel, sb, sb);
            this.Model.AddElement(beam1);
        }

        [Fact(Skip = "Benchmark")]
        public void Benchmark()
        {
            var sw = new Stopwatch();
            sw.Start();

            var x = 0.0;
            var z = 0.0;
            var profile = _wideFlangeFactory.AllProfiles().First();
            var n = 100000;
            for (var i = 0; i < n; i++)
            {
                var line = new Line(new Vector3(x, 0, z), new Vector3(x, 3, z));
                var beam = new Beam(line, profile, BuiltInMaterials.Steel);
                beam.UpdateRepresentations();
                var mesh = beam.Representation.SolidOperations.First().Solid.Tessellate();
                x += 2.0;
                if (x > 20.0)
                {
                    z += 2.0;
                    x = 0.0;
                }
            }

            sw.Stop();
            Console.WriteLine($"{sw.Elapsed.TotalMilliseconds} ms for creating {n} beams.");
            Console.WriteLine($"{GC.GetTotalMemory(true)} bytes allocated.");
        }

        [Fact]
        public void SweepsBecomeLinearSegments()
        {
            this.Name = "SweptBeam";
            var circle = new Circle(5.0);
            var profile = _wideFlangeFactory.GetProfileByType(WideFlangeProfileType.W12x106);
            var beam = new Beam(circle, profile);
            this.Model.AddElement(beam);

            var pline = circle.ToPolygon();
            this.Model.AddElement(new Mass(pline));

            for (var x = 0.0; x <= 5.0; x += 1.0)
            {
                var line = new Line(new Vector3(x, 0, 0), new Vector3(x, 5, x));
                var straightBeam = new Beam(line, profile, rotation: x * (360.0 / 5.0));
                this.Model.AddElement(straightBeam);
            }
        }
    }
}