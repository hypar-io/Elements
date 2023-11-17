using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Newtonsoft.Json;

namespace Elements.Fittings
{
    public class CrossSettings
    {

        public double Distance_A;
        public double Distance_B;
        public double Distance_C;
        public double Distance_Trunk;
        public double Diameter_A;
        public double Diameter_B;
        public double Diameter_C;
        public double Diameter;
        public double[] AllowedBranchAngles;
        public double AngleTolerance = 0.1;
        public double PortsDistanceTolerance = 0.001;
        public CrossSettings()
        {
            Distance_Trunk = 0.06;
            Distance_A = 0.05;
            Distance_B = 0.05;
            Distance_C = 0.05;

            Diameter = 0.1;
            Diameter_A = 0.1;
            Diameter_B = 0.05;
            Diameter_C = 0.05;

            AllowedBranchAngles = new[] { 90d };
        }
    }

    public partial class Cross
    {
        [JsonProperty]
        public double AngleTolerance { get; set; }

        [JsonProperty]
        public double PositionTolerance { get; set; }

        public Cross(Vector3 position, Vector3 trunkDirection, Vector3 directionA, Vector3 directionB, Vector3 directionC, CrossSettings crossSettings, Material material = null) :
                                                                                         base(false,
                                                                                              FittingLocator.Empty(),
                                                                                              new Transform(),
                                                                                              material == null ? FittingTreeRouting.DefaultFittingMaterial : material,
                                                                                              new Representation(new List<SolidOperation>()),
                                                                                              false,
                                                                                              Guid.NewGuid(),
                                                                                              "")
        {
            this.Transform = new Transform(position);

            this.Trunk = new Port(position + trunkDirection.Unitized() * crossSettings.Distance_Trunk, trunkDirection.Unitized(), crossSettings.Diameter);
            this.BranchA = new Port(position + directionA.Unitized() * crossSettings.Distance_A, directionA.Unitized(), crossSettings.Diameter_A);
            this.BranchB = new Port(position + directionB.Unitized() * crossSettings.Distance_B, directionB.Unitized(), crossSettings.Diameter_B);
            this.BranchC = new Port(position + directionC.Unitized() * crossSettings.Distance_C, directionC.Unitized(), crossSettings.Diameter_C);

            AngleTolerance = crossSettings.AngleTolerance;
            PositionTolerance = crossSettings.PortsDistanceTolerance;

            var branchAngleA = directionA.AngleTo(trunkDirection);
            if (!branchAngleA.ApproximatelyEquals(180, AngleTolerance))
            {
                throw new ArgumentOutOfRangeException($"This cross fitting type expects that the branch A is opposite to the trunk direction, so the angle should be 180.  The branch A direction is {branchAngleA} degrees from the trunk direction.");
            }

            var branchAngleB = directionB.AngleTo(trunkDirection);
            if (crossSettings.AllowedBranchAngles.Count() > 0 && crossSettings.AllowedBranchAngles.All(a => !branchAngleB.ApproximatelyEquals(a, AngleTolerance)))
            {
                throw new ArgumentOutOfRangeException($"That branch directions provided make an angle of {branchAngleB} which is not allowed for this cross settings");
            }

            var branchAngleC = directionC.AngleTo(trunkDirection);
            if (crossSettings.AllowedBranchAngles.Count() > 0 && crossSettings.AllowedBranchAngles.All(a => !branchAngleC.ApproximatelyEquals(a, AngleTolerance)))
            {
                throw new ArgumentOutOfRangeException($"That branch directions provided make an angle of {branchAngleC} which is not allowed for this cross settings");
            }
        }

        public override Port[] GetPorts()
        {
            return new[] { Trunk, BranchA, BranchB, BranchC };
        }

        public override List<Port> BranchSidePorts()
        {
            return new List<Port> { BranchA, BranchB, BranchC };
        }

        public override Port TrunkSidePort()
        {
            return Trunk;
        }

        public ComponentBase GetBranchSideComponent(Port connector)
        {
            if (connector != BranchA && connector != BranchB && connector != BranchC)
            {
                return null;
            }

            return BranchSideComponents.SingleOrDefault(x =>
            {
                if (x is StraightSegment)
                {
                    return x.TrunkSidePort().IsIdenticalConnector(connector, PositionTolerance, AngleTolerance);
                }

                return x.TrunkSidePort().IsComplimentaryConnector(connector, PositionTolerance, AngleTolerance);
            });
        }

        public override void UpdateRepresentations()
        {
            var trunkPosition = Trunk.Position;
            var positionA = BranchA.Position;
            var positionB = BranchB.Position;
            var positionC = BranchC.Position;
            var origin = this.Transform.Origin;

            var trunkProfile = new Circle(new Vector3(), this.Trunk.Diameter / 2).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS);
            var trunkLine = new Line(Vector3.Origin, trunkPosition - origin);
            var trunk = new Sweep(trunkProfile, trunkLine, 0, 0, 0, false);

            var profileA = new Circle(new Vector3(), this.BranchA.Diameter / 2).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS);
            var lineA = new Line(Vector3.Origin, positionA - origin);
            var A = new Sweep(profileA, lineA, 0, 0, 0, false);

            var profileB = new Circle(new Vector3(), this.BranchB.Diameter / 2).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS);
            var lineB = new Line(Vector3.Origin, positionB - origin);
            var B = new Sweep(profileB, lineB, 0, 0, 0, false);

            var profileC = new Circle(new Vector3(), this.BranchC.Diameter / 2).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS);
            var lineC = new Line(Vector3.Origin, positionC - origin);
            var C = new Sweep(profileC, lineC, 0, 0, 0, false);

            var arrows = this.Trunk.GetArrow(this.Transform.Origin).Concat(this.BranchB.GetArrow(this.Transform.Origin)).Concat(this.BranchA.GetArrow(this.Transform.Origin)).Concat(this.BranchC.GetArrow(this.Transform.Origin));
            var solidOps = new List<SolidOperation> { trunk, A, B, C }.Concat(arrows).Concat(GetExtensions()).ToList();
            this.Representation = new Geometry.Representation(solidOps);
        }

        public override Transform GetRotatedTransform()
        {
            var branches = new[] { BranchA, BranchB, BranchC };
            var zAxis = branches.Select(b => Trunk.Direction.Cross(b.Direction).Unitized()).First(axis => !axis.IsZero());
            var t = new Transform(Vector3.Origin, Trunk.Direction, zAxis);
            return t;
        }
    }
}