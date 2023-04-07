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
            var linearBeam = new Beam(line, profile, 0, 0, 15, material: BuiltInMaterials.Wood);
            var lineT = line.TransformAt(0).ToModelCurves(linearBeam.Transform);

            // Create a polygon beam.
            var polygon = Polygon.Ngon(5, 2);
            var polygonBeam = new Beam(polygon, profile, 0, 0, 45, new Transform(6, 0, 0), BuiltInMaterials.Steel);
            var polyT = polygon.TransformAt(0).ToModelCurves(polygonBeam.Transform);

            // Create a curved beam.
            var arc = new Arc(new Transform(Vector3.Origin, Vector3.XAxis), 5.0, Math.PI * 0.25, Math.PI * 0.75);
            var arcBeam = new Beam(arc, profile, 0, 0, 0, new Transform(12, 0, 0), BuiltInMaterials.Steel);
            var arcT = arc.TransformAt(arc.Domain.Min).ToModelCurves(arcBeam.Transform);

            // Create an ellipse beam.
            var ellipticalArc = new EllipticalArc(Vector3.Origin, 2.5,1.5, 0, 210);
            var ellipticBeam = new Beam(ellipticalArc, profile, 0,0,0, new Transform(18,0,0), BuiltInMaterials.Steel);
            var ellipseT = ellipticalArc.TransformAt(ellipticalArc.Domain.Min).ToModelCurves(ellipticBeam.Transform);
            // </example>

            this.Model.AddElement(linearBeam);
            this.Model.AddElements(lineT);
            this.Model.AddElement(polygonBeam);
            this.Model.AddElements(polyT);
            this.Model.AddElement(arcBeam);
            this.Model.AddElements(arcT);
            this.Model.AddElement(ellipticBeam);
            this.Model.AddElements(ellipseT);
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

            BoundedCurve cl = null;
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

            var beam = new Beam(cl, this._testProfile, material: BuiltInMaterials.Steel) { StartSetback = startSetback, EndSetback = endSetback };
            Assert.Equal(BuiltInMaterials.Steel, beam.Material);
            Assert.Equal(cl, beam.Curve);

            this.Model.AddElement(beam);
        }

        [Fact]
        public void NonLinearVolumeException()
        {
            var cl = ModelTest.TestArc;
            var beam = new Beam(cl, this._testProfile, material: BuiltInMaterials.Steel);
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
                var beam = new Beam(line, profile, material: new Material(Guid.NewGuid().ToString(), color, 0.0f, 0.0f));
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
                var beam = new Beam(line, profile, material: m);
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
                var beam = new Beam(line, profile, material: new Material(Guid.NewGuid().ToString(), color, 0.0f, 0.0f));
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
                var beam = new Beam(line, profile, material: new Material(Guid.NewGuid().ToString(), color, 0.0f, 0.0f));
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
            var column = new Column(Vector3.Origin, 3.0, null, this._testProfile);
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
            var beam = new Beam(line, this._testProfile, 2, 2, 0, material: BuiltInMaterials.Steel);
            this.Model.AddElement(beam);

            var line1 = new Line(new Vector3(2, 0, 0), new Vector3(5, 3, 0));
            var mc1 = new ModelCurve(line1, BuiltInMaterials.XAxis);
            this.Model.AddElement(mc1);

            var sb = line1.Length() / 2;
            // Setbacks longer in total than the beam.
            // We are testing to ensure that the beam gets created
            // without throwing. It will not have setbacks.
            var beam1 = new Beam(line1, this._testProfile, sb, sb, 0, material: BuiltInMaterials.Steel);
            this.Model.AddElement(beam1);

            // Curve setbacks
            var arc = new Arc(new Vector3(5,0), 1.5, 0, 180);
            var mc2 = new ModelCurve(arc, BuiltInMaterials.XAxis);
            this.Model.AddElement(mc2);
            var curvedBeam = new Beam(arc, this._testProfile, 2, 2, 0, material: BuiltInMaterials.Steel);
            this.Model.AddElement(curvedBeam);
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
                var straightBeam = new Beam(line, profile, 0, 0, x * (360.0 / 5.0));
                this.Model.AddElement(straightBeam);
            }
        }

        [Fact, Trait("Category", "Examples")]
        public async void Joists()
        {
            Name = "Elements_Joist";

            // <joist-example>
            var xSpacing = 10.0;
            var yLength = 30;

            var profileFactory = new LProfileFactory();
            var profile8 = await profileFactory.GetProfileByTypeAsync(LProfileType.L8X8X5_8);
            var profile2 = await profileFactory.GetProfileByTypeAsync(LProfileType.L2X2X1_8);
            var profile5 = await profileFactory.GetProfileByTypeAsync(LProfileType.L5X5X1_2);

            var line = new Line(new Vector3(0, 0, 0), new Vector3(0, yLength, 0));
            var joist = new Joist(line,
                                  profile8,
                                  profile8,
                                  profile2,
                                  Units.InchesToMeters(48),
                                  20,
                                  Units.InchesToMeters(2.5),
                                  Units.FeetToMeters(2.0),
                                  BuiltInMaterials.Steel)
            {
                IsElementDefinition = true
            };


            var cl = new Line(new Vector3(0, 0, 0), new Vector3(xSpacing, 0, 0));
            var firstJoist = new Joist(cl,
                                       profile5,
                                       profile5,
                                       profile2,
                                       Units.InchesToMeters(24),
                                       10,
                                       Units.InchesToMeters(2.5),
                                       Units.FeetToMeters(1),
                                       BuiltInMaterials.Steel)
            {
                IsElementDefinition = true
            };

            for (var x = 0.0; x < 100.0; x += xSpacing)
            {
                var t = new Transform(new Vector3(x, 0, 4));
                var joistInstance = joist.CreateInstance(t, $"joist_girder_{x}");
                Model.AddElement(joistInstance);

                var joistPoints = joist.JoistPoints.Select(jp => t.OfPoint(jp)).ToList();
                for (var i = 0; i < joistPoints.Count; i++)
                {
                    var pt = joistPoints[i];
                    var innerJoistInstance = firstJoist.CreateInstance(new Transform(pt), $"joist_{x}_{i}");
                    Model.AddElement(innerJoistInstance);
                }
            }
            // </joist-example>
        }
    }
}