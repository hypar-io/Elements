using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements.Fittings
{
    public partial class Coupler
    {
        public Coupler(string type, Vector3 position, Vector3 direction, double length, double diameter, Material material = null) :
                base(false, FittingLocator.Empty(), new Transform(position),
                    material == null ? FittingTreeRouting.DefaultFittingMaterial : material,
                    new Representation(new List<SolidOperation>()),
                    false,
                    Guid.NewGuid(),
                    "")
        {
            this.Diameter = diameter;
            this.CouplerType = type;
            this.Start = new Port(position, direction.Negate(), diameter);
            this.End = new Port(position + direction.Unitized() * length, direction, diameter);
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

        public double Length()
        {
            return this.End.Position.DistanceTo(this.Start.Position);
        }

        public void AssignToStart(StraightSegment pipe)
        {
            pipe.Start = this.End;
            this.TrunkSideComponent = pipe;
            this.BranchSideComponents.Clear();
            foreach (var component in pipe.BranchSideComponents)
            {
                this.BranchSideComponents.Add(component);
                component.TrunkSideComponent = this;
            }
            pipe.BranchSideComponents.Clear();
            pipe.BranchSideComponents.Add(this);
        }

        public void AssignToEnd(StraightSegment pipe)
        {
            var trunkComponent = pipe.TrunkSideComponent;
            pipe.End = this.Start;
            pipe.TrunkSideComponent = this;
            this.BranchSideComponents.Add(pipe);
            this.TrunkSideComponent = trunkComponent;
            trunkComponent.BranchSideComponents.Remove(pipe);
            trunkComponent.BranchSideComponents.Add(this);
        }

        public override void UpdateRepresentations()
        {
            var start = this.Start.Position - Transform.Origin;
            if (start.IsAlmostEqualTo(this.End.Position - Transform.Origin))
            {
                start += this.Start.Direction * 1E-3;
            }
            var line = new Line(End.Position - Transform.Origin, start);
            var profile = new Circle(Vector3.Origin, Start.Diameter / 2).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS);
            var main = new Sweep(profile, line, 0, 0, 0, false);

            var arrows = this.Start.GetArrow(this.Transform.Origin).Concat(End.GetArrow(Transform.Origin));
            Representation = new Representation(new List<SolidOperation> { main }.Concat(arrows).Concat(GetExtensions()).ToList());
        }

        public override Transform GetRotatedTransform()
        {
            var zAxis = Vector3.ZAxis;
            if (End.Direction.IsParallelTo(zAxis))
            {
                zAxis = Vector3.XAxis;
            }
            var t = new Transform(Vector3.Origin, End.Direction, zAxis);
            return t;
        }

        public override string GetRepresentationHash()
        {
            throw new NotImplementedException();
        }
    }
}