using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction
{
    internal class MappingInfo
    {
        public Guid MappingId { get; private set; }
        public Transform MappingTransform { get; private set; }

        public MappingInfo(Guid mappingId, Transform mappingTransform)
        {
            MappingId = mappingId;
            MappingTransform = mappingTransform;
        }
    }
}
