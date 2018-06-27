using Hypar.Elements;
using Hypar.Geometry;
using System.IO;
using System.Linq;
using Xunit;

namespace Hypar.Tests
{
    public class BeamSystemTests
    {
        [Fact]
        public void BeamSystem()
        {
            var model = new Model();
            var profile = Profiles.WideFlangeProfile(1.0, 2.0, 0.1, 0.1);

            // Create the edge lines of the system.
            var l1 = Line.FromStart(new Vector3(0,0,0)).ToEnd(new Vector3(20,0,0));
            var l2 = Line.FromStart(new Vector3(0,20,0)).ToEnd(new Vector3(20,20,10));
            
            // Create points at n equal spaces along each edge.
            var v1 = Vector3.AtNEqualSpacesAlongLine(l1, 5);
            var v2 = Vector3.AtNEqualSpacesAlongLine(l2, 5);

            // Create lines spanning between those points.
            var cls = Line.FromStart(v1).ToEnd(v2);

            // Create beams along all of those lines.
            var beams = cls.AlongEachCreate<Beam>(l => {
                return Beam.AlongLine(l).WithProfile(profile);
            });

            model.AddElements(beams);
            Assert.Equal(6, model.Elements.Count);
        }
    }
}