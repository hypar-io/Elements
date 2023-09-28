using Elements.Geometry.Solids;
using IFC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction.Parsers
{
    internal class IfcBooleanClippingResultParser : IIfcRepresentationParser
    {
        public bool Matches(IfcRepresentationItem ifcRepresentationItem)
        {
            return ifcRepresentationItem is IfcBooleanClippingResult;
        }

        // TODO: It is possible that an operand is also IfcBooleanClippingResult or some other representation
        // item type.
        public RepresentationData ParseRepresentationItem(IfcRepresentationItem ifcRepresentationItem)
        {
            if (!(ifcRepresentationItem is IfcBooleanClippingResult ifcBooleanClippingResult))
            {
                return null;
            }

            var solidRep = ifcBooleanClippingResult.FirstOperand.Choice as IfcExtrudedAreaSolid;
            if (solidRep == null)
            {
                solidRep = ifcBooleanClippingResult.SecondOperand.Choice as IfcExtrudedAreaSolid;
            }

            if (solidRep == null)
            {
                return null;
            }

            var extrudeParser = new IfcExtrudedAreaSolidParser();
            return extrudeParser.ParseRepresentationItem(solidRep);
        }
    }
}
