using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Representations.DoorRepresentations
{
    internal static class DoorSchematicVisualizationHelper
    {
        public static List<Vector3> CollectPointsForSchematicVisualization(Door door)
        {
            var points = new List<Vector3>();

            if (door.OpeningSide == DoorOpeningSide.Undefined || door.OpeningType == DoorOpeningType.Undefined)
            {
                return points;
            }

            if (door.OpeningSide != DoorOpeningSide.LeftHand)
            {
                points.AddRange(CollectSchematicVisualizationLines(door, false, false, 90));
            }

            if (door.OpeningSide != DoorOpeningSide.RightHand)
            {
                points.AddRange(CollectSchematicVisualizationLines(door, true, false, 90));
            }

            if (door.OpeningType == DoorOpeningType.SingleSwing)
            {
                return points;
            }

            if (door.OpeningSide != DoorOpeningSide.LeftHand)
            {
                points.AddRange(CollectSchematicVisualizationLines(door, false, true, 90));
            }

            if (door.OpeningSide != DoorOpeningSide.RightHand)
            {
                points.AddRange(CollectSchematicVisualizationLines(door, true, true, 90));
            }

            return points;
        }

        private static List<Vector3> CollectSchematicVisualizationLines(Door door, bool leftSide, bool inside, double angle)
        {
            var fullDoorWidthWithoutFrame = door.GetFullDoorWidthWithoutFrame();

            // Depending on which side door in there are different offsets.
            var doorOffset = leftSide ? fullDoorWidthWithoutFrame / 2 : -fullDoorWidthWithoutFrame / 2;
            var horizontalOffset = leftSide ? Door.DOOR_THICKNESS : -Door.DOOR_THICKNESS;
            var verticalOffset = inside ? Door.DOOR_THICKNESS : -Door.DOOR_THICKNESS;
            var widthOffset = inside ? door.ClearWidth : -door.ClearWidth;

            // Draw open door silhouette rectangle.
            Vector3 corner = Vector3.XAxis * doorOffset;
            var c0 = corner + Vector3.YAxis * verticalOffset;
            var c1 = c0 + Vector3.YAxis * widthOffset;
            var c2 = c1 - Vector3.XAxis * horizontalOffset;
            var c3 = c0 - Vector3.XAxis * horizontalOffset;

            // Rotate silhouette is it's need to be drawn as partially open.
            if (!angle.ApproximatelyEquals(90))
            {
                double rotation = 90 - angle;
                if (!leftSide)
                {
                    rotation = -rotation;
                }

                if (!inside)
                {
                    rotation = -rotation;
                }

                Transform t = new Transform();
                t.RotateAboutPoint(c0, Vector3.ZAxis, rotation);
                c1 = t.OfPoint(c1);
                c2 = t.OfPoint(c2);
                c3 = t.OfPoint(c3);
            }
            List<Vector3> points = new List<Vector3>() { c0, c1, c1, c2, c2, c3, c3, c0 };

            // Calculated correct arc angles based on door orientation.
            double adjustedAngle = inside ? angle : -angle;
            double anchorAngle = leftSide ? 180 : 0;
            double endAngle = leftSide ? 180 - adjustedAngle : adjustedAngle;
            if (endAngle < 0)
            {
                endAngle = 360 + endAngle;
                anchorAngle = 360;
            }

            // If arc is constructed from bigger angle to smaller is will have incorrect domain 
            // with max being smaller than min and negative length.
            // ToPolyline will return 0 points for it.
            // Until it's fixed angles should be aligned manually.
            bool flipEnds = endAngle < anchorAngle;
            if (flipEnds)
            {
                (anchorAngle, endAngle) = (endAngle, anchorAngle);
            }

            // Draw the arc from closed door to opened door.
            Arc arc = new Arc(c0, door.ClearWidth, anchorAngle, endAngle);
            var tessalatedArc = arc.ToPolyline((int)(Math.Abs(angle) / 2));
            for (int i = 0; i < tessalatedArc.Vertices.Count - 1; i++)
            {
                points.Add(tessalatedArc.Vertices[i]);
                points.Add(tessalatedArc.Vertices[i + 1]);
            }

            return points;
        }
    }
}
