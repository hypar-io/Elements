using System;
using System.Collections.Generic;
using Elements.Geometry.Solids;

namespace Elements
{
    public static class DoorRepresentationStorage
    {
        private static readonly Dictionary<string, List<RepresentationInstance>> _doors = new Dictionary<string, List<RepresentationInstance>>();
        public static Dictionary<string, List<RepresentationInstance>> Doors => _doors;

        public static void SetDoorRepresentation(Door door)
        {
            var hash = door.GetRepresentationHash();
            if (!_doors.ContainsKey(hash))
            {
                _doors.Add(hash, door.GetInstances());
            }
            door.RepresentationInstances = _doors[hash];
        }
    }

}