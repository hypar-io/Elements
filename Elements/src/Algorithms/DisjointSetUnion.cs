using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Algorithms
{
    class DisjointSetUnion
    {
        public DisjointSetUnion(int size)
        {
            n = size;
            e = 0;
            p = new int[n];
            sz = new int[n];

            Reset();
        }

        public void Reset()
        {
            for (var i = 0; i < n; ++i)
            {
                p[i] = i;
                sz[i] = 1;
            }
        }

        public void Resize(int size)
        {
            n = size;
            Reset();
        }

        public int GetParent(int v)
        {
            return v == p[v] ? v : p[v] = GetParent(p[v]);
        }

        public bool AddEdge(int u, int v)
        {
            u = GetParent(u);
            v = GetParent(v);

            if (u == v)
                return false;
            ++e;

            if (sz[u] < sz[v]) (u, v) = (v, u);
            p[v] = u;
            sz[u] += sz[v];

            return true;
        }

        public int ComponentSize(int u)
        {
            return sz[GetParent(u)];
        }

        public int Size { get { return n; } }
        public int NumComponents  {  get { return n - e; } }

        private int n;
        private int e;
        private int[] p;
        private int[] sz;
    }
}
