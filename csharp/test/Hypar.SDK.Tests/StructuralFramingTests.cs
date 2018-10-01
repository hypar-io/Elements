using Hypar.Elements;
using Hypar.Geometry;
using System;
using System.Linq;
using Xunit;

namespace Hypar.Tests
{
    public class BeamTests
    {
        [Fact]
        public void Example()
        {
            var line = new Line(Vector3.Origin, new Vector3(2,0,0));
            var beam = new Beam(line, WideFlangeProfileServer.Instance.GetProfileByName("W44x335"), BuiltInMaterials.Steel, null, 0.05, 0.05);
            var model = new Model();
            model.AddElement(beam);
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
            var b = new Beam(l, new WideFlangeProfile());
            Assert.Equal(BuiltInMaterials.Steel, b.Material);
            Assert.Equal(l, b.CenterLine);
        }

        [Fact]
        public void Construct_Column()
        {
            var c = new Column(Vector3.Origin, 10.0, new WideFlangeProfile());
            Assert.Equal(BuiltInMaterials.Steel, c.Material);
            Assert.Equal(10.0, c.CenterLine.Length);
        }

        [Fact]
        public void Construct_Brace()
        {
            var l = new Line(Vector3.Origin, new Vector3(5,5,5));
            var b = new Brace(l, new WideFlangeProfile());
            Assert.Equal(BuiltInMaterials.Steel, b.Material);
            Assert.Equal(l, b.CenterLine);
        }
    }
}