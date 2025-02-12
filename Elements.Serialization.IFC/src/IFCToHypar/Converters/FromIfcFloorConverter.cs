using Elements;
using Elements.Geometry;
using Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction;
using IFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.Converters
{
    internal class FromIfcFloorConverter : IFromIfcProductConverter
    {
        public GeometricElement ConvertToElement(IfcProduct ifcProduct, RepresentationData repData, List<string> constructionErrors)
        {
            if (ifcProduct is not IfcSlab slab)
            {
                return null;
            }

            if (repData.Extrude == null)
            {
                constructionErrors.Add($"#{ifcProduct.StepId}: Conversion of IfcSlab without extrude or mapped item representation to Floor is not supported.");
                return null;
            }

            var localTransform = repData.Extrude.LocalTransform ?? new Transform();
            var floor = new Floor(repData.Extrude.Profile.Transformed(localTransform),
                                  repData.Extrude.Height,
                                  transform: repData.Transform,
                                  isElementDefinition: false,
                                  id: IfcGuid.FromIfcGUID(slab.GlobalId),
                                  name: slab.Name ?? "")
            {
                RepresentationInstances = repData.RepresentationInstances
            };

            return floor;
        }

        public bool CanConvert(IfcProduct ifcProduct)
        {
            return ifcProduct is IfcSlab;
        }
    }
}
