using Elements.Geometry;
using IFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.Converters
{
    internal class IfcBeamToBeamConverter : IIfcProductToElementConverter
    {
        public Element ConvertToElement(IfcProduct ifcProduct, List<string> constructionErrors)
        {
            if (!(ifcProduct is IfcBeam ifcBeam))
            {
                return null;
            }

            var elementTransform = ifcBeam.ObjectPlacement.ToTransform();

            var solid = ifcBeam.RepresentationsOfType<IfcExtrudedAreaSolid>().FirstOrDefault();

            // foreach (var cis in beam.ContainedInStructure)
            // {
            //     cis.RelatingStructure.ObjectPlacement.ToTransform().Concatenate(transform);
            // }

            if (solid != null)
            {
                var solidTransform = solid.Position.ToTransform();

                var c = solid.SweptArea.ToCurve();
                if (c is Polygon polygon)
                {
                    var cl = new Line(Vector3.Origin,
                        solid.ExtrudedDirection.ToVector3(), (IfcLengthMeasure)solid.Depth);
                    var result = new Beam(cl.TransformedLine(solidTransform),
                                          new Profile(polygon),
                                          0,
                                          0,
                                          0,
                                          elementTransform,
                                          BuiltInMaterials.Steel,
                                          null,
                                          false,
                                          IfcGuid.FromIfcGUID(ifcBeam.GlobalId),
                                          ifcBeam.Name);
                    return result;
                }
            }
            return null;
        }

        public bool Matches(IfcProduct ifcProduct)
        {
            return ifcProduct is IfcBeam;
        }
    }
}
