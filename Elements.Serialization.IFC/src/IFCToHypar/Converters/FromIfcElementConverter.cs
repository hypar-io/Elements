using Elements.Geometry;
using Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction;
using IFC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.Converters
{
    internal class FromIfcElementConverter : IFromIfcProductConverter
    {
        public GeometricElement ConvertToElement(IfcProduct ifcProduct, RepresentationData repData, List<string> constructionErrors)
        {
            if (!CanConvert(ifcProduct))
            {
                return null;
            }

            if (repData == null)
            {
                constructionErrors.Add($"#{ifcProduct.StepId}: There was no representation for an element of type {ifcProduct.GetType()}.");
                return null;
            }

            if (repData.RepresentationInstances.Count == 0)
            {
                constructionErrors.Add($"#{ifcProduct.StepId}: {ifcProduct.GetType().Name} did not have any representation items that could be converted to RepresentationInstance.");
                return null;
            }

            var geom = new GeometricElement(transform: repData.Transform,
                                            isElementDefinition: false,
                                            id: IfcGuid.FromIfcGUID(ifcProduct.GlobalId),
                                            name: ifcProduct.Name)
            {
                RepresentationInstances = repData.RepresentationInstances
            };

            // geom.Representation.SkipCSGUnion = true;
            return geom;
        }

        public bool CanConvert(IfcProduct ifcProduct)
        {
            return ifcProduct switch
            {
                IfcBuildingElement => true,
                IfcFurnishingElement => true,
                IfcSpace => true,
                IfcSite => true,
                _ => false
            };
        }
    }
}
