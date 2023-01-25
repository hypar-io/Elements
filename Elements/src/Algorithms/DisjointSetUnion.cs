using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Algorithms
{
    class DisjointSetUnion
    {
        // constructs an empty dsu that acts on n vertices
        public DisjointSetUnion(int n)
        {
            numVertices = n;
            numEdges = 0;
            parent = new int[n];
            size = new int[n];

            Reset();
        }

        // empties the dsu
        public void Reset()
        {
            for (var i = 0; i < numVertices; ++i)
            {
                parent[i] = i;
                size[i] = 1;
            }
        }

        // returns the root of the vertice's component, relabeling the closest parent links along the way
        public int GetParent(int v)
        {
            return v == parent[v] ? v : parent[v] = GetParent(p[v]);
        }

        // inserts an edge into the structure, connects the smaller tree to the bigger
        // returns whether the edge connects two separate components
        public bool AddEdge(int u, int v)
        {
            u = GetParent(u);
            v = GetParent(v);

            if (u == v)
            {
                return false;
            }
            ++numEdges;

            if (size[u] < size[v])
            {
                (u, v) = (v, u);
            }
            parent[v] = u;
            size[u] += size[v];

            return true;
        }

        // returns the size of the component, in which the vertex lies
        public int ComponentSize(int u)
        {
            return size[GetParent(u)];
        }

        // returns the number of vertices
        public int Size { get { return numVertices; } }
        // returns the number of components
        public int NumComponents  {  get { return numVertices - numEdges; } }

        private int numVertices; // number of vertices
        private int numEdges; // number of edges that form a spanning tree
        private int[] parent; // the top-most certain parent of the vertex 
        private int[] size; // size of the vertex's subcomponent
    }
}
