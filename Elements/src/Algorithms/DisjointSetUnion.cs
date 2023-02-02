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
            _numVertices = n;
            _numEdges = 0;
            _parent = new int[n];
            _size = new int[n];

            Reset();
        }

        // empties the dsu
        public void Reset()
        {
            for (var i = 0; i < _numVertices; ++i)
            {
                _parent[i] = i;
                _size[i] = 1;
            }
        }

        // returns the root of the vertice's component, relabeling the closest parent links along the way
        public int GetParent(int v)
        {
            return v == _parent[v] ? v : _parent[v] = GetParent(_parent[v]);
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
            ++_numEdges;

            if (_size[u] < _size[v])
            {
                (u, v) = (v, u);
            }
            _parent[v] = u;
            _size[u] += _size[v];

            return true;
        }

        // returns the size of the component, in which the vertex lies
        public int ComponentSize(int u)
        {
            return _size[GetParent(u)];
        }

        // returns the number of vertices
        public int Size { get { return _numVertices; } }
        // returns the number of components
        public int NumComponents  {  get { return _numVertices - _numEdges; } }

        private int _numVertices; // number of vertices
        private int _numEdges; // number of edges that form a spanning tree
        private int[] _parent; // the top-most certain parent of the vertex 
        private int[] _size; // size of the vertex's subcomponent
    }
}
