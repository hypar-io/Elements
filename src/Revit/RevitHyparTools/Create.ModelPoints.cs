using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Elements;

namespace Hypar.Revit
{
    public static partial class Create
    {
        public static ModelPoints ModelPointsFromPoints(IEnumerable<XYZ> points, string tag)
        {
            return new ModelPoints(points.Select(p => p.ToVector3(true)).ToList(), id: Guid.NewGuid(), name: tag);
        }
    }
}