using Elements.Geometry.Solids;
using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.BuildingElements.Door
{
    internal static class DoorRepresentationFactory
    {
        /// <summary>
        /// Create a solid representation of a <paramref name="door"/>.
        /// </summary>
        /// <param name="door">Parameters of <paramref name="door"/> 
        /// will be used for the representation creation.</param>
        /// <returns>A solid representation, created from properties of <paramref name="door"/>.</returns>
        public static RepresentationInstance CreateSolidDoorRepresentation(Door door)
        {
            double fullDoorWidthWithoutFrame = door.GetFullDoorWidthWithoutFrame();

            Vector3 left = Vector3.XAxis * fullDoorWidthWithoutFrame / 2;
            Vector3 right = Vector3.XAxis.Negate() * fullDoorWidthWithoutFrame / 2;

            var doorPolygon = new Polygon(new List<Vector3>() {
                left + Vector3.YAxis * Door.DOOR_THICKNESS,
                left - Vector3.YAxis * Door.DOOR_THICKNESS,
                right - Vector3.YAxis * Door.DOOR_THICKNESS,
                right + Vector3.YAxis * Door.DOOR_THICKNESS});

            var doorPolygons = new List<Polygon>();

            if (door.OpeningSide == DoorOpeningSide.DoubleDoor)
            {
                doorPolygons = doorPolygon.Split(new Polyline(new Vector3(0, Door.DOOR_THICKNESS, 0), new Vector3(0, -Door.DOOR_THICKNESS, 0)));
            }
            else
            {
                doorPolygons.Add(doorPolygon);
            }

            var doorExtrusions = new List<SolidOperation>();

            foreach (var polygon in doorPolygons)
            {
                var doorExtrude = new Extrude(new Profile(polygon.Offset(-0.005)[0]), door.ClearHeight, Vector3.ZAxis);
                doorExtrusions.Add(doorExtrude);
            }

            var frameLeft = left + Vector3.XAxis * Door.DOOR_FRAME_WIDTH;
            var frameRight = right - Vector3.XAxis * Door.DOOR_FRAME_WIDTH;
            var frameOffset = Vector3.YAxis * Door.DOOR_FRAME_THICKNESS;
            var doorFramePolygon = new Polygon(new List<Vector3>() {
                left + Vector3.ZAxis * door.ClearHeight - frameOffset,
                left - frameOffset,
                frameLeft - frameOffset,
                frameLeft + Vector3.ZAxis * (door.ClearHeight + Door.DOOR_FRAME_WIDTH) - frameOffset,
                frameRight + Vector3.ZAxis * (door.ClearHeight + Door.DOOR_FRAME_WIDTH) - frameOffset,
                frameRight - frameOffset,
                right - frameOffset,
                right + Vector3.ZAxis * door.ClearHeight - frameOffset });
            var doorFrameExtrude = new Extrude(new Profile(doorFramePolygon), Door.DOOR_FRAME_THICKNESS * 2, Vector3.YAxis);
            doorExtrusions.Add(doorFrameExtrude);

            var solidRep = new SolidRepresentation(doorExtrusions);
            var repInstance = new RepresentationInstance(solidRep, door.Material, true);
            return repInstance;
        }
    }
}
