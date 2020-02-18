using System;
using Autodesk.Revit.DB;
using Elements.Geometry;
using GeometryEx;
using ElemGeom = Elements.Geometry;

using Revit = Autodesk.Revit.DB;

namespace RevitHyparTools {
    public static partial class Create {

        public static Elements.Column ColumnFromRevitColumn(Revit.FamilyInstance column) {
            if (column.Category.Id.IntegerValue != (int) BuiltInCategory.OST_StructuralColumns) {
                throw new InvalidCastException("The incoming family instance is not a structural column");
            }

            var geom = column.get_Geometry(new Options());

            return new Elements.Column(Vector3.Origin, 10, Polygon.Rectangle(1,2));
        }
    }
}