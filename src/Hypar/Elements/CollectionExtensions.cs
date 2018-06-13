using Hypar.Geometry;
using System;
using System.Collections.Generic;

namespace Hypar.Elements
{
    public static class CollectionExtensions
    {
        public static IEnumerable<T> AlongEachCreate<T>(this IEnumerable<Line> lines, Func<Line,T> creator)
        {
            var result = new List<T>();
            foreach(var l in lines)
            {
                result.Add(creator(l));
            }
            return result;
        }

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