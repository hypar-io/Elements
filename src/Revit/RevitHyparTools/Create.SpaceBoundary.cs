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
            var spatialElementCalculator = new SpatialElementGeometryCalculator(doc);
            var geometryCalculationResult = spatialElementCalculator.CalculateSpatialElementGeometry(area);
            var geom = geometryCalculationResult.GetGeometry();
            var face = geom.Faces.get_Item(0) as PlanarFace;

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
    }
}