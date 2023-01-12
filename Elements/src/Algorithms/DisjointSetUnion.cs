using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Algorithms
{
    class DisjointSetUnion
    {
        // constructs an empty dsu that acts on n vertices
        public DisjointSetUnion(int size)
        {
            n = size;
            e = 0;
            p = new int[n];
            sz = new int[n];

            Reset();
        }

        // empties the dsu
        public void Reset()
        {
            for (var i = 0; i < n; ++i)
            {
                p[i] = i;
                sz[i] = 1;
            }
        }

        // returns the root of the vertice's component, relabeling the closest parent links along the way
        public int GetParent(int v)
        {
            return v == p[v] ? v : p[v] = GetParent(p[v]);
        }

        // inserts an edge into the structure, connects the smaller tree to the bigger
        // returns whether the edge connects two separate components
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

        // returns the size of the component, in which the vertex lies
        public int ComponentSize(int u)
        {
            return sz[GetParent(u)];
        }

        // returns the number of vertices
        public int Size { get { return n; } }
        // returns the number of components
        public int NumComponents  {  get { return n - e; } }

        private int n; // number of vertices
        private int e; // number of edges that form a spanning tree
        private int[] p; // the top-most certain parent of the vertex 
        private int[] sz; // size of the vertex's subcomponent
    }
}
