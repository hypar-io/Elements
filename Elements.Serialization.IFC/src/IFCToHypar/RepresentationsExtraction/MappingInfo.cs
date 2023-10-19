using Elements.Geometry;
using System;

namespace Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction
{
    /// <summary>Mapping information, extracted from IfcMappedItem representation.</summary>
    internal class MappingInfo
    {
        /// <summary>Guid of MappingSource.MappedRepresentation of IfcMappedItem.</summary>
        public Guid MappingId { get; private set; }
        /// <summary>The transform of the MappingSource of IfcMappedItem.</summary>
        public Transform MappingTransform { get; private set; }

        /// <summary>
        /// Create an object that contains the mapping information from IfcMappedItem.
        /// </summary>
        /// <param name="mappingId">Guid of MappingSource.MappedRepresentation of IfcMappedItem.</param>
        /// <param name="mappingTransform">The transform of the MappingSource of IfcMappedItem.</param>
        public MappingInfo(Guid mappingId, Transform mappingTransform)
        {
            MappingId = mappingId;
            MappingTransform = mappingTransform;
        }
    }
}
