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

            if (repData.SolidOperations.Count == 0)
            {
                constructionErrors.Add($"#{ifcProduct.StepId}: {ifcProduct.GetType().Name} did not have any solid operations in it's representation.");
                return null;
            }

            var geom = new GeometricElement(repData.Transform,
                                            repData.Material ?? BuiltInMaterials.Default,
                                            new Representation(repData.SolidOperations),
                                            false,
                                            IfcGuid.FromIfcGUID(ifcProduct.GlobalId),
                                            ifcProduct.Name);

            // geom.Representation.SkipCSGUnion = true;
            return geom;
        }

        public bool CanConvert(IfcProduct ifcProduct)
        {
            if (ifcProduct is IfcBuildingElement)
            {
                return true;
            }

            if (ifcProduct is IfcFurnishingElement)
            {
                return true;
            }

            if (ifcProduct is IfcSpace)
            {
                return true;
            }

            if (ifcProduct is IfcSite)
            {
                return true;
            }

            return false;
        }
    }
}
