using Elements.Geometry.Solids;
using IFC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction.Parsers
{
    internal class IfcBooleanClippingResultParser : IIfcRepresentationParser
    {
        private readonly IfcRepresentationDataExtractor _representationDataExtractor;

        public IfcBooleanClippingResultParser(IfcRepresentationDataExtractor refDataExtractor) 
        {
            _representationDataExtractor = refDataExtractor;
        }

        public bool CanParse(IfcRepresentationItem ifcRepresentationItem)
        {
            return ifcRepresentationItem is IfcBooleanClippingResult;
        }

        public RepresentationData ParseRepresentationItem(IfcRepresentationItem ifcRepresentationItem)
        {
            if (!(ifcRepresentationItem is IfcBooleanClippingResult ifcBooleanClippingResult))
            {
                return null;
            }

            // TODO: Apply clipping operation with second operand.
            if (!(ifcBooleanClippingResult.FirstOperand.Choice is IfcRepresentationItem firstOperand))
            {
                return null;
            }

            return _representationDataExtractor.ParseRepresentationItem(firstOperand);
        }
    }
}
