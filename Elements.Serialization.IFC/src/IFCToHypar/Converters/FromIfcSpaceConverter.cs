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
        private static readonly Material DEFAULT_MATERIAL = new Material("space", new Color(1.0f, 0.0f, 1.0f, 0.5f), 0.0f, 0.0f);

        public GeometricElement ConvertToElement(IfcProduct product, RepresentationData repData, List<string> constructionErrors)
        {
            if (!(product is IfcSpace ifcSpace))
            {
                return null;
            }

            var elementMaterial = repData.Material ?? DEFAULT_MATERIAL;

            var extrude = repData.Extrude;

            if (extrude != null)
            {

                var result = new Space(extrude.Profile,
                                       extrude.Height,
                                       elementMaterial,
                                       repData.Transform,
                                       new Representation(repData.SolidOperations),
                                       false,
                                       Guid.NewGuid(),
                                       ifcSpace.Name);
                return result;
            }

            var solid = repData.SolidOperations.FirstOrDefault()?.Solid;

            if (solid == null)
            {
                constructionErrors.Add($"#{product.StepId}: Conversion of IfcSpace without solid or mapped item representation to Space is not supported.");
                return null;
            }

            return new Space(solid, repData.Transform, elementMaterial, false, Guid.NewGuid(), ifcSpace.Name);
        }

        public bool CanConvert(IfcProduct ifcProduct)
        {
            return ifcProduct is IfcSpace;
        }
    }
}
