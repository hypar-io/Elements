using Elements.Fittings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Fittings
{
    /// <summary>
    /// The interface for flow update strategy.
    /// </summary>
    public interface IFlowUpdateStrategy
    {
        /// <summary>
        /// Clear all internal states.
        /// </summary>
        public void Reset();

        /// <summary>
        /// Check if flow parameters of leaf terminals need to be updated based on other parameters.
        /// If update is necessary, the function will update flow parameter of the leaf terminals.
        /// Other fittings need to be updated by calling AssignFlowCalcs function.
        /// </summary>
        /// <param name="tree">Tree that contains leaf terminals that will be checked.</param>
        /// <returns>True if leaf flow was updated.</returns>
        public bool UpdateLeafFlow(FittingTree tree);
    }
}
