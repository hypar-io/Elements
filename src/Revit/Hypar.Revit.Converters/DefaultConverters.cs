

using System;
using System.Linq;
using Autodesk.Revit.DB;


namespace Hypar.Revit.Converters
{

    public class WallConverter : IFromRevitConverter<Autodesk.Revit.DB.Wall, Elements.WallByProfile>
    {
        public BuiltInCategory Category { get { return BuiltInCategory.OST_Walls; } }
        public Elements.WallByProfile[] FromRevit(Autodesk.Revit.DB.Wall wall, Autodesk.Revit.DB.Document document)
        {
            return Create.WallsFromRevitWall(wall, document);
        }
    }

    public class FloorConverter : IFromRevitConverter<Autodesk.Revit.DB.Floor, Elements.Floor>
    {
        public BuiltInCategory Category { get { return BuiltInCategory.OST_Floors; } }
        public Elements.Floor[] FromRevit(Autodesk.Revit.DB.Floor floor, Autodesk.Revit.DB.Document document)
        {
            return Create.FloorsFromRevitFloor(document, floor);
        }
    }
}
