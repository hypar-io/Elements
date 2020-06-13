using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Elements;
using Elements.Geometry;
using Hypar.Model;
using Serilog;

namespace Hypar.Revit
{
    internal class ExecutionRequestedEvent : RevitEventWrapper<Execution>
    {
        public ExecutionRequestedEvent(ILogger logger) : base(logger) { }

        public override void Execute(UIApplication app, Execution execution)
        {
            if (HyparHubApp.CurrentWorkflow == null)
            {
                return;
            }

            var functionInstance = HyparHubApp.CurrentWorkflow.FunctionInstances.FirstOrDefault(fi => fi.FunctionId == execution.FunctionId);
            if (functionInstance == null)
            {
                this.logger.Information("The execution's function instance could not be found in the current workflow.");
                return;
            }

            var groupId = CreateRevitGroupFromFunctionInstanceData(functionInstance.Id, this.logger, app);
            if (groupId == null)
            {
                this.logger.Information("No group was created.");
                return;
            }

            if (HyparHubApp.FunctionInstanceGroupCache.ContainsKey(functionInstance.Id))
            {
                using (Transaction transaction = new Transaction(app.ActiveUIDocument.Document, $"Hypar Cleanup"))
                {
                    transaction.Start();
                    try
                    {
                        logger.Debug("Deleting existing model group {Id}.", functionInstance.Id);
                        var group = (Group)app.ActiveUIDocument.Document.GetElement(HyparHubApp.FunctionInstanceGroupCache[functionInstance.Id]);
                        if (group != null)
                        {
                            logger.Debug("Deleting existing group and group type...");
                            app.ActiveUIDocument.Document.Delete(group.Id);
                            app.ActiveUIDocument.Document.Delete(group.GroupType.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Debug(ex.Message);
                        logger.Debug(ex.StackTrace);
                    }
                    transaction.Commit();
                }
                HyparHubApp.FunctionInstanceGroupCache[functionInstance.Id] = groupId;
            }
            else
            {
                HyparHubApp.FunctionInstanceGroupCache.Add(functionInstance.Id, groupId);
            }
        }

        private static ElementId CreateRevitGroupFromFunctionInstanceData(string functionInstanceId, ILogger logger, UIApplication app)
        {
            using (Transaction transaction = new Transaction(app.ActiveUIDocument.Document, $"Hypar"))
            {
                var modelPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $".hypar/workflows/{HyparHubApp.CurrentWorkflow.Id}/{functionInstanceId}/model.json");
                if (!File.Exists(modelPath))
                {
                    return null;
                }

                transaction.Start();

                var model = Elements.Model.FromJson(File.ReadAllText(modelPath));

                var revitElements = new List<Autodesk.Revit.DB.ElementId>();

                var floors = model.AllElementsOfType<Elements.Floor>();
                if (floors.Count() > 0)
                {
                    foreach (var floor in floors)
                    {
                        try
                        {
                            var edge = (Polygon)floor.Profile.Perimeter.Transformed(floor.Transform);
                            var f = floor.Transform.ToFrame(app.Application);
                            var p = Autodesk.Revit.DB.Plane.Create(f);
                            var sp = SketchPlane.Create(app.ActiveUIDocument.Document, p);
                            foreach (var s in edge.Segments())
                            {
                                var mc = app.ActiveUIDocument.Document.Create.NewModelCurve(s.ToRevitLine(app.Application), sp);
                                revitElements.Add(mc.Id);
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }

                var beams = model.AllElementsOfType<ElementInstance>().Where(e => e.BaseDefinition.GetType() == typeof(Elements.Beam));
                if (beams.Count() > 0)
                {
                    foreach (var beam in beams)
                    {
                        try
                        {
                            var beamBase = (Elements.Beam)beam.BaseDefinition;
                            var beamCurve = (Elements.Geometry.Line)((Elements.Geometry.Line)beamBase.Curve).Transformed(beam.Transform);
                            var bt = beamCurve.TransformAt(0);
                            var f = new Autodesk.Revit.DB.Frame(bt.Origin.ToXYZ(app.Application), bt.XAxis.ToXYZ(app.Application).Normalize(), bt.ZAxis.ToXYZ(app.Application).Normalize(), bt.YAxis.ToXYZ(app.Application).Normalize());
                            var p = Autodesk.Revit.DB.Plane.Create(f);
                            var sp = SketchPlane.Create(app.ActiveUIDocument.Document, p);
                            var mc = app.ActiveUIDocument.Document.Create.NewModelCurve(beamCurve.ToRevitLine(app.Application), sp);
                            revitElements.Add(mc.Id);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }

                var columns = model.AllElementsOfType<ElementInstance>().Where(e => e.BaseDefinition.GetType() == typeof(Elements.Column));
                if (columns.Count() > 0)
                {
                    foreach (var column in columns)
                    {
                        try
                        {
                            var baseColumn = (Elements.Column)column.BaseDefinition;
                            var colCurve = (Elements.Geometry.Line)((Elements.Geometry.Line)baseColumn.Curve).Transformed(column.Transform);
                            var bt = colCurve.TransformAt(0);
                            var f = new Autodesk.Revit.DB.Frame(bt.Origin.ToXYZ(app.Application), bt.XAxis.ToXYZ(app.Application).Normalize(), bt.ZAxis.ToXYZ(app.Application).Normalize(), bt.YAxis.ToXYZ(app.Application).Normalize());
                            var p = Autodesk.Revit.DB.Plane.Create(f);
                            var sp = SketchPlane.Create(app.ActiveUIDocument.Document, p);
                            var mc = app.ActiveUIDocument.Document.Create.NewModelCurve(colCurve.ToRevitLine(app.Application), sp);
                            revitElements.Add(mc.Id);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }

                if (revitElements.Count == 0)
                {
                    transaction.Commit();
                    return null;
                }

                var group = app.ActiveUIDocument.Document.Create.NewGroup(revitElements);

                transaction.Commit();

                return group.Id;
            }
        }
    }
}