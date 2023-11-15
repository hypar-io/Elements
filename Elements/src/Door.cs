using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Elements.Geometry.Solids;

namespace Elements
{
        /// <summary>Definition of a door</summary>
        public class Door : GeometricElement
        {
                private readonly Material DEFAULT_MATERIAL = BuiltInMaterials.Wood;
                private readonly Material FRAME_MATERIAL = new Material(Colors.Gray, 0.5, 0.25, false, null, false, false, null, false, null, 0, false, default, "Silver Frame");

                /// <summary>Default thickness of a door.</summary>
                public const double DOOR_THICKNESS = 1.375 * 0.0254;
                /// <summary>Default thickness of a door frame.</summary>
                public const double DOOR_FRAME_THICKNESS = 4 * 0.0254;
                /// <summary>Default width of a door frame.</summary>
                public const double DOOR_FRAME_WIDTH = 2 * 0.0254; //2 inches
                /// <summary>Door width without a frame</summary>
                public double ClearWidth { get; private set; }
                /// <summary>The opening type of the door that should be placed</summary>
                public DoorOpeningType OpeningType { get; private set; }
                /// <summary>The opening side of the door that should be placed</summary>
                public DoorOpeningSide OpeningSide { get; private set; }
                /// <summary>Height of a door without a frame.</summary>
                public double ClearHeight { get; private set; }
                /// <summary>Door thickness.</summary>
                public double Thickness { get; private set; }
                /// <summary>Original position of the door used for override identity</summary>
                public Vector3 OriginalPosition { get; set; }

                [JsonIgnore]
                private double fullDoorWidthWithoutFrame => GetDoorFullWidthWithoutFrame(ClearWidth, OpeningSide);

                /// <summary>
                /// Create a door.
                /// </summary>
                /// <param name="clearWidth">The width of a single door.</param>
                /// <param name="clearHeight">Height of the door without frame.</param>
                /// <param name="thickness">Door thickness.</param>
                /// <param name="openingSide">The side where the door opens.</param>
                /// <param name="openingType">The way the door opens.</param>
                /// <param name="transform">The door's transform. X-direction is aligned with the door, Y-direction is the opening direction.</param>
                /// <param name="material">The door's material.</param>
                /// <param name="representation">The door's representation.</param>
                /// <param name="isElementDefinition">Is this an element definition?</param>
                /// <param name="id">The door's id.</param>
                /// <param name="name">The door's name.</param>
                [JsonConstructor]
                public Door(double clearWidth,
                        double clearHeight,
                        double thickness,
                        DoorOpeningSide openingSide,
                        DoorOpeningType openingType,
                        Transform transform = null,
                        Material material = null,
                        Representation representation = null,
                        bool isElementDefinition = false,
                        Guid id = default,
                        string name = "Door"
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
                        Thickness = thickness;
                        Material = material ?? DEFAULT_MATERIAL;
                }

                /// <summary>
                /// Create a door at the certain point of a line.
                /// </summary>
                /// <param name="line">The line where the door is placed.</param>
                /// <param name="tPos">Relative position on the line where door is placed. Should be in [0; 1].</param>
                /// <param name="clearWidth">The width of a single door.</param>
                /// <param name="clearHeight">Height of the door without frame.</param>
                /// <param name="thickness">Door thickness.</param>
                /// <param name="openingSide">The side where the door opens.</param>
                /// <param name="openingType">The way the door opens.</param>
                /// <param name="material">The door's material.</param>
                /// <param name="representation">The door's representation.</param>
                /// <param name="isElementDefinition">Is this an element definition?</param>
                /// <param name="id">The door's id.</param>
                /// <param name="name">The door's name.</param>
                public Door(Line line,
                            double tPos,
                            double clearWidth,
                            double clearHeight,
                            double thickness,
                            DoorOpeningSide openingSide,
                            DoorOpeningType openingType,
                            Material material = null,
                            Representation representation = null,
                            bool isElementDefinition = false,
                            Guid id = default,
                            string name = "Door"
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
                        Thickness = thickness;
                        Material = material ?? DEFAULT_MATERIAL;
                        Transform = GetDoorTransform(line.PointAtNormalized(tPos), line);
                }

