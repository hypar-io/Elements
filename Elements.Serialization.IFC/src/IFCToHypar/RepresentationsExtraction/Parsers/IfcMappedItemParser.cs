using IFC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction.Parsers
{
    /// <summary>Parser for IfcMappedItem representation.</summary>
    internal class IfcMappedItemParser : IIfcRepresentationParser
    {
        private readonly IfcRepresentationDataExtractor _representationDataExtractor;
        private readonly Dictionary<Guid, RepresentationData> _representationsMap;

        /// <summary>
        /// Create IfcMappedItem parser.
        /// </summary>
        /// <param name="ifcRepresentationDataExtractor">
        /// General IfcRepresentationItem parser to extract RepresentationData of the definition.
        /// </param>
        public IfcMappedItemParser(IfcRepresentationDataExtractor ifcRepresentationDataExtractor)
        {
            _representationDataExtractor = ifcRepresentationDataExtractor;
            _representationsMap = new Dictionary<Guid, RepresentationData>();
        }

        public bool CanParse(IfcRepresentationItem ifcRepresentationItem)
        {
            return ifcRepresentationItem is IfcMappedItem;
        }

        /// <summary>
        /// Returns RepresentationData - the result of IfcMappedItem parsing. Saves RepresentationData
        /// of the MappedSourse. If it's saved version already exists - uses the existing one.
        /// </summary>
        /// <param name="ifcRepresentationItem">
        /// IfcRepresentationItem that will be parsed.
        /// </param>
        public RepresentationData ParseRepresentationItem(IfcRepresentationItem ifcRepresentationItem)
        {
            if (ifcRepresentationItem is not IfcMappedItem mappedItem)
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

                if (parsedData is null)
                {
                    return null;
                }

                var mappingTransform = mappedItem.MappingSource.MappingOrigin.ToTransform().Concatenated(mappedItem.MappingTarget.ToTransform());
                parsedData.Transform = mappingTransform;

                _representationsMap.Add(mappedItem.MappingSource.MappedRepresentation.Id, parsedData);
                return parsedData;
            }
        }
    }
}
