using System;
using System.Linq;
using Autodesk.Revit.DB;
using Elements;

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

    public class ColumnConverter : IFromRevitConverter<Autodesk.Revit.DB.FamilyInstance, Elements.Column>
    {
        public BuiltInCategory Category => BuiltInCategory.OST_Columns;

        public Elements.Column[] FromRevit(Autodesk.Revit.DB.FamilyInstance revitElement, Document doc)
        {
            return new[] { Create.ColumnFromRevitColumn(revitElement, doc) };
        }
    }

    public class AreaConverter : IFromRevitConverter<Autodesk.Revit.DB.Area, Elements.SpaceBoundary>
    {
        public BuiltInCategory Category => BuiltInCategory.OST_Areas;

        public SpaceBoundary[] FromRevit(Area revitElement, Document doc)
        {
            return Create.SpaceBoundaryFromRevitArea(revitElement, doc);
        }
    }
}
