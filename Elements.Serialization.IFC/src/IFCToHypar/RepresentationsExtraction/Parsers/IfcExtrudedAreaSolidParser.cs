using Elements.Geometry;
using Elements.Geometry.Solids;
using IFC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction.Parsers
{
    internal class IfcExtrudedAreaSolidParser : FromIfcElementRepresentationExtractor
    {
        public IfcExtrudedAreaSolidParser(MaterialExtractor materialExtractor) : base(materialExtractor)
        {
        }

        public override bool CanParse(IfcRepresentationItem ifcRepresentationItem)
        {
            return ifcRepresentationItem is IfcExtrudedAreaSolid;
        }

        protected override ElementRepresentation ExtractElementRepresentation(IfcRepresentationItem ifcRepresentationItem)
        {
            if (ifcRepresentationItem is not IfcExtrudedAreaSolid ifcSolid)
            {
                return null;
            }

            var profile = ifcSolid.SweptArea.ToProfile();
            if (profile == null)
            {
                throw new NotImplementedException($"{profile.GetType().Name} is not supported for IfcExtrudedAreaSolid.");
            }

            double height = (IfcLengthMeasure)ifcSolid.Depth;

            var solidTransform = ifcSolid.Position.ToTransform();
            var direction = ifcSolid.ExtrudedDirection.ToVector3();
            var extrude = new Extrude(solidTransform.OfProfile(profile),
                                        height,
                                        solidTransform.OfVector(direction).Unitized(),
                                        false);

            return new SolidRepresentation(extrude);
        }
    }
}
