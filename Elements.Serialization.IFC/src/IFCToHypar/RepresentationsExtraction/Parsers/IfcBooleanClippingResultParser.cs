using IFC;

namespace Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction.Parsers
{
    internal class IfcBooleanClippingResultParser : IIfcRepresentationParser
    {
        private readonly IfcRepresentationDataExtractor _representationDataExtractor;

        /// <summary>
        /// Create a parser of IfcBooleanClippingResult.
        /// </summary>
        /// <param name="refDataExtractor">
        /// General representation data extractor for the extraction of the solids of the first operand.
        /// </param>
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
            if (ifcRepresentationItem is not IfcBooleanClippingResult ifcBooleanClippingResult)
            {
                return null;
            }

            // TODO: Apply clipping operation with second operand.
            if (ifcBooleanClippingResult.FirstOperand.Choice is not IfcRepresentationItem firstOperand)
            {
                return null;
            }

            return _representationDataExtractor.ParseRepresentationItem(firstOperand);
        }
    }
}
