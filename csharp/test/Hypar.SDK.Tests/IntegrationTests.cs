using Hypar.Geometry;
using Hypar.Elements;
using System;
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

            var buildingHeight = 30;

            var profile = new Polyline(new[]{a,b,c,d});
            var mass = new Mass(profile, 0, profile, buildingHeight);
            
            var elevations = new List<double>();
            for(var i=0.0; i<=buildingHeight; i += 4.0)
            {
                elevations.Add(i);
            }

            var faces = mass.Faces();
            foreach (var f in faces)
            {
                var g = new Grid(f, 14, elevations.Count-1);
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

            var floors = mass.CreateFloors(elevations, 0.2, BuiltInMaterials.Concrete);
            model.AddElements(floors);

            var shaft = Profiles.Rectangular(new Vector3(10,10), 5, 5);
            var walls = shaft.Segments().Select(l=>{
                return new Wall(l, 0.1, buildingHeight, BuiltInMaterials.Concrete);
            });
            model.AddElements(walls);

            var offset = profile.Offset(1.5);
            var columns = offset.Segments().SelectMany(l=> {
                var ts = new []{0.5, 1.0};
                var sideColumns = new List<Column>();
                foreach(var t in ts)
                {
                    sideColumns.Add(new Column(l.PointAt(t), buildingHeight, Profiles.Rectangular(width:1.0, height:1.0), BuiltInMaterials.Concrete));
                }
                return sideColumns;
            });
            model.AddElements(columns);

            model.SaveGlb("massBuilding.glb");
        }
    }
}