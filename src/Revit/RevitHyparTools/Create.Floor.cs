using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Elements.Geometry;
using GeometryEx;
using ElemGeom = Elements.Geometry;

using ADSK = Autodesk.Revit.DB;

namespace Hypar.Revit
{
    public static partial class Create
    {
        public static Elements.Floor[] FloorsFromRevitFloor(ADSK.Document doc, ADSK.Floor revitFloor)
        {
            var profiles = GetProfilesOfTopFacesOfFloor(doc, revitFloor);
            var thickness = revitFloor.LookupParameter("Thickness")?.AsDouble();

            var floors = new List<Elements.Floor>();
            foreach(var profile in profiles) 
            {
                var zMove = profile.Perimeter.Vertices.Max(v => v.Z);
                var transform = new ElemGeom.Transform(0,0,-zMove);
                
                var zeroedProfile = transform.OfProfile(profile);

                transform.Invert();
                var floor = new Elements.Floor(zeroedProfile, thickness.HasValue ? thickness.Value : 1, transform);
                floors.Add(floor);
            }
            return floors.ToArray();
        }


        private static ElemGeom.Profile[] GetProfilesOfTopFacesOfFloor(Document doc, Floor floor)
        {
            var geom = floor.get_Geometry(new Options());
            var topFaces = geom.Cast<Solid>().Where(g => g!=null).SelectMany(g => Utils.GetMostLikelyTopFacesOfSolid(g));
            var profiles = topFaces.SelectMany(f => Utils.GetProfilesOfFace(f));

            return profiles.ToArray();
        }

    }
}
