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
        public static void Convert(Document doc, View view)
        {
            // var convert = new Hypar.Revit.Converters.WallConverter();
            // var walls = new FilteredElementCollector(doc)
            //                                 .OfCategory(BuiltInCategory.OST_Walls)
            //                                 .WhereElementIsNotElementType()
            //                                 .ToElements();
            // var wall = walls.Cast<Wall>().First(e => e != null);
            // var hyWalls = convert.FromRevit(wall as Wall, doc);

            var toConvert = new Dictionary<BuiltInCategory, Element[]> {
                {BuiltInCategory.OST_Floors, GetFirstOfCategory(doc, BuiltInCategory.OST_Floors)},
                {BuiltInCategory.OST_Walls, GetFirstOfCategory(doc, BuiltInCategory.OST_Walls)}
            };

            var model = ConversionRunner.RunConverters(toConvert, doc);

            TaskDialog.Show("done it", $"Num elemets = {model.Elements.Count()}");

            var savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "FromRevit.json");
            File.WriteAllText(savePath, model.ToJson());

            return;
        }

        private static Element[] GetFirstOfCategory(Document document, BuiltInCategory category)
        {
            var elems = new FilteredElementCollector(document)
                                            .OfCategory(category)
                                            .WhereElementIsNotElementType()
                                            .ToElements();
            var elem = elems.First(e => e != null);
            return elems.Where(e => e != null).ToArray();
        }
    }
}