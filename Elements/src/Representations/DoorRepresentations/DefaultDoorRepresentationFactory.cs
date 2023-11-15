using Elements.Geometry;
using Elements.Geometry.Solids;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Representations.DoorRepresentations
{
    internal class DefaultDoorRepresentationFactory : DoorRepresentationFactory
    {
        public override RepresentationInstance CreateDoorCurveRepresentation(Door door)
        {
            var points = DoorSchematicVisualizationHelper.CollectPointsForSchematicVisualization(door);
            var curve = new IndexedPolycurve(points);
            var curveRep = new CurveRepresentation(curve, false);
            var repInstance = new RepresentationInstance(curveRep, BuiltInMaterials.Black);
            return repInstance;
        }

        public override RepresentationInstance CreateDoorFrameRepresentation(Door door)
        {
            double fullDoorWidthWithoutFrame = door.GetFullDoorWidthWithoutFrame();
            Vector3 left = Vector3.XAxis * (fullDoorWidthWithoutFrame / 2);
            Vector3 right = Vector3.XAxis.Negate() * (fullDoorWidthWithoutFrame / 2);

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
            
            var solidRep = new SolidRepresentation(doorFrameExtrude);
            var repInstance = new RepresentationInstance(solidRep, door.Material, true);
            return repInstance;
        }

        public override RepresentationInstance CreateDoorSolidRepresentation(Door door)
        {
            double fullDoorWidthWithoutFrame = door.GetFullDoorWidthWithoutFrame();

            Vector3 left = Vector3.XAxis * (fullDoorWidthWithoutFrame / 2);
            Vector3 right = Vector3.XAxis.Negate() * (fullDoorWidthWithoutFrame / 2);

            var doorPolygon = new Polygon(new List<Vector3>() {
                left + Vector3.YAxis * door.Thickness,
                left - Vector3.YAxis * door.Thickness,
                right - Vector3.YAxis * door.Thickness,
                right + Vector3.YAxis * door.Thickness});

            var doorPolygons = new List<Polygon>();

            if (door.OpeningSide == DoorOpeningSide.DoubleDoor)
            {
                doorPolygons = doorPolygon.Split(new Polyline(new Vector3(0, door.Thickness, 0), new Vector3(0, -door.Thickness, 0)));
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

            var solidRep = new SolidRepresentation(doorExtrusions);
            var repInstance = new RepresentationInstance(solidRep, door.Material, true);
            return repInstance;
        }
    }
}
