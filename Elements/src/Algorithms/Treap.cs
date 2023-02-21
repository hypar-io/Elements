using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace Elements.Algorithms
{
    /// <summary>
    /// Iterator over a treap, supports moving forward and backward, indexing, subtracting, adding, has special 'end' value for invalid requests.
    /// </summary>
    /// <typeparam name="TKey">Type of the treap's underlying values.</typeparam>
    public class TreapIterator<TKey>
    {
        private TreapNode<TKey> t;
        private Treap<TKey> _treap;

        /// <summary>
        /// Initializes the iterator to the beginning of the treap.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap
        /// </summary>
        /// <param name="treap">The treap to which the iterator will be bound.</param>
        public TreapIterator(Treap<TKey> treap)
        {
            _treap = treap;
            if (treap == null)
            {
                return;
            }
            t = treap.Root;
            if (t == null)
            {
                return;
            }
            t = t.Min;
        }

        /// <summary>
        /// Initializes the iterator to the specific point in the treap.
        /// Asymptotic complexity: O(1)     - worst case
        /// </summary>
        /// <param name="treap">The treap to which the iterator will be bound.</param>
        /// <param name="node">The node to which the iterator will initially point.</param>
        public TreapIterator(Treap<TKey> treap, TreapNode<TKey> node)
        {
            _treap = treap;
            if (treap == null)
            {
                return;
            }
            t = node;
        }

        /// <summary>
        /// Checks whether the iterator points to the point after the end of the treap.
        /// Throws an exception if the treap is null.
        /// Asymptotic complexity: O(1)     - worst case
        /// </summary>
        public bool IsEnd
        {
            get
            {
                if (_treap == null)
                {
                    throw new Exception("The Treap is non-existent.");
                }
                return t == null;
            }
        }

        /// <summary>
        /// Returns the value to which the iterator points.
        /// Throws an exception if the treap is null or if the iterator points to 'end'.
        /// Asymptotic complexity: O(n)     - worst case
        /// where n - size of the treap
        /// </summary>
        public TKey Get
        {
            get
            {
                if (_treap == null)
                {
                    throw new Exception("The Treap is non-existent.");
                }
                if (t == null)
                {
                    throw new Exception("The enumerator is at the end of the Treap.");
                }
                return t.val;
            }
        }

        /// <summary>
        /// Moves the iterator one position forward in the treap.
        /// Moves the last one to 'end'.
        /// Does not change 'end'.
        /// Throws an exception if the treap is null.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        ///                        O(n)     - total worst case when moving the iterator from beginning to end
        /// where n - size of the treap
        /// </summary>
        /// <returns>True if we are still not at the 'end' after moving.</returns>
        public bool MoveNext()
        {
            if (_treap == null)
            {
                throw new Exception("The Treap is non-existent.");
            }
            if (t == null)
            {
                return false;
            }

            if (t.right != null)
            {
                t = t.right.Min;
                return true;
            }
            while (t != null)
            {
                TreapNode<TKey> q = t;
                t = t.parent;
                if (t != null && t.left == q)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Moves the iterator one position backward in the treap.
        /// Does not change if it already points to the first element.
        /// Moves 'end' to the last element.
        /// Throws an exception if the treap is null.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        ///                        O(n)     - total worst case when moving the iterator from end to beginning
        /// where n - size of the treap
        /// </summary>
        /// <returns>True if we are not at the beginning before the move.</returns>
        public bool MovePrevious()
        {
            if (_treap == null)
            {
                throw new Exception("The Treap is non-existent.");
            }
            if (t == null)
            {
                t = _treap.Root;
                if (t == null)
                {
                    return false;
                }
                t = t.Max;
                return true;
            }

            if (t.left != null)
            {
                t = t.left.Max;
                return true;
            }
            while (t != null)
            {
                TreapNode<TKey> q = t;
                t = t.parent;
                if (t != null && t.right == q)
                {
                    return true;
                }
            }

            t = _treap.Root.Min;
            return false;
        }

        /// <summary>
        /// Returns the index of the item, to which the iterator points in the sorted list representation of the treap.
        /// Returns the size of the treap when the iterator is 'end'.
        /// Throws an exception if the treap is null.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        ///                        O(n)     - total worst case when using it on all elements once
        /// where n - size of the treap
        /// </summary>
        public int Index
        {
            get
            {
                if (_treap == null)
                {
                    throw new Exception("The Treap is non-existent.");
                }

                if (t == null)
                {
                    return _treap.Size;
                }

                int ans = t.left == null ? 0 : t.left.sz;
                TreapNode<TKey> v = t;
                while (v != null)
                {
                    TreapNode<TKey> u = v;
                    v = v.parent;
                    if (v != null && v.right == u)
                    {
                        ans += 1 + (v.left == null ? 0 : v.left.sz);
                    }
                }
                return ans;
            }
        }

        /// <summary>
        /// Returns the difference of positions, to which two iterators point.
        /// Throws an exception if the treaps are different or if the treap is null.
        /// Just subtracts iterators' indices.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap
        /// </summary>
        /// <param name="treapIterator1"></param>
        /// <param name="treapIterator2"></param>
        /// <returns></returns>
        public static int operator -(TreapIterator<TKey> treapIterator1, TreapIterator<TKey> treapIterator2)
        {
            if (treapIterator1._treap != treapIterator2._treap)
            {
                throw new Exception("The supplied iterators correspond to different Treaps.");
            }

            return treapIterator1.Index - treapIterator2.Index;
        }

        /// <summary>
        /// Erases the element, to which the iterator points.
        /// Does nothing of the iterator is 'end'.
        /// Throws an exception if the treap is null.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap
        /// </summary>
        public void Erase()
        {
            if (_treap == null)
            {
                throw new Exception("The Treap is non-existent.");
            }

            if (t != null)
            {
                _treap.Erase(t);
            }
        }

        /// <summary>
        /// Moves an iterator of fixed numver of steps.
        /// Throws an exception if the treap is null or if the resulting iterator points to an invalid point.
        /// </summary>
        /// <param name="treapIterator">The iterator.</param>
        /// <param name="dt">Number of steps which to move.</param>
        /// <returns></returns>
        public static TreapIterator<TKey> operator +(TreapIterator<TKey> treapIterator, int dt)
        {
            if (treapIterator._treap == null)
            {
                throw new Exception("The Treap is non-existent.");
            }

            return treapIterator._treap.Get(treapIterator.Index + dt);
        }
    }

    /// <summary>
    /// Essentially a helper class for the Treap, it's not recommeded to use it separately.
    /// Represents a node in the actual treap.
    /// </summary>
    /// <typeparam name="TKey">The type of the underlying values.</typeparam>
    public class TreapNode<TKey>
    {
        /// <summary>
        /// Left and right sons, nulls represent their absence.
        /// Parent is always null for the root.
        /// </summary>
        public TreapNode<TKey> left, right, parent;
        /// <summary>
        /// The value this node stores.
        /// It is used as the key as well.
        /// </summary>
        public TKey val;
        /// <summary>
        /// Priority of the node.
        /// Lower priorities go higher in the treap, closer to the root.
        /// Typically it's set to a random value.
        /// </summary>
        public int prior;
        /// <summary>
        /// Size of the subtreap this node roots.
        /// </summary>
        public int sz;

        /// <summary>
        /// An empty constructor, all values are set to default values.
        /// Don't use it.
        /// </summary>
        public TreapNode() { }
        /// <summary>
        /// Constructs a separate node with given value (i.e. key) and priority.
        /// Left, right, parent are nulls.
        /// Size is correspondingly 1.
        /// </summary>
        /// <param name="x">The value</param>
        /// <param name="priority">The priority</param>
        public TreapNode(TKey x, int priority)
        {
            val = x;
            prior = priority;
            parent = left = right = null;
            sz = 1;
        }

        /// <summary>
        /// Returns the leftmost node, the one that has the smallest value, in the subtree, which this node roots.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap subtree
        /// </summary>
        public TreapNode<TKey> Min
        {
            get
            {
                TreapNode<TKey> t = this;
                while (t.left != null)
                {
                    t = t.left;
                }
                return t;
            }
        }

        /// <summary>
        /// Returns the rightmost node, the one that has the largest value, in the subtree, which this node roots.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap subtree
        /// </summary>
        public TreapNode<TKey> Max
        {
            get
            {
                TreapNode<TKey> t = this;
                while (t.right != null)
                {
                    t = t.right;
                }
                return t;
            }
        }
    }

    /// <summary>
    /// The class that represent a treap.
    /// Supports inserting, erasing values, computing lower and upper bound.
    /// Stores the values in the increasing order.
    /// </summary>
    /// <typeparam name="TKey">The type of the values inside the treap.</typeparam>
    public class Treap<TKey>
    {
        /// <summary>
        /// A generator of random values to use as priorities for the nodes.
        /// </summary>
        static Random priorityGenerator = new Random(228);
        /// <summary>
        /// A function that returns a random priority for a node.
        /// </summary>
        static int nextPriority() { return priorityGenerator.Next(); }

        /// <summary>
        /// Comparer to define the order of the TKey values.
        /// </summary>
        private Comparer<TKey> _cmp;
        /// <summary>
        /// The root node.
        /// </summary>
        private TreapNode<TKey> root;

        /// <summary>
        /// A read-only root of the treap.
        /// Normally, you shouldn't use it.
        /// </summary>
        public TreapNode<TKey> Root { get { return root; } }

        /// <summary>
        /// The size of the treap.
        /// </summary>
        public int Size { get { return root.sz; } }

        /// <summary>
        /// Returns the value at the specified index.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap
        /// </summary>
        public TKey this[int index] { get { return Get(index).Get; } }

        /// <summary>
        /// Return an iterator that points to the beginning of the Treap.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap
        /// </summary>
        public TreapIterator<TKey> Begin { get { return new TreapIterator<TKey>(this, Root.Min); } }
        /// <summary>
        /// Returns an iterator that points to 'end'.
        /// Asymptotic complexity: O(1)     - worst case
        /// where n - size of the treap
        /// </summary>
        public TreapIterator<TKey> End { get { return new TreapIterator<TKey>(this, null); } }

        /// <summary>
        /// A recursive function that writes all the values in the specific subtreap to the specified list.
        /// Asymptotic complexity: O(n)     - worst case
        /// where n - size of the subtreap
        /// </summary>
        /// <param name="t">The root of the subtreap.</param>
        /// <param name="lst">The list in which to insert all values.</param>
        private void ToListDfs(TreapNode<TKey> t, List<TKey> lst)
        {
            if (t == null)
            {
                return;
            }
            ToListDfs(t.left, lst);
            lst.Add(t.val);
            ToListDfs(t.right, lst);
        }
        /// <summary>
        /// Return the list of all the values in the treap, in sorted order.
        /// Asymptotic complexity: O(n)     - worst case
        /// where n - size of the treap
        /// </summary>
        public List<TKey> ToList()
        {
            var ans = new List<TKey>();
            ToListDfs(root, ans);
            return ans;
        }

        /// <summary>
        /// Initializes an empty Treap with the given comparator.
        /// If the comparator is not supplied or is null the TKey must inherit from IComparable{TKey}, in which case we take the default comparator.
        /// </summary>
        /// <param name="cmp"></param>
        public Treap(Comparer<TKey> cmp = null)
        {
            if (cmp == null)
            {
                if (typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey)))
                {
                    cmp = Comparer<TKey>.Default;
                }
                else
                {
                    throw new Exception("No comparer was supplied and the underlying type is not implicitly comparable");
                }
            }
            _cmp = cmp;
            root = null;
        }

        /// <summary>
        /// Splits a treap t into treaps l and r, where 
        /// for all elements x in l: f(x) compares less (not more) to key via cmp
        /// for all elements x in r: f(x) compares not less (more) to key via cmp
        /// when less is true (false)
        /// Used to compute lower (upper) bound.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap t
        /// </summary>
        /// <typeparam name="T">The type to which all elements are mapped.</typeparam>
        /// <param name="t">The treap to split.</param>
        /// <param name="l">The left of the resulting subtrees.</param>
        /// <param name="r">The right of the resulting subtrees.</param>
        /// <param name="key">The value by which we split values.</param>
        /// <param name="f">The mapping function</param>
        /// <param name="cmp">Comparator of the mapped values. MUST be compatible with the Treap's comparator.</param>
        /// <param name="less">Whether to compute lower or upper bound, i.e. whether the values equivalent to key are in the left or right resulting treap.</param>
        private void Split<T>(TreapNode<TKey> t, out TreapNode<TKey> l, out TreapNode<TKey> r, T key, Func<TKey, T> f, Comparer<T> cmp, bool less)
        {
            if (t == null)
            {
                l = r = null;
                return;
            }

            int res = cmp.Compare(f(t.val), key);
            if (res < 0 || (res == 0 && !less))
            {
                Split(t.right, out t.right, out r, key, f, cmp, less);
                l = t;
            }
            else
            {
                Split(t.left, out l, out t.left, key, f, cmp, less);
                r = t;
            }
            update(t);
        }

        /// <summary>
        /// The simplified version of the split, when the comparing type is the same as the original one and the comparator does not change either.
        /// For more information look at <see cref="Split{T}(TreapNode{TKey}, out TreapNode{TKey}, out TreapNode{TKey}, T, Func{TKey, T}, Comparer{T}, bool)"/>
        /// </summary>
        private void Split(TreapNode<TKey> t, out TreapNode<TKey> l, out TreapNode<TKey> r, TKey key, bool less = true)
        {
            Split(t, out l, out r, key, p => p, _cmp, less);
        }

        /// <summary>
        /// Merges two treaps l and r into one treap t.
        /// No element in r can compare `less` to any element in l via the Treap's comparator.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the resulting treap
        /// </summary>
        /// <param name="t">The resulting treap.</param>
        /// <param name="l">The left (smaller values) treap.</param>
        /// <param name="r">The right (larger values) treap.</param>
        private void Merge(out TreapNode<TKey> t, TreapNode<TKey> l, TreapNode<TKey> r)
        {
            if (l == null)
            {
                t = r;
                return;
            }
            if (r == null)
            {
                t = l;
                return;
            }

            if (l.prior < r.prior)
            {
                Merge(out l.right, l.right, r);
                t = l;
            }
            else
            {
                Merge(out r.left, l, r.left);
                t = r;
            }
            update(t);
        }

        /// <summary>
        /// Splits a treap t into two treaps l and r, where l has k smallest values from t.
        /// If k is more than the size of t, then l becomes t and r becomes null.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap t
        /// </summary>
        /// <param name="t">The treap to split.</param>
        /// <param name="l">The left resulting treap.</param>
        /// <param name="r">The right resulting treap.</param>
        /// <param name="k">The number of smallest values to separate.</param>
        private void splitBySize(TreapNode<TKey> t, out TreapNode<TKey> l, out TreapNode<TKey> r, int k)
        {
            if (t == null)
            {
                l = r = null;
                return;
            }

            if (k <= getSize(t.left))
            {
                splitBySize(t.left, out l, out t.left, k);
                r = t;
            }
            else
            {
                splitBySize(t.right, out t.right, out r, k - getSize(t.left) - 1);
                l = t;
            }
            update(t);
        }

        /// <summary>
        /// Gets an iterator that points to the index's smallest value.
        /// Throws an exception if the supplied index is less than 0 or is supposed to point to after 'end'.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap
        /// </summary>
        /// <param name="index">The index to which the iterator should point.</param>
        /// <returns></returns>
        public TreapIterator<TKey> Get(int index)
        {
            if (index < 0 || Size < index)
            {
                throw new Exception("The requested index is out of bounds.");
            }

            TreapNode<TKey> l, r;
            splitBySize(root, out l, out r, index);
            var res = new TreapIterator<TKey>(this, r?.Min);
            Merge(out root, l, r);
            return res;
        }

        /// <summary>
        /// Deletes a specific node from the treap.
        /// Again, you should try to avoid using this function, since using nodes directly is not recommended.
        /// See <see cref="TreapIterator{TKey}.Erase"/> for the proper function. 
        /// </summary>
        /// <param name="node">The node to delete.</param>
        public void Erase(TreapNode<TKey> node)
        {
            if (node == null)
            {
                return;
            }

            var p = node.parent;
            bool left = p == null || p.right == node ? false : true;
            Merge(out node, node.left, node.right);
            if (p != null)
            {
                if (left)
                {
                    p.left = node;
                }
                else
                {
                    p.right = node;
                }
            }
            else
            {
                root = node;
            }
        }

        /// <summary>
        /// Returns the size of a treap rooted at t, even if t is null.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private int getSize(TreapNode<TKey> t)
        {
            return t == null ? 0 : t.sz;
        }

        /// <summary>
        /// Updates the values in the treap node that depend on its subtree.
        /// Assumes the children have proper values.
        /// Asymptotic complexity: O(1)     - worst case
        /// where n - size of the treap
        /// </summary>
        /// <param name="t">the node to update.</param>
        private void update(TreapNode<TKey> t)
        {
            if (t == null)
            {
                return;
            }

            t.sz = 1;
            t.parent = null;
            if (t.left != null)
            {
                t.left.parent = t;
                t.sz += t.left.sz;
            }
            if (t.right != null)
            {
                t.right.parent = t;
                t.sz += t.right.sz;
            }
        }

        /// <summary>
        /// Inserts a value into the treap.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap
        /// </summary>
        /// <param name="x">The value to insert.</param>
        public void Insert(TKey x)
        {
            TreapNode<TKey> l, r;
            Split(root, out l, out r, x);
            Merge(out l, l, new TreapNode<TKey>(x, nextPriority()));
            Merge(out root, l, r);
        }

        /// <summary>
        /// Erases a value from the treap.
        /// Only deletes one instance if there are multiple.
        /// Does nothing if there are none.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap
        /// </summary>
        /// <param name="x">The value to delete.</param>
        public void Erase(TKey x)
        {
            TreapNode<TKey> t1, t2;
            Split(root, out root, out t1, x);
            Split(t1, out t1, out t2, x, false);
            if (t1 != null)
            {
                Merge(out t1, t1.left, t1.right);
            }
            Merge(out t1, t1, t2);
            Merge(out root, root, t1);
        }

        /// <summary>
        /// A function to find the smallest element in the Treap that compares not less (more) via cmp to x after having been transformed with the function f as lower is true (false).
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap
        /// </summary>
        /// <typeparam name="T">The type used for comparison.</typeparam>
        /// <param name="x">The value relative to which to find the lower (upper) bound.</param>
        /// <param name="f">The function to transform the Treap's values for comparison.</param>
        /// <param name="cmp">The function to compare the transformed values.</param>
        /// <param name="lower">Whether we are looking for a lower or an upper bound.</param>
        /// <returns>The iterator pointing to the element. 'End' if there is no sucj element.</returns>
        private TreapIterator<TKey> LowerUpperBound<T>(T x, Func<TKey, T> f, Comparer<T> cmp, bool lower)
        {
            if (root == null)
            {
                return new TreapIterator<TKey>(this, null);
            }
            TreapNode<TKey> t1;
            Split(root, out root, out t1, x, f, cmp, lower);
            if (t1 == null)
            {
                return new TreapIterator<TKey>(this, null);
            }
            var ans = new TreapIterator<TKey>(this, t1.Min);
            Merge(out root, root, t1);
            return ans;
        }

        /// <summary>
        /// A simplified version of the <see cref="LowerUpperBound{T}(T, Func{TKey, T}, Comparer{T}, bool)"/> that assumes that neither type nor the comparer changed.
        /// </summary>
        private TreapIterator<TKey> LowerUpperBound(TKey x, bool lower)
        {
            return LowerUpperBound(x, p => p, _cmp, lower);
        }

        /// <summary>
        /// A lower bound version of the <see cref="LowerUpperBound{T}(T, Func{TKey, T}, Comparer{T}, bool)"/>.
        /// If the comparer is null or not supplied the comparison type T must derive from IComparable{T} in which case the comparer is taken to be the default one.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap
        /// </summary>
        /// <typeparam name="T">The type used for comparison.</typeparam>
        /// <param name="x">The value for lower bound.</param>
        /// <param name="f">The function tranforming the treap's elements for comparison.</param>
        /// <param name="cmp">The comparator function for lower bound.</param>
        public TreapIterator<TKey> LowerBound<T>(T x, Func<TKey, T> f, Comparer<T> cmp = null)
        {
            if (cmp == null)
            {
                if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
                {
                    cmp = Comparer<T>.Default;
                }
                else
                {
                    throw new Exception("No comparer was supplied and the supplied type is not implicitly comparable");
                }
            }
            return LowerUpperBound(x, f, cmp, true);
        }

        /// <summary>
        /// A upper bound version of the <see cref="LowerUpperBound{T}(T, Func{TKey, T}, Comparer{T}, bool)"/>.
        /// If the comparer is null or not supplied the comparison type T must derive from IComparable{T} in which case the comparer is taken to be the default one.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap
        /// </summary>
        /// <typeparam name="T">The type used for comparison.</typeparam>
        /// <param name="x">The value for upper bound.</param>
        /// <param name="f">The function tranforming the treap's elements for comparison.</param>
        /// <param name="cmp">The comparator function for upper bound.</param>
        public TreapIterator<TKey> UpperBound<T>(T x, Func<TKey, T> f, Comparer<T> cmp = null)
        {
            if (cmp == null)
            {
                if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
                {
                    cmp = Comparer<T>.Default;
                }
                else
                {
                    throw new Exception("No comparer was supplied and the supplied type is not implicitly comparable");
                }
            }
            return LowerUpperBound(x, f, cmp, false);
        }

        /// <summary>
        /// A lower bound version of the <see cref="LowerUpperBound{T}(T, Func{TKey, T}, Comparer{T}, bool)"/>.
        /// The comparator is the same one, as in the Treap.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap
        /// </summary>
        /// <param name="x">The value for lower bound.</param>
        public TreapIterator<TKey> LowerBound(TKey x)
        {
            return LowerUpperBound(x, true);
        }

        /// <summary>
        /// A upper bound version of the <see cref="LowerUpperBound{T}(T, Func{TKey, T}, Comparer{T}, bool)"/>.
        /// The comparator is the same one, as in the Treap.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap
        /// </summary>
        /// <param name="x">The value for upper bound.</param>
        public TreapIterator<TKey> UpperBound(TKey x)
        {
            return LowerUpperBound(x, false);
        }
    }
}
