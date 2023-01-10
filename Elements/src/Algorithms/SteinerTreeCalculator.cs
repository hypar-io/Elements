using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Algorithms
{
    class SteinerTreeCalculator
    {
        public SteinerTreeCalculator((int, double)[][] graph)
        {
            n = graph.Length;
            g = new Dictionary<int, double>[n];
            for (var i = 0; i < n; ++i)
                foreach (var (k, v) in graph[i])
                    g[i][k] = v;
            dsu = new DisjointSetUnion(n);
        }

        public SteinerTreeCalculator(int size)
        {
            n = size;
            g = new Dictionary<int, double>[n];
            dsu = new DisjointSetUnion(n);
        }

        public void AddEdge(int u, int v, double l)
        {
            g[u][v] = g[v][u] = l;
        }

        public void RemoveEdge(int u, int v)
        {
            g[u].Remove(v);
            g[v].Remove(u);
        }

        public void Resize(int size)
        {
            Array.Resize(ref g, size);
            dsu.Resize(size);
        }

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

        public void GetTree(int[] vertices, out (int, int, double)[] edges)
        {
            Algorithms.PriorityQueue<double, (int, int)> q = new PriorityQueue<double, (int, int)>();
            int[] used = new int[n];
            var gg = new Dictionary<int, double>[n];
            var e = new List<(int, int, double)>();
            dsu.Reset();
            int cnt = vertices.Length;

            foreach (var v in vertices)
            {
                used[v] = 1;
                foreach (var ed in g[v]) q.Insert(ed.Value, (v, ed.Key));
            }

            while (cnt > 1 && !q.Empty)
            {
                var tp = q.Pop();
                int u = tp.Item2.Item1, v = tp.Item2.Item2;
                double l = tp.Item1;
                if (!dsu.AddEdge(u, v)) continue;

                --cnt;
                gg[u][v] = gg[v][u] = l;
                if (used[v] > 0) continue;
                used[v] = 1;
                foreach (var ed in g[v]) q.Insert(ed.Value, (v, ed.Key));
            }

            HashSet<int> hs = new HashSet<int>(vertices);
            foreach (var v in vertices)
            {
                if (used[v] == 2) continue;
                used[v] = 2;
                dfs_trim(v, -1, ref gg, ref hs, ref used, ref e);
            }

            edges = e.ToArray();
        }

        public int Size { get { return n; } }

        private int n;
        private Dictionary<int, double>[] g;
        private DisjointSetUnion dsu;
    }
}
