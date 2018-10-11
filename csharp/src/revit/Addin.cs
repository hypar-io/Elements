using Autodesk.Revit;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Hypar.Elements;
using Hypar.Geometry;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Line = Hypar.Geometry.Line;
using Wall = Hypar.Elements.Wall;
using Beam = Hypar.Elements.Beam;
using Floor = Hypar.Elements.Floor;
using Profile = Hypar.Geometry.Profile;

namespace Hypar.Revit
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Addin : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var doc = uiapp.ActiveUIDocument.Document;
            var app = doc.Application;

            var json = File.ReadAllText(@"C:\Users\Ian\Downloads\8a803288-5c13-46de-a2b7-c591f7cf31a3_elements\elements.json");
            var model = Model.FromJson(json);

            var beamSymbol = GetStructuralSymbol(doc, BuiltInCategory.OST_StructuralFraming, "200UB25.4");
            var columnSymbol = GetStructuralSymbol(doc, BuiltInCategory.OST_StructuralColumns, "450 x 450mm");
            var floorType = GetFloorType(doc, "Insitu Concrete");
            var fec = new FilteredElementCollector(doc);
            var levels = fec.OfClass(typeof(Level)).Cast<Level>().ToList();

            // var model = CreateTestModel();

            Transaction trans = new Transaction(doc);
            trans.Start("Hypar");

            CreateBeams(model, beamSymbol, levels, doc, app);
            CreateWalls(model, levels, doc, app);
            CreateColumns(model, columnSymbol, levels, doc, app);
            CreateFloors(model, floorType, levels, doc, app);

            trans.Commit();
            
            return Result.Succeeded;
        }

        private Model CreateTestModel()
        {
            var model = new Model();
            var line = new Line(Vector3.Origin, new Vector3(5,5,5));
            var beam = new Beam(line, new WideFlangeProfile(), BuiltInMaterials.Steel);
            model.AddElement(beam);

            var wallLine = new Line(new Vector3(10,0,0), new Vector3(15,10,0));
            var wall = new Wall(wallLine, 0.2, 5.0, BuiltInMaterials.Concrete);
            model.AddElement(wall);

            var column = new Column(Vector3.Origin, 5.0, new WideFlangeProfile(), BuiltInMaterials.Steel);
            model.AddElement(column);

            var floor = new Floor(Polygon.Rectangle(Vector3.Origin, 10, 10), 5.0, 0.2, BuiltInMaterials.Concrete);
            model.AddElement(floor);

            return model;
        }

        private FamilySymbol GetStructuralSymbol(Document doc, BuiltInCategory category, string symbolName)
        {
            var fec = new Autodesk.Revit.DB.FilteredElementCollector(doc);
            var symbol = fec.OfCategory(category)
                .OfClass(typeof(Autodesk.Revit.DB.FamilySymbol))
                .Cast<Autodesk.Revit.DB.FamilySymbol>()
                .FirstOrDefault(x => x.Name == symbolName);
            if (symbol == null)
            {
                throw new Exception($"The family symbol, {symbolName}, could not be found.");
            }
            return symbol;
        }

        private FloorType GetFloorType(Document doc, string name)
        {
            var fec = new Autodesk.Revit.DB.FilteredElementCollector(doc);
            var floorType = fec.OfClass(typeof(Autodesk.Revit.DB.FloorType))
                            .Cast<FloorType>()
                            .First(x => x.Name == name);
            if(floorType == null)
            {
                throw new Exception($"The floor type, {name}, could not be found.");
            }
            return floorType;
        }

        private void CreateBeams(Model model, FamilySymbol symbol, IList<Level> levels, Document doc, Application app)
        {
            var hyparBeams = model.Values.OfType<Hypar.Elements.Beam>();
            foreach(var b in hyparBeams)
            {
                var level = FindClosestLevel(b.CenterLine.Start.Z.ToFeet(), levels);
                doc.Create.NewFamilyInstance(b.CenterLine.ToRevitLine(app), symbol, level, StructuralType.Beam);
            }
        }

        private void CreateWalls(Model model, IList<Level> levels, Document doc, Application app)
        {
            var hyparWalls = model.Values.OfType<Hypar.Elements.Wall>();
            foreach(var w in hyparWalls)
            {
                var level = FindClosestLevel(w.CenterLine.Start.Z.ToFeet(), levels);
                var curve = w.CenterLine.ToRevitLine(app);
                var wall = Autodesk.Revit.DB.Wall.Create(doc, curve, level.Id, true);
            }
        }

        private void CreateColumns(Model model, FamilySymbol symbol, IList<Level> levels, Document doc, Application app)
        {
            var hyparColumns = model.Values.OfType<Column>();
            foreach(var c in hyparColumns)
            {
                var l = FindClosestLevel(c.CenterLine.Start.ToXYZ(app).Z, levels);
                doc.Create.NewFamilyInstance(c.CenterLine.Start.ToXYZ(app), symbol, l, StructuralType.Column);
            } 
        }

        private void CreateFloors(Model model, FloorType floorType, IList<Level> levels, Document doc, Application app)
        {
            var hyparFloors = model.Values.OfType<Floor>();
            foreach(var f in hyparFloors)
            {
                var l = FindClosestLevel(f.Elevation.ToFeet(), levels);
                doc.Create.NewFloor(f.Profile.ToRevitCurveArray(app), floorType, l, false);
            }
        }

        private Level FindClosestLevel(double elevation, IList<Level> levels)
        {
            var delta = double.PositiveInfinity;
            Level level = null;
            foreach(var l in levels)
            {
                var currDelta = Math.Abs(elevation - l.Elevation);
                if(currDelta < delta)
                {
                    delta = currDelta;
                    level = l;
                }
            }
            return level;
        }
    }

    public static class RevitGeometryExtensions
    {
        public static double ToFeet(this double meters)
        {
            return meters * 3.28084;
        }

        public static XYZ ToXYZ(this Hypar.Geometry.Vector3 vector, Application app)
        {
            return app.Create.NewXYZ(vector.X.ToFeet(), vector.Y.ToFeet(), vector.Z.ToFeet());
        }

        public static Autodesk.Revit.DB.Line ToRevitLine(this Hypar.Geometry.Line line, Application app)
        {
            return Autodesk.Revit.DB.Line.CreateBound(line.Start.ToXYZ(app), line.End.ToXYZ(app));
        }

        public static CurveArray ToRevitCurveArray(this Profile profile, Application app)
        {
            var curveArr = profile.Perimeter.ToRevitCurveArray(app);
            return curveArr;
        }

        public static CurveArray ToRevitCurveArray(this Polygon polygon, Application app)
        {
            var curveArr = new CurveArray();
            foreach(var l in polygon.Segments())
            {
                curveArr.Append(l.ToRevitLine(app));
            }
            return curveArr; 
        }
    }
}
