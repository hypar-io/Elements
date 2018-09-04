using Hypar.Geometry;
using Hypar.Elements;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Xunit;

namespace Hypar.Tests
{
    public class IntegrationTests
    {
        [Fact]
        public void MassBuilding()
        {
            var model = new Model();
            var a = new Vector3();
            var b = new Vector3(30, 10);
            var c = new Vector3(20, 50);
            var d = new Vector3(-10, 5);

            var profile = new Polyline(new[]{a,b,c,d});
            var mass = new Mass(profile, 0, profile, 28);
            
            var faces = mass.Faces();
            foreach (var f in faces)
            {
                var g = new Grid(f, 7, 7);
                foreach(var cell in g.AllCells())
                {
                    var panel = new Panel(cell, BuiltInMaterials.Glass);
                    var bProfile = Profiles.WideFlangeProfile();
                    var beam1 = new Beam(cell.Segment(0), bProfile, BuiltInMaterials.Steel, panel.Normal);
                    var beam2 = new Beam(cell.Segment(2), bProfile, BuiltInMaterials.Steel, panel.Normal);
                    var beam3 = new Beam(cell.Segment(1), bProfile, BuiltInMaterials.Steel, panel.Normal);
                    model.AddElements(new Element[]{panel, beam1, beam2, beam3});
                }
            }

            var elevations = new double[]{0.0, 4.0, 8.0, 12.0, 16.0, 20.0, 24.0, 28.0};
            var floors = mass.CreateFloors(elevations, 0.2, BuiltInMaterials.Concrete);
            model.AddElements(floors);

            var shaft = Profiles.Rectangular(new Vector3(10,10), 5, 5);
            var walls = shaft.Segments().Select(l=>{
                return new Wall(l, 0.1, 32, BuiltInMaterials.Concrete);
            });
            model.AddElements(walls);

            model.SaveGlb("massBuilding.glb");
        }
    }
}