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

            var pline =new Polyline(new[]{new Vector3(0,0,0), new Vector3(20,0,0), new Vector3(20,10,30), new Vector3(0,0,30)});
            var grid = ElementsFactory.CreateGrid()
                                        .WithinPerimeter(pline)
                                        .WithUDivisions(5)
                                        .WithVDivisions(5);
                                        
            var profile = Profiles.WideFlangeProfile(0.5, 0.5, 0.1, 0.1, Profiles.VerticalAlignment.Center);

            var model = new Model();
            foreach(var c in grid.Cells())
            {
                var panel = Panel.WithinPerimeter(c)
                                .OfMaterial(BuiltIntMaterials.Glass);

                var beam1 = ElementsFactory.CreateBeam()
                                .AlongLine(c.Segment(0))
                                .WithProfile(profile)
                                .WithUpAxis(panel.Normal)
                                .OfMaterial(BuiltIntMaterials.Steel);
                                
                var beam2 = ElementsFactory.CreateBeam()
                                .AlongLine(c.Segment(2))
                                .WithProfile(profile)
                                .WithUpAxis(panel.Normal)
                                .OfMaterial(BuiltIntMaterials.Steel);
                                
                var beam3 = ElementsFactory.CreateBeam()
                                .AlongLine(c.Segment(1))
                                .WithProfile(profile)
                                .WithUpAxis(panel.Normal)
                                .OfMaterial(BuiltIntMaterials.Steel);
                
                model.AddElements(new Element[]{panel, beam1, beam2, beam3});
            }
            Assert.Equal(100, model.Elements.Count);
        }
    }
}