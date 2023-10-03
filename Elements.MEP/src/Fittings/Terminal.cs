using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Flow;

namespace Elements.Fittings
{
    public partial class Terminal
    {
        public Terminal(Vector3 fittingPosition, Vector3 portDirection, double endLength, double diameter, Material material) :
                                                                        base(false, FittingLocator.Empty(), new Transform(fittingPosition),
                                                                            material == null ? FittingTreeRouting.DefaultFittingMaterial : material,
                                                                            new Representation(new List<SolidOperation>()),
                                                                            false,
                                                                            Guid.NewGuid(),
                                                                            "")
        {
            this.Port = new Port(fittingPosition + portDirection.Unitized() * endLength, portDirection.Unitized(), diameter);
        }

        public Terminal(Vector3 fittingPosition, Vector3 portDirection, Vector3 connectorPoint, double diameter, Material material) :
            base(false, FittingLocator.Empty(), new Transform(fittingPosition),
                material == null ? FittingTreeRouting.DefaultFittingMaterial : material,
                new Representation(new List<SolidOperation>()),
                false,
                Guid.NewGuid(),
                "")
        {
            this.Port = new Port(connectorPoint, portDirection.Unitized(), diameter);
        }

        public override void UpdateRepresentations()
        {
            var profile = new Circle((this.Port.Diameter > 0 ? this.Port.Diameter : 0.001) / 2).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS);
            var arrows = this.Port.GetArrow(this.Transform.Origin);
            var lineEnd = this.Port.Position - this.Transform.Origin;
            Representation = new Representation(new List<SolidOperation>(arrows));

            if (lineEnd.IsAlmostEqualTo(Vector3.Origin))
            {
                Representation.SolidOperations.Add(new Lamina(profile));
            }
            else if (Transform.Origin.ClosestPointOn(new Line(Port.Position, Port.Position + Port.Direction), true)
                     .IsAlmostEqualTo(Transform.Origin))
            {
                var line = new Line(Vector3.Origin, lineEnd);
                var sweep = new Sweep(profile, line, 0, 0, 0, false);
                Representation.SolidOperations.Add(sweep);
            }
            else
            {
                var elbowPoint = new Vector3(0, 0, Port.Position.Z - Transform.Origin.Z);
                var line = new Polyline(Vector3.Origin, elbowPoint, lineEnd);
                var sweep = new Sweep(profile, line, 0, 0, 0, false);
                Representation.SolidOperations.Add(sweep);
            }

            foreach (var so in GetExtensions())
            {
                Representation.SolidOperations.Add(so);
            }
        }

        public double? GetFinalStaticPressure()
        {
            var totalChange = this.PressureCalculations?.StaticGain - this.PressureCalculations?.PipeLoss - this.PressureCalculations?.ZLoss;
            if (totalChange == null)
            {
                return null;
            }
            else
            {
                return this.Port.Flow?.StaticPressure - totalChange.Value;
            }
        }

        public override Port[] GetPorts()
        {
            return new[] { this.Port };
        }

        public override List<Port> BranchSidePorts()
        {
            if (BranchSideComponents.Any())
            {
                return new List<Port> { Port };
            }
            return new List<Port>();
        }

        public override Port TrunkSidePort()
        {
            if (TrunkSideComponent != null)
            {
                return Port;
            }
            return null;
        }

        public override Transform GetRotatedTransform()
        {
            var zAxis = Vector3.ZAxis;
            if (Port.Direction.IsParallelTo(zAxis))
            {
                zAxis = Vector3.XAxis;
            }
            var t = new Transform(Vector3.Origin, Port.Direction, zAxis);
            return t;
        }
    }
}