using Elements.Geometry;
using Elements.Geometry.Profiles;
using System.Collections.Generic;
using Xunit;

namespace Elements.Tests
{
    public class BuildingTests : ModelTest
    {
        public BuildingTests()
        {
            this.GenerateGlb = false;
            this.GenerateJson = false;
        }

        [Fact]
        public void Building()
        {
            this.Name = "Building";
            var elevations = new[]{0.0, 3.0, 6.0, 9.0, 12.0, 15.0};
            var opening = new Opening(5, 5, 2, 2);
            var site = Polygon.Ngon(6, 20.0);

            foreach(var el in elevations)
            {
                var slab = new Floor(site, 0.15, el, null, new List<Opening>(){opening});
                this.Model.AddElement(slab);
                var edgeBeams = slab.CreateEdgeBeams(WideFlangeProfileServer.Instance.GetProfileByName("W36x245"));
                this.Model.AddElements(edgeBeams);

                var segs = site.Segments();
                for (var i=0; i<segs.Length - 2; i++)
                {
                    var wall = new StandardWall(slab.Transform.OfLine(segs[i]), 0.1, 3.0);
                    this.Model.AddElement(wall);
                }
            }
        }
    }

    public static class BuildingExtensions
    {
        public static List<Beam> CreateEdgeBeams(this Floor floor, Profile beamProfile)
        {
            var beams = new List<Beam>();
            foreach(var s in floor.Profile.Perimeter.Segments())
            {
                var beam = new Beam(floor.Transform.OfLine(s), beamProfile, BuiltInMaterials.Steel, 0, 0);
                beams.Add(beam);
            }
            return beams;
        }
    }
}