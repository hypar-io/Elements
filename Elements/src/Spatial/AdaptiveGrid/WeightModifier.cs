using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Spatial.AdaptiveGrid
{
    /// <summary>
    /// Object that lets you apply an edge weight factor to edges that meet a Condition filter function.
    /// If an edge meets the nameof(Condition) of several WeightModifier objects the lowest factor is chosen.
    /// </summary>
    public class WeightModifier
    {
        /// <summary>
        /// Basic constructor for a WeightModifier
        /// </summary>
        /// <param name="name">Name of the modifier.</param>
        /// <param name="condition">Filter function.</param>
        /// <param name="factor">Weight to be applied.</param>
        public WeightModifier(string name, Func<Vertex, Vertex, bool> condition, double factor)
        {
            Name = name;
            Condition = condition;
            Factor = factor;
        }

        /// <summary>
        ///  WeightModifier name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Filter function that the determines if this WeightModifier applies to an edge.
        /// </summary>
        public Func<Vertex, Vertex, bool> Condition;

        /// <summary>
        /// Weight to be applied according to this WeightModifier.
        /// </summary>
        public double Factor;
    }
}
