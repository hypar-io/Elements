using Elements.Geometry;
using Elements.Geometry.Solids;
using IFC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction.Parsers
{
    internal class IfcMappedItemParser : IIfcRepresentationParser
    {
        private readonly IfcRepresentationDataExtractor _representationDataExtractor;
        private readonly Dictionary<Guid, RepresentationData> _representationsMap;

        public IfcMappedItemParser(IfcRepresentationDataExtractor ifcRepresentationDataExtractor)
        {
            _representationDataExtractor = ifcRepresentationDataExtractor;
            _representationsMap = new Dictionary<Guid, RepresentationData>();
        }

        public bool Matches(IfcRepresentationItem ifcRepresentationItem)
        {
            return ifcRepresentationItem is IfcMappedItem;
        }

        public RepresentationData ParseRepresentationItem(IfcRepresentationItem ifcRepresentationItem)
        {
            if (!(ifcRepresentationItem is IfcMappedItem mappedItem))
            {
                return null;
            }

            if (_representationsMap.ContainsKey(mappedItem.MappingSource.MappedRepresentation.Id))
            {
                var ops = _representationsMap[mappedItem.MappingSource.MappedRepresentation.Id];
                return ops;
            }
            else
            {
                var parsedData = _representationDataExtractor.ParseRepresentationItems(mappedItem.MappingSource.MappedRepresentation.Items);
                var repData = new RepresentationData(mappedItem.MappingSource.MappedRepresentation.Id, mappedItem.MappingTarget.ToTransform(), parsedData);

                _representationsMap.Add(mappedItem.MappingSource.MappedRepresentation.Id, repData);
                return repData;
            }
        }
    }
}
