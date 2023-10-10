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
    internal class FromIfcBeamConverter : IFromIfcProductConverter
    {
        public GeometricElement ConvertToElement(IfcProduct ifcProduct, RepresentationData repData, List<string> constructionErrors)
        {
            if (!(ifcProduct is IfcBeam ifcBeam))
            {
                return null;
            }
            
            var elementTransform = repData.Transform;

            if (repData.Extrude == null)
            {
                constructionErrors.Add($"#{ifcProduct.StepId}: Conversion of IfcBeam without extrude or mapped item representation to Beam is not supported.");
                return null;
            }

            var representation = new Representation(repData.SolidOperations);

            var centerLine = new Line(Vector3.Origin, repData.Extrude.Direction, repData.Extrude.Height);
            var transformedLine = centerLine.TransformedLine(repData.ExtrudeTransform);
            var result = new Beam(transformedLine,
                                    repData.Extrude.Profile,
                                    0,
                                    0,
                                    0,
                                    elementTransform,
                                    repData.Material,
                                    representation,
                                    false,
                                    IfcGuid.FromIfcGUID(ifcBeam.GlobalId),
                                    ifcBeam.Name);

            return result;
        }

        public bool Matches(IfcProduct ifcProduct)
        {
            return ifcProduct is IfcBeam;
        }
    }
}
