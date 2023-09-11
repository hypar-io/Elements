using Elements.Fittings;
using Elements.Flow;
using System;
using System.Collections.Generic;
using Elements.Geometry;
using System.Text;
using System.Linq;

namespace Elements.Fittings
{
    /// <summary>
    /// A class for calculating flow.
    /// </summary>
    public class FullFlowCalculator : FlowCalculator
    {
        /// <summary>
        /// Traverses the entire FittingTree and assigns flow to each port.
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public override List<FittingError> AssignFlowCalcs(FittingTree tree) 
        {
            var errors = new List<FittingError>();
            var terminals = tree.FittingsOfType<Terminal>();

            var leafTerminals = terminals.Where(t => t.TrunkSidePort() != null);
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
                if (t.FlowNode is Leaf)
                {
                    var leafFlow = (t.FlowNode as Leaf).Flow;
                    tree.PropagateFlow(t, leafFlow);
                }
                else
                {
                    errors.Add(new FittingError($"Terminal FlowNode {t.Name} is not a leaf.",
                                                t.TrunkSidePort()?.Position));
                }
            }

            // connectors for loop sections won't have flow here, so need to set it manually
            var connectorsWithoutFlow = tree.Fittings.SelectMany(f => f.GetPorts()).Where(c => c.Flow == null);
            foreach (var connector in connectorsWithoutFlow)
            {
                connector.AddFlow(0);
            }

            return errors;
        }
    }
}
