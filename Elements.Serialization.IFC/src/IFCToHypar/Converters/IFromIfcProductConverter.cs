using Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction;
using IFC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.Converters
{
    /// <summary>Converts an IfcProduct to a GeometricElement.</summary>
    internal interface IFromIfcProductConverter
    {
        /// <summary>
        /// Converts <paramref name="ifcProduct"/> to a GeometricElement. Returns null if the conversion was unsuccessful.
        /// </summary>
        /// <param name="ifcProduct">IfcProduct to convert to a GeometricElement.</param>
        /// <param name="representationData">Parsed Representation of <paramref name="ifcProduct"/>.</param>
        /// <param name="constructionErrors">The list of construction errors that appeared during conversion.</param>
        GeometricElement ConvertToElement(IfcProduct ifcProduct, RepresentationData representationData, List<string> constructionErrors);
        /// <summary>
        /// Returns true, if it is an appropriate converter for <paramref name="ifcProduct"/>.
        /// </summary>
        /// <param name="ifcProduct">IfcProduct that will be checked if it can be converted with this converter.</param>
        bool CanConvert(IfcProduct ifcProduct);
    }
}
