using Elements.Geometry;
using Elements.Geometry.Solids;
using IFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction.Parsers
{
    internal class IfcFacetedBrepParser : IIfcRepresentationParser
    {
        public bool Matches(IfcRepresentationItem ifcRepresentationItem)
        {
            return ifcRepresentationItem is IfcFacetedBrep;
        }

        public RepresentationData ParseRepresentationItem(IfcRepresentationItem ifcRepresentationItem)
        {
            if (!(ifcRepresentationItem is IfcFacetedBrep ifcSolid))
            {
                return null;
            }

            var shell = ifcSolid.Outer;
            var newSolid = new Solid();
            for (var i = 0; i < shell.CfsFaces.Count; i++)
            {
                var f = shell.CfsFaces[i];
                foreach (var b in f.Bounds)
                {
                    var loop = (IfcPolyLoop)b.Bound;
                    var poly = loop.Polygon.ToPolygon();
                    newSolid.AddFace(poly);
                }
            }

            var solidOperation = new ImportSolid(newSolid);
            return new RepresentationData(new List<SolidOperation>() { solidOperation });
        }
    }
}
