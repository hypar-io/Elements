using Elements.Geometry;
using IFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Serialization.IFC.Serialization.IFC.IFCToElementConverters
{
    internal class IfcFloorToFloorConverter : IIfcProductToElementConverter
    {
        public Element ConvertToElement(IfcProduct ifcProduct, List<string> constructionErrors)
        {
            if (!(ifcProduct is IfcSlab slab))
            {
                return null;
            }

            var transform = new Transform();
            transform.Concatenate(slab.ObjectPlacement.ToTransform());
            // Console.WriteLine($"IfcSlab transform:\n{transform}\n");

            // Check if the slab is contained in a building storey
            foreach (var cis in slab.ContainedInStructure)
            {
                transform.Concatenate(cis.RelatingStructure.ObjectPlacement.ToTransform());
            }

            var repItems = slab.Representation.Representations.SelectMany(r => r.Items);
            if (!repItems.Any())
            {
                throw new Exception("The provided IfcSlab does not have any representations.");
            }

            var solid = slab.RepresentationsOfType<IfcExtrudedAreaSolid>().FirstOrDefault();
            if (solid == null)
            {
                return null;
            }

            var outline = (Polygon)solid.SweptArea.ToCurve();
            var solidTransform = solid.Position.ToTransform();

            solidTransform.Concatenate(transform);
            var floor = new Floor(new Profile(outline), (IfcLengthMeasure)solid.Depth,
                solidTransform, BuiltInMaterials.Concrete, null, false, IfcGuid.FromIfcGUID(slab.GlobalId));

            return floor;
        }

        public bool Matches(IfcProduct ifcProduct)
        {
            return ifcProduct is IfcSlab;
        }
    }
}
