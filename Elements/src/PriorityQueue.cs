using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Elements
{
    /// <summary>
    /// PriorityQueue is a collection that allows you to retrieve the item with the lowest
    /// priority in constant time and be able update priority of an item with log complexity.
    /// Items are unique within the collection but priorities can have duplicate values.
    /// </summary>
    /// <typeparam name="TPriority">Type of items' priorities. Must support comparing.</typeparam>
    /// <typeparam name="TValue">Type of items' values. Must be equitable.</typeparam>
    public class PriorityQueue<TPriority, TValue> 
        where TPriority : IComparable<TPriority>
        where TValue : IEquatable<TValue>
    {
        private List<(TValue Value, TPriority Priority)> _priorities;
        private Dictionary<TValue, int> _positions;

        /// <summary>
        /// Creates an empty collection.
        /// </summary>
        public PriorityQueue()
        {
            _priorities = new List<(TValue, TPriority)>();
            _positions = new Dictionary<TValue, int>();
        }

        /// <summary>
        /// Creates collection from input list.
        /// All items in the list are set to priority 0
        /// </summary>
        /// <param name="uniqueCollection">List of items.</param>
        public PriorityQueue(IEnumerable<TValue> uniqueCollection)
        {
            _priorities = new List<(TValue, TPriority)>(uniqueCollection.Count());
            _positions = new Dictionary<TValue, int>();

            int i = 0;
            foreach (var item in uniqueCollection.Skip(1))
            {
                _priorities.Add((item, default(TPriority)));
                _positions[item] = i;
                i++;
            }
        }

        /// <summary>
        /// Returns the lowest priority item from collection.
        /// Throws an exception if called on an empty collection.
        /// </summary>
        /// <returns>The item with lowest priority and its priority.</returns>
        public (TValue Value, TPriority Priority) PopMin()
        {
            var min = _priorities[0];
            Swap(0, _priorities.Count - 1);
            _positions.Remove(min.Value);
            _priorities.RemoveAt(_priorities.Count - 1);
            ShiftDown(0);
            return min;
        }

        /// <summary>
        /// Adds a new item to the collection.
        /// If an equivalent item already exists in collection - it's priority will be updated.
        /// </summary>
        /// <param name="value">The item.</param>
        /// <param name="priority">New priority.</param>
        public void AddOrUpdate(TValue value, TPriority priority)
        {
            if (_positions.TryGetValue(value, out var index))
            {
                Update(index, priority);
            }
            else
            {
                _priorities.Add((value, priority));
                _positions[value] = _priorities.Count - 1;
                ShiftUp(_priorities.Count - 1);
            }
        }

        /// <summary>
        /// Checks if certain item is in the queue.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(TValue value)
        {
            return _positions.ContainsKey(value);
        }

        /// <summary>
        /// Sets priority for the item.
        /// Does nothing if item is not present in the queue.
        /// </summary>
        /// <param name="value">The item.</param>
        /// <param name="priority">New priority.</param>
        public void UpdatePriority(TValue value, TPriority priority)
        {
            if (_positions.TryGetValue(value, out var index))
            {
                Update(index, priority);
            }
        }

        /// <summary>
        /// Is the collection empty.
        /// </summary>
        public bool Empty()
        {
            return !_priorities.Any();
        }

        private void Update(int node, TPriority priority)
        {
            TPriority oldPriority = _priorities[node].Priority;
            _priorities[node] = (_priorities[node].Value, priority);

            if (priority.CompareTo(oldPriority) < 0)
            {
                ShiftUp(node);
            }
            else
            {
                ShiftDown(node);
            }
        }

        private int Parent(int node)
        {
            return (node - 1) / 2;
        }

        private int LeftChild(int node)
        {
            return 2 * node + 1;
        }

        private int RightChild(int node)
        {
            return 2 * node + 2;
        }

        private void Swap(int left, int right)
        {
            _positions[_priorities[left].Value] = right;
            _positions[_priorities[right].Value] = left;
            (_priorities[left], _priorities[right]) = (_priorities[right], _priorities[left]);
        }

        private void ShiftUp(int node)
        {
            while (node > 0 && _priorities[Parent(node)].Priority.CompareTo(_priorities[node].Priority) > 0)
            {
                Swap(Parent(node), node);
                node = Parent(node);
            }
        }

        private void ShiftDown(int node)
        {
            int min = node;
            var count = _priorities.Count;
            int l = LeftChild(node);
            int r = RightChild(node);

            if (l < count && _priorities[l].Priority.CompareTo(_priorities[min].Priority) < 0)
            {
                min = l;
            }

            if (r < count && _priorities[r].Priority.CompareTo(_priorities[min].Priority) < 0)
            {
                min = r;
            }

            if (node != min)
            {
                Swap(node, min);
                ShiftDown(min);
            }
        }
    }

    /// <summary>
    /// A priority queue with double as its priority type.
    /// </summary>
    /// <typeparam name="TValue">The type of items. Must be equitable.</typeparam>
    public class PriorityQueue<TValue> : PriorityQueue<double, TValue> where TValue : IEquatable<TValue> 
    {
        public PriorityQueue() : base() { }
        public PriorityQueue(IEnumerable<TValue> uniqueCollection) : base(uniqueCollection) { }
    };
}
