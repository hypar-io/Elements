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
            var doorWidth = (IfcLengthMeasure) ifcDoor.OverallWidth;
            var doorHeight = (IfcLengthMeasure) ifcDoor.OverallHeight;

            var result = new Door(doorWidth,
                                  doorHeight,
                                  openingSide,
                                  openingType,
                                  repData.Transform,
                                  repData.Material,
                                  new Representation(repData.SolidOperations),
                                  IfcGuid.FromIfcGUID(ifcDoor.GlobalId),
                                  ifcDoor.Name
                                  );

            return result;
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
