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
    /// <typeparam name="T">Type of the item. Must support hash and comparing.</typeparam>
    public class PriorityQueue<T> where T : IComparable<T>
    {
        private List<(T Id, double Priority)> _priorities;
        private Dictionary<T, int> _positions;

        /// <summary>
        /// Creates an empty collection.
        /// </summary>
        public PriorityQueue()
        {
            _priorities = new List<(T, double)>();
            _positions = new Dictionary<T, int>();
        }

        /// <summary>
        /// Creates collection from input list.
        /// First item in the list is set to priority 0, others with double.MaxValue.
        /// </summary>
        /// <param name="uniqueCollection">List of ids.</param>
        public PriorityQueue(IEnumerable<T> uniqueCollection)
        {
            _priorities = new List<(T, double)>(uniqueCollection.Count());
            _positions = new Dictionary<T, int>();
            _priorities.Add((uniqueCollection.First(), 0d));
            _positions[uniqueCollection.First()] = 0;

            int i = 1;
            foreach (var item in uniqueCollection.Skip(1))
            {
                _priorities.Add((item, double.PositiveInfinity));
                _positions[item] = i;
                i++;
            }
        }

        /// <summary>
        /// Returns the lowest priority item from collection.
        /// Throws an exception if called on an empty collection.
        /// </summary>
        /// <returns>Id of the item with lowest priority.</returns>
        public T PopMin()
        {
            var min = _priorities[0];
            Swap(0, _priorities.Count - 1);
            _positions.Remove(min.Id);
            _priorities.RemoveAt(_priorities.Count - 1);
            ShiftDown(0);
            return min.Id;
        }

        /// <summary>
        /// Adds a new item to the collection.
        /// If an item with id already exist in collection - it's priority will be updated.
        /// </summary>
        /// <param name="id">Id of the item.</param>
        /// <param name="priority">New priority.</param>
        public void AddOrUpdate(T id, double priority)
        {
            if (_positions.TryGetValue(id, out var index))
            {
                Update(index, priority);
            }
            else
            {
                _priorities.Add((id, priority));
                _positions[id] = _priorities.Count - 1;
                ShiftUp(_priorities.Count - 1);
            }
        }

        /// <summary>
        /// Checks if certain Id is in the queue.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Contains(T id)
        {
            return _positions.ContainsKey(id);
        }

        /// <summary>
        /// Sets priority for the item with given id.
        /// Does nothing if item is not present in the queue.
        /// </summary>
        /// <param name="id">Id of the item.</param>
        /// <param name="priority">New priority.</param>
        public void UpdatePriority(T id, double priority)
        {
            if (_positions.TryGetValue(id, out var index))
            {
                Update(index, priority);
            }
        }

        /// <summary>
        /// Is collection empty.
        /// </summary>
        public bool Empty()
        {
            return !_priorities.Any();
        }

        private void Update(int node, double priority)
        {
            double oldPriority = _priorities[node].Priority;
            _priorities[node] = (_priorities[node].Id, priority);

            if (priority < oldPriority)
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
            _positions[_priorities[left].Id] = right;
            _positions[_priorities[right].Id] = left;
            (_priorities[left], _priorities[right]) = (_priorities[right], _priorities[left]);
        }

        private void ShiftUp(int node)
        {
            while (node > 0 && _priorities[Parent(node)].Priority > _priorities[node].Priority)
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

            if (l < count && _priorities[l].Priority < _priorities[min].Priority)
            {
                min = l;
            }

            if (r < count && _priorities[r].Priority < _priorities[min].Priority)
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
}
