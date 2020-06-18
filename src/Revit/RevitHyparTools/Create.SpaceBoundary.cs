using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Elements;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Hypar.Revit;
using ADSK = Autodesk.Revit.DB;

namespace Hypar.Revit
{
    public static partial class Create
    {
        public static SpaceBoundary[] SpaceBoundaryFromRevitArea(ADSK.Area area, Document doc)
        {
            var geom = area.get_Geometry(new Options()
            {
                View = GetViewWhereElemIsVisible(doc, area)
            });
            var face = geom.Where(g => g.GetType() == typeof(ADSK.Solid)).Cast<ADSK.Solid>().Where(g => g != null).First().Faces.get_Item(0) as PlanarFace;

            var boundaries = new List<SpaceBoundary>();
            foreach (var p in face.GetProfiles(true))
            {
                var boundary = new SpaceBoundary(p,
                                                 new Elements.Geometry.Transform(),
                                                 BuiltInMaterials.Default,
                                                 null,
                                                 false,
                                                 Guid.NewGuid(),
                                                 "");
                boundaries.Add(boundary);
            }
            return boundaries.ToArray();
        }

        private static View GetViewWhereElemIsVisible(Document doc, ADSK.Element elem)
        {
            var col = new FilteredElementCollector(doc).WhereElementIsNotElementType().OfClass(typeof(ViewPlan)).Cast<ViewPlan>().Where(e => !e.IsTemplate);
            foreach (var view in col)
            {
                if (new FilteredElementCollector(doc, view.Id).ToElementIds().Contains(elem.Id))
                {
                    return view as View;
                }
            }
            return null;
        }
    }
}