using Elements.Geometry.Solids;
using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Text;
using Elements.CoreModels;

namespace Elements
{
    internal class DoorRepresentationProvider : RepresentationProvider<Door>
    {
        private readonly Dictionary<DoorProperties, List<RepresentationInstance>> _doorTypeToRepresentations;

        public DoorRepresentationProvider()
        {
            _doorTypeToRepresentations = new Dictionary<DoorProperties, List<RepresentationInstance>>();
        }

        public override List<RepresentationInstance> GetInstances(Door door)
        {
            var doorProps = new DoorProperties(door);

            if (_doorTypeToRepresentations.TryGetValue(doorProps, out var representations))
            {
                return representations;
            }

            var representationInstances = new List<RepresentationInstance>()
            {
                CreateSolidDoorRepresentation(door),
                CreateCurveDoorRepresentation(door)
            };

            _doorTypeToRepresentations[doorProps] = representationInstances;
            return representationInstances;
        }

        /// <summary>
        /// Create a solid representation of a <paramref name="door"/>.
        /// </summary>
        /// <param name="door">Parameters of <paramref name="door"/> 
        /// will be used for the representation creation.</param>
        /// <returns>A solid representation, created from properties of <paramref name="door"/>.</returns>
        private static RepresentationInstance CreateSolidDoorRepresentation(Door door)
        {
            double fullDoorWidthWithoutFrame = door.GetFullDoorWidthWithoutFrame();

            Vector3 left = Vector3.XAxis * (fullDoorWidthWithoutFrame / 2);
            Vector3 right = Vector3.XAxis.Negate() * (fullDoorWidthWithoutFrame / 2);

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

        /// <summary>
        /// Create a curve 2D representation of a <paramref name="door"/>.
        /// </summary>
        /// <param name="door">Parameters of <paramref name="door"/> 
        /// will be used for the representation creation.</param>
        /// <returns>A curve 2D representation, created from properties of <paramref name="door"/>.</returns>
        private static RepresentationInstance CreateCurveDoorRepresentation(Door door)
        {
            var points = CollectPointsForSchematicVisualization(door);
            var curve = new IndexedPolycurve(points);
            var curveRep = new CurveRepresentation(curve, false);
            var repInstance = new RepresentationInstance(curveRep, BuiltInMaterials.Black);
            return repInstance;
        }

        private static List<Vector3> CollectPointsForSchematicVisualization(Door door)
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
