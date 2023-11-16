using Elements.Geometry;
using Elements.Serialization.IFC.IFCToHypar.RepresentationsExtraction;
using Elements.BIM;
using IFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Serialization.IFC.IFCToHypar.Converters
{
    internal class FromIfcDoorConverter : IFromIfcProductConverter
    {
        public GeometricElement ConvertToElement(IfcProduct ifcProduct, RepresentationData repData, List<string> constructionErrors)
        {
            if (!(ifcProduct is IfcDoor ifcDoor))
            {
                return null;
            }

            if (ifcDoor.PredefinedType != IfcDoorTypeEnum.DOOR)
            {
                constructionErrors.Add($"#{ifcProduct.StepId}: Doors of type {ifcDoor.PredefinedType} are not supported yet.");
                return null;
            }

            var openingSide = ifcDoor.GetDoorOpeningSide();
            var openingType = ifcDoor.GetDoorOpeningType();

            if (openingSide == DoorOpeningSide.Undefined || openingType == DoorOpeningType.Undefined)
            {
                constructionErrors.Add($"#{ifcProduct.StepId}: Doors of operation type {ifcDoor.OperationType} are not supported yet.");
                return null;
            }

            // TODO: Implement during the connections establishment.
            //var wall = GetWallFromDoor(ifcDoor, allWalls);
            var doorWidth = (IfcLengthMeasure)ifcDoor.OverallWidth;
            var doorHeight = (IfcLengthMeasure)ifcDoor.OverallHeight;

            var result = new Door(doorWidth,
                                  doorHeight,
                                  Door.DOOR_THICKNESS,
                                  openingSide,
                                  openingType,
                                  repData.Transform,
                                  repData.Material,
                                  new Representation(repData.SolidOperations),
                                  false,
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

            return matchingWalls.FirstOrDefault();
        }

        public bool CanConvert(IfcProduct ifcProduct)
        {
            return ifcProduct is IfcDoor;
        }
    }
}
