using Elements;
using Elements.Geometry;
using Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction;
using IFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.Converters
{
    internal class IfcWallToWallConverter : IIfcProductToElementConverter
    {
        
        public Element ConvertToElement(IfcProduct ifcProduct, RepresentationData repData, List<string> constructionErrors)
        {
            if (!(ifcProduct is IfcWall wall))
            {
                return null;
            }

            if (repData.Extrude == null)
            {
                return null;
            }

            var result = new Wall(repData.Extrude.Profile,
                                  repData.Extrude.Height,
                                  repData.Material,
                                  repData.Transform,
                                  new Representation(repData.SolidOperations),
                                  false,
                                  IfcGuid.FromIfcGUID(wall.GlobalId),
                                  wall.Name);
            return result;
        }

        public bool Matches(IfcProduct ifcProduct)
        {
            return ifcProduct is IfcWall;
        }
    }
}
