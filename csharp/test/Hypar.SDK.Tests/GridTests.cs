using Hypar.Geometry;
using Hypar.Elements;
using System.IO;
using Xunit;

namespace Hypar.Tests
{
    public class GridTests
    {
        [Fact]
        public void ValidValues_Construct_Success()
        {

            var pline =new Polyline(new[]{new Vector3(0,0,0), new Vector3(20,0,0), new Vector3(20,10,30), new Vector3(0,0,30)});
            var grid = new Grid(pline, 5, 5);
                                        
            var profile = Profiles.WideFlangeProfile(0.5, 0.5, 0.1, 0.1, Profiles.VerticalAlignment.Center);

            var model = new Model();
            foreach(var c in grid.AllCells())
            {
                var panel = new Panel(c,BuiltInMaterials.Glass);

                var beam1 = new Beam(c.Segment(0), profile, BuiltInMaterials.Steel, panel.Normal);
                var beam2 = new Beam(c.Segment(2), profile, BuiltInMaterials.Steel, panel.Normal);
                var beam3 = new Beam(c.Segment(1), profile, BuiltInMaterials.Steel, panel.Normal);
                
                model.AddElements(new Element[]{panel, beam1, beam2, beam3});
            }
            Assert.Equal(100, model.Count);
            model.SaveGlb("gridTests.glb");
        }
    }
}