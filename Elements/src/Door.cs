using Elements.Geometry.Solids;
using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements
{
    /// <summary>Definition of a door</summary>
    public class Door : GeometricElement
    {
        public const double DOOR_THICKNESS = 0.125;
        public const double DOOR_FRAME_THICKNESS = 0.15;
        public const double DOOR_FRAME_WIDTH = 2 * 0.0254; //2 inches

        /// <summary>Door width without a frame</summary>
        public double ClearWidth { get; private set; }
        /// <summary>The opening type of the door that should be placed</summary>
        public DoorOpeningType OpeningType { get; private set; }
        /// <summary>The opening side of the door that should be placed</summary>
        public DoorOpeningSide OpeningSide { get; private set; }
        /// <summary>The wall on which a door is placed.</summary>
        public Wall Wall { get; private set; }
        /// <summary>Height of a door without a frame.</summary>
        public double ClearHeight { get; private set; }
        /// <summary>Position where door was placed originally.</summary>
        public Vector3 OriginalPosition { get; private set; }
        /// <summary>Opening for a door.</summary>
        public Opening Opening { get; private set; }

        public Door(Wall wall,
                    Line wallLine,
                    Vector3 originalPosition,
                    Vector3 currentPosition,
                    double width,
                    double height,
                    DoorOpeningSide openingSide,
                    DoorOpeningType openingType,
                    double depthFront = 1,
                    double depthBack = 1,
                    bool flip = false)
        {
            Wall = wall;
            OpeningType = openingType;
            OpeningSide = openingSide;
            OriginalPosition = originalPosition;
            ClearWidth = WidthWithoutFrame(width, openingSide);
            ClearHeight = height;
            Material = new Material("Door material", Colors.White);
            Transform = GetDoorTransform(currentPosition, wallLine, flip);
            Representation = new Representation(new List<SolidOperation>() { });
            Opening = new Opening(Polygon.Rectangle(width, height), depthFront, depthBack, GetOpeningTransform(wallLine.Direction()));
        }

        public Door(Wall wall,
                Transform transform,
                double width,
                double height,
                DoorOpeningSide openingSide,
                DoorOpeningType openingType,
                double depthFront = 1,
                double depthBack = 1)
        {
            Wall = wall;
            Transform = transform;
            OpeningSide = openingSide;
            OpeningType = openingType;
            ClearHeight = height;
            ClearWidth = WidthWithoutFrame(width, openingSide);
            Material = new Material("Door material", Colors.White);
            Representation = new Representation(new List<SolidOperation>() { });
            Opening = new Opening(Polygon.Rectangle(width, height), depthFront, depthBack, GetOpeningTransform(transform.XAxis));
            OriginalPosition = Transform.Origin;
        }

        public Door(Wall wall,
                    Line wallLine,
                    double tPos,
                    double width,
                    double height,
                    DoorOpeningSide openingSide,
                    DoorOpeningType openingType,
                    double depthFront = 1,
                    double depthBack = 1,
                    bool flip = false)
            : this(wall,
                   wallLine,
                   wallLine.PointAtNormalized(tPos),
                   wallLine.PointAtNormalized(tPos),
                   width,
                   height,
                   openingSide,
                   openingType,
                   depthFront,
                   depthBack,
                   flip)
        {
        }

        private Transform GetOpeningTransform(Vector3 xAxis)
        {
            var halfHeightDir = 0.5 * (ClearHeight + DOOR_FRAME_THICKNESS) * Vector3.ZAxis;
            var openingTransform = new Transform(Transform.Origin + halfHeightDir, xAxis, xAxis.Cross(Vector3.ZAxis));
            return openingTransform;
        }

        private Transform GetDoorTransform(Vector3 currentPosition, Line centerLine, bool flip)
        {
            var adjustedPosition = GetClosestValidDoorPos(centerLine, currentPosition);
            var xDoorAxis = flip ? centerLine.Direction().Negate() : centerLine.Direction();
            return new Transform(adjustedPosition, xDoorAxis, Vector3.ZAxis);
        }

        public static bool CanFit(Line wallLine, DoorOpeningSide openingSide, double width)
        {
            var doorWidth = WidthWithoutFrame(width, openingSide) + DOOR_FRAME_WIDTH * 2;
            return wallLine.Length() - doorWidth > DOOR_FRAME_WIDTH * 2;
        }

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

            id = $"{this.Id}_door";
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
            var doorWidth = OpeningSide == DoorOpeningSide.DoubleDoor ? ClearWidth / 2 : ClearWidth;

            // Depending on which side door in there are different offsets.
            var doorOffset = leftSide ? ClearWidth / 2 : -ClearWidth / 2;
            var horizontalOffset = leftSide ? DOOR_THICKNESS : -DOOR_THICKNESS;
            var verticalOffset = inside ? DOOR_THICKNESS : -DOOR_THICKNESS;
            var widthOffset = inside ? doorWidth : -doorWidth;

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
            Arc arc = new Arc(c0, doorWidth, anchorAngle, endAngle);
            var tessalatedArc = arc.ToPolyline((int)(Math.Abs(angle) / 2));
            for (int i = 0; i < tessalatedArc.Vertices.Count - 1; i++)
            {
                points.Add(tessalatedArc.Vertices[i]);
                points.Add(tessalatedArc.Vertices[i + 1]);
            }

            return points;
        }

        public override void UpdateRepresentations()
        {
            Vector3 left = Vector3.XAxis * ClearWidth / 2;
            Vector3 right = Vector3.XAxis.Negate() * ClearWidth / 2;

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

            Representation.SolidOperations.Add(doorFrameExtrude);
            foreach (var extrusion in doorExtrusions)
            {
                Representation.SolidOperations.Add(extrusion);
            }
        }

        private Vector3 GetClosestValidDoorPos(Line wallLine, Vector3 currentPosition)
        {
            var fullWidth = ClearWidth + DOOR_FRAME_WIDTH * 2;
            double wallWidth = wallLine.Length();
            Vector3 p1 = wallLine.PointAt(0.5 * fullWidth);
            Vector3 p2 = wallLine.PointAt(wallWidth - 0.5 * fullWidth);
            var reducedWallLine = new Line(p1, p2);
            return currentPosition.ClosestPointOn(reducedWallLine);
        }

        private static double WidthWithoutFrame(double internalWidth, DoorOpeningSide openingSide)
        {
            switch (openingSide)
            {
                case DoorOpeningSide.LeftHand:
                case DoorOpeningSide.RightHand:
                    return internalWidth;
                case DoorOpeningSide.DoubleDoor:
                    return internalWidth * 2;
            }
            return 0;
        }
    }
}
