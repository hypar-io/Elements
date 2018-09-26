using Autodesk.Revit;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Hypar.Elements;
using Hypar.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;
using Line = Hypar.Geometry.Line;
using Wall = Hypar.Elements.Wall;
using Beam = Hypar.Elements.Beam;

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

            var symbol = GetStructuralSymbol(doc);
            var fec = new FilteredElementCollector(doc);
            var levels = fec.OfClass(typeof(Level)).Cast<Level>().ToList();

            var model = CreateTestModel();

            Transaction trans = new Transaction(doc);
            trans.Start("Hypar");

            CreateBeams(model, symbol, levels, doc, app);
            CreateWalls(model, levels, doc, app);
            CreateColumns(model, symbol, levels, doc, app);

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

            return model;
        }

        private FamilySymbol GetStructuralSymbol(Document doc)
        {
            var fec = new Autodesk.Revit.DB.FilteredElementCollector(doc);
            var familyName = "UB-Universal Beams (AS 3679_1)";
            var family = fec.OfClass(typeof(Autodesk.Revit.DB.Family))
                .Cast<Autodesk.Revit.DB.Family>()
                .FirstOrDefault(x => x.Name == familyName);
            if (family == null)
            {
                throw new Exception($"The family, {familyName}, could not be found.");
            }
            var symbol = (FamilySymbol)doc.GetElement(family.GetFamilySymbolIds().First());
            return symbol;
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
                doc.Create.NewFamilyInstance(c.CenterLine.ToRevitLine(app), symbol, null, StructuralType.Column);
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
    }
}
