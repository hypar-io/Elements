using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Elements.Geometry;

namespace Hypar.Revit
{
    internal static class RevitExtensions
    {
        /// <summary>
        /// Convert the Revit XYZ to an Elements Vector3, with an option to scale the 
        /// Vector3 from feet to meters.  This scaling should be done with any XYZ that represents
        /// a geometric property, but should maybe be avoided for other vectors.
        /// </summary>
        internal static Vector3 ToVector3(this XYZ xyz, bool scaleToMeters = false)
        {
            var scale = scaleToMeters ? Elements.Units.FeetToMeters(1) : 1.0;
            return new Vector3(xyz.X, xyz.Y, xyz.Z) * scale;
        }

        /// <summary>
        /// Retrieve all of the Element.Geometry.Profiles that describe the Revit PlanarFace
        /// Soemtimes a single PlanarFace in Revit and actually multiple "faces" should be translated into multiple Element Profiles
        /// </summary>
        internal static Elements.Geometry.Profile[] GetProfiles(this PlanarFace face)
        {
            var polygons = face.GetEdgesAsCurveLoops().Select(cL => cL.ToPolygon());

            var polygonLoopDict = MatchOuterLoopPolygonsWithInnerHoles(polygons);

            var profiles = polygonLoopDict.Select(kvp => new Elements.Geometry.Profile(kvp.Key, kvp.Value, Guid.NewGuid(), "Revit Profile"));
            var scaledProfiles = profiles.Select(p => p);
            return scaledProfiles.ToArray();
        }

        /// <summary>
        /// Analyze all of the PlanarFaces of a Revit solid and returns those that 
        /// face "up" within a certain threshold.
        /// </summary>
        /// <param name="solid">The revit Solid</param>
        /// <param name="verticalThreshold">The angle (in degrees) that represents the threshold for a face to be considered facing up.  A completely horizontal face will have an angle of 0.  A face that does not face up or down at all will have an angle of 90.</param>
        private static PlanarFace[] GetMostLikelyTopFaces(this Solid solid, double verticalThreshold = 30)
        {
            var faces = new List<PlanarFace>();
            foreach (PlanarFace face in solid.Faces)
            {
                if (face.FaceNormal.DotProduct(XYZ.BasisZ) > Math.Cos(Elements.Units.DegreesToRadians(verticalThreshold)) && face.FaceNormal.DotProduct(XYZ.BasisZ) <= 1)
                {
                    faces.Add(face);
                }
            }
            return faces.ToArray();
        }

        private static Polygon ToPolygon(this CurveLoop curveLoop, bool scaleToMeters = false)
        {
            return new Polygon(curveLoop.Select(l => l.GetEndPoint(0).ToVector3(scaleToMeters)).ToList());
        }

        private static Dictionary<Polygon, List<Polygon>> MatchOuterLoopPolygonsWithInnerHoles(IEnumerable<Polygon> polygons)
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
    }
}