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
    internal class FromIfcWallConverter : IFromIfcProductConverter
    {
        
        public GeometricElement ConvertToElement(IfcProduct ifcProduct, RepresentationData repData, List<string> constructionErrors)
        {
            if (ifcProduct is not IfcWall wall)
            {
                return null;
            }

            if (repData.Extrude == null)
            {
                constructionErrors.Add($"#{ifcProduct.StepId}: Conversion of IfcWall without extrude or mapped item representation to Wall is not supported.");
                return null;
            }

            var result = new Wall(repData.Extrude.Profile,
                                  repData.Extrude.Height,
                                  transform: repData.Transform,
                                  isElementDefinition: false,
                                  id: IfcGuid.FromIfcGUID(wall.GlobalId),
                                  name: wall.Name)
            {
                RepresentationInstances = repData.RepresentationInstances
            };
            return result;
        }

        public bool CanConvert(IfcProduct ifcProduct)
        {
            return ifcProduct is IfcWall;
        }
    }
}
