using IFC;

namespace Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction.Parsers
{
    /// <summary>Parses an IfcRepresentationItem into a RepresentationData.</summary>
    internal interface IIfcRepresentationParser
    {
        /// <summary>
        /// Parses <paramref name="ifcRepresentationItem"/> into a RepresentationData. 
        /// Returns null if the conversion was unsuccessful.
        /// </summary>
        /// <param name="ifcRepresentationItem">IfcRepresentationItem to parse.</param>
        RepresentationData ParseRepresentationItem(IfcRepresentationItem ifcRepresentationItem);
        /// <summary>
        /// Returns true, if it is an appropriate parser for <paramref name="ifcRepresentationItem"/>.
        /// </summary>
        /// <param name="ifcRepresentationItem">IfcRepresentationItem that will be checked if it can be parsed with this parser.</param>
        bool CanParse(IfcRepresentationItem ifcRepresentationItem);
    }
}
