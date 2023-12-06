using System;
using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements.Fittings
{
    public partial class StraightSegment
    {
        public const double MinLength = 0;

        public Guid Connection { get; set; }

        public StraightSegment(int wallThickness, Port end, Port start, Material material = null, bool allowMismatch = false) : base(null, new Transform(),
                                                                                                                                    material ?? FittingTreeRouting.DefaultPipeMaterial,
                                                                                                                                    null,
                                                                                                                                    false,
                                                                                                                                    Guid.NewGuid(),
                                                                                                                                    "")
        {
            if (!allowMismatch && end.Diameter != start.Diameter)
            {
                throw new ArgumentException("The two pipe end connectors are not the same size, this is not allowed");
            }
            if (end.Diameter.ApproximatelyEquals(0) || start.Diameter.ApproximatelyEquals(0))
            {
                throw new ArgumentException("Cannot create a pipe with 0 diameter.  Check the size of your ends and try again.");
            }
            this.Diameter = end.Diameter;
            this.WallThickness = wallThickness;
            this.End = end;
            this.Start = start;
            this.ComponentLocator = FittingLocator.Empty();
            this.SetPath();
        }


        public void SetPath()
        {
            this.Path = new Polyline(new List<Vector3> { End.Position, Start.Position });
        }

        public double Length()
        {
            return this.End.Position.DistanceTo(this.Start.Position);
        }

        public override void UpdateRepresentations()
        {
            if (this.End == null || this.Start == null)
            {
                Representation = null;
            }
            else if (this.End.Position.IsAlmostEqualTo(this.Start.Position))
            {
                Representation = new Representation(new List<SolidOperation> { new Lamina(Polygon.Rectangle(0.2, 0.2).TransformedPolygon(new Transform(this.Start.Position))) });
            }
            else
            {
                var diameter = this.Diameter;
                if (diameter == 0)
                {
                    diameter = 0.001;
                }
                var profile = new Circle(Vector3.Origin, diameter / 2).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS);
                var line = new Line(this.End.Position, this.Start.Position);
                var pipe = new Sweep(profile, line, 0, 0, 0, false);
                Representation = new Representation(new List<SolidOperation> { pipe });
            }
        }

        public bool IsVertical()
        {
            var direction = this.End.Position - this.Start.Position;
            return direction.IsParallelTo(Vector3.ZAxis);
        }

        public bool IsValidConnection(out Transform updatedTransform)
        {
            // Note, we handle either start and end swapping places (an inverted pipe)
            // or the start and end becoming misaligned due to a trunksideTransform
            // but not both.
            // TODO devise a test and ensure we can handle a wider range of strange pipe conditions.
            var end = End.Position;
            var start = Start.Position;

            if (this.TrunkSideComponent != null)
            {
                end = TrunkSideComponent.GetPropagatedTransform(TransformDirection.TrunkToBranch).OfPoint(end);
            }

            var direction = (End.Position - Start.Position).Unitized();
            var newDirection = (end - start).Unitized();
            bool startAndEndSwitchedPlaces = newDirection.Negate().IsAlmostEqualTo(direction);

            updatedTransform = new Transform();

            double newDistance = start.DistanceTo(end);
            if (newDistance >= MinLength && !startAndEndSwitchedPlaces)
            {
                if (!newDirection.IsAlmostEqualTo(direction))
                {
                    updatedTransform = TrunkSideComponent.AdditionalTransform.Inverted();
                    return false;
                }
                return true;
            }

            if (startAndEndSwitchedPlaces)
            {
                updatedTransform.Move(direction * -(newDistance + MinLength));
            }
            else if (MinLength >= newDistance)
            {
                updatedTransform.Move(direction * -(MinLength - newDistance));
            }

            return false;
        }

        public override bool PropagateAdditionalTransform(Transform transform, TransformDirection transformDirection)
        {
            if (End.Position.IsAlmostEqualTo(Start.Position))
            {
                AdditionalTransform.Concatenate(transform);
                return true;
            }

            var length = End.Position.DistanceTo(Start.Position);
            Vector3 endPoint, startPoint;
            if (transformDirection == TransformDirection.TrunkToBranch)
            {
                endPoint = End.Position;
                startPoint = Start.Position;
            }
            else
            {
                endPoint = Start.Position;
                startPoint = End.Position;
            }

            var direction = (startPoint - endPoint).Unitized();
            var newEndPoint = transform.OfPoint(endPoint);
            if (newEndPoint.IsAlmostEqualTo(endPoint))
            {
                // Even if the new endpoint of the pipe is unchanged we still want to propogate the transform.
                // This is expected when 'transform' is an identity transform, so we don't need to update anything.
                return true;
            }

            var v = newEndPoint - endPoint;
            var d = v.Dot(direction);
            if (d.ApproximatelyEquals(0))
            {
                AdditionalTransform.Concatenate(transform);
                return true;
            }

            var leftover = newEndPoint - startPoint;
            if (d < length)
            {
                var closest = endPoint + direction * d;
                leftover = newEndPoint - closest;
            }

            if (leftover.IsZero())
            {
                return false;
            }
            else
            {
                var inverseTranslationDelta = (leftover - transform.Origin);
                AdditionalTransform.Concatenate(transform.Moved(inverseTranslationDelta));
            }

            return true;
        }

        public override Transform GetPropagatedTransform(TransformDirection transformDirection)
        {
            return AdditionalTransform;
        }

        public override void ApplyAdditionalTransform()
        {
            ClearAdditionalTransform();
        }

        public override void ClearAdditionalTransform()
        {
            var inverted = new Transform(AdditionalTransform);
            inverted.Invert();
            AdditionalTransform.Concatenate(inverted);
        }

        public override Port TrunkSidePort()
        {
            return End;
        }

        public override List<Port> BranchSidePorts()
        {
            return new List<Port> { Start };
        }
    }
}