using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Algorithms
{
    /// <summary>
    /// Common interface for all bidirectional iterators that support moving by multiple steps
    /// </summary>
    /// <typeparam name="T"></typeparam>
    interface IIterator<T>
    {
        /// <summary>
        /// The structure to which the iterator is bound
        /// </summary>
        IIterable<T> _parent { get; }

        /// <summary>
        /// `End` is a specific value that points to a virtual place after the end of the current container
        /// </summary>
        bool IsEnd { get; }

        /// <summary>
        /// Gets the item to which the iterator points
        /// </summary>
        T Item { get; }

        /// <summary>
        /// Tries to move the iterator 1 step forward
        /// Does nothing if it already points to `End`
        /// Returns whether the new value is not `End`
        /// </summary>
        /// <returns></returns>
        bool MoveNext();

        /// <summary>
        /// Tries to move the iterator 1 step backward
        /// Does nothing if it already points to the first position
        /// If it points to `end`
        ///                       stays `end` if the container is empty
        ///                       starts to point to the last element in the container
        /// Returns true if the iterator has changed
        /// </summary>
        /// <returns></returns>
        bool MovePrevious();

        /// <summary>
        /// Returns an index within the container, to which the iterator points
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Moves the iterator by a specific delta (forward or backward)
        /// Returns a new iterator that points to the new position
        /// Throws an exception if the new position is out of bounds and not `end`
        /// </summary>
        /// <param name="delta"></param>
        /// <returns></returns>
        IIterator<T> Advance(int delta);
    }
}
