using System;
using System.Linq;
using Autodesk.Revit.DB;

using ADSK = Autodesk.Revit.DB;

namespace Hypar.Revit
{
    public static partial class Create
    {
        public static Elements.Column ColumnFromRevitColumn(ADSK.FamilyInstance column)
        {
            if (column.Category.Id.IntegerValue != (int)BuiltInCategory.OST_Columns)
            {
                throw new InvalidCastException("The incoming family instance is not a column");
            }

            var geom = column.get_Geometry(new Options());
            var downfaces = geom.Where(g => g.GetType() == typeof(Solid)).Cast<Solid>().Where(s => s != null).SelectMany(s => s.GetMostLikelyHorizontalFaces(30, true));

            var profile = downfaces.First().GetProfiles(true).First();
            if (profile.Area() < 0)
            {
                profile = profile.Reverse();
            }

            var baseCenter = profile.Perimeter.Centroid();
            var transform = new Elements.Geometry.Transform(-baseCenter.X, -baseCenter.Y, -baseCenter.Z);

            var zeroedProfile = transform.OfProfile(profile);
            transform.Invert();

            var col = new Elements.Column(transform.Origin, Elements.Units.FeetToMeters(GetHeightFromColumn(column)), zeroedProfile);

            return col;
        }

        private static double GetHeightFromColumn(FamilyInstance column)
        {
            var bbox = column.get_BoundingBox(null);
            return bbox.Max.Z - bbox.Min.Z;
        }
    }
}