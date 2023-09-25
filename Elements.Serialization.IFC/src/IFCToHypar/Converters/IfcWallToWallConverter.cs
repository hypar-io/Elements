using Elements.BuildingElements;
using Elements.Geometry;
using IFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.Converters
{
    internal class IfcWallToWallConverter : IIfcProductToElementConverter
    {
        public Element ConvertToElement(IfcProduct ifcProduct, List<string> constructionErrors)
        {
            if (!(ifcProduct is IfcWall wall))
            {
                return null;
            }

            var transform = new Transform();
            transform.Concatenate(wall.ObjectPlacement.ToTransform());

            // An extruded face solid.
            var solid = wall.RepresentationsOfType<IfcExtrudedAreaSolid>().FirstOrDefault();
            if (solid == null)
            {
                // It's possible that the rep is a boolean.
                var boolean = wall.RepresentationsOfType<IfcBooleanClippingResult>().FirstOrDefault();
                if (boolean != null)
                {
                    solid = boolean.FirstOperand.Choice as IfcExtrudedAreaSolid;
                    if (solid == null)
                    {
                        solid = boolean.SecondOperand.Choice as IfcExtrudedAreaSolid;
                    }
                }

                // if(solid == null)
                // {
                //     throw new Exception("No usable solid was found when converting an IfcWallStandardCase to a Wall.");
                // }
            }

            // A centerline wall with material layers.
            // var axis = (Polyline)wall.RepresentationsOfType<IfcPolyline>().FirstOrDefault().ToICurve(false);

            foreach (var cis in wall.ContainedInStructure)
            {
                cis.RelatingStructure.ObjectPlacement.ToTransform().Concatenate(transform);
            }

            if (solid != null)
            {
                var c = solid.SweptArea.ToCurve();
                if (c is Polygon polygon)
                {
                    transform.Concatenate(solid.Position.ToTransform());
                    var result = new Wall(polygon,
                                          (IfcLengthMeasure)solid.Depth,
                                          null,
                                          transform,
                                          null,
                                          false,
                                          IfcGuid.FromIfcGUID(wall.GlobalId),
                                          wall.Name);
                    return result;
                }
            }
            return null;
        }

        public bool Matches(IfcProduct ifcProduct)
        {
            return ifcProduct is IfcWall;
        }
    }
}
