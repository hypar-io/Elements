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
            if (!(ifcProduct is IfcElement ifcElement))
            {
                return null;
            }

            if (repData == null)
            {
                constructionErrors.Add($"#{ifcElement.StepId}: There was no representation for an element of type {ifcElement.GetType()}.");
                return null;
            }

            if (repData.SolidOperations.Count == 0)
            {
                constructionErrors.Add($"#{ifcElement.StepId}: {ifcElement.GetType().Name} did not have any solid operations in its representation.");
                return null;
            }

            var geom = new GeometricElement(repData.Transform,
                                            repData.Material ?? BuiltInMaterials.Default,
                                            new Representation(repData.SolidOperations),
                                            false,
                                            IfcGuid.FromIfcGUID(ifcElement.GlobalId),
                                            ifcElement.Name);

            // geom.Representation.SkipCSGUnion = true;
            return geom;
        }

        public bool Matches(IfcProduct ifcProduct)
        {
            return ifcProduct is IfcElement;
        }
    }
}
