using LibTessDotNet.Double;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Elements.Geometry
{
    /// <summary>
    /// An indexed mesh.
    /// </summary>
    public class Mesh
    {
        private List<double> _vertices = new List<double>();
        private List<double> _normals = new List<double>();
        private List<ushort> _indices = new List<ushort>();
        private List<float> _colors = new List<float>();

        private ushort _index_max;
        private ushort _index_min;
        
        private double[] _v_max = new double[3]{double.MinValue, double.MinValue, double.MinValue};
        private double[] _v_min= new double[3]{double.MaxValue, double.MaxValue, double.MaxValue};
        private double[] _n_max = new double[3]{double.MinValue, double.MinValue, double.MinValue};
        private double[] _n_min = new double[3]{double.MaxValue, double.MaxValue, double.MaxValue};

        private float[] _c_min = new float[]{float.MaxValue, float.MaxValue, float.MaxValue};
        private float[] _c_max = new float[]{float.MinValue, float.MinValue, float.MinValue};

        /// <summary>
        /// The maximum vertex.
        /// </summary>
        public double[] VMax
        {
            get{return _v_max;}
        }

        /// <summary>
        /// The minimum vertex.
        /// </summary>
        public double[] VMin
        {
            get{return _v_min;}
        }

        /// <summary>
        /// The maximum normal.
        /// </summary>
        public double[] NMax
        {
            get{return _n_max;}
        }

        /// <summary>
        /// The minimum normal.
        /// </summary>
        public double[] NMin
        {
            get{return _n_min;}
        }

        /// <summary>
        /// The maximum index.
        /// </summary>
        public ushort IMax
        {
            get{return _index_max;}
        }

        /// <summary>
        /// The minimum index.
        /// </summary>
        public ushort IMin
        {
            get{return _index_min;}
        }

        /// <summary>
        /// The minimum color.
        /// </summary>
        public float[] CMin
        {
            get{return _c_min;}
        }

        /// <summary>
        /// The maximum color.
        /// </summary>
        public float[] CMax
        {
            get{return _c_max;}
        }

        /// <summary>
        /// The vertices of the mesh.
        /// </summary>
        public List<double> Vertices
        {
            get{return _vertices;}
        }

        /// <summary>
        /// The normals of the mesh.
        /// </summary>
        public List<double> Normals
        {
            get{return _normals;}
        }
        
        /// <summary>
        /// The indices of the mesh.
        /// </summary>
        public List<ushort> Indices
        {
            get{return _indices;}
        }

        /// <summary>
        /// The colors of the mesh.
        /// </summary>
        public List<float> Colors
        {
            get{return _colors;}
        }

        /// <summary>
        /// Construct an empty mesh.
        /// </summary>
        public Mesh()
        {
            // An empty mesh.
        }

        /// <summary>
        /// Construct a mesh from vertices, normals, and indices.
        /// </summary>
        /// <param name="vertices">An array containing doubles of the form [x1, y1, z1, x2, y2, z2...].</param>
        /// <param name="normals">An array containing doubles of the form [nx1, ny1, nz1, nx2, ny2, nz2...]</param>
        /// <param name="indices">An array containing integers of the form [0, 1, 2, 0, 2, 3...].</param>
        /// <param name="colors">An array containing floats of the form [r,g,b,r,g,b...]</param>
        public Mesh(double[] vertices, double[] normals, ushort[] indices, float[] colors)
        {
            this._vertices.AddRange(vertices);
            this._normals.AddRange(normals);
            this._indices.AddRange(indices);
        }

        /// <summary>
        /// Get a string representation of the mesh.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $@"
Vertices:{_vertices.Count/3}, 
Normals:{_normals.Count/3}, 
Indices:{_indices.Count}, 
VMax:{string.Join(",", _v_max)}, 
VMin:{string.Join(",", _v_min)}, 
NMax:{string.Join(",", _n_max)}, 
NMin:{string.Join(",", _n_min)}, 
IMax:{_index_max}, 
IMin:{_index_min}";
        }

        /// <summary>
        /// Add a triangle to the mesh.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="an"></param>
        /// <param name="bn"></param>
        /// <param name="cn"></param>
        /// <param name="ac"></param>
        /// <param name="bc"></param>
        /// <param name="cc"></param>
        internal void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 an = null, Vector3 bn = null, Vector3 cn = null, Color ac = null, Color bc = null, Color cc = null)
        {
            // Calculate the face normal
            var v1 = b - a;
            var v2 = c - a;
            var n = v1.Cross(v2).Normalized();
            if(Double.IsNaN(n.X) || Double.IsNaN(n.Y) || Double.IsNaN(n.Z))
            {
                Debug.WriteLine("Degenerate triangle found.");
                return;
            }

            an = an == null ? n : an;
            bn = bn == null ? n : bn;
            cn = cn == null ? n : cn;

            AddVertex(a, an, ac);
            AddVertex(b, bn, bc);
            AddVertex(c, cn, cc);
        }

        /// <summary>
        /// Add a vertex to the mesh.
        /// </summary>
        /// <param name="v">The position of the vertex.</param>
        /// <param name="n">The vertex's normal.</param>
        /// <param name="c">The vertex's color.</param>
        /// <returns>The index of the newly created vertex.</returns>
        internal int AddVertex(Vector3 v, Vector3 n, Color c = null)
        {
            var vArr = v.ToArray();
            var nArr = n.ToArray();
            
            this._vertices.AddRange(vArr);
            this._normals.AddRange(nArr);

            _v_max[0] = Math.Max(_v_max[0], vArr[0]);
            _v_max[1] = Math.Max(_v_max[1], vArr[1]);
            _v_max[2] = Math.Max(_v_max[2], vArr[2]);
            _v_min[0] = Math.Min(_v_min[0], vArr[0]);
            _v_min[1] = Math.Min(_v_min[1], vArr[1]);
            _v_min[2] = Math.Min(_v_min[2], vArr[2]);

            _n_max[0] = Math.Max(_n_max[0], nArr[0]);
            _n_max[1] = Math.Max(_n_max[1], nArr[1]);
            _n_max[2] = Math.Max(_n_max[2], nArr[2]);
            _n_min[0] = Math.Min(_n_min[0], nArr[0]);
            _n_min[1] = Math.Min(_n_min[1], nArr[1]);
            _n_min[2] = Math.Min(_n_min[2], nArr[2]);

            if(c != null)
            {
                var cArr = c.ToArray();
                this._colors.AddRange(cArr);
                _c_max[0] = Math.Max(_c_max[0], cArr[0]);
                _c_max[1] = Math.Max(_c_max[1], cArr[1]);
                _c_max[2] = Math.Max(_c_max[2], cArr[2]);
                _c_min[0] = Math.Min(_c_min[0], cArr[0]);
                _c_min[1] = Math.Min(_c_min[1], cArr[1]);
                _c_min[2] = Math.Min(_c_min[2], cArr[2]);
            }

            var start = (ushort)_indices.Count;
            this._indices.Add(start);
            _index_max = Math.Max(_index_max, start);
            _index_min = Math.Min(_index_min, start);

            return (this._vertices.Count/3)-1;
        }

        /// <summary>
        /// Add two triangles to the mesh by splitting a rectangular region in two.
        /// </summary>
        /// <param name="vertices"></param>
        internal void AddQuad(IList<Vector3> vertices)
        {
            var v1 = vertices[1] - vertices[0];
            var v2 = vertices[2] - vertices[0];
            var n1 = v1.Cross(v2).Normalized();
            if(Double.IsNaN(n1.X) || Double.IsNaN(n1.Y) || Double.IsNaN(n1.Z))
            {
                Console.WriteLine("Degenerate triangle found.");
                Console.WriteLine(v1);
                Console.WriteLine(v2);
                return;
            }

            AddVertex(vertices[0], n1);
            AddVertex(vertices[1], n1);
            AddVertex(vertices[2], n1);
            AddVertex(vertices[3], n1);

            var start = (ushort)_indices.Count;
            this._indices.AddRange(new[]{start,(ushort)(start+1),(ushort)(start+2),(ushort)(start),(ushort)(start+2),(ushort)(start+3)});
            _index_max = Math.Max(_index_max, start);
            _index_max = Math.Max(_index_max, (ushort)(start+1));
            _index_max = Math.Max(_index_max, (ushort)(start+2));
            _index_max = Math.Max(_index_max, (ushort)(start+3));
            _index_min = Math.Min(_index_min, start);
            _index_min = Math.Min(_index_min, (ushort)(start+1));
            _index_min = Math.Min(_index_min, (ushort)(start+2));
            _index_min = Math.Min(_index_min, (ushort)(start+3));
        }
    }

    internal static class TessExtensions
    {
        internal static Vector3 ToVector3(this Vec3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
    }
}