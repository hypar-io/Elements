using Elements.Geometry;
using IFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.Converters
{
    internal class IfcColumnToColumnConverter : IIfcProductToElementConverter
    {
        public Element ConvertToElement(IfcProduct ifcProduct, List<string> constructionErrors)
        {
            if (!(ifcProduct is IfcColumn ifcColumn))
            {
                return null;
            }

            var elementTransform = ifcColumn.ObjectPlacement.ToTransform();

            var solid = ifcColumn.RepresentationsOfType<IfcExtrudedAreaSolid>().FirstOrDefault();
            foreach (var cis in ifcColumn.ContainedInStructure)
            {
                cis.RelatingStructure.ObjectPlacement.ToTransform().Concatenate(elementTransform);
            }

            if (solid != null)
            {
                var solidTransform = solid.Position.ToTransform();
                var c = solid.SweptArea.ToCurve();
                var result = new Column(solidTransform.Origin,
                                        (IfcLengthMeasure)solid.Depth,
                                        null,
                                        new Profile((Polygon)c),
                                        0,
                                        0,
                                        0,
                                        elementTransform,
                                        BuiltInMaterials.Steel,
                                        null,
                                        false,
                                        IfcGuid.FromIfcGUID(ifcColumn.GlobalId),
                                        ifcColumn.Name);
                return result;
            }
            return null;
        }

        public bool Matches(IfcProduct ifcProduct)
        {
            return ifcProduct is IfcColumn;
        }
    }
}
