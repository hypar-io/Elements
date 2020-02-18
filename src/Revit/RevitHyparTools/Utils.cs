using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autodesk.Revit.DB;
using Elements.Geometry;

namespace Hypar.Revit
{
    public static class Utils
    {
        public static Elements.Geometry.Profile ScaleProfileFtToMeters(Elements.Geometry.Profile profile) {
            var transform = new Elements.Geometry.Transform();
            transform.Scale(Elements.Units.FeetToMeters(1));
            var transformedProfile = transform.OfProfile(profile);
            
            return new Elements.Geometry.Profile(transformedProfile.Perimeter, transformedProfile.Voids, Guid.NewGuid(), transformedProfile.Name);
        }
        public static Elements.Geometry.Profile ReverseProfile(Elements.Geometry.Profile profile)
        {
            var perimeter = profile.Perimeter.Reversed();
            var voids = profile.Voids.Select(v => v.Reversed());

            return new Elements.Geometry.Profile(perimeter, voids.ToList(), Guid.NewGuid(), "");
        }

        // Revit stores lengths in ft, but Hypar's standard is meters, so we need to scale the faces when we convert them to profiles
        public static Elements.Geometry.Profile[] GetScaledProfilesOfFace(PlanarFace f)
        {
            var polygons = f.GetEdgesAsCurveLoops().Select(cL => cL.ToPolygon());

            var polygonLoopDict = MatchOuterLoopPolygonsWithInnerHoles(polygons);

            var profiles = polygonLoopDict.Select(kvp => new Elements.Geometry.Profile(kvp.Key, kvp.Value, Guid.NewGuid(), "Revit Profile"));
            var scaledProfiles = profiles.Select(p => ScaleProfileFtToMeters(p));
            return scaledProfiles.ToArray();
        }

        internal static Elements.Geometry.Line ScaleLineFtToMeters(Elements.Geometry.Line line)
        {
            var transform = new Elements.Geometry.Transform();
            transform.Scale(Elements.Units.FeetToMeters(1));
            return transform.OfLine(line);
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


        /// <summary>
        /// Analyzes all of the PlanarFaces of a Revit solid and returns those that 
        /// face "up" within a certain threshold.
        /// </summary>
        /// <param name="solid">The revit Solid</param>
        /// <param name="verticalThreshold">The angle (in degrees) that represents the threshold for a face to be considered facing up.  A completely horizontal face will have an angle of 0.  A face that does not face up or down at all will have an angle of 90.</param>
        public static PlanarFace[] GetMostLikelyTopFacesOfSolid(Solid solid, double verticalThreshold = 30)
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
    }
}