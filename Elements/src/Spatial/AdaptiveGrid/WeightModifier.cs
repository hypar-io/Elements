using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Spatial.AdaptiveGrid
{
    /// <summary>
    /// Object that hold function that allows to apply factor multiplier to edge travel cost.
    /// If edge passes check of several WeightModifier objects - lowest factor is chosen.
    /// </summary>
    public class WeightModifier
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="condition"></param>
        /// <param name="factor"></param>
        public WeightModifier(string name, Func<Vertex, Vertex, bool> condition, double factor)
        {
            Name = name;
            Condition = condition;
            Factor = factor;
        }

        /// <summary>
        ///  WeightModifier description.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Function, edge need to pass, can include additional data, captured by lambda.
        /// </summary>
        public Func<Vertex, Vertex, bool> Condition;

        /// <summary>
        /// Multiplier number that is applied to edge traveling cost.
        /// </summary>
        public double Factor;
    }
}
