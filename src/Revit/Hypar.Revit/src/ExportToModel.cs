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
            var toConvert = ConversionRunner.Converters.Keys.ToDictionary(cat => cat, cat => GetAllOfCategory(doc, cat, null, elementIds));

            ConvertAndExportElements(doc, toConvert);
        }

        public static void ConvertAll(Document doc)
        {
            var toConvert = ConversionRunner.Converters.Keys.ToDictionary(cat => cat, cat => GetAllOfCategory(doc, cat));

            ConvertAndExportElements(doc, toConvert);
        }

        public static void ConvertView(Document doc, View view)
        {
            var toConvert = ConversionRunner.Converters.Keys.ToDictionary(cat => cat, cat => GetAllOfCategory(doc, cat, view));

            ConvertAndExportElements(doc, toConvert);
        }

        private static void ConvertAndExportElements(Document doc, Dictionary<BuiltInCategory, Element[]> toConvert)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var model = ConversionRunner.RunConverters(toConvert, doc, out List<Exception> conversionExceptions);

            sw.Stop();
            if (conversionExceptions.Count > 0)
            {
                // TODO this is acceptable messaging for debugging, but we'll want to provide different message and do logging before release
                var exceptionMessage = String.Join("\n", conversionExceptions.Select(e => e.InnerException?.Message));
                TaskDialog.Show("Some Problems", exceptionMessage);
            }
            TaskDialog.Show("Export Results", $"Num elemets = {model.Elements.Count()}\nTime = {sw.ElapsedMilliseconds}ms");

            var savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "FromRevit.json");
            File.WriteAllText(savePath, model.ToJson());

            return;
        }

        private static Element[] GetAllOfCategory(Document document, BuiltInCategory category = BuiltInCategory.INVALID, View view = null, ICollection<ElementId> selectedElementIds = null)
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

            var elems = collector.ToElements().ToList();

            return elems.Where(e => e != null).ToArray();
        }
    }
}