using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Spatial.AdaptiveGrid
{
    /// <summary>
    /// Object that lets you apply an edge weight factor to edges that meet a Condition filter function.
    /// If an edge meets the condition of several WeightModifier objects
    /// they will be grouped by group name and factor aggregator function will be applied to WeightModifiers <see cref="AdaptiveGraphRouting.SetWeightModifiersGroupFactorAggregator"/>.
    /// By default - the lowest factor of group is chosen.
    /// Factors of all groups will be multiplied.
    /// </summary>
    public class WeightModifier
    {
        /// <summary>
        /// Basic constructor for a WeightModifier
        /// </summary>
        /// <param name="name">Name of the modifier.</param>
        /// <param name="condition">Filter function.</param>
        /// <param name="factor">Weight to be applied.</param>
        /// <param name="group">Group name of the modifier.</param>
        public WeightModifier(string name, Func<Vertex, Vertex, bool> condition, double factor, string group = null)
        {
            Name = name;
            Condition = condition;
            Factor = factor;
            Group = group ?? "default";
        }

        /// <summary>
        ///  WeightModifier name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Filter function that determines if this WeightModifier applies to an edge.
        /// </summary>
        public Func<Vertex, Vertex, bool> Condition;

        /// <summary>
        /// Weight to be applied according to this WeightModifier.
        /// </summary>
        public double Factor;

        /// <summary>
        /// Group name of the modifier.
        /// </summary>
        public readonly string Group;
    }
}
