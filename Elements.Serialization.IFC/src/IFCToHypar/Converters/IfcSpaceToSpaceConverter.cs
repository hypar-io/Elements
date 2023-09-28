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
    internal class IfcSpaceToSpaceConverter : IIfcProductToElementConverter
    {
        private static readonly Material DEFAULT_MATERIAL = new Material("space", new Color(1.0f, 0.0f, 1.0f, 0.5f), 0.0f, 0.0f);

        public Element ConvertToElement(IfcProduct product, RepresentationData repData, List<string> constructionErrors)
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

            var solid = repData.SolidOperations.First()?.Solid;

            if (solid == null)
            {
                return null;
            }

            return new Space(solid, repData.Transform, elementMaterial, false, Guid.NewGuid(), ifcSpace.Name);
        }

        public bool Matches(IfcProduct ifcProduct)
        {
            return ifcProduct is IfcSpace;
        }
    }
}
