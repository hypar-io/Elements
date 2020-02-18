using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Elements.Geometry;
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
            foreach (var profile in profiles)
            {
                var zMove = profile.Perimeter.Vertices.Max(v => v.Z);
                var transform = new ElemGeom.Transform(0, 0, -zMove);

                var zeroedProfile = transform.OfProfile(profile);

                transform.Invert();
                var floorThickness = thickness.HasValue ? Elements.Units.FeetToMeters(thickness.Value) : Elements.Units.FeetToMeters(1);
                // Revit floors are extrusions down, and currently Hypar floors are extrusions up, so we also must move by the floor thickness
                transform.Move(new Vector3(0, 0, -floorThickness));
                var floor = new Elements.Floor(zeroedProfile,
                                               floorThickness,
                                               transform);
                floors.Add(floor);
            }
            return floors.ToArray();
        }

        private static ElemGeom.Profile[] GetProfilesOfTopFacesOfFloor(Document doc, Floor floor)
        {
            var geom = floor.get_Geometry(new Options());
            var topFaces = geom.Cast<Solid>().Where(g => g != null).SelectMany(g => g.GetMostLikelyTopFaces());
            var profiles = topFaces.SelectMany(f => f.GetProfiles());

            return profiles.ToArray();
        }
    }
}
