using Elements.Fittings;
using Elements.Flow;
using Elements.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Fittings
{
    /// <summary>
    /// A class that computes flow for specific equipment selected by the user.
    /// </summary>
    public class RemoteAreaFlowCalculator : FlowCalculator
    {
        private Polygon RemoteArea;

        /// <summary>
        /// Constructs RemoteAreaCalculator taking account remote area defined by the user.
        /// </summary>
        /// <param name="area">Polygon that marks equipment which flow will not be equal to 0.</param>
        public RemoteAreaFlowCalculator(Polygon area)
        {
            RemoteArea = new Polygon(area.Vertices.Select(v => new Vector3(v.X, v.Y, .0)).ToList());
        }

        /// <summary>
        /// Set Flow parameter for equipment that is located within remote area defined by the user.
        /// </summary>
        /// <param name="tree">Tree that contains equipment that will be updated.</param>
        /// <returns>Errors encountered during assignment process.</returns>
        public override List<FittingError> AssignFlowCalcs(FittingTree tree)
        {
            var errors = new List<FittingError>();
            var terminals = tree.FittingsOfType<Terminal>();

            var leafTerminals = terminals.Where(t => t.TrunkSidePort() != null).ToList();
            // double check that there's only one trunk terminal;
            if (leafTerminals.Count() > terminals.Count - 1)
            {
                errors.Add(new FittingError("There are multiple trunk terminals in the FittingTree.  This is not supported."));
                return errors;
            }

            foreach (var port in tree.Fittings.Where(f => f is not Terminal t || t.FlowNode is Trunk)
                                              .SelectMany(f => f.GetPorts()))
            {
                if (port.Flow != null)
                {
                    port.Flow.FlowRate = 0;
                }
            }

            foreach (var t in leafTerminals)
            {
                if (t.FlowNode is not Leaf)
                {
                    errors.Add(new FittingError($"Terminal FlowNode {t.Name} is not a leaf.", t.TrunkSidePort()?.Position));
                    continue;
                }

                var leafFlow = (t.FlowNode as Leaf).Flow;
                if (!IsInsideRemoteArea(t))
                {
                    leafFlow = .0;
                }
                
                tree.PropagateFlow(t, leafFlow);
            }

            // connectors for loop sections won't have flow here, so need to set it manually
            var connectorsWithoutFlow = tree.Fittings.SelectMany(f => f.GetPorts()).Where(c => c.Flow == null);
            foreach (var connector in connectorsWithoutFlow)
            {
                connector.AddFlow(0);
            }

            return errors;
        }

        private bool IsInsideRemoteArea(Terminal terminal)
        {
            var projectedTerminalCentre = terminal.Transform.Origin;
            projectedTerminalCentre.Z = .0;
            return RemoteArea.Contains(projectedTerminalCentre);
        }
    }
}
