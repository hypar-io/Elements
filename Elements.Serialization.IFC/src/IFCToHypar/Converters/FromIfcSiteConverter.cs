using Elements.Geometry;
using Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction;
using IFC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.Converters
{
    internal class FromIfcSiteConverter : IFromIfcProductConverter
    {
        public GeometricElement ConvertToElement(IfcProduct product, RepresentationData repData, List<string> constructionErrors)
        {
            if (!(product is IfcSite ifcSite))
            {
                return null;
            }

            var geom = new GeometricElement(repData.Transform,
                                            repData.Material ?? BuiltInMaterials.Default,
                                            new Representation(repData.SolidOperations),
                                            false,
                                            IfcGuid.FromIfcGUID(ifcSite.GlobalId),
                                            ifcSite.Name);
            return geom;
        }

        public bool Matches(IfcProduct ifcProduct)
        {
            return ifcProduct is IfcSite;
        }
    }
}
