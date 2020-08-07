using System;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ADSK = Autodesk.Revit.DB;
using Hypar.Revit.Converters;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

namespace Hypar.Revit
{
    public static class ExportToModel
    {
        public static void ConvertSelectedElements(Document doc, ICollection<ElementId> elementIds)
        {
            var toConvert = ConversionRunner.AllCategories.SelectMany(category => GetElements(doc, category, null, elementIds));

            var model = ConvertElements(doc, toConvert.ToArray(), out List<ExportException> conversionExceptions);
            SaveModel(model);
            FinishAndShowResults(conversionExceptions);
        }

        public static void ConvertAll(Document doc)
        {
            var model = new Elements.Model();
            var levels = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().ToElements().Cast<Level>();
            var allConversionErrors = new List<ExportException>();
            foreach (var level in levels)
            {
                var toConvert = ConversionRunner.AllCategories.SelectMany(category => GetElements(doc, category, null, null, level));

                var elements = ConvertElements(doc, toConvert.ToArray(), out List<ExportException> conversionExceptions);
                var levelElements = new Elements.LevelElements(elements.Elements.Values.ToList(), Guid.NewGuid(), level.Name);
                allConversionErrors.AddRange(conversionExceptions);

                model.AddElement(levelElements);
            }
            SaveModel(model);
            FinishAndShowResults(allConversionErrors);
        }

        public static void ConvertView(Document doc, View view)
        {
            var toConvert = ConversionRunner.AllCategories.SelectMany(category => GetElements(doc, category, view, null));

            var model = ConvertElements(doc, toConvert.ToArray(), out List<ExportException> conversionExceptions);
            SaveModel(model);
            FinishAndShowResults(conversionExceptions);
        }

        private static Elements.Model ConvertElements(Document doc, Element[] toConvert, out List<ExportException> exceptions)
        {
            var model = ConversionRunner.RunConverters(toConvert, doc, out List<ExportException> conversionExceptions);
            exceptions = conversionExceptions;

            return model;
        }

        private static void FinishAndShowResults(List<ExportException> conversionExceptions)
        {
            if (conversionExceptions.Count > 0)
            {
                // TODO this is acceptable messaging for debugging, but we'll want to provide different message and do logging before release
                var exceptionMessage = String.Join("\n", conversionExceptions.Select(e => $"{e.ElementId.IntegerValue} - {e.Exception.InnerException?.Message}"));
                var dialog = new TaskDialog("Hypar Errors");
                dialog.MainInstruction = "Export completed, but there were some exceptions.  You may be able to ignore some of these";
                dialog.MainContent = exceptionMessage;
                dialog.Show();
            }
            else
            {
                TaskDialog.Show("Export Complete", "Exporting Revit to Hypar Elements is complete.");
            }
        }

        private static void SaveModel(Elements.Model model)
        {
            var savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "FromRevit.json");
            File.WriteAllText(savePath, model.ToJson());
        }

        private static Element[] GetElements(Document document, BuiltInCategory category = BuiltInCategory.INVALID, View view = null, ICollection<ElementId> selectedElementIds = null, Level level = null)
        {
            FilteredElementCollector collector = null;
            if (view != null)
            {
                collector = new FilteredElementCollector(document, view.Id);
            }
            else if (selectedElementIds != null)
            {
                collector = new FilteredElementCollector(document, selectedElementIds);
            }
            else
            {
                collector = new FilteredElementCollector(document);
            }

            if (category != BuiltInCategory.INVALID)
            {
                collector = collector.OfCategory(category);
            }

            if (level != null)
            {
                ElementLevelFilter levelFilter = new ElementLevelFilter(level.Id);
                collector = collector.WherePasses(levelFilter);
            }

            var elems = collector.ToElements().ToList();

            return elems.Where(e => e != null).ToArray();
        }
    }
}