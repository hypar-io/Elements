using Elements.Geometry;
using Elements.Geometry.Solids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction
{
    internal class RepresentationData
    {
        public Transform Transform { get; set; }
        public Material Material { get; set; }

        public List<SolidOperation> SolidOperations { get; private set; }
        public Extrude Extrude { get; private set; }
        public Transform ExtrudeTransform { get; private set; }

        public MappingInfo MappingInfo { get; private set; }

        public RepresentationData(List<SolidOperation> solidOperations)
        {
            SolidOperations = solidOperations;
        }

        public RepresentationData(Extrude extrude, Transform extrudeTransform) : this(new List<SolidOperation>() { extrude })
        {
            Extrude = extrude;
            ExtrudeTransform = extrudeTransform;
        }

        public RepresentationData(Guid mappedId, Transform mappedTransform, RepresentationData mappedRepresentation)
        {
            SolidOperations = mappedRepresentation.SolidOperations;
            Extrude = mappedRepresentation.Extrude;
            ExtrudeTransform = mappedRepresentation.ExtrudeTransform;
            Material = mappedRepresentation.Material;
            MappingInfo = new MappingInfo(mappedId, mappedTransform);
        }

        // TODO: Change the way the representations are merged when a Representation,
        // combined from different representations with different materials is possible.
        public RepresentationData(List<RepresentationData> representations)
        {
            SolidOperations = representations.SelectMany(x => x.SolidOperations).ToList();

            var repsWithMaterial = representations.Where(rep => rep.Material != null);
            if (repsWithMaterial.Any())
            {
                Material = repsWithMaterial.First().Material;
            }

            var mappedReps = representations.Where(rep => rep.MappingInfo != null);
            if (mappedReps.Any())
            {
                MappingInfo = mappedReps.First().MappingInfo;
            }

            var extrudeReps = representations.Where(rep => rep.Extrude != null);
            if (extrudeReps.Any())
            {
                var extrudeRep = extrudeReps.First();
                Extrude = extrudeRep.Extrude;
                ExtrudeTransform = extrudeRep.ExtrudeTransform;
            }
        }
    }
}
