using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements.Fittings
{
    public partial class Elbow
    {
        public Elbow(Vector3 position, Vector3 startDirection, Vector3 endDirection, double sideLength, double diameter, Material material = null, double? bendRadius = 0) :
                                                                        base(false, FittingLocator.Empty(), new Transform(position),
                                                                            material == null ? FittingTreeRouting.DefaultFittingMaterial : material,
                                                                            new Representation(new List<SolidOperation>()),
                                                                            false,
                                                                            Guid.NewGuid(),
                                                                            "")
        {
            this.Start = new Port(position + startDirection.Unitized() * sideLength + BendRadiusOffset(bendRadius, startDirection),
                                  startDirection,
                                  diameter);
            this.End = new Port(position + endDirection.Unitized() * sideLength + BendRadiusOffset(bendRadius, endDirection),
                                endDirection,
                                diameter);
            this.Diameter = diameter;
            this.Angle = Math.Abs(180 - this.Start.Direction.AngleTo(this.End.Direction));
            this.BendRadius = bendRadius.Value;
        }

        public override void UpdateRepresentations()
        {
            var profile = new Circle(Vector3.Origin, Start.Diameter / 2).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS);

            var oneSweep = new Sweep(profile,
                                     GetSweepLine(),
                                     0,
                                     0,
                                     0,
                                     false);

            var arrows = new List<SolidOperation>();
            arrows.AddRange(Start.GetArrow(Transform.Origin, fittingRotationTransform: GetRotatedTransform()));
            arrows.AddRange(End.GetArrow(Transform.Origin, fittingRotationTransform: GetRotatedTransform()));
            var solidOperations = new List<SolidOperation> { oneSweep }.Concat(arrows).Concat(GetExtensions()).ToList();

            if (UseRepresentationInstances)
            {
                FittingRepresentationStorageClass.SetFittingRepresentation(this, () => solidOperations);
            }
            else
            {
                Representation = new Representation(solidOperations);
            }
        }

        public override Port[] GetPorts()
        {
            return new[] { this.Start, this.End };
        }

        public override List<Port> BranchSidePorts()
        {
            return new List<Port> { Start };
        }

        public override Port TrunkSidePort()
        {
            return End;
        }

        private Vector3 BendRadiusOffset(double? bendRadius, Vector3 direction)
        {
            return bendRadius != null && bendRadius != 0 ? direction.Unitized() * bendRadius.Value : Vector3.Origin;
        }

        private Polyline GetSweepLine()
        {
            var sweepLine = new List<Vector3>
            {
                Start.Position - Transform.Origin
            };

            if (BendRadius != 0)
            {
                var startDirection = Vector3.XAxis;
                var startPoint = startDirection * BendRadius;
                var startNormal = startDirection.Cross(Vector3.ZAxis).Unitized();

                var originalPlane = new Polygon(Vector3.Origin, (Start.Position - Transform.Origin).Unitized(), (End.Position - Transform.Origin).Unitized());
                var transform = originalPlane.ToTransform();
                var inverted = transform.Inverted();
                originalPlane.Transform(inverted);

                var angleBetweenOriginalVectors = originalPlane.Vertices[1].PlaneAngleTo(originalPlane.Vertices[2]) * Math.PI / 180;
                var endDirection = new Vector3(Math.Cos(angleBetweenOriginalVectors), Math.Sin(angleBetweenOriginalVectors));
                var endPoint = endDirection * BendRadius;
                var endNormal = endDirection.Cross(Vector3.ZAxis).Unitized();

                new Ray(startPoint, startNormal).Intersects(new Ray(endPoint, endNormal), out var intersectionPoint, true);

                if (intersectionPoint.IsZero())
                {
                    sweepLine.Add(Vector3.Origin);
                }
                else
                {
                    var startAngle = Vector3.XAxis.PlaneAngleTo((startPoint - intersectionPoint).Unitized());
                    var endAngle = Vector3.XAxis.PlaneAngleTo((endPoint - intersectionPoint).Unitized());

                    var arc = new Arc(intersectionPoint, startPoint.DistanceTo(intersectionPoint), startAngle, endAngle).ToPolyline().TransformedPolyline(transform);
                    sweepLine.AddRange(arc.Vertices);
                }
            }
            else
            {
                sweepLine.Add(Vector3.Origin);
            }

            sweepLine.Add(End.Position - Transform.Origin);

            if (UseRepresentationInstances)
            {
                var t = GetRotatedTransform().Inverted();
                return new Polyline(sweepLine.Select(v => t.OfPoint(v)).ToList());
            }
            else
            {
                return new Polyline(sweepLine);
            }
        }

        public override Transform GetRotatedTransform()
        {
            var zAxis = End.Direction.Cross(Start.Direction).Unitized();
            var t = new Transform(Vector3.Origin, End.Direction, zAxis);
            return t;
        }

        /// <inheritdoc/>
        public override string GetRepresentationHash()
        {
            return $"{this.GetType().Name}-{this.Diameter}-{this.BendRadius}-{this.Angle}";
        }
    }
}