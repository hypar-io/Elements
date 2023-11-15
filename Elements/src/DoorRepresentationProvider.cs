using Elements.Geometry.Solids;
using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Text;
using Elements.Representations.DoorRepresentations;

namespace Elements
{
    internal class DoorRepresentationProvider
    {
        private readonly Dictionary<DoorProperties, List<RepresentationInstance>> _doorTypeToRepresentations;
        private readonly DoorRepresentationFactory _doorRepresentationFactory;

        private static readonly Material frameMaterial = new Material(Colors.Gray, 0.5, 0.25, false, null, false, false, null, false, null, 0, false, default, "Silver Frame");

        public DoorRepresentationProvider()
        {
            _doorTypeToRepresentations = new Dictionary<DoorProperties, List<RepresentationInstance>>();
            _doorRepresentationFactory = doorRepresentationFactory;
        }

        public List<RepresentationInstance> GetInstances(Door door)
        {
            var doorProps = new DoorProperties(door);

            if (_doorTypeToRepresentations.TryGetValue(doorProps, out var representations))
            {
                return representations;
            }

            var representationInstances = _doorRepresentationFactory.CreateAllRepresentationInstances(door);

            _doorTypeToRepresentations[doorProps] = representationInstances;
            return representationInstances;
        }
    }
}
