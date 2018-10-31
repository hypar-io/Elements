using Autodesk.Revit;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.Attributes;
using DesignAutomationFramework;
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
using FloorType = Hypar.Elements.FloorType;
using WallType = Hypar.Elements.WallType;

namespace Hypar.Revit
{
    internal class Execution
    {
        [JsonProperty("id")]
        public string Id{get;set;}

        [JsonProperty("model")]
        public Model Model{get;set;}

        public Execution(string id, Model model)
        {
            this.Id = id;
            this.Model = model;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class HyparRevitApp : IExternalDBApplication
    {
        public void CreateHyparModel(DesignAutomationData data)
        {   
            #if !DEBUG
            var exec = ParseExecution("execution.json");
            #else
            var exec = ParseExecution(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "execution.json"));
            #endif
            var model = exec.Model;

            var beamSymbol = GetStructuralSymbol(data.RevitDoc, BuiltInCategory.OST_StructuralFraming, "200UB25.4");
            var columnSymbol = GetStructuralSymbol(data.RevitDoc, BuiltInCategory.OST_StructuralColumns, "450 x 450mm");
            var floorType = GetFloorType(data.RevitDoc, "Insitu Concrete");
            var fec = new FilteredElementCollector(data.RevitDoc);
            var levels = fec.OfClass(typeof(Level)).Cast<Level>().ToList();

            Transaction trans = new Transaction(data.RevitDoc);
            trans.Start("Hypar");

            CreateBeams(model, beamSymbol, levels, data.RevitDoc, data.RevitApp);
            CreateWalls(model, levels, data.RevitDoc, data.RevitApp);
            CreateColumns(model, columnSymbol, levels, data.RevitDoc, data.RevitApp);
            CreateFloors(model, floorType, levels, data.RevitDoc, data.RevitApp);

            trans.Commit();

            #if !DEBUG
            var path = ModelPathUtils.ConvertUserVisiblePathToModelPath($"{exec.Id}.rvt");
            data.RevitDoc.SaveAs(path, new SaveAsOptions());
            #else
            data.RevitDoc.SaveAs(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"{exec.Id}.rvt"));
            #endif
        }

        private Execution ParseExecution(string jsonPath)
        {
            try
            {
                var json = File.ReadAllText(jsonPath);
                var dict = JsonConvert.DeserializeObject<Dictionary<string,string>>(json);
                return new Execution(dict["id"], Model.FromJson(dict["model"]));
            }
            catch(Exception ex)
            {
                Console.WriteLine($"The execution.json file could not be parsed: {ex.Message}");
                return null;
            }
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

        private Autodesk.Revit.DB.FloorType GetFloorType(Document doc, string name)
        {
            var fec = new Autodesk.Revit.DB.FilteredElementCollector(doc);
            var floorType = fec.OfClass(typeof(Autodesk.Revit.DB.FloorType))
                            .Cast<Autodesk.Revit.DB.FloorType>()
                            .First(x => x.Name == name);
            if(floorType == null)
            {
                throw new Exception($"The floor type, {name}, could not be found.");
            }
            return floorType;
        }

        private void CreateBeams(Model model, FamilySymbol symbol, IList<Level> levels, Document doc, Application app)
        {
            var hyparBeams = model.ElementsOfType<Hypar.Elements.Beam>();
            foreach(var b in hyparBeams)
            {
                var level = FindClosestLevel(b.CenterLine.Start.Z.ToFeet(), levels);
                doc.Create.NewFamilyInstance(b.CenterLine.ToRevitLine(app), symbol, level, StructuralType.Beam);
            }
        }

        private void CreateWalls(Model model, IList<Level> levels, Document doc, Application app)
        {
            var hyparWalls = model.ElementsOfType<Hypar.Elements.Wall>();
            foreach(var w in hyparWalls)
            {
                var level = FindClosestLevel(w.CenterLine.Start.Z.ToFeet(), levels);
                var curve = w.CenterLine.ToRevitLine(app);
                var wall = Autodesk.Revit.DB.Wall.Create(doc, curve, level.Id, true);
            }
        }

        private void CreateColumns(Model model, FamilySymbol symbol, IList<Level> levels, Document doc, Application app)
        {
            var hyparColumns = model.ElementsOfType<Column>();
            foreach(var c in hyparColumns)
            {
                var l = FindClosestLevel(c.CenterLine.Start.ToXYZ(app).Z, levels);
                doc.Create.NewFamilyInstance(c.CenterLine.Start.ToXYZ(app), symbol, l, StructuralType.Column);
            } 
        }

        private void CreateFloors(Model model, Autodesk.Revit.DB.FloorType floorType, IList<Level> levels, Document doc, Application app)
        {
            var hyparFloors = model.ElementsOfType<Floor>();
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

        public ExternalDBApplicationResult OnStartup(ControlledApplication application)
        {
            #if DEBUG
            application.ApplicationInitialized += HandleApplicationInitializedEvent;
            #else
            DesignAutomationBridge.DesignAutomationReadyEvent += HandleDesignAutomationReadyEvent;
            #endif

            return ExternalDBApplicationResult.Succeeded;
        }

        public void HandleDesignAutomationReadyEvent(object sender, DesignAutomationReadyEventArgs e)
        {
            CreateHyparModel(e.DesignAutomationData);
            e.Succeeded = true;
        }

        #if DEBUG
        public void HandleApplicationInitializedEvent(object sender, Autodesk.Revit.DB.Events.ApplicationInitializedEventArgs e)
        {
            Autodesk.Revit.ApplicationServices.Application app = sender as Autodesk.Revit.ApplicationServices.Application;
            DesignAutomationData data = new DesignAutomationData(app, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "sdk/csharp/src/revit/HyparRevitApp/hypar.rvt"));
            CreateHyparModel(data);
        }
        #endif

        public ExternalDBApplicationResult OnShutdown(ControlledApplication application)
        {
            return ExternalDBApplicationResult.Succeeded;
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

        public static Autodesk.Revit.DB.Line ToRevitLine(this ICurve line, Application app)
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
