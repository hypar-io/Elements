using Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction;
using IFC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.Converters
{
    internal interface IFromIfcProductConverter
    {
        GeometricElement ConvertToElement(IfcProduct product, RepresentationData representationData, List<string> constructionErrors);
        bool Matches(IfcProduct ifcProduct);
    }
}
