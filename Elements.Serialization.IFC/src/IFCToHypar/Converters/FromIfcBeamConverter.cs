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
        public Element ConvertToElement(IfcProduct ifcProduct, RepresentationData repData, List<string> constructionErrors)
        {
            if (!(ifcProduct is IfcBeam ifcBeam))
            {
                return null;
            }
            
            var elementTransform = repData.Transform;

            if (repData.Extrude == null)
            {
                return null;
            }

            var representation = new Representation(repData.SolidOperations);

            var cl = new Line(Vector3.Origin, repData.Extrude.Direction, repData.Extrude.Height);
            var result = new Beam(cl.TransformedLine(repData.ExtrudeTransform),
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
