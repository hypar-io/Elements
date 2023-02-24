using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Algorithms
{
    /// <summary>
    /// Common interface for iterable containers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    interface IIterable<T>
    {
        /// <summary>
        /// The iterator that points to the smallest item.
        /// </summary>
        IIterator<T> Begin { get; }

        /// <summary>
        /// The iterator that points to `end`
        /// </summary>
        IIterator<T> End { get; }

        /// <summary>
        /// Returns a sorted list of items inside the class.
        /// </summary>
        List<T> ToList();

        /// <summary>
        /// Gets an interator that points to index's value.
        /// Throws an exception when the index is out of bounds.
        /// </summary>
        IIterator<T> GetIterator(int index);

        /// <summary>
        /// Deletes an item, to which the iterator points.
        /// Does nothing if the iterator is `end`.
        /// Throws an exception if the iterator is not bound to this structure.
        /// </summary>
        void Erase(IIterator<T> iterator);

        /// <summary>
        /// Inserts an item into the structure.
        /// </summary>
        void Insert(T x);

        /// <summary>
        /// Returns an iterator to an item that equals x, when run through f.
        /// Returns `end` if no such value exists.
        /// </summary>
        /// <typeparam name="TCompare">The type in which the values are compared.</typeparam>
        /// <param name="x">The value to which to compare.</param>
        /// <param name="f">The transformation function.</param>
        /// <param name="cmp">The comparator to use to find matches.</param>
        IIterator<T> Find<TCompare>(TCompare x, Func<T, TCompare> f, Comparer<TCompare> cmp);

        /// <summary>
        /// Erases one item from the structure if there are any, does nothing if not.
        /// </summary>
        /// <typeparam name="TCompare">The type in which the values are compared.</typeparam>
        /// <param name="x">The value to which to compare.</param>
        /// <param name="f">The transformation function.</param>
        /// <param name="cmp">The comparator to use to find matches.</param>
        void Erase<TCompare>(TCompare x, Func<T, TCompare> f, Comparer<TCompare> cmp);

        /// <summary>
        /// Erases all items that are equal to x from the structure if there are any, does nothing if not.
        /// </summary>
        /// <typeparam name="TCompare">The type in which the values are compared.</typeparam>
        /// <param name="x">The value to which to compare.</param>
        /// <param name="f">The transformation function.</param>
        /// <param name="cmp">The comparator to use to find matches.</param>
        void EraseAll<TCompare>(TCompare x, Func<T, TCompare> f, Comparer<TCompare> cmp);

        /// <summary>
        /// Returns an iterator that points to where the first item that does not compare less to the value.
        /// </summary>
        /// <typeparam name="TCompare">The type in which the values are compared.</typeparam>
        /// <param name="x">The value to which to compare.</param>
        /// <param name="f">The transformation function.</param>
        /// <param name="cmp">The comparator to use to find matches.</param>
        IIterator<T> LowerBound<TCompare>(TCompare x, Func<T, TCompare> f, Comparer<TCompare> cmp);

        /// <summary>
        /// Returns an iterator that points to where the first item that compares greater to the value.
        /// </summary>
        /// <typeparam name="TCompare">The type in which the values are compared.</typeparam>
        /// <param name="x">The value to which to compare.</param>
        /// <param name="f">The transformation function.</param>
        /// <param name="cmp">The comparator to use to find matches.</param>
        IIterator<T> UpperBound<TCompare>(TCompare x, Func<T, TCompare> f, Comparer<TCompare> cmp);
    }
}
