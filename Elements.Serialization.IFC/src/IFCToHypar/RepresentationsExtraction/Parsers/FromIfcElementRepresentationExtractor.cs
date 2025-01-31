using IFC;

namespace Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction.Parsers
{
    /// <summary>
    /// An IIfcRepresentationParser that extracts the ElementRepresentation from an IfcRepresentationItem
    /// and then forms a RepresentationData from it.
    /// </summary>
    internal abstract class FromIfcElementRepresentationExtractor : IIfcRepresentationParser
    {
        protected readonly MaterialExtractor _materialExtractor;

        /// <summary>
        /// Create a FromIfcElementRepresentationExtractor that extracts the ElementRepresentation from 
        /// an IfcRepresentationItem. The extracted ElementRepresentation and the material, extracted
        /// with the <paramref name="materialExtractor"/>, are used to form a RepresentationData.
        /// </summary>
        /// <param name="materialExtractor">
        /// Extracts the material from IfcRepresentationItem. The material is used to
        /// create a RepresentationInstance to form a RepresentationData from the 
        /// extracted ElementRepresentation.
        /// </param>
        public FromIfcElementRepresentationExtractor(MaterialExtractor materialExtractor) 
        {
            _materialExtractor = materialExtractor;
        }

        /// <summary>
        /// Returns true, if it is an appropriate parser for <paramref name="ifcRepresentationItem"/>.
        /// </summary>
        /// <param name="ifcRepresentationItem">IfcRepresentationItem that will be checked if it can be parsed with this parser.</param>
        public abstract bool CanParse(IfcRepresentationItem ifcRepresentationItem);

        public RepresentationData ParseRepresentationItem(IfcRepresentationItem ifcRepresentationItem)
        {
            var material = _materialExtractor.ExtractMaterial(ifcRepresentationItem);
            var elementRepresentation = ExtractElementRepresentation(ifcRepresentationItem);
            var repInstance = new RepresentationInstance(elementRepresentation, material);
            return new RepresentationData(repInstance);
        }

        /// <summary>
        /// Extracts ElementRepresentation from <paramref name="ifcRepresentationItem"/>.
        /// </summary>
        /// <param name="ifcRepresentationItem"></param>
        /// <returns>The extracted ElementRepresentation.</returns>
        protected abstract ElementRepresentation ExtractElementRepresentation(IfcRepresentationItem ifcRepresentationItem);
    }
}
