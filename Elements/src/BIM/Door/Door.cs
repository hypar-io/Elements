using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Elements.Geometry.Solids;
using System.Linq;

namespace Elements
{
        /// <summary>Definition of a door</summary>
        public class Door : GeometricElement
        {
                /// <summary>The material to be used on the door frame</summary>
                public Material FrameMaterial { get; set; } = new Material(Colors.Gray, 0.5, 0.25, false, null, false, false, null, false, null, 0, false, default, "Silver Frame");
                /// <summary>The opening type of the door that should be placed</summary>
                [JsonProperty("Door Opening Type")]
                [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
                public DoorOpeningType OpeningType { get; private set; }
                /// <summary>The opening side of the door that should be placed</summary>
                [JsonProperty("Door Opening Side")]
                [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
                public DoorOpeningSide OpeningSide { get; private set; }
                /// <summary>Width of a door without a frame.</summary>
                [JsonProperty("Door Width")]
                public double DoorWidth { get; set; }
                /// <summary>Height of a door without a frame.</summary>
                [JsonProperty("Door Height")]
                public double DoorHeight { get; set; }
                [JsonProperty("Door Type")]
                public string DoorType { get; set; }
                /// <summary>Default door thickness.</summary>
                public static double DEFAULT_DOOR_THICKNESS = 2 * 0.0254;
                /// <summary>Door thickness.</summary>
                public double DoorThickness { get; set; } = DEFAULT_DOOR_THICKNESS;
                /// <summary>Default thickness of a door frame.</summary>
                public double FrameDepth { get; set; } = 4 * 0.0254;
                /// <summary>Default width of a door frame.</summary>
                public double FrameWidth { get; set; } = 2 * 0.0254; //2 inches
                /// <summary>Height of the door handle from the ground</summary>
                public double HandleHeight { get; set; } = 42 * 0.0254;
                /// <summary>Radius of the fixture against the door</summary>
                public double HandleBaseRadius { get; set; } = 1.35 * 0.0254;
                /// <summary>Radius of the handle</summary>
                public double HandleRadius { get; set; } = 0.45 * 0.0254;
                /// <summary>Length of the handle</summary>
                public double HandleLength { get; set; } = 5 * 0.0254;
                /// <summary>Depth of the handle from the face of the door</summary>
                public double HandleDepth { get; set; } = 2 * 0.0254;
                /// <summary>Original position of the door used for override identity</summary>
                public Vector3 OriginalPosition { get; set; }

                [JsonIgnore]
                private double FullDoorWidthWithoutFrame => GetDoorFullWidthWithoutFrame();
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
                        DoorHeight = clearHeight;
                        DoorWidth = clearWidth;
                        DoorThickness = thickness;
                        Material = material ?? BuiltInMaterials.Default;
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
                        DoorWidth = clearWidth;
                        DoorHeight = clearHeight;
                        DoorThickness = thickness;
                        Material = material ?? BuiltInMaterials.Default;
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
                        var openingWidth = FullDoorWidthWithoutFrame + 2 * FrameWidth;
                        var openingHeight = DoorHeight + FrameWidth;

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
                /// Update the representations.
                /// </summary>
                public override void UpdateRepresentations()
                {
                        if (RepresentationInstances.Count == 0)
                        {
                                DoorRepresentationStorage.SetDoorRepresentation(this);
                        }
                }

                /// <summary>
                /// Get Hash for representation storage dictionary
                /// </summary>
                public string GetRepresentationHash()
                {
                        return $"{this.GetType().Name}-{this.DoorWidth}-{this.DoorHeight}-{this.DoorThickness}-{this.FrameDepth}-{this.FrameWidth}{this.FrameMaterial.Name}-{this.OpeningType}-{this.OpeningSide}-{this.Material.Name}";
                }

                public List<RepresentationInstance> GetInstances()
                {
                        var representationInstances = new List<RepresentationInstance>()
                        {
                                this.CreateDoorSolidRepresentation(),
                                this.CreateDoorFrameRepresentation(),
                                this.CreateDoorHandleRepresentation()
                        };

                        representationInstances.AddRange(this.CreateDoorCurveRepresentation());

                        return representationInstances.Where(instance => instance != null).ToList();
                }

                private Vector3 GetClosestValidDoorPos(Line wallLine, Vector3 currentPosition)
                {
                        var fullWidth = FullDoorWidthWithoutFrame + FrameWidth * 2;
                        double wallWidth = wallLine.Length();
                        Vector3 p1 = wallLine.PointAt(0.5 * fullWidth);
                        Vector3 p2 = wallLine.PointAt(wallWidth - 0.5 * fullWidth);
                        var reducedWallLine = new Line(p1, p2);
                        return currentPosition.ClosestPointOn(reducedWallLine);
                }

                private double GetDoorFullWidthWithoutFrame()
                {
                        switch (this.OpeningSide)
                        {
                                case DoorOpeningSide.LeftHand:
                                case DoorOpeningSide.RightHand:
                                        return this.DoorWidth;
                                case DoorOpeningSide.DoubleDoor:
                                        return this.DoorWidth * 2;
                        }
                        return 0;
                }

                private List<RepresentationInstance> CreateDoorCurveRepresentation()
                {
                        var repInstances = CollectPointsForSchematicVisualization();

                        return repInstances;
                }

                private RepresentationInstance CreateDoorFrameRepresentation()
                {
                        if (FrameDepth == 0 || FrameWidth == 0)
                        {
                                return null;
                        }

                        Vector3 left = Vector3.XAxis * (FullDoorWidthWithoutFrame / 2);
                        Vector3 right = Vector3.XAxis.Negate() * (FullDoorWidthWithoutFrame / 2);

                        var frameLeft = left + Vector3.XAxis * this.FrameWidth;
                        var frameRight = right - Vector3.XAxis * this.FrameWidth;
                        var frameOffset = Vector3.YAxis * this.FrameDepth / 2;
                        var doorFramePolygon = new Polygon(new List<Vector3>() {
                                left + Vector3.ZAxis * this.DoorHeight - frameOffset,
                                left - frameOffset,
                                frameLeft - frameOffset,
                                frameLeft + Vector3.ZAxis * (this.DoorHeight + this.FrameWidth) - frameOffset,
                                frameRight + Vector3.ZAxis * (this.DoorHeight + this.FrameWidth) - frameOffset,
                                frameRight - frameOffset,
                                right - frameOffset,
                                right + Vector3.ZAxis * this.DoorHeight - frameOffset });
                        var doorFrameExtrude = new Extrude(new Profile(doorFramePolygon), this.FrameDepth, Vector3.YAxis);

                        var solidRep = new SolidRepresentation(doorFrameExtrude);
                        solidRep.SetSnappingPoints(new List<SnappingPoints>());
                        var repInstance = new RepresentationInstance(solidRep, FrameMaterial, true);
                        return repInstance;
                }

                private RepresentationInstance CreateDoorSolidRepresentation()
                {
                        Vector3 left = Vector3.XAxis * (FullDoorWidthWithoutFrame / 2);
                        Vector3 right = Vector3.XAxis.Negate() * (FullDoorWidthWithoutFrame / 2);

                        var doorPolygon = new Polygon(new List<Vector3>() {
                                left + Vector3.YAxis * this.DoorThickness/2,
                                left - Vector3.YAxis * this.DoorThickness/2,
                                right - Vector3.YAxis * this.DoorThickness/2,
                                right + Vector3.YAxis * this.DoorThickness/2});

                        var doorPolygons = new List<Polygon>();

                        if (this.OpeningSide == DoorOpeningSide.DoubleDoor)
                        {
                                doorPolygons = doorPolygon.Split(new Polyline(new Vector3(0, this.DoorThickness / 2, 0), new Vector3(0, -this.DoorThickness / 2, 0)));
                        }
                        else
                        {
                                doorPolygons.Add(doorPolygon);
                        }

                        var doorExtrusions = new List<SolidOperation>();

                        foreach (var polygon in doorPolygons)
                        {
                                var doorExtrude = new Extrude(new Profile(polygon.Offset(-0.005)[0]), this.DoorHeight, Vector3.ZAxis);
                                doorExtrusions.Add(doorExtrude);
                        }

                        var solidRep = new SolidRepresentation(doorExtrusions);
                        solidRep.SetSnappingPoints(new List<SnappingPoints>(new SnappingPoints[] { new SnappingPoints(new[] { left, Vector3.Origin, right }, SnappingEdgeMode.LineStrip) }));
                        var repInstance = new RepresentationInstance(solidRep, this.Material, true);
                        return repInstance;
                }

                private List<RepresentationInstance> CollectPointsForSchematicVisualization()
                {
                        var representationInstances = new List<RepresentationInstance>();

                        if (this.OpeningSide == DoorOpeningSide.Undefined || this.OpeningType == DoorOpeningType.Undefined)
                        {
                                return representationInstances;
                        }

                        if (this.OpeningSide != DoorOpeningSide.LeftHand)
                        {
                                var points = CollectSchematicVisualizationLines(this, false, false, 90);
                                points.Add(points[0]);
                                var curve = new IndexedPolycurve(points);
                                var curveRep = new CurveRepresentation(curve, false);
                                curveRep.SetSnappingPoints(new List<SnappingPoints>());
                                var repInstance = new RepresentationInstance(curveRep, BuiltInMaterials.Black);
                                representationInstances.Add(repInstance);
                        }

                        if (this.OpeningSide != DoorOpeningSide.RightHand)
                        {
                                var points = CollectSchematicVisualizationLines(this, true, false, 90);
                                points.Add(points[0]);
                                var curve = new IndexedPolycurve(points);
                                var curveRep = new CurveRepresentation(curve, false);
                                curveRep.SetSnappingPoints(new List<SnappingPoints>());
                                var repInstance = new RepresentationInstance(curveRep, BuiltInMaterials.Black);
                                representationInstances.Add(repInstance);
                        }

                        if (this.OpeningType == DoorOpeningType.DoubleSwing)
                        {

                                if (this.OpeningSide != DoorOpeningSide.LeftHand)
                                {
                                        var points = CollectSchematicVisualizationLines(this, false, true, 90);
                                        points.Add(points[0]);
                                        var curve = new IndexedPolycurve(points);
                                        var curveRep = new CurveRepresentation(curve, false);
                                        curveRep.SetSnappingPoints(new List<SnappingPoints>());
                                        var repInstance = new RepresentationInstance(curveRep, BuiltInMaterials.Black);
                                        representationInstances.Add(repInstance);
                                }

                                if (this.OpeningSide != DoorOpeningSide.RightHand)
                                {
                                        var points = CollectSchematicVisualizationLines(this, true, true, 90);
                                        points.Add(points[0]);
                                        var curve = new IndexedPolycurve(points);
                                        var curveRep = new CurveRepresentation(curve, false);
                                        curveRep.SetSnappingPoints(new List<SnappingPoints>());
                                        var repInstance = new RepresentationInstance(curveRep, BuiltInMaterials.Black);
                                        representationInstances.Add(repInstance);
                                }
                        }

                        return representationInstances;
                }

                private List<Vector3> CollectSchematicVisualizationLines(Door door, bool leftSide, bool inside, double angle)
                {
                        // Depending on which side door in there are different offsets.
                        var doorOffset = leftSide ? FullDoorWidthWithoutFrame / 2 : -FullDoorWidthWithoutFrame / 2;
                        var horizontalOffset = leftSide ? door.DoorThickness : -door.DoorThickness;
                        var verticalOffset = inside ? door.DoorThickness : -door.DoorThickness;
                        var widthOffset = inside ? door.DoorWidth : -door.DoorWidth;

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
                        Arc arc = new Arc(c0, door.DoorWidth, anchorAngle, endAngle);
                        var tessalatedArc = arc.ToPolyline((int)(Math.Abs(angle) / 2));
                        for (int i = 0; i < tessalatedArc.Vertices.Count - 1; i++)
                        {
                                points.Add(tessalatedArc.Vertices[i]);
                                points.Add(tessalatedArc.Vertices[i + 1]);
                        }

                        return points;
                }

                private RepresentationInstance CreateDoorHandleRepresentation()
                {
                        var solidOperationsList = new List<SolidOperation>();

                        if (OpeningSide == DoorOpeningSide.DoubleDoor)
                        {
                                var handlePair1 = CreateHandlePair(-3 * 0.0254, false);
                                solidOperationsList.AddRange(handlePair1);

                                var handlePair2 = CreateHandlePair(3 * 0.0254, true);
                                solidOperationsList.AddRange(handlePair2);
                        }
                        else if (OpeningSide != DoorOpeningSide.Undefined)
                        {
                                var xPos = OpeningSide == DoorOpeningSide.LeftHand ? -(FullDoorWidthWithoutFrame / 2 - 2 * 0.0254) : (FullDoorWidthWithoutFrame / 2 - 2 * 0.0254);
                                var handle = CreateHandlePair(xPos, OpeningSide == DoorOpeningSide.LeftHand);
                                solidOperationsList.AddRange(handle);
                        }

                        var solidRep = new SolidRepresentation(solidOperationsList);
                        solidRep.SetSnappingPoints(new List<SnappingPoints>());
                        var repInst = new RepresentationInstance(solidRep, FrameMaterial);
                        return repInst;
                }

                private List<SolidOperation> CreateHandlePair(double xRelPos, bool isCodirectionalToX)
                {
                        var xOffset = xRelPos * DoorWidth * Vector3.XAxis;
                        var yOffset = DoorThickness * Vector3.YAxis;
                        var zOffset = HandleHeight * Vector3.ZAxis;

                        var solidOperationsList = new List<SolidOperation>();
                        var handleDir = isCodirectionalToX ? Vector3.XAxis : Vector3.XAxis.Negate();

                        var handleOrigin1 = xOffset + yOffset + zOffset;
                        var handle1Ops = CreateHandle(handleOrigin1, handleDir, Vector3.YAxis);
                        solidOperationsList.AddRange(handle1Ops);

                        var handleOrigin2 = xOffset - yOffset + zOffset;
                        var handle2Ops = CreateHandle(handleOrigin2, handleDir, Vector3.YAxis.Negate());
                        solidOperationsList.AddRange(handle2Ops);

                        return solidOperationsList;
                }

                private List<SolidOperation> CreateHandle(Vector3 origin, Vector3 handleDir, Vector3 yDir)
                {
                        var circleTransform = new Transform(origin, handleDir, yDir);
                        var circle = new Circle(circleTransform, HandleBaseRadius).ToPolygon();
                        var circleOperation = new Extrude(circle, 0.1 * HandleDepth, yDir);

                        var cyl1Transform = new Transform(origin + 0.1 * HandleDepth * yDir, handleDir, yDir);
                        var cyl1Circle = new Circle(cyl1Transform, HandleRadius).ToPolygon();
                        var cyl1Operation = new Extrude(cyl1Circle, 0.9 * HandleDepth, yDir);

                        var cyl2Origin = cyl1Transform.Origin + cyl1Operation.Height * yDir + handleDir.Negate() * HandleRadius;
                        var cyl2Transform = new Transform(cyl2Origin, handleDir);
                        var cyl2Circle = new Circle(cyl2Transform, HandleRadius).ToPolygon();
                        var cyl2Operation = new Extrude(cyl2Circle, HandleLength, handleDir);

                        var handleSolids = new List<SolidOperation>() { circleOperation, cyl1Operation, cyl2Operation };
                        return handleSolids;
                }
        }
}
