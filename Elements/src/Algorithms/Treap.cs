using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace Elements.Algorithms
{
    /// <summary>
    /// Iterator over a treap. See <see cref="IIterator{T}"/> for more details
    /// </summary>
    /// <typeparam name="TKey">Type of the treap's underlying values.</typeparam>
    public class TreapIterator<TKey> : IIterator<TKey>
    {
        public TreapNode<TKey> node { get; private set; }
        public Treap<TKey> _treap { get; private set; }
        IIterable<TKey> IIterator<TKey>._parent { get { return _treap; } }

        /// <summary>
        /// Initializes the iterator to the beginning of the treap.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap
        /// </summary>
        /// <param name="treap">The treap to which the iterator will be bound.</param>
        public TreapIterator(Treap<TKey> treap)
        {
            if (treap == null)
            {
                throw new Exception("The Treap is non-existent.");
            }

            _treap = treap;
            node = treap.root;
            if (node == null)
            {
                return;
            }
            node = node.Min;
        }

        /// <summary>
        /// Initializes the iterator to the specific point in the treap.
        /// Asymptotic complexity: O(1)     - worst case
        /// </summary>
        /// <param name="treap">The Treap to which the iterator is bound.</param>
        /// <param name="node">The node to which the iterator will initially point.</param>
        public TreapIterator(Treap<TKey> treap, TreapNode<TKey> node)
        {
            if (treap == null)
            {
                throw new Exception("The Treap is non-existent.");
            }

            if (node != null && treap != node._parent)
            {
                throw new Exception("The node does not belong to the specified treap.");
            }    

            _treap = treap;
            this.node = node;
        }

        /// <summary>
        /// Asymptotic complexity: O(1)     - worst case
        /// See <see cref="IIterator{T}.IsEnd"/> for more details
        /// </summary>
        public bool IsEnd
        {
            get
            {
                return node == null;
            }
        }

        /// <summary>
        /// Asymptotic complexity: O(1)     - worst case
        /// See <see cref="IIterator{T}.Item"/> for more details
        /// </summary>
        public TKey Item
        {
            get
            {
                if (IsEnd)
                {
                    throw new Exception("The enumerator is at the end of the Treap.");
                }
                return node.val;
            }
        }

        /// <summary>
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        ///                        O(n)     - total worst case when moving the iterator from beginning to end
        /// where n - size of the treap
        /// See <see cref="IIterator{T}.MoveNext"/> for more details
        /// </summary>
        public bool MoveNext()
        {
            if (node == null)
            {
                return false;
            }

            if (node.right != null)
            {
                node = node.right.Min;
                return true;
            }
            while (node != null)
            {
                TreapNode<TKey> q = node;
                node = node.parent;
                if (node != null && node.left == q)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        ///                        O(n)     - total worst case when moving the iterator from end to beginning
        /// where n - size of the treap
        /// See <see cref="IIterator{T}.MovePrevious"/> for more details
        /// </summary>
        public bool MovePrevious()
        {
            if (node == null)
            {
                node = _treap.root;
                if (node == null)
                {
                    return false;
                }
                node = node.Max;
                return true;
            }

            if (node.left != null)
            {
                node = node.left.Max;
                return true;
            }
            while (node != null)
            {
                TreapNode<TKey> q = node;
                node = node.parent;
                if (node != null && node.right == q)
                {
                    return true;
                }
            }

            node = _treap.root.Min;
            return false;
        }

        /// <summary>
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        ///                        O(n)     - total worst case when using it on all elements once
        /// where n - size of the treap
        /// See <see cref="IIterator{T}.Index"/> for more details
        /// </summary>
        public int Index
        {
            get
            {
                if (_treap == null)
                {
                    throw new Exception("The Treap is non-existent.");
                }

                if (node == null)
                {
                    return _treap.Size;
                }

                int ans = TreapNode<TKey>.GetSize(node.left);
                TreapNode<TKey> v = node;
                while (v != null)
                {
                    TreapNode<TKey> u = v;
                    v = v.parent;
                    if (v != null && v.right == u)
                    {
                        ans += 1 + TreapNode<TKey>.GetSize(v.left);
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
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap
        /// See <see cref="IIterator{T}.Advance"/> for more details
        /// </summary>
        public TreapIterator<TKey> Advance(int delta)
        {
            return _treap.GetIterator(Index + delta);
        }

        IIterator<TKey> IIterator<TKey>.Advance(int delta)
        {
            return Advance(delta);
        }

        /// <summary>
        /// A user-friendly version of <see cref="Advance"/>
        /// </summary>
        /// <param name="treapIterator">The iterator.</param>
        /// <param name="delta">Number of steps which to move.</param>
        /// <returns></returns>
        public static TreapIterator<TKey> operator +(TreapIterator<TKey> treapIterator, int delta)
        {
            return treapIterator.Advance(delta);
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
        /// Left son, null means there isn't one
        /// </summary>
        public TreapNode<TKey> left { get; private set; }
        /// <summary>
        /// Right son, null means there isn't one
        /// </summary>
        public TreapNode<TKey> right { get; private set; }
        /// <summary>
        /// Parent node, null means the node is the root
        /// </summary>
        public TreapNode<TKey> parent { get; private set; }
        /// <summary>
        /// The value this node stores.
        /// It is used as the key as well.
        /// </summary>
        public TKey val { get; private set; }
        /// <summary>
        /// Priority of the node.
        /// Lower priorities go higher in the treap, closer to the root.
        /// Typically it's set to a random value.
        /// </summary>
        public int prior { get; private set; }
        /// <summary>
        /// Size of the subtreap this node roots.
        /// </summary>
        public int sz { get; private set; }
        /// <summary>
        /// The treap to which the node belongs
        /// </summary>
        public Treap<TKey> _parent { get; private set; }

        /// <summary>
        /// Constructs a separate node with given value (i.e. key) and priority.
        /// Left, right, parent are nulls.
        /// Size is correspondingly 1.
        /// </summary>
        /// <param name="treap">The treap to which this node belongs</param>
        /// <param name="x">The value</param>
        /// <param name="priority">The priority</param>
        public TreapNode(Treap<TKey> treap, TKey x, int priority)
        {
            val = x;
            prior = priority;
            parent = left = right = null;
            sz = 1;
            _parent = treap;
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

        /// <summary>
        /// A way to get the size of a node even if it's null
        /// </summary>
        /// <param name="node">The node in question</param>
        /// <returns>Its size</returns>
        public static int GetSize(TreapNode<TKey> node)
        {
            return node == null ? 0 : node.sz;
        }

        /// <summary>
        /// Updates the internal values of a node, the ones that depend on the subtree
        /// Assumes that choldren already have proper values
        /// </summary>
        private void update()
        {
            sz = 1;
            parent = null;
            if (left != null)
            {
                left.parent = this;
                sz += left.sz;
            }
            if (right != null)
            {
                right.parent = this;
                sz += right.sz;
            }
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
        public static void Split<T>(TreapNode<TKey> t, out TreapNode<TKey> l, out TreapNode<TKey> r, T key, Func<TKey, T> f, Comparer<T> cmp, bool less)
        {
            if (t == null)
            {
                l = r = null;
                return;
            }

            int res = cmp.Compare(f(t.val), key);
            if (res < 0 || (res == 0 && !less))
            {
                TreapNode<TKey> tmp;
                Split(t.right, out tmp, out r, key, f, cmp, less);
                t.right = tmp;
                l = t;
            }
            else
            {
                TreapNode<TKey> tmp;
                Split(t.left, out l, out tmp, key, f, cmp, less);
                t.left = tmp;
                r = t;
            }
            t.update();
        }

        /// <summary>
        /// The simplified version of the split, when the comparing type is the same as the original one and the comparator does not change either.
        /// For more information look at <see cref="Split{T}(TreapNode{TKey}, out TreapNode{TKey}, out TreapNode{TKey}, T, Func{TKey, T}, Comparer{T}, bool)"/>
        /// </summary>
        public static void Split(TreapNode<TKey> t, out TreapNode<TKey> l, out TreapNode<TKey> r, TKey key, bool less = true)
        {
            Split(t, out l, out r, key, p => p, t?._parent._cmp, less);
        }

        /// <summary>
        /// Split the treap into values that compare less, equal, more to key via cmp after being transformed through f.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the resulting treap
        /// </summary>
        /// <typeparam name="T">The type where the comparisons take place.</typeparam>
        /// <param name="t">The treap root to split.</param>
        /// <param name="l">The values that compare less.</param>
        /// <param name="m">The values that compare equal.</param>
        /// <param name="r">The values that compare more.</param>
        /// <param name="key">The value to which to compare.</param>
        /// <param name="f">The function by which to transform.</param>
        /// <param name="cmp">The comparator to use.</param>
        public static void SplitEqual<T>(TreapNode<TKey> t, out TreapNode<TKey> l, out TreapNode<TKey> m, out TreapNode<TKey> r, T key, Func<TKey, T> f, Comparer<T> cmp)
        {
            Split(t, out l, out m, key, f, cmp, true);
            Split(m, out m, out r, key, f, cmp, false);
        }

        /// <summary>
        /// The simplified version of <see cref="SplitEqual{T}(TreapNode{TKey}, out TreapNode{TKey}, out TreapNode{TKey}, out TreapNode{TKey}, T, Func{TKey, T}, Comparer{T})"/>, where the comparer is the one used in the Treap and the transforming function is identity.
        /// </summary>
        public static void SplitEqual(TreapNode<TKey> t, out TreapNode<TKey> l, out TreapNode<TKey> m, out TreapNode<TKey> r, TKey key)
        {
            SplitEqual(t, out l, out m, out r, key, p => p, t?._parent._cmp);
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
        public static void Merge(out TreapNode<TKey> t, TreapNode<TKey> l, TreapNode<TKey> r)
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
                TreapNode<TKey> tmp;
                Merge(out tmp, l.right, r);
                l.right = tmp;
                t = l;
            }
            else
            {
                TreapNode<TKey> tmp;
                Merge(out tmp, l, r.left);
                r.left = tmp;
                t = r;
            }
            t.update();
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
        public static void splitBySize(TreapNode<TKey> t, out TreapNode<TKey> l, out TreapNode<TKey> r, int k)
        {
            if (t == null)
            {
                l = r = null;
                return;
            }

            if (k <= GetSize(t.left))
            {
                TreapNode<TKey> tmp;
                splitBySize(t.left, out l, out tmp, k);
                t.left = tmp;
                r = t;
            }
            else
            {
                TreapNode<TKey> tmp;
                splitBySize(t.right, out tmp, out r, k - GetSize(t.left) - 1);
                t.right = tmp;
                l = t;
            }
            t.update();
        }

        /// <summary>
        /// Deletes the specific node by merging its subtrees.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap t
        /// </summary>
        /// <param name="node">The node to delete</param>
        public static void DeleteRoot(ref TreapNode<TKey> node)
        {
            var p = node.parent;
            bool isLeft = p != null && p.left == node;
            TreapNode<TKey>.Merge(out node, node.left, node.right);
            if (p != null)
            {
                if (isLeft)
                {
                    p.left = node;
                }
                else
                {
                    p.right = node;
                }
            }
            while (p != null)
            {
                p.update();
                p = p.parent;
            }
        }
    }

    /// <summary>
    /// The class that represent a treap.
    /// Supports inserting, erasing values, computing lower and upper bound.
    /// Stores the values in the increasing order.
    /// </summary>
    /// <typeparam name="TKey">The type of the values inside the treap.</typeparam>
    public class Treap<TKey> : IIterable<TKey>
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
        public Comparer<TKey> _cmp { get; private set; }
        /// <summary>
        /// The root node.
        /// </summary>
        public TreapNode<TKey> root { get; private set; }

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
        public TKey this[int index] { get { return GetIterator(index).Item; } }

        /// <summary>
        /// Return an iterator that points to the beginning of the Treap.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap
        /// </summary>
        public TreapIterator<TKey> Begin { get { return root == null ? End : new TreapIterator<TKey>(this, root.Min); } }
        IIterator<TKey> IIterable<TKey>.Begin { get { return Begin; } }
        /// <summary>
        /// Returns an iterator that points to 'end'.
        /// Asymptotic complexity: O(1)     - worst case
        /// where n - size of the treap
        /// </summary>
        public TreapIterator<TKey> End { get { return new TreapIterator<TKey>(this, null); } }
        IIterator<TKey> IIterable<TKey>.End { get { return End; } }

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
        /// Gets an iterator that points to the index's smallest value.
        /// Throws an exception if the supplied index is less than 0 or is supposed to point to after 'end'.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap
        /// </summary>
        /// <param name="index">The index to which the iterator should point.</param>
        /// <returns></returns>
        public TreapIterator<TKey> GetIterator(int index)
        {
            if (index < 0 || Size < index)
            {
                throw new Exception("The requested index is out of bounds.");
            }

            TreapNode<TKey> l, r;
            TreapNode<TKey>.splitBySize(root, out l, out r, index);
            var res = new TreapIterator<TKey>(this, r?.Min);
            TreapNode<TKey> tmp;
            TreapNode<TKey>.Merge(out tmp, l, r);
            root = tmp;
            return res;
        }
        IIterator<TKey> IIterable<TKey>.GetIterator(int index)
        {
            return GetIterator(index);
        }

        /// <summary>
        /// Deletes a specific node from the treap, identified by the iterator.
        /// Does nothing if the iterator is `end`
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap
        /// </summary>
        public void Erase(TreapIterator<TKey> iterator)
        {
            if (this != iterator._treap)
            {
                throw new Exception("The iterator is not of this Treap.");
            }

            if (iterator.IsEnd)
            {
                return;
            }

            var node = iterator.node;
            bool isRoot = node == root;
            TreapNode<TKey>.DeleteRoot(ref node);
            if (isRoot)
            {
                root = node;
            }
        }
        void IIterable<TKey>.Erase(IIterator<TKey> iterator)
        {
            var properIterator = iterator as TreapIterator<TKey>;
            if (properIterator == null)
            {
                throw new Exception("The iterator is not valid.");
            }
            Erase(properIterator);
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
            TreapNode<TKey>.Split(root, out l, out r, x);
            TreapNode<TKey>.Merge(out l, l, new TreapNode<TKey>(this, x, nextPriority()));
            TreapNode<TKey> tmp;
            TreapNode<TKey>.Merge(out tmp, l, r);
            root = tmp;
        }

        /// <summary>
        /// Returns an iterator that points to the leftmost item that compares equal to the specified value.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap
        /// </summary>
        /// <typeparam name="T">The type in which comparisons take place.</typeparam>
        /// <param name="x">The value to compare to.</param>
        /// <param name="f">The function to transform the existing values with.</param>
        /// <param name="cmp">The comparator to use.</param>
        /// <returns></returns>
        public TreapIterator<TKey> Find<T>(T x, Func<TKey, T> f, Comparer<T> cmp = null)
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
            TreapNode<TKey> t1, t2, t3;
            TreapNode<TKey>.SplitEqual<T>(root,  out t1, out t2, out t3, x, f, cmp);
            var ans = new TreapIterator<TKey>(this, t2 == null ? t2 : t2.Min);
            TreapNode<TKey>.Merge(out t2, t2, t3);
            TreapNode<TKey>.Merge(out t1, t1, t2);
            root = t1;
            return ans;
        }
        IIterator<TKey> IIterable<TKey>.Find<TCompare>(TCompare x, Func<TKey, TCompare> f, Comparer<TCompare> cmp)
        {
            return Find(x, f, cmp);
        }

        /// <summary>
        /// The simplified version of <see cref="Find{T}(T, Func{TKey, T}, Comparer{T})"/> that assumes that comparator is the same one as in the Treap and the transformation is identity.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public TreapIterator<TKey> Find(TKey x)
        {
            return Find(x, p => p, _cmp);
        }

        /// <summary>
        /// Erases a value from the treap.
        /// Only deletes one instance if there are multiple.
        /// (For the function that deletes all see <see cref="EraseAll{T}(T, Func{TKey, T}, Comparer{T})"/>.)
        /// Does nothing if there are none.
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap
        /// </summary>
        /// <param name="x">The value to delete.</param>
        /// <param name="f">The transformation function.</param>
        /// <param name="cmp">The comparer to use.</param>
        public void Erase<T>(T x, Func<TKey, T> f, Comparer<T> cmp = null)
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
            Erase(Find(x, f, cmp));
        }

        /// <summary>
        /// The simplified version of <see cref="Erase{T}(T, Func{TKey, T}, Comparer{T})"/> that assumes that comparator is the same as in the Treao and the function is identity.
        /// </summary>
        /// <param name="x"></param>
        public void Erase(TKey x)
        {
            Erase(x, p => p, _cmp);
        }

        /// <summary>
        /// Erases values from the treap.
        /// Only deletes all instances if there are multiple.
        /// (For the function that deletes only one instance see <see cref="Erase{T}(T, Func{TKey, T}, Comparer{T})"/>.)
        /// Asymptotic complexity: O(log n) - average
        ///                        O(n)     - worst case
        /// where n - size of the treap
        /// </summary>
        /// <param name="x">The value to delete.</param>
        /// <param name="f">The transformation function.</param>
        /// <param name="cmp">The comparer to use.</param>
        public void EraseAll<T>(T x, Func<TKey, T> f, Comparer<T> cmp = null)
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
            TreapNode<TKey> t1, t2, t3;
            TreapNode<TKey>.SplitEqual(root, out t1, out t2, out t3, x, f, cmp);
            TreapNode<TKey>.Merge(out t1, t1, t3);
            root = t1;
        }

        /// <summary>
        /// The simplified version of <see cref="EraseAll{T}(T, Func{TKey, T}, Comparer{T})"/> that assumes that comparator is the same as in the Treao and the function is identity.
        /// </summary>
        /// <param name="x"></param>
        public void EraseAll(TKey x)
        {
            EraseAll(x, p => p, _cmp);
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
            TreapNode<TKey> t1, t2;
            TreapNode<TKey>.Split(root, out t1, out t2, x, f, cmp, lower);
            if (t2 == null)
            {
                return new TreapIterator<TKey>(this, null);
            }
            var ans = new TreapIterator<TKey>(this, t2.Min);
            TreapNode<TKey>.Merge(out t1, t1, t2);
            root = t1;
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
        IIterator<TKey> IIterable<TKey>.LowerBound<TCompare>(TCompare x, Func<TKey, TCompare> f, Comparer<TCompare> cmp)
        {
            return LowerBound(x, f, cmp);
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
        IIterator<TKey> IIterable<TKey>.UpperBound<TCompare>(TCompare x, Func<TKey, TCompare> f, Comparer<TCompare> cmp)
        {
            return UpperBound(x, f, cmp);
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
