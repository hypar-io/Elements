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
    internal class FromIfcFloorConverter : IFromIfcProductConverter
    {
        public Element ConvertToElement(IfcProduct ifcProduct, RepresentationData repData, List<string> constructionErrors)
        {
            if (!(ifcProduct is IfcSlab slab))
            {
                return null;
            }

            if (repData.Extrude == null)
            {
                return null;
            }

            var floor = new Floor(repData.Extrude.Profile,
                                  repData.Extrude.Height,
                                  repData.Transform,
                                  repData.Material,
                                  new Representation(repData.SolidOperations),
                                  false,
                                  IfcGuid.FromIfcGUID(slab.GlobalId));

            return floor;
        }

        public bool Matches(IfcProduct ifcProduct)
        {
            return ifcProduct is IfcSlab;
        }
    }
}
