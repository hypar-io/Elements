using System;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ADSK = Autodesk.Revit.DB;

namespace Hypar.Revit
{
    public static class ExportToModel
    {
        public static void Convert(Document doc, View view)
        {
            var convert = new Hypar.Revit.Converters.WallConverter();

            var walls = new FilteredElementCollector(doc)
                                            .OfCategory(BuiltInCategory.OST_Walls)
                                            .WhereElementIsNotElementType()
                                            .ToElements();

            var wall = walls.Cast<Wall>().First(e => e != null);

            var hyWalls = convert.FromRevit(wall as Wall, doc);

            TaskDialog.Show("done it", $"Num walls = {hyWalls.Count()}");

            return;
        }
    }
}