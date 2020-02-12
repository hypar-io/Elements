using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Elements.Geometry;

namespace Hypar.Revit {
    public static class Utils {
        
        public static Elements.Geometry.Profile[] GetProfilesOfFace(PlanarFace f)
        {
            var polygons = f.GetEdgesAsCurveLoops().Select(cL => CurveLoopToPolygon(cL));

            var polygonLoopDict = MatchOuterLoopPolygonsWithInnerHoles(polygons);

            var profiles = polygonLoopDict.Select(kvp => new Elements.Geometry.Profile(kvp.Key, kvp.Value, Guid.NewGuid(), "Floor Profile"));
            return profiles.ToArray();
        }

        public static Dictionary<Polygon, List<Polygon>> MatchOuterLoopPolygonsWithInnerHoles(IEnumerable<Polygon> polygons)
        {
            var polygonLoopDict = new Dictionary<Polygon, List<Polygon>>();
            foreach (var polygon in polygons)
            {
                bool polyIsInnerLoop = false;
                foreach (var outerLoop in polygonLoopDict.Keys)
                {
                    // TODO possibly replace this with the updated covers(polygon) method when it is brought into elements.
                    if (polygon.Vertices.All(v => outerLoop.Covers(v)))
                    {
                        polyIsInnerLoop = true;
                        polygonLoopDict[outerLoop].Add(polygon);
                        break;
                    }
                }
                if (!polyIsInnerLoop)
                {
                    polygonLoopDict.Add(polygon, new List<Polygon>());
                }
            }

            return polygonLoopDict;
        }

        public static Polygon CurveLoopToPolygon(CurveLoop cL)
        {
            return new Polygon(cL.Select(l => l.GetEndPoint(0).ToVector3()).ToList());
        }

        public static PlanarFace[] GetMostLikelyTopFacesOfSolid(Solid solid) {
            var faces = new List<PlanarFace>();
            foreach(PlanarFace face in solid.Faces) {
                if (face.FaceNormal.DotProduct(XYZ.BasisZ) > 0.85 && face.FaceNormal.DotProduct(XYZ.BasisZ) <= 1)
                {
                    faces.Add(face);
                }
            }
            return faces.ToArray();
        }
    }
}