using Elements.Geometry;
using Elements.Geometry.Solids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction
{
    // TODO: Use RepresentationInstances instead of this.
    /// <summary>
    /// A piece of IfcProduct representation information.
    /// </summary>
    internal class RepresentationData
    {
        /// <summary>
        /// A transform of IfcRepresentationItem.
        /// </summary>
        public Transform Transform { get; set; }
        /// <summary>
        /// A material of IfcRepresentationItem.
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// Solid operations for the Representation.
        /// </summary>
        public List<SolidOperation> SolidOperations { get; private set; }
        /// <summary>
        /// An extrude that is used for the Element defining properties extraction.
        /// </summary>
        public Extrude Extrude { get; private set; }
        /// <summary>
        /// The transform of the Extrude.
        /// </summary>
        public Transform ExtrudeTransform { get; private set; }

        /// <summary>
        /// Information about mapping that can be used for ElementInstance creation.
        /// </summary>
        public MappingInfo MappingInfo { get; private set; }

        /// <summary>
        /// Create a RepresentationData from <paramref name="solidOperations"/>.
        /// </summary>
        /// <param name="solidOperations">
        /// A collection of solid operations that define representation of IfcProduct.
        /// </param>
        public RepresentationData(List<SolidOperation> solidOperations)
        {
            SolidOperations = solidOperations;
        }

        /// <summary>
        /// Create a RepresentationData from a single <paramref name="extrude"/>.
        /// </summary>
        /// <param name="extrude">The extrude that defines the representation item.</param>
        /// <param name="extrudeTransform">The transform of the <paramref name="extrude"/>.</param>
        public RepresentationData(Extrude extrude, Transform extrudeTransform) : this(new List<SolidOperation>() { extrude })
        {
            Extrude = extrude;
            ExtrudeTransform = extrudeTransform;
        }

        /// <summary>
        /// Create a RepresentationData for IfcMappedItem.
        /// </summary>
        /// <param name="mappedId">Guid of MappingSource.MappedRepresentation of IfcMappedItem.</param>
        /// <param name="mappedTransform">The transform of the MappingSource of IfcMappedItem.</param>
        /// <param name="mappedRepresentation">The representation data of the definition.</param>
        public RepresentationData(Guid mappedId, Transform mappedTransform, RepresentationData mappedRepresentation)
        {
            SolidOperations = mappedRepresentation.SolidOperations;
            Extrude = mappedRepresentation.Extrude;
            ExtrudeTransform = mappedRepresentation.ExtrudeTransform;
            Material = mappedRepresentation.Material;
            MappingInfo = new MappingInfo(mappedId, mappedTransform);
        }

        // TODO: Change the way the representations are merged when multiple representations
        // are supported.
        /// <summary>
        /// Merge multiple representations into a single one.
        /// </summary>
        /// <param name="representations">Multiple representations that should be merged into a single one.</param>
        public RepresentationData(List<RepresentationData> representations)
        {
            // Combine solid operations of all representations.
            SolidOperations = representations.SelectMany(x => x.SolidOperations).ToList();

            // Use first found material as the Material of the GeometricElement.
            // TODO: Single IfcProduct can have multiple representations. Each of them
            // has it's own material. Change the material assignement behavior when
            // multiple representations are supported.
            var repsWithMaterial = representations.Where(rep => rep.Material != null);
            if (repsWithMaterial.Any())
            {
                Material = repsWithMaterial.First().Material;
            }

            // TODO: IfcProduct can have several differend IfcMappedItem representations.
            // Each of them should be handled separately. Now just the first IfcMappedItem
            // is used.
            var mappedReps = representations.Where(rep => rep.MappingInfo != null);
            if (mappedReps.Any())
            {
                MappingInfo = mappedReps.First().MappingInfo;
            }

            // TODO: IfcProducts can include several extrudes. Now single extrude
            // is supported. Moreover, most of the representation parsers work
            // with the first found extrude. They require an extrude for the 
            // Element defining properties to be extracted.
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
