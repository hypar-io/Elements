using System;
using System.Linq;
using Autodesk.Revit.DB;
using Elements;
using Elements.Geometry;
using ElemGeom = Elements.Geometry;

using ADSK = Autodesk.Revit.DB;

namespace Hypar.Revit
{
    public static partial class Create
    {
        public static Elements.WallByProfile[] WallsFromRevitWall(ADSK.Wall wall, Document doc)
        {

            var side_faces = HostObjectUtils.GetSideFaces(wall, ShellLayerType.Exterior);
            if (side_faces.Count != 1)
            {
                throw new InvalidOperationException($"This wall has ${(side_faces.Count < 1 ? "not enough" : "too many")} interior faces");
            }

            var wallFace = doc.GetElement(side_faces[0]).GetGeometryObjectFromReference(side_faces[0]);
            if (!(wallFace is PlanarFace))
            {
                throw new InvalidCastException("This wall does not have planar faces");
            }

            var wallPlane = wallFace as PlanarFace;
            var profiles = Utils.GetScaledProfilesOfFace(wallPlane);

            var centerline = (wall.Location as LocationCurve).Curve;
            var line = new ElemGeom.Line(centerline.GetEndPoint(0).ToVector3(), centerline.GetEndPoint(1).ToVector3());
            line = Utils.ScaleLineFtToMeters(line);
            

            var walls = profiles.Select(p => new WallByProfile(Utils.ReverseProfile(p),
                                                               wall.Width * Utils.FT_TO_METER_FACTOR,
                                                               line));
            return walls.ToArray();
        }

    }
}