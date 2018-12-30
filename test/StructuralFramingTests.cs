using Elements;
using Elements.Geometry;
using System;
using System.Linq;
using Xunit;

namespace Hypar.Tests
{
    public class StructuralFramingTests
    {
        [Fact]
        public void Example()
        {
            var section = new WideFlangeProfile("test", 1.0, 2.0, 0.1, 0.1);

            var line = new Line(new Vector3(1,1,1), new Vector3(2,2,2));
            var beam = new Beam(line, section, BuiltInMaterials.Steel, null);
            
            var arc = new Arc(new Vector3(2,0,0), 5.0, 0.0, 45.0);
            var curvedBeam = new Beam(arc, section, BuiltInMaterials.Steel, null);

            var circularSection = new Profile(Polygon.Circle(0.5), Polygon.Circle(0.25));
            var pline = new Polyline(new []{new Vector3(1,0), new Vector3(1,2), new Vector3(0,3,1)});
            var plineBeam = new Beam(pline, circularSection, BuiltInMaterials.Steel, null);

            var ngon = Polygon.Ngon(5, 2);
            var ngonBeam = new Beam(ngon, section, BuiltInMaterials.Steel, null);

            var model = new Model();
            model.AddElement(beam);
            model.AddElement(curvedBeam);
            model.AddElement(plineBeam);
            // model.AddElement(ngonBeam);
            model.SaveGlb("beam.glb");
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
        public void Construct_Beam()
        {
            var l = new Line(Vector3.Origin, new Vector3(5,5,5));
            var b = new Beam(l, new WideFlangeProfile("test"));
            Assert.Equal(BuiltInMaterials.Steel, b.Material);
            Assert.Equal(l, b.Curve);
        }

        [Fact]
        public void Construct_Column()
        {
            var c = new Column(Vector3.Origin, 10.0, new WideFlangeProfile("test"));
            Assert.Equal(BuiltInMaterials.Steel, c.Material);
            Assert.Equal(10.0, c.Curve.Length());
        }

        [Fact]
        public void Construct_Brace()
        {
            var l = new Line(Vector3.Origin, new Vector3(5,5,5));
            var b = new Brace(l, new WideFlangeProfile("test"));
            Assert.Equal(BuiltInMaterials.Steel, b.Material);
            Assert.Equal(l, b.Curve);
        }
    }
}