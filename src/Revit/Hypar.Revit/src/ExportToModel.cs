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
            var toConvert = ConversionRunner.AllCategories.ToDictionary(cat => cat, cat => GetAllOfCategory(doc, cat, null, elementIds));

            ConvertAndExportElements(doc, toConvert);
        }

        public static void ConvertAll(Document doc)
        {
            var toConvert = ConversionRunner.AllCategories.ToDictionary(cat => cat, cat => GetAllOfCategory(doc, cat));

            ConvertAndExportElements(doc, toConvert);
        }

        public static void ConvertView(Document doc, View view)
        {
            var toConvert = ConversionRunner.AllCategories.ToDictionary(cat => cat, cat => GetAllOfCategory(doc, cat, view));

            ConvertAndExportElements(doc, toConvert);
        }

        private static void ConvertAndExportElements(Document doc, Dictionary<BuiltInCategory, Element[]> toConvert)
        {
            var model = ConversionRunner.RunConverters(toConvert, doc, out List<Exception> conversionExceptions);

            var exceptionMessage = String.Join("\n", conversionExceptions.Select(e => e.InnerException?.Message));
            TaskDialog.Show("Some Problems", exceptionMessage);
            TaskDialog.Show("Export Results", $"Num elemets = {model.Elements.Count()}");

            var savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "FromRevit.json");
            File.WriteAllText(savePath, model.ToJson());

            return;
        }

        private static Element[] GetAllOfCategory(Document document, BuiltInCategory category, View view = null, ICollection<ElementId> selectedElementIds = null)
        {
            List<Element> elems = null;
            if (view != null)
            {
                elems = new FilteredElementCollector(document, view.Id)
                                                .OfCategory(category)
                                                .WhereElementIsNotElementType()
                                                .ToElements().ToList();
            }
            else if (selectedElementIds != null)
            {
                elems = new FilteredElementCollector(document, selectedElementIds)
                                                .OfCategory(category)
                                                .WhereElementIsNotElementType()
                                                .ToElements().ToList();
            }
            else
            {
                elems = new FilteredElementCollector(document)
                                                .OfCategory(category)
                                                .WhereElementIsNotElementType()
                                                .ToElements().ToList();
            }

            return elems.Where(e => e != null).ToArray();
        }
    }
}