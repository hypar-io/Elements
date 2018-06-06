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
            var bottom = Line.FromStart(new Vector3(0,0,0)).ToEnd(new Vector3(20,0,0));
            var top = Line.FromStart(new Vector3(0,0,30)).ToEnd(new Vector3(20,10,30));

            var grid = Grid.WithinPerimeter(bottom, top, 5, 5);
            var profile = Profiles.WideFlangeProfile(0.5,0.5, 0.1,0.1, 0.25);

            var model = new Model();
            foreach(var row in grid.Cells)
            {
                foreach(var c in row)
                {   
                    var panel = Panel.WithinPerimeter(c.Perimeter)
                                    .OfMaterial(BuiltIntMaterials.Glass);

                    var beam1 = Beam.AlongLine(c.Perimeter.Segment(0))
                                    .WithProfile(profile)
                                    .WithUpAxis(panel.Normal)
                                    .OfMaterial(BuiltIntMaterials.Steel);
                                    
                    var beam2 = Beam.AlongLine(c.Perimeter.Segment(2))
                                    .WithProfile(profile)
                                    .WithUpAxis(panel.Normal)
                                    .OfMaterial(BuiltIntMaterials.Steel);
                                    
                    var beam3 = Beam.AlongLine(c.Perimeter.Segment(1))
                                    .WithProfile(profile)
                                    .WithUpAxis(panel.Normal)
                                    .OfMaterial(BuiltIntMaterials.Steel);
                    
                    model.AddElements(new Element[]{panel, beam1, beam2, beam3});
                }
            }
            Assert.Equal(100, model.Elements.Count);
        }
    }
}