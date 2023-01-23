using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Algorithms
{
    /// <summary>
    /// A lightweight implementation of the maximal binary heap data structure
    /// See
    /// https://en.wikipedia.org/wiki/Binary_heap
    /// for a detailed explanation of the operations
    /// </summary>
    /// <typeparam name="TKey">Type of the keys in the heap (must be comparable)</typeparam>
    /// <typeparam name="TValue">Type of the values in the heap</typeparam>
    class BinaryHeap<TKey, TValue> where TKey : IComparable<TKey>
    {
        public BinaryHeap()
        {
            n = 0;
            a = new List<(TKey, TValue)>();
        }

        private void heapify(int i)
        {
            // corrects a heap, rooted in the ith vertex, assuming the children are already roots of valid heaps
            // it swaps the root with the maximal of its children, if necessary, and calls recursively on that child
            // tail-recursion is expanded into a loop
            while (true)
            {
                int j = (i << 1) + 1;
                if (n <= j) break;
                if (j + 1 < n && a[j + 1].Item1.CompareTo(a[j].Item1) > 0) ++j;
                if (a[j].Item1.CompareTo(a[i].Item1) <= 0) break;
                (a[i], a[j]) = (a[j], a[i]);
                i = j;
            }
        }

        public BinaryHeap((TKey, TValue)[] array)
        {
            // construct a binary heap from the given array of numbers
            // Asymptotic complexity: O(n), where n is the size of the array
            // 
            // Algorithm: makes the subtrees valid heaps starting from the bottom up
            n = array.Length;
            a = new List<(TKey, TValue)>(array);
            for (int i = n - 1; i >= 0; --i) heapify(i);
        }

        public void Insert(TKey key, TValue value)
        {
            // insert a new number into the heap
            // Asymptotic complexity: O(log n) worst case
            //                        O(1)     average (assuming no Extract are intermixed)
            //
            // Algorithm: adds the element to the bottom layer and swaps it with its parents until the root is reached or swaps are no longer needed
            if (n < a.Count) a[n] = (key, value);
            else a.Add((key, value));
            int i = n++;
            while (i > 0)
            {
                int j = (i - 1) >> 1;
                if (a[i].Item1.CompareTo(a[j].Item1) <= 0) break;
                (a[i], a[j]) = (a[j], a[i]);
                i = j;
            }
        }

        public void Insert((TKey, TValue) element)
        {
            // just another version of the insert
            Insert(element.Item1, element.Item2);
        }

        public (TKey, TValue) Extract()
        {
            // extracts and returns the top element from the heap
            // Asymptotic complexity: O(log n) worst case
            //
            // Algorithm: swaps the top element with the last one, heapifies the very root
            (a[0], a[n - 1]) = (a[n - 1], a[0]);
            --n;
            heapify(0);
            return a[n];
        }

        public void Clear()
        {
            // empties the heap
            n = 0;
        }

        // checks whether the heap is empty
        public bool Empty { get { return n == 0; } }
        // returns the size of the heap
        public int Size { get { return n; } }
        // returns the top element of the heap
        public (TKey, TValue) Max { get { return a[0]; } }


        // current size of the heap
        private int n;
        // the actual container for the heap elements
        private List<(TKey, TValue)> a;
    }
}
