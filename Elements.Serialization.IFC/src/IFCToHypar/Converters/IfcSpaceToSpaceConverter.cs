using Elements.Geometry.Solids;
using Elements.Geometry;
using IFC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Elements.Serialization.IFC.IFCToHypar.Converters
{
    internal class IfcSpaceToSpaceConverter : IIfcProductToElementConverter
    {
        public Element ConvertToElement(IfcProduct product, List<string> constructionErrors)
        {
            if (!(product is IfcSpace ifcSpace))
            {
                return null;
            }

            var transform = new Transform();

            var repItems = ifcSpace.Representation.Representations.SelectMany(r => r.Items);
            if (!repItems.Any())
            {
                throw new Exception("The provided IfcSlab does not have any representations.");
            }

            var localPlacement = ifcSpace.ObjectPlacement.ToTransform();
            transform.Concatenate(localPlacement);

            var foundSolid = repItems.First();
            var material = new Material("space", new Color(1.0f, 0.0f, 1.0f, 0.5f), 0.0f, 0.0f);
            if (foundSolid.GetType() == typeof(IfcExtrudedAreaSolid))
            {
                var solid = (IfcExtrudedAreaSolid)foundSolid;
                var profileDef = (IfcArbitraryClosedProfileDef)solid.SweptArea;
                transform.Concatenate(solid.Position.ToTransform());
                var pline = (IfcPolyline)profileDef.OuterCurve;
                var outline = pline.ToPolygon(true);
                var result = new Space(new Profile(outline), (IfcLengthMeasure)solid.Depth, material, transform, null, false, IfcGuid.FromIfcGUID(ifcSpace.GlobalId), ifcSpace.Name);
                return result;
            }
            else if (foundSolid.GetType() == typeof(IfcFacetedBrep))
            {
                var solid = (IfcFacetedBrep)foundSolid;
                var shell = solid.Outer;
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
                var result = new Space(newSolid, transform, null, false, Guid.NewGuid(), ifcSpace.Name);

                return result;
            }

            return null;
        }

        public bool Matches(IfcProduct ifcProduct)
        {
            return ifcProduct is IfcSpace;
        }
    }
}
