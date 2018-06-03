using Hypar.Geometry;
using Hypar.Elements;
using System.IO;
using Xunit;

namespace Hypar.Tests
{
    public class GridTests
    {
        [Fact]
        public void Grid()
        {
            var model = new Model();
            var bottom = new Line(new Vector3(0,0,0), new Vector3(20,0,0));
            var top = new Line(new Vector3(0,0,30), new Vector3(20,10,30));
            var grid = new Grid(bottom, top, 5, 5);
            var profile = new WideFlangeProfile(0.5,0.5, 0.1,0.1, 0.25);
            var steel = model.Materials[BuiltInMaterials.STEEL];

            foreach(var row in grid.Cells)
            {
                foreach(var c in row)
                {   
                    var panel = new Panel(c.Perimeter, model.Materials[BuiltInMaterials.GLASS]);
                    model.AddElement(panel);
                    var beam1 = new Beam(c.Perimeter.Segment(0), profile, steel, panel.Normal);
                    var beam2 = new Beam(c.Perimeter.Segment(2), profile, steel, panel.Normal);
                    var beam3 = new Beam(c.Perimeter.Segment(1), profile, steel, panel.Normal);
                    model.AddElement(beam1);
                    model.AddElement(beam2);
                    model.AddElement(beam3);
                }
            }
            model.SaveGlb("grid.glb");
            Assert.True(File.Exists("grid.glb"));
            Assert.Equal(100, model.Elements.Count);
        }
    }
}