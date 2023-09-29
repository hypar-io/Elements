using Elements.Geometry;
using Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction;
using IFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.Converters
{
    internal class FromIfcDoorConverter : IFromIfcProductConverter
    {
        public Element ConvertToElement(IfcProduct ifcProduct, RepresentationData repData, List<string> constructionErrors)
        {
            if (!(ifcProduct is IfcDoor ifcDoor))
            {
                return null;
            }

            if (ifcDoor.PredefinedType != IfcDoorTypeEnum.DOOR)
            {
                throw new Exception("Door types except DOOR are not supported yet.");
            }

            var openingSide = ifcDoor.GetDoorOpeningSide();
            var openingType = ifcDoor.GetDoorOpeningType();

            if (openingSide == DoorOpeningSide.Undefined || openingType == DoorOpeningType.Undefined)
            {
                throw new Exception("This DoorOperationType is not supported yet.");
            }

            // TODO: Implement during the connections establishment.
            //var wall = GetWallFromDoor(ifcDoor, allWalls);

            var result = new Door(null,
                                  repData.Transform,
                                  (IfcLengthMeasure)ifcDoor.OverallWidth,
                                  (IfcLengthMeasure)ifcDoor.OverallHeight,
                                  openingSide,
                                  openingType,
                                  repData.Material,
                                  new Representation(repData.SolidOperations)
                                  );
            return result;
        }

        private static Transform GetTransformFromIfcElement(IfcElement ifcElement)
        {
            // TODO: AC20-Institute-Var-2.ifc model contains doors with IfcFacetedBrep based representation.
            var repItems = ifcElement.Representation.Representations.SelectMany(r => r.Items);
            if (!repItems.Any())
            {
                throw new Exception("The provided IfcDoor does not have any representations.");
            }

            var containedInStructureTransform = new Transform();
            containedInStructureTransform.Concatenate(ifcElement.ObjectPlacement.ToTransform());

            // Check if the door is contained in a building storey
            foreach (var cis in ifcElement.ContainedInStructure)
            {
                containedInStructureTransform.Concatenate(cis.RelatingStructure.ObjectPlacement.ToTransform());
            }

            var repMappedItems = repItems.OfType<IfcMappedItem>();

            if (repMappedItems.Any())
            {
                var representation = repMappedItems.FirstOrDefault();
                var localOrigin = representation.MappingTarget.LocalOrigin.ToVector3();
                return new Transform(localOrigin).Concatenated(containedInStructureTransform);
            }

            var repSolidItems = repItems.OfType<IfcExtrudedAreaSolid>();

            if (repSolidItems.Any())
            {
                var representation = repSolidItems.FirstOrDefault();
                var solidTransform = representation.Position.ToTransform();
                return solidTransform.Concatenated(containedInStructureTransform);
            }

            return containedInStructureTransform;
        }

        private static Wall GetWallFromDoor(IfcDoor door, List<Wall> allWalls)
        {
            var walls = door.Decomposes.Select(rel => rel.RelatingObject).OfType<IfcWall>();

            if (!walls.Any())
            {
                return null;
            }

            var ifcWall = walls.First();
            var matchingWalls = allWalls.Where(w => w.Id.Equals(IfcGuid.FromIfcGUID(ifcWall.GlobalId)));

            return matchingWalls.Any() ? matchingWalls.First() : null;
        }

        public bool Matches(IfcProduct ifcProduct)
        {
            return ifcProduct is IfcDoor;
        }
    }
}
