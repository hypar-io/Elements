using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Elements;
using ADSK = Autodesk.Revit.DB;

namespace Hypar.Revit
{
    public static partial class Create
    {
        public static SpaceBoundary[] SpaceBoundaryFromRevitArea(ADSK.Area area, Document doc, View view = null)
        {
            if (view == null)
            {
                view = GetViewWhereElemIsVisible(doc, area);
                if (view == null)
                {
                    return Array.Empty<SpaceBoundary>();
                }
            }

            var geom = area.get_Geometry(new Options()
            {
                View = view
            });
            var solid = geom.Where(g => typeof(ADSK.Solid) == g.GetType()).Cast<ADSK.Solid>().Where(s => s != null);
            var face = solid.First().Faces.get_Item(0) as PlanarFace;

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