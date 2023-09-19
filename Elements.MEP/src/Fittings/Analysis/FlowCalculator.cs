using System;
using Elements;
using System.Collections.Generic;
using System.Text;
using Elements.Flow;
using Elements.Fittings;
using Elements.Geometry;
using System.Linq;

namespace Elements.Fittings
{
    /// <summary>
    /// An abstract class for calculating flow.
    /// </summary>
    public abstract class FlowCalculator
    {
        /// <summary>
        /// Set Flow parameter for equipment included in the tree.
        /// </summary>
        /// <param name="tree">Tree that contains fittings that will be updated.</param>
        /// <returns>Errors encountered during assignment process.</returns>
        public abstract List<FittingError> AssignFlowCalcs(FittingTree tree);

        /// <summary>
        /// Flow update strategy object that is used to update flow parameters of leaf terminals.
        /// Used by UpdateFlow function.
        /// </summary>
        public IFlowUpdateStrategy FlowUpdateStrategy { get; set; }

        /// <summary>
        /// The behavior is controlled by FlowUpdateStrategy object.
        /// If FlowUpdateStrategy is set, it will be used to update flow parameters of leaf terminals.
        /// </summary>
        /// <param name="tree">Tree that contains leaf terminals that will be checked.</param>
        /// <returns>True if leaf flow was updated. False if FlowUpdateStrategy is not set.</returns>
        public bool UpdateLeafFlow(FittingTree tree)
        {
            if (FlowUpdateStrategy == null)
            {
                return false;
            }
            else
            {
                return FlowUpdateStrategy.UpdateLeafFlow(tree);
            }
        }
    }
}