                /// <summary>
                /// Create an opening for the door.
                /// </summary>
                /// <param name="depthFront">The door's opening depth front.</param>
                /// <param name="depthBack">The door's opening depth back.</param>
                /// <param name="flip">Is the opening flipped?</param>
                /// <returns>An opening where the door can be inserted.</returns>
                public Opening CreateDoorOpening(double depthFront, double depthBack, bool flip)
                {
                        var openingWidth = fullDoorWidthWithoutFrame + 2 * DOOR_FRAME_WIDTH;
                        var openingHeight = ClearHeight + DOOR_FRAME_WIDTH;

                        var openingDir = flip ? Vector3.YAxis.Negate() : Vector3.YAxis;
                        var widthDir = flip ? Vector3.XAxis.Negate() : Vector3.XAxis;
                        var openingTransform = new Transform(0.5 * openingHeight * Vector3.ZAxis, widthDir, openingDir);

                        var openingPolygon = Polygon.Rectangle(openingWidth, openingHeight).TransformedPolygon(openingTransform);

                        var opening = new Opening(openingPolygon, openingDir, depthFront, depthBack, Transform);
                        return opening;
                }

                private Transform GetDoorTransform(Vector3 currentPosition, Line wallLine)
                {
                        var adjustedPosition = GetClosestValidDoorPos(wallLine, currentPosition);
                        var xDoorAxis = wallLine.Direction();
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
                /// Update the representations.
                /// </summary>
                public override void UpdateRepresentations()
                {
                        RepresentationInstances = this.GetInstances(this);
                }

                public List<RepresentationInstance> GetInstances(Door door)
                {
                        var representationInstances = new List<RepresentationInstance>()
            {
                this.CreateDoorSolidRepresentation(),
                this.CreateDoorFrameRepresentation(),
                this.CreateDoorCurveRepresentation()
            };

                        return representationInstances;
                }

                private Vector3 GetClosestValidDoorPos(Line wallLine, Vector3 currentPosition)
                {
                        var fullWidth = fullDoorWidthWithoutFrame + DOOR_FRAME_WIDTH * 2;
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


                private RepresentationInstance CreateDoorCurveRepresentation()
                {
                        var points = CollectPointsForSchematicVisualization();
                        var curve = new IndexedPolycurve(points);
                        var curveRep = new CurveRepresentation(curve, false);
                        var repInstance = new RepresentationInstance(curveRep, BuiltInMaterials.Black);
                        return repInstance;
                }

                private RepresentationInstance CreateDoorFrameRepresentation()
                {
                        Vector3 left = Vector3.XAxis * (fullDoorWidthWithoutFrame / 2);
                        Vector3 right = Vector3.XAxis.Negate() * (fullDoorWidthWithoutFrame / 2);

                        var frameLeft = left + Vector3.XAxis * Door.DOOR_FRAME_WIDTH;
                        var frameRight = right - Vector3.XAxis * Door.DOOR_FRAME_WIDTH;
                        var frameOffset = Vector3.YAxis * Door.DOOR_FRAME_THICKNESS;
                        var doorFramePolygon = new Polygon(new List<Vector3>() {
                                left + Vector3.ZAxis * this.ClearHeight - frameOffset,
                                left - frameOffset,
                                frameLeft - frameOffset,
                                frameLeft + Vector3.ZAxis * (this.ClearHeight + Door.DOOR_FRAME_WIDTH) - frameOffset,
                                frameRight + Vector3.ZAxis * (this.ClearHeight + Door.DOOR_FRAME_WIDTH) - frameOffset,
                                frameRight - frameOffset,
                                right - frameOffset,
                                right + Vector3.ZAxis * this.ClearHeight - frameOffset });
                        var doorFrameExtrude = new Extrude(new Profile(doorFramePolygon), Door.DOOR_FRAME_THICKNESS * 2, Vector3.YAxis);

                        var solidRep = new SolidRepresentation(doorFrameExtrude);
                        var repInstance = new RepresentationInstance(solidRep, FRAME_MATERIAL, true);
                        return repInstance;
                }

                private RepresentationInstance CreateDoorSolidRepresentation()
                {
                        Vector3 left = Vector3.XAxis * (fullDoorWidthWithoutFrame / 2);
                        Vector3 right = Vector3.XAxis.Negate() * (fullDoorWidthWithoutFrame / 2);

                        var doorPolygon = new Polygon(new List<Vector3>() {
                left + Vector3.YAxis * this.Thickness,
                left - Vector3.YAxis * this.Thickness,
                right - Vector3.YAxis * this.Thickness,
                right + Vector3.YAxis * this.Thickness});

                        var doorPolygons = new List<Polygon>();

                        if (this.OpeningSide == DoorOpeningSide.DoubleDoor)
                        {
                                doorPolygons = doorPolygon.Split(new Polyline(new Vector3(0, this.Thickness, 0), new Vector3(0, -this.Thickness, 0)));
                        }
                        else
                        {
                                doorPolygons.Add(doorPolygon);
                        }

                        var doorExtrusions = new List<SolidOperation>();

                        foreach (var polygon in doorPolygons)
                        {
                                var doorExtrude = new Extrude(new Profile(polygon.Offset(-0.005)[0]), this.ClearHeight, Vector3.ZAxis);
                                doorExtrusions.Add(doorExtrude);
                        }

                        var solidRep = new SolidRepresentation(doorExtrusions);
                        var repInstance = new RepresentationInstance(solidRep, this.Material, true);
                        return repInstance;
                }

                private List<Vector3> CollectPointsForSchematicVisualization()
                {
                        var points = new List<Vector3>();

                        if (this.OpeningSide == DoorOpeningSide.Undefined || this.OpeningType == DoorOpeningType.Undefined)
                        {
                                return points;
                        }

                        if (this.OpeningSide != DoorOpeningSide.LeftHand)
                        {
                                points.AddRange(CollectSchematicVisualizationLines(this, false, false, 90));
                        }

                        if (this.OpeningSide != DoorOpeningSide.RightHand)
                        {
                                points.AddRange(CollectSchematicVisualizationLines(this, true, false, 90));
                        }

                        if (this.OpeningType == DoorOpeningType.SingleSwing)
                        {
                                return points;
                        }

                        if (this.OpeningSide != DoorOpeningSide.LeftHand)
                        {
                                points.AddRange(CollectSchematicVisualizationLines(this, false, true, 90));
                        }

                        if (this.OpeningSide != DoorOpeningSide.RightHand)
                        {
                                points.AddRange(CollectSchematicVisualizationLines(this, true, true, 90));
                        }

                        return points;
                }

                private List<Vector3> CollectSchematicVisualizationLines(Door door, bool leftSide, bool inside, double angle)
                {
                        // Depending on which side door in there are different offsets.
                        var doorOffset = leftSide ? fullDoorWidthWithoutFrame / 2 : -fullDoorWidthWithoutFrame / 2;
                        var horizontalOffset = leftSide ? door.Thickness : -door.Thickness;
                        var verticalOffset = inside ? door.Thickness : -door.Thickness;
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

        public enum DoorOpeningSide
        {
                [System.Runtime.Serialization.EnumMember(Value = @"Undefined")]
                Undefined,
                [System.Runtime.Serialization.EnumMember(Value = @"Left Hand")]
                LeftHand,
                [System.Runtime.Serialization.EnumMember(Value = @"Right Hand")]
                RightHand,
                [System.Runtime.Serialization.EnumMember(Value = @"Double Door")]
                DoubleDoor
        }

        public enum DoorOpeningType
        {
                [System.Runtime.Serialization.EnumMember(Value = @"Undefined")]
                Undefined,
                [System.Runtime.Serialization.EnumMember(Value = @"Single Swing")]
                SingleSwing,
                [System.Runtime.Serialization.EnumMember(Value = @"Double Swing")]
                DoubleSwing
        }
}
