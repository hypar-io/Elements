using Elements.Geometry.Solids;
using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Elements.BuildingElements.Door
{
    /// <summary>Definition of a door</summary>
    public class Door : GeometricElement
    {
        private readonly Material DEFAULT_MATERIAL = new Material("Door material", Colors.White);

        /// <summary>
        /// Default thickness of a door.
        /// </summary>
        public const double DOOR_THICKNESS = 0.125;
        /// <summary>
        /// Default thickness of a door frame.
        /// </summary>
        public const double DOOR_FRAME_THICKNESS = 0.15;
        /// <summary>
        /// Default width of a door frame.
        /// </summary>
        public const double DOOR_FRAME_WIDTH = 2 * 0.0254; //2 inches

        /// <summary>Door width without a frame</summary>
        public double ClearWidth { get; private set; }
        /// <summary>The opening type of the door that should be placed</summary>
        public DoorOpeningType OpeningType { get; private set; }
        /// <summary>The opening side of the door that should be placed</summary>
        public DoorOpeningSide OpeningSide { get; private set; }
        /// <summary>Height of a door without a frame.</summary>
        public double ClearHeight { get; private set; }
        /// <summary>Opening for a door.</summary>
        public Opening Opening { get; private set; }

        private readonly double _fullDoorWidthWithoutFrame;

        /// <summary>
        /// Create a door.
        /// </summary>
        /// <param name="clearWidth">The width of a single door.</param>
        /// <param name="clearHeight">The door's height.</param>
        /// <param name="openingSide">The side where the door opens.</param>
        /// <param name="openingType">The way the door opens.</param>
        /// <param name="transform">The door's transform. X-direction is aligned with the door, Y-direction is the opening direction.</param>
        /// <param name="material">The door's material.</param>
        /// <param name="representation">The door's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The door's id.</param>
        /// <param name="name">The door's name.</param>
        /// <param name="depthFront">The door's opening depth front.</param>
        /// <param name="depthBack">The door's opening depth back.</param>
        [JsonConstructor]
        public Door(double clearWidth,
                double clearHeight,
                DoorOpeningSide openingSide,
                DoorOpeningType openingType,
                Transform transform = null,
                Material material = null,
                Representation representation = null,
                bool isElementDefinition = false,
                Guid id = default,
                string name = "Door",
                double depthFront = 1,
                double depthBack = 1
            ) : base(
                    transform: transform,
                    representation: representation,
                    isElementDefinition: isElementDefinition,
                    id: id,
                    name: name
                )
        {
            OpeningSide = openingSide;
            OpeningType = openingType;
            ClearHeight = clearHeight;
            ClearWidth = clearWidth;
            Material = material ?? DEFAULT_MATERIAL;
            _fullDoorWidthWithoutFrame = GetDoorFullWidthWithoutFrame(clearWidth, openingSide);
            Opening = new Opening(Polygon.Rectangle(_fullDoorWidthWithoutFrame, clearHeight), depthFront, depthBack, GetOpeningTransform());
        }

        /// <summary>
        /// Create a door at the certain point of a line.
        /// </summary>
        /// <param name="line">The line where the door is placed.</param>
        /// <param name="tPos">Relative position on the line where door is placed. Should be in [0; 1].</param>
        /// <param name="clearWidth">The width of a single door.</param>
        /// <param name="clearHeight">The door's height.</param>
        /// <param name="openingSide">The side where the door opens.</param>
        /// <param name="openingType">The way the door opens.</param>
        /// <param name="material">The door's material.</param>
        /// <param name="representation">The door's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The door's id.</param>
        /// <param name="name">The door's name.</param>
        /// <param name="depthFront">The door's opening depth front.</param>
        /// <param name="depthBack">The door's opening depth back.</param>
        /// <param name="flip">Is the door flipped?</param>
        public Door(Line line,
                    double tPos,
                    double clearWidth,
                    double clearHeight,
                    DoorOpeningSide openingSide,
                    DoorOpeningType openingType,
                    Material material = null,
                    Representation representation = null,
                    bool isElementDefinition = false,
                    Guid id = default,
                    string name = "Door",
                    double depthFront = 1,
                    double depthBack = 1,
                    bool flip = false
            ) : base(
                    representation: representation,
                    isElementDefinition: isElementDefinition,
                    id: id,
                    name: name
                )
        {
            OpeningType = openingType;
            OpeningSide = openingSide;
            ClearWidth = clearWidth;
            ClearHeight = clearHeight;
            Material = material ?? DEFAULT_MATERIAL;
            _fullDoorWidthWithoutFrame = GetDoorFullWidthWithoutFrame(ClearWidth, openingSide);
            Transform = GetDoorTransform(line.PointAtNormalized(tPos), line, flip);
            Opening = new Opening(Polygon.Rectangle(_fullDoorWidthWithoutFrame, clearHeight), depthFront, depthBack, GetOpeningTransform());
        }

        private Transform GetOpeningTransform()
        {
            var halfHeightDir = 0.5 * (ClearHeight + DOOR_FRAME_THICKNESS) * Vector3.ZAxis;
            var openingTransform = new Transform(Transform.Origin + halfHeightDir, Transform.XAxis, Transform.XAxis.Cross(Vector3.ZAxis));
            return openingTransform;
        }

        private Transform GetDoorTransform(Vector3 currentPosition, Line wallLine, bool flip)
        {
            var adjustedPosition = GetClosestValidDoorPos(wallLine, currentPosition);
            var xDoorAxis = flip ? wallLine.Direction().Negate() : wallLine.Direction();
            return new Transform(adjustedPosition, xDoorAxis, Vector3.ZAxis);
        }

        /// <summary>
        /// Checks if the door can fit into the wall with the center line @<paramref name="wallLine"/>.
        /// </summary>
        public static bool CanFit(Line wallLine, DoorOpeningSide openingSide, double width)
        {
            var doorWidth = GetDoorFullWidthWithoutFrame(width, openingSide) + DOOR_FRAME_WIDTH * 2;
            return wallLine.Length() - doorWidth > DOOR_FRAME_WIDTH * 2;
        }

        /// <summary>
        /// Get graphics buffers and other metadata required to modify a GLB.
        /// </summary>
        /// <returns>
        /// True if there is graphicsbuffers data applicable to add, false otherwise.
        /// Out variables should be ignored if the return value is false.
        /// </returns>
        public override bool TryToGraphicsBuffers(out List<GraphicsBuffers> graphicsBuffers, out string id, out glTFLoader.Schema.MeshPrimitive.ModeEnum? mode)
        {
            var points = CollectPointsForSchematicVisualization();
            GraphicsBuffers buffer = new GraphicsBuffers();
            Color color = Colors.Black;
            for (int i = 0; i < points.Count; i++)
            {
                buffer.AddVertex(points[i], default, default, color);
                buffer.AddIndex((ushort)i);
            }

            id = $"{Id}_door";
            // Only one type is allowed, since line are not linked into one loop, LINES is used.
            // This mean that each line segment need both endpoints stored, often duplicated.
            mode = glTFLoader.Schema.MeshPrimitive.ModeEnum.LINES;
            graphicsBuffers = new List<GraphicsBuffers> { buffer };
            return true;
        }

        // TODO: Move visualization logic out of the class in case of DoorOpeningType enum extension.
        private List<Vector3> CollectPointsForSchematicVisualization()
        {
            var points = new List<Vector3>();

            if (OpeningSide == DoorOpeningSide.Undefined || OpeningType == DoorOpeningType.Undefined)
            {
                return points;
            }

            if (OpeningSide != DoorOpeningSide.LeftHand)
            {
                points.AddRange(CollectSchematicVisualizationLines(false, false, 90));
            }

            if (OpeningSide != DoorOpeningSide.RightHand)
            {
                points.AddRange(CollectSchematicVisualizationLines(true, false, 90));
            }

            if (OpeningType == DoorOpeningType.SingleSwing)
            {
                return points;
            }

            if (OpeningSide != DoorOpeningSide.LeftHand)
            {
                points.AddRange(CollectSchematicVisualizationLines(false, true, 90));
            }

            if (OpeningSide != DoorOpeningSide.RightHand)
            {
                points.AddRange(CollectSchematicVisualizationLines(true, true, 90));
            }

            return points;
        }

        private List<Vector3> CollectSchematicVisualizationLines(bool leftSide, bool inside, double angle)
        {
            // Depending on which side door in there are different offsets.
            var doorOffset = leftSide ? _fullDoorWidthWithoutFrame / 2 : -_fullDoorWidthWithoutFrame / 2;
            var horizontalOffset = leftSide ? DOOR_THICKNESS : -DOOR_THICKNESS;
            var verticalOffset = inside ? DOOR_THICKNESS : -DOOR_THICKNESS;
            var widthOffset = inside ? ClearWidth : -ClearWidth;

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
            Arc arc = new Arc(c0, ClearWidth, anchorAngle, endAngle);
            var tessalatedArc = arc.ToPolyline((int)(Math.Abs(angle) / 2));
            for (int i = 0; i < tessalatedArc.Vertices.Count - 1; i++)
            {
                points.Add(tessalatedArc.Vertices[i]);
                points.Add(tessalatedArc.Vertices[i + 1]);
            }

            return points;
        }

        /// <summary>
        /// Update the representations.
        /// </summary>
        public override void UpdateRepresentations()
        {
            Vector3 left = Vector3.XAxis * _fullDoorWidthWithoutFrame / 2;
            Vector3 right = Vector3.XAxis.Negate() * _fullDoorWidthWithoutFrame / 2;

            var doorPolygon = new Polygon(new List<Vector3>() {
                left + Vector3.YAxis * DOOR_THICKNESS,
                left - Vector3.YAxis * DOOR_THICKNESS,
                right - Vector3.YAxis * DOOR_THICKNESS,
                right + Vector3.YAxis * DOOR_THICKNESS});

            var doorPolygons = new List<Polygon>();

            if (OpeningSide == DoorOpeningSide.DoubleDoor)
            {
                doorPolygons = doorPolygon.Split(new Polyline(new Vector3(0, DOOR_THICKNESS, 0), new Vector3(0, -DOOR_THICKNESS, 0)));
            }
            else
            {
                doorPolygons.Add(doorPolygon);
            }

            var doorExtrusions = new List<Extrude>();

            foreach (var polygon in doorPolygons)
            {
                var doorExtrude = new Extrude(new Profile(polygon.Offset(-0.005)[0]), ClearHeight, Vector3.ZAxis);
                doorExtrusions.Add(doorExtrude);
            }

            var frameLeft = left + Vector3.XAxis * DOOR_FRAME_WIDTH;
            var frameRight = right - Vector3.XAxis * DOOR_FRAME_WIDTH;
            var frameOffset = Vector3.YAxis * DOOR_FRAME_THICKNESS;
            var doorFramePolygon = new Polygon(new List<Vector3>() {
                left + Vector3.ZAxis * ClearHeight - frameOffset,
                left - frameOffset,
                frameLeft - frameOffset,
                frameLeft + Vector3.ZAxis * (ClearHeight + DOOR_FRAME_WIDTH) - frameOffset,
                frameRight + Vector3.ZAxis * (ClearHeight + DOOR_FRAME_WIDTH) - frameOffset,
                frameRight - frameOffset,
                right - frameOffset,
                right + Vector3.ZAxis * ClearHeight - frameOffset });
            var doorFrameExtrude = new Extrude(new Profile(doorFramePolygon), DOOR_FRAME_THICKNESS * 2, Vector3.YAxis);

            Representation.SolidOperations.Clear();
            Representation.SolidOperations.Add(doorFrameExtrude);
            foreach (var extrusion in doorExtrusions)
            {
                Representation.SolidOperations.Add(extrusion);
            }
        }

        private Vector3 GetClosestValidDoorPos(Line wallLine, Vector3 currentPosition)
        {
            var fullWidth = _fullDoorWidthWithoutFrame + DOOR_FRAME_WIDTH * 2;
            double wallWidth = wallLine.Length();
            Vector3 p1 = wallLine.PointAt(0.5 * fullWidth);
            Vector3 p2 = wallLine.PointAt(wallWidth - 0.5 * fullWidth);
            var reducedWallLine = new Line(p1, p2);
            return currentPosition.ClosestPointOn(reducedWallLine);
        }

        private static double GetDoorFullWidthWithoutFrame(double doorClearWidth, DoorOpeningSide doorOpeningSide)
        {
            switch (doorOpeningSide)
            {
                case DoorOpeningSide.LeftHand:
                case DoorOpeningSide.RightHand:
                    return doorClearWidth;
                case DoorOpeningSide.DoubleDoor:
                    return doorClearWidth * 2;
            }
            return 0;
        }
    }
}
