using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Flow;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Newtonsoft.Json;

namespace Elements.Fittings
{
    public class WyeSettings
    {
        public double MainDistance;
        public double BranchDistance;
        public double TrunkDistance;
        public double MainDiameter;
        public double BranchDiameter;
        public double Diameter;
        public double AngleTolerance;
        public double PortsDistanceTolerance;
        public double[] AllowedBranchAngles = new[] { 45.0, 90, 180.0 };
        public WyeSettings()
        {
            this.TrunkDistance = 0.06;
            this.MainDistance = 0.1;
            this.BranchDistance = 0.1;
            this.MainDiameter = FittingTreeRouting.DefaultDiameter;
            this.BranchDiameter = FittingTreeRouting.DefaultDiameter;
            this.Diameter = FittingTreeRouting.DefaultDiameter;
            this.AngleTolerance = 0.1;
            this.PortsDistanceTolerance = 0.001;
        }
        public WyeSettings(double trunkDiameter,
            double mainDiameter,
            double branchDiameter,
            double trunkDistance,
            double mainDistance,
            double branchDistance,
            double[] allowedAngles = null,
            double angleTolerance = 0.1,
            double portsDistanceTolerance = 0.001)
        {
            this.Diameter = trunkDiameter;
            this.MainDiameter = mainDiameter;
            this.BranchDiameter = branchDiameter;

            this.TrunkDistance = trunkDistance;
            this.MainDistance = mainDistance;
            this.BranchDistance = branchDistance;

            if (allowedAngles != null)
            {
                this.AllowedBranchAngles = allowedAngles;
            }

            this.AngleTolerance = angleTolerance;
            this.PortsDistanceTolerance = portsDistanceTolerance;
        }
    }
    public partial class Wye
    {
        [JsonProperty]
        public double AngleTolerance { get; set; }

        [JsonProperty]
        public double PositionTolerance { get; set; }

        public Wye(Vector3 position, Vector3 mainDirection, Vector3 branchDirection, WyeSettings wyeSettings, Material material) : this(position,
                                                                                                                                 mainDirection.Negate(),
                                                                                                                                 mainDirection,
                                                                                                                                 branchDirection,
                                                                                                                                 wyeSettings,
                                                                                                                                 material)
        { }

        public Wye(Vector3 position, Vector3 trunkDirection, Vector3 mainDirection, Vector3 branchDirection, WyeSettings wyes, Material material) :
                                                                                         base(false, FittingLocator.Empty(), new Transform(),
                                                                                              material == null ? FittingTreeRouting.DefaultFittingMaterial : material,
                                                                                              new Representation(new List<SolidOperation>()),
                                                                                              false,
                                                                                              Guid.NewGuid(),
                                                                                              "")
        {
            this.Transform = new Transform(position);

            this.Trunk = new Port(position + trunkDirection.Unitized() * wyes.TrunkDistance, trunkDirection.Unitized(), wyes.Diameter);
            this.MainBranch = new Port(position + mainDirection.Unitized() * wyes.MainDistance, mainDirection.Unitized(), wyes.MainDiameter);

            var branchAngle = branchDirection.AngleTo(mainDirection);
            if (wyes.AllowedBranchAngles.Count() > 0 && wyes.AllowedBranchAngles.All(a => !branchAngle.ApproximatelyEquals(a, wyes.AngleTolerance)))
            {
                throw new ArgumentOutOfRangeException($"That branch directions provided make an angle of {branchAngle} which is not allowed for this wyes settings");
            }

            var branchEnd = position + branchDirection.Unitized() * wyes.BranchDistance;

            this.SideBranch = new Port(branchEnd, branchDirection, wyes.BranchDiameter);

            AngleTolerance = wyes.AngleTolerance;
            PositionTolerance = wyes.PortsDistanceTolerance;
        }

        public static (Connection mainConnection, Connection branchConnection) GetMainAndBranch(Connection[] connections, Connection outgoing)
        {
            var firstAngle = connections[0].Direction().AngleTo(outgoing.Direction());
            var secondAngle = connections[1].Direction().AngleTo(outgoing.Direction());
            if (firstAngle.ApproximatelyEquals(0, 1))
            {
                return (connections[0], connections[1]);
            }
            else if (secondAngle.ApproximatelyEquals(0, 1))
            {
                return (connections[1], connections[0]);
            }
            else
            {
                // fairly robust fallback if neither incoming branch is aligned with the outgoing branch
                if (firstAngle % 0 > secondAngle % 90)
                {
                    return (connections[0], connections[1]);
                }
                else
                {
                    return (connections[1], connections[0]);
                }
            }
        }

        public override void UpdateRepresentations()
        {
            var trunkPosition = Trunk.Position;
            var mainPosition = MainBranch.Position;
            var branchPosition = SideBranch.Position;
            var origin = this.Transform.Origin;

            var trunkProfile = new Circle(new Vector3(), this.Trunk.Diameter / 2).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS);
            var trunkLine = new Line(Vector3.Origin, trunkPosition - origin);
            var trunk = new Sweep(trunkProfile, trunkLine, 0, 0, 0, false);

            var mainProfile = new Circle(new Vector3(), this.MainBranch.Diameter / 2).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS);
            var mainLine = new Line(Vector3.Origin, mainPosition - origin);
            var main = new Sweep(mainProfile, mainLine, 0, 0, 0, false);

            var branchProfile = new Circle(new Vector3(), this.SideBranch.Diameter / 2).ToPolygon(FlowSystemConstants.CIRCLE_SEGMENTS);
            var branchLine = new Line(Vector3.Origin, branchPosition - origin);
            var branch = new Sweep(branchProfile, branchLine, 0, 0, 0, false);

            var arrows = this.Trunk.GetArrow(this.Transform.Origin).Concat(this.SideBranch.GetArrow(this.Transform.Origin)).Concat(this.MainBranch.GetArrow(this.Transform.Origin));
            var solidOps = new List<SolidOperation> { trunk, main, branch }.Concat(arrows).Concat(GetExtensions()).ToList();
            this.Representation = new Geometry.Representation(solidOps);
        }

        public override Port[] GetPorts()
        {
            return new[] { this.Trunk, this.MainBranch, this.SideBranch };
        }

        public override List<Port> BranchSidePorts()
        {
            return new List<Port> { MainBranch, SideBranch };
        }

        public override Port TrunkSidePort()
        {
            return Trunk;
        }

        public ComponentBase GetBranchSideComponent(Port connector)
        {
            if (connector != MainBranch && connector != SideBranch)
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

        public override Transform GetRotatedTransform()
        {
            var zAxis = Trunk.Direction.Cross(SideBranch.Direction).Unitized();
            var t = new Transform(Vector3.Origin, Trunk.Direction, zAxis);
            return t;
        }
    }
}