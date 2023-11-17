using System;
using System.Collections.Generic;
using Elements.Geometry.Solids;

namespace Elements.BIM
{
    static class DoorRepresentationStorage
    {
        private static readonly Dictionary<string, List<RepresentationInstance>> _fittings = new Dictionary<string, List<RepresentationInstance>>();
        public static Dictionary<string, List<RepresentationInstance>> Fittings => _fittings;

        public static void SetDoorRepresentation(Door door)
        {
            var hash = door.GetRepresentationHash();
            if (!_fittings.ContainsKey(hash))
            {
                _fittings.Add(hash, door.GetInstances());
            }
            door.RepresentationInstances = _fittings[hash];
        }
    }

}