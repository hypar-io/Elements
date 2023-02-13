using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace Elements.Algorithms
{
    public class TreapIterator<TKey>
    {
        private TreapNode<TKey> t;
        private Treap<TKey> _treap;

        public TreapIterator(Treap<TKey> treap)
        {
            _treap = treap;
            if (treap == null) return;
            t = treap.Root;
            if (t == null) return;
            t = t.Min;
        }

        public TreapIterator(Treap<TKey> treap, TreapNode<TKey> node)
        {
            _treap = treap;
            if (treap == null) return;
            t = node;
        }

        public bool IsEnd
        {
            get
            {
                if (_treap == null) throw new Exception("The Treap is non-existent");
                return t == null;
            }
        }

        public TKey Get
        {
            get
            {
                if (_treap == null) throw new Exception("The Treap is non-existent");
                if (t == null) throw new Exception("The enumerator is at the end of the Treap");
                return t.val;
            }
        }

        public bool MoveNext()
        {
            if (_treap == null) throw new Exception("The Treap is non-existent");
            if (t == null) return false;

            if (t.right != null)
            {
                t = t.right.Min;
                return true;
            }
            while (t != null)
            {
                TreapNode<TKey> q = t;
                t = t.parent;
                if (t != null && t.left == q) return true;
            }

            return false;
        }

        public bool MovePrevious()
        {
            if (_treap == null) throw new Exception("The Treap is non-existent");
            if (t == null)
            {
                t = _treap.Root;
                if (t == null) return false;
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
                if (t != null && t.right == q) return true;
            }

            t = _treap.Root.Min;
            return false;
        }
    }

    public class TreapNode<TKey>
    {
        public TreapNode<TKey> left, right, parent;
        public TKey val;
        public int prior;
        public int sz;

        public TreapNode() { }
        public TreapNode(TKey x, int priority)
        {
            val = x;
            prior = priority;
            parent = left = right = null;
            sz = 1;
        }

        public TreapNode<TKey> Min 
        { 
            get 
            {
                TreapNode<TKey> t = this;
                while (t.left != null) t = t.left;
                return t;
            } 
        }

        public TreapNode<TKey> Max
        {
            get
            {
                TreapNode<TKey> t = this;
                while (t.right != null) t = t.right;
                return t;
            }
        }
    }

    public class Treap<TKey>
    {
        static Random priorityGenerator = new Random(228);
        static int nextPriority() { return priorityGenerator.Next(); } 

        private Comparer<TKey> _cmp;
        private TreapNode<TKey> root;

        public TreapNode<TKey> Root { get { return root; } }

        private void ToListDfs(TreapNode<TKey> t, List<TKey> lst)
        {
            if (t == null) return;
            ToListDfs(t.left, lst);
            lst.Add(t.val);
            ToListDfs(t.right, lst);
        }
        public List<TKey> ToList()
        {
            var ans = new List<TKey>();
            ToListDfs(root, ans);
            return ans;
        }

        public Treap(Comparer<TKey> cmp = null)
        {
            if (cmp == null)
            {
                if (typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey))) cmp = Comparer<TKey>.Default;
                else throw new Exception("No comparer was supplied and the underlying type is not implicitly comparable");
            }
            _cmp = cmp;
            root = null;
        }

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

        private void Split(TreapNode<TKey> t, out TreapNode<TKey> l, out TreapNode<TKey> r, TKey key, bool less = true)
        {
            Split(t, out l, out r, key, p => p, _cmp, less);
        }

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

        private int getSize(TreapNode<TKey> t)
        {
            return t == null ? 0 : t.sz;
        }

        private void update(TreapNode<TKey> t)
        {
            if (t == null) return;

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

        public void Insert(TKey x)
        {
            TreapNode<TKey> l, r;
            Split(root, out l, out r, x);
            Merge(out l, l, new TreapNode<TKey>(x, nextPriority()));
            Merge(out root, l, r);
        }

        public void Erase(TKey x)
        {
            TreapNode<TKey> t1, t2;
            Split(root, out root, out t1, x);
            Split(t1, out t1, out t2, x, false);
            if (t1 != null) Merge(out t1, t1.left, t1.right);
            Merge(out t1, t1, t2);
            Merge(out root, root, t1);
        }

        private TreapIterator<TKey> LowerUpperBound<T> (T x, Func<TKey, T> f, Comparer<T> cmp, bool lower) 
        {
            if (root == null) return new TreapIterator<TKey>(this, null);
            TreapNode<TKey> t1;
            Split(root, out root, out t1, x, f, cmp, lower);
            if (t1 == null) return new TreapIterator<TKey>(this, null);
            var ans = new TreapIterator<TKey>(this, t1.Min);
            Merge(out root, root, t1);
            return ans;
        }

        private TreapIterator<TKey> LowerUpperBound(TKey x, bool lower)
        {
            return LowerUpperBound(x, p => p, _cmp, lower);
        }

        public TreapIterator<TKey> LowerBound<T>(T x, Func<TKey, T> f, Comparer<T> cmp = null)
        {
            if (cmp == null)
            {
                if (typeof(IComparable<T>).IsAssignableFrom(typeof(T))) cmp = Comparer<T>.Default;
                else throw new Exception("No comparer was supplied and the supplied type is not implicitly comparable");
            }
            return LowerUpperBound(x, f, cmp, true);
        }

        public TreapIterator<TKey> UpperBound<T>(T x, Func<TKey, T> f, Comparer<T> cmp = null)
        {
            if (cmp == null)
            {
                if (typeof(IComparable<T>).IsAssignableFrom(typeof(T))) cmp = Comparer<T>.Default;
                else throw new Exception("No comparer was supplied and the supplied type is not implicitly comparable");
            }
            return LowerUpperBound(x, f, cmp, false);
        }

        public TreapIterator<TKey> LowerBound(TKey x)
        {
            return LowerUpperBound(x, true);
        }

        public TreapIterator<TKey> UpperBound(TKey x)
        {
            return LowerUpperBound(x, false);
        }
    }
}
