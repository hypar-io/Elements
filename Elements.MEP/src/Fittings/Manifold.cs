using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements.Fittings
{
    public partial class Manifold
    {
        private double _size;

        public Manifold(Vector3 position, Vector3 trunkDirection, double trunkDiameter, List<(Vector3 direction, double diameter)> branches, Material material = null) :
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
            _size = branches.Max(d => d.diameter) * 1.5;
            var distance = _size / 2;
            this.Trunk = new Port(position + trunkDirection.Unitized() * distance, trunkDirection.Unitized(), trunkDiameter);
            this.Branches = new List<Port>();
            foreach (var (direction, diameter) in branches)
            {
                Branches.Add(new Port(position + direction.Unitized() * distance, direction.Unitized(), diameter));
            }
        }

        public override List<Port> BranchSidePorts()
        {
            return Branches.ToList();
        }

        public override Port[] GetPorts()
        {
            return new[] {Trunk}.Concat(Branches).ToArray();
        }

        public override Port TrunkSidePort()
        {
            return Trunk;
        }

        public override void UpdateRepresentations()
        {
            var extrude = new Extrude(Polygon.Rectangle(_size, _size).TransformedPolygon(new Transform(new Vector3(0, 0, -_size / 2))), _size, Vector3.ZAxis, false);
            var arrows = new List<Sweep>();
            arrows.AddRange(this.Trunk.GetArrow(this.Transform.Origin));
            foreach (var branch in Branches)
            {
                arrows.AddRange(branch.GetArrow(this.Transform.Origin));
            }
            var solidOps = new List<SolidOperation> { extrude }.Concat(arrows).Concat(GetExtensions()).ToList();
            this.Representation = new Geometry.Representation(solidOps);
        }

        public override Transform GetRotatedTransform()
        {
            throw new NotImplementedException();
        }
    }
}