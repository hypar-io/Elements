using Elements.Geometry;
using Elements.Geometry.Solids;
using IFC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction.Parsers
{
    internal interface IIfcRepresentationParser
    {
        RepresentationData ParseRepresentationItem(IfcRepresentationItem ifcRepresentationItem);
        bool Matches(IfcRepresentationItem ifcRepresentationItem);
    }
}
