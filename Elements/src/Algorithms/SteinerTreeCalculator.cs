using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Algorithms
{
    class SteinerTreeCalculator
    {
        // initializes from a graph - an array of pairs (v, len) for each vertex u - vertices with which u is connected, len is the length of the edge
        // graph must be undirected!
        public SteinerTreeCalculator((int, double)[][] graph)
        {
            n = graph.Length;
            g = new Dictionary<int, double>[n];
            for (var i = 0; i < n; ++i)
            {
                g[i] = new Dictionary<int, double>();
                foreach (var (k, v) in graph[i])
                    g[i][k] = v;
            }
            dsu = new DisjointSetUnion(n);
        }

        // initializes an empty graph with n vertices
        public SteinerTreeCalculator(int size)
        {
            n = size;
            g = new Dictionary<int, double>[n];
            for (var i = 0; i < n; ++i) g[i] = new Dictionary<int, double>();
            dsu = new DisjointSetUnion(n);
        }

        // adds an undirected weighed edge to the graph
        public void AddEdge(int u, int v, double l)
        {
            g[u][v] = g[v][u] = l;
        }

        // removes an undirected edge from the graph
        public void RemoveEdge(int u, int v)
        {
            g[u].Remove(v);
            g[v].Remove(u);
        }

        // a helper function that removes hanging unnecessary vertices from the built steiner tree (based on dfs)
        // it's always called from a necessary vertex
        // asymptotic complexity: O(k), where k is the number of vertices in the subtree
        private void dfs_trim(int v, int p, ref Dictionary <int, double>[] g, ref HashSet<int> hs, ref int[] used, ref List<(int, int, double)> e)
        {
            used[v] = 2;
            foreach (var ed in g[v])
            {
                if (used[ed.Key] == 2) continue;
                dfs_trim(ed.Key, v, ref g, ref hs, ref used, ref e);
            }
            if (g[v].Count == 1 && !hs.Contains(v)) g[p].Remove(v);
            else if (p != -1) e.Add((p, v, g[p][v]));
        }

        public (int, int, double)[] GetTree(int[] vertices)
        {
            // Actually calculates a set of edges in a Steiner tree based in the given set of vertices
            // Algorithm:
            // We start with a set of 1-vertex trees (given vertices)
            // We take the shortes edge originating in our forest, add it to the forest 
            // We do so until we have no edges left or we've ended up with only one tree
            // We trim the tree with our dfs_trim function
            // Details:
            // The heap contains a list of edges originating from our current forest
            // used describes which vertices are currently in the forest
            // gg describes the current subtree
            // dsu is used to check that we do not create cycles in the forest
            Algorithms.BinaryHeap<double, (int, int)> q = new BinaryHeap<double, (int, int)>();
            int[] used = new int[n];
            var gg = new Dictionary<int, double>[n];
            for (var i = 0; i < n; ++i) gg[i] = new Dictionary<int, double>();
            var e = new List<(int, int, double)>();
            dsu.Reset();
            int cnt = vertices.Length;

            foreach (var v in vertices)
            {
                used[v] = 1;
                foreach (var ed in g[v]) q.Insert(-ed.Value, (v, ed.Key));
            }

            while (cnt > 1 && !q.Empty)
            {
                var tp = q.Extract();
                int u = tp.Item2.Item1, v = tp.Item2.Item2;
                double l = -tp.Item1;
                if (!dsu.AddEdge(u, v)) continue;

                gg[u][v] = gg[v][u] = l;
                if (used[v] > 0) --cnt;
                else
                {
                    used[v] = 1;
                    foreach (var ed in g[v]) q.Insert(-ed.Value, (v, ed.Key));
                }
            }

            HashSet<int> hs = new HashSet<int>(vertices);
            foreach (var v in vertices)
            {
                if (used[v] == 2) continue;
                used[v] = 2;
                dfs_trim(v, -1, ref gg, ref hs, ref used, ref e);
            }

            return e.ToArray();
        }

        // returns the number of vertices in the graph
        public int Size { get { return n; } }

        private int n; // number of vertices
        private Dictionary<int, double>[] g; // compressed adjancency matrix
        private DisjointSetUnion dsu; // helper dsu
    }
}
