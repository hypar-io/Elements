using Hypar.Geometry;
using System;
using System.Collections.Generic;

namespace Hypar.Elements
{
    /// <summary>
    /// Extension methods for collections.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// For each line in the provided collection, apply a creator function.
        /// </summary>
        /// <param name="lines">A collection of lines.</param>
        /// <param name="creator">The function to apply.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>A collection of T.</returns>
        public static IEnumerable<T> AlongEachCreate<T>(this IEnumerable<Line> lines, Func<Line,T> creator)
        {
            var result = new List<T>();
            foreach(var l in lines)
            {
                result.Add(creator(l));
            }
            return result;
        }

        /// <summary>
        /// For each polyline in the collection, apply a creator function.
        /// </summary>
        /// <param name="polylines">A collection of polylines.</param>
        /// <param name="creator">The function to apply.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>A collection of T.</returns>
        public static IEnumerable<T> WithinEachCreate<T>(this IEnumerable<Polyline> polylines, Func<Polyline,T> creator)
        {
            var result = new List<T>();
            foreach(var pline in polylines)
            {
                result.Add(creator(pline));
            }
            return result;
        }
    }
}