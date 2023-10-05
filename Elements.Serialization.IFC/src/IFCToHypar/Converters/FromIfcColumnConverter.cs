using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction;
using IFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.Converters
{
    internal class FromIfcColumnConverter : IFromIfcProductConverter
    {
        public GeometricElement ConvertToElement(IfcProduct ifcProduct, RepresentationData repData, List<string> constructionErrors)
        {
            if (!(ifcProduct is IfcColumn ifcColumn))
            {
                return null;
            }

            var elementTransform = repData.Transform;

            if (repData.Extrude == null)
            {
                return null;
            }

            var result = new Column(repData.ExtrudeTransform.Origin,
                                    repData.Extrude.Height,
                                    null,
                                    repData.Extrude.Profile,
                                    0,
                                    0,
                                    0,
                                    elementTransform,
                                    repData.Material,
                                    new Representation(repData.SolidOperations),
                                    false,
                                    IfcGuid.FromIfcGUID(ifcColumn.GlobalId),
                                    ifcColumn.Name);
            return result;
        }

        public bool Matches(IfcProduct ifcProduct)
        {
            return ifcProduct is IfcColumn;
        }
    }
}
