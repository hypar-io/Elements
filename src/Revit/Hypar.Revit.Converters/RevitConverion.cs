
using System;
using System.Linq;
using Autodesk.Revit.DB;


namespace Hypar.Revit.Converters
{
    public interface IFromRevitConverter<R, H>
    {
        H[] FromRevit(R revitType, Autodesk.Revit.DB.Document doc);
    }

    public class WallConverter : IFromRevitConverter<Autodesk.Revit.DB.Wall, Elements.WallByProfile>
    {
        public WallConverter()
        {
        }

        public Elements.WallByProfile[] FromRevit(Autodesk.Revit.DB.Wall wall, Autodesk.Revit.DB.Document doc)
        {
            return Create.WallsFromRevitWall(wall, doc);
        }
    }
}