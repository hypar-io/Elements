using Elements.Geometry.Solids;
using Elements.Geometry;
using IFC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction;

namespace Elements.Serialization.IFC.IFCToHypar.Converters
{
    internal class FromIfcSpaceConverter : IFromIfcProductConverter
    {
        public GeometricElement ConvertToElement(IfcProduct product, RepresentationData repData, List<string> constructionErrors)
        {
            if (product is not IfcSpace ifcSpace)
            {
                return null;
            }

            var extrude = repData.Extrude;

            if (extrude != null)
            {
                var result = new Space(extrude.Profile,
                                       extrude.Height,
                                       transform: repData.Transform,
                                       isElementDefinition: false,
                                       id: Guid.NewGuid(),
                                       name: ifcSpace.Name ?? "");

                return result;
            }

            var solidOperations = repData.GetSolidOperations();
            var solid = solidOperations.FirstOrDefault()?.Solid;

            if (solid == null)
            {
                constructionErrors.Add($"#{product.StepId}: Conversion of IfcSpace without solid or mapped item representation to Space is not supported.");
                return null;
            }

            var space = new Space(solid, transform: repData.Transform, isElementDefinition: false, id: Guid.NewGuid(), name: ifcSpace.Name)
            {
                RepresentationInstances = repData.RepresentationInstances
            };

            return space;
        }

        public bool CanConvert(IfcProduct ifcProduct)
        {
            return ifcProduct is IfcSpace;
        }
    }
}
