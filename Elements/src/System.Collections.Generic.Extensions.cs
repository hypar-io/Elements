using System;
using System.Collections.Generic;

namespace Elements.Collections.Generics
{
    internal static class Extensions
    {
        public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source, int count)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (count < 0) throw new ArgumentOutOfRangeException("count");
            var array = new TSource[count];
            int i = 0;
            foreach (var item in source)
            {
                array[i++] = item;
            }
            return array;
        }
    }
}