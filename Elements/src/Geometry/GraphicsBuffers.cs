using System;
using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// A container for graphics data.
    /// The buffers used in this class align with webgl requirements.
    /// </summary>
    public class GraphicsBuffers : IGraphicsBuffers
    {
        private const int _preallocationVertexCount = 100;

        /// <summary>
        /// The number of vertices represented by the buffer.
        /// </summary>
        public int VertexCount
        {
            get { return this.Vertices.Count / sizeof(float) / 3; }
        }

        /// <summary>
        /// The number of facets represeted by the buffer.
        /// </summary>
        public int FacetCount
        {
            get { return this.Indices.Count / sizeof(ushort) / 3; }
        }

        /// <summary>
        /// A collection of vertex positions stored as sequential bytes.
        /// </summary>
        public List<byte> Vertices { get; private set; }

        /// <summary>
        /// A collection of indices stored as sequential bytes.
        /// </summary>
        public List<byte> Indices { get; private set; }

        /// <summary>
        /// A collection of sequential normal values stored as sequential bytes.
        /// </summary>
        public List<byte> Normals { get; private set; }

        /// <summary>
        /// A collection of sequential color values stored as sequential bytes.
        /// </summary>
        public List<byte> Colors { get; private set; }

        /// <summary>
        /// A collection of UV values stored as sequential bytes.
        /// </summary>
        public List<byte> UVs { get; private set; }

        /// <summary>
        /// The maximum of the axis-aligned bounding box of the data as [x,y,z].
        /// </summary>
        public double[] VMax { get; private set; }

        /// <summary>
        /// The minimum of the axis-aligned bounding box of the data as [x,y,z].
        /// </summary>
        public double[] VMin { get; private set; }

        /// <summary>
        /// The minimum normal of the data as [x,y,z].
        /// </summary>
        public double[] NMin { get; private set; }

        /// <summary>
        /// The maximum normal of the data as [x,y,z].
        /// </summary>
        public double[] NMax { get; private set; }

        /// <summary>
        /// The minimum color value as [r,g,b].
        /// </summary>
        public double[] CMin { get; private set; }

        /// <summary>
        /// The maximum color value as [r,g,b].
        /// </summary>
        public double[] CMax { get; private set; }

        /// <summary>
        /// The maximum index value.
        /// </summary>
        public ushort IMax { get; internal set; } = ushort.MinValue;

        /// <summary>
        /// The minimum index value.
        /// </summary>
        public ushort IMin { get; internal set; } = ushort.MaxValue;

        /// <summary>
        /// The maximum UV value as [u,v].
        /// </summary>
        public double[] UVMin { get; private set; }

        /// <summary>
        /// The maximum UV value as [u,v].
        /// </summary>
        public double[] UVMax { get; private set; }

        /// <summary>
        /// Construct an empty graphics buffers object.
        /// </summary>
        public GraphicsBuffers()
        {
            Initialize();
        }

        /// <summary>
        /// Add a vertex to the graphics buffers.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="normal">The normal of the vertex.</param>
        /// <param name="uv">The UV of the vertex.</param>
        /// <param name="color">The vertex color.</param>
        public void AddVertex(Vector3 position, Vector3 normal, UV uv, Color? color = null)
        {
            this.AddVertex(position.X, position.Y, position.Z, normal.X, normal.Y, normal.Z, uv.U, uv.V, color);
        }

        /// <summary>
        /// Add a vertex to the graphics buffers.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="nx"></param>
        /// <param name="ny"></param>
        /// <param name="nz"></param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <param name="color"></param>
        public void AddVertex(double x, double y, double z, double nx, double ny, double nz, double u, double v, Color? color)
        {
            this.Vertices.AddRange(BitConverter.GetBytes((float)x));
            this.Vertices.AddRange(BitConverter.GetBytes((float)y));
            this.Vertices.AddRange(BitConverter.GetBytes((float)z));

            this.Normals.AddRange(BitConverter.GetBytes((float)nx));
            this.Normals.AddRange(BitConverter.GetBytes((float)ny));
            this.Normals.AddRange(BitConverter.GetBytes((float)nz));

            this.UVs.AddRange(BitConverter.GetBytes((float)u));
            this.UVs.AddRange(BitConverter.GetBytes((float)v));

            this.VMax[0] = Math.Max(this.VMax[0], x);
            this.VMax[1] = Math.Max(this.VMax[1], y);
            this.VMax[2] = Math.Max(this.VMax[2], z);
            this.VMin[0] = Math.Min(this.VMin[0], x);
            this.VMin[1] = Math.Min(this.VMin[1], y);
            this.VMin[2] = Math.Min(this.VMin[2], z);

            this.NMax[0] = Math.Max(this.NMax[0], nx);
            this.NMax[1] = Math.Max(this.NMax[1], ny);
            this.NMax[2] = Math.Max(this.NMax[2], nz);
            this.NMin[0] = Math.Min(this.NMin[0], nx);
            this.NMin[1] = Math.Min(this.NMin[1], ny);
            this.NMin[2] = Math.Min(this.NMin[2], nz);

            this.UVMax[0] = Math.Max(this.UVMax[0], u);
            this.UVMax[1] = Math.Max(this.UVMax[1], v);
            this.UVMin[0] = Math.Min(this.UVMin[0], u);
            this.UVMin[1] = Math.Min(this.UVMin[1], v);

            if (color.HasValue && color.Value != default)
            {
                this.CMax[0] = Math.Max(this.CMax[0], color.Value.Red);
                this.CMax[1] = Math.Max(this.CMax[1], color.Value.Green);
                this.CMax[2] = Math.Max(this.CMax[2], color.Value.Blue);
                this.CMin[0] = Math.Min(this.CMin[0], color.Value.Red);
                this.CMin[1] = Math.Min(this.CMin[1], color.Value.Green);
                this.CMin[2] = Math.Min(this.CMin[2], color.Value.Blue);

                this.Colors.AddRange(BitConverter.GetBytes((float)color.Value.Red));
                this.Colors.AddRange(BitConverter.GetBytes((float)color.Value.Green));
                this.Colors.AddRange(BitConverter.GetBytes((float)color.Value.Blue));
            }
        }

        /// <summary>
        /// Add vertices to the graphics buffers.
        /// </summary>
        public void AddVertices(IList<(Vector3 position, Vector3 normal, UV uv, Color? color)> vertices)
        {
            var vertexStep = sizeof(float) * 3;
            var normalStep = sizeof(float) * 3;
            var uvStep = sizeof(float) * 2;
            var colorStep = sizeof(float) * 3;

            var allPositions = new byte[vertices.Count * vertexStep];
            var allNormals = new byte[vertices.Count * normalStep];
            var allUvs = new byte[vertices.Count * uvStep];
            var allColors = new byte[vertices.Count * colorStep];

            var hasVertexColors = false;

            for (var i = 0; i < vertices.Count; i++)
            {
                var (position, normal, uv, color) = vertices[i];

                Buffer.BlockCopy(BitConverter.GetBytes((float)position.X), 0, allPositions, vertexStep * i, sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes((float)position.Y), 0, allPositions, vertexStep * i + sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes((float)position.Z), 0, allPositions, vertexStep * i + sizeof(float) * 2, sizeof(float));
                this.VMax[0] = Math.Max(this.VMax[0], position.X);
                this.VMax[1] = Math.Max(this.VMax[1], position.Y);
                this.VMax[2] = Math.Max(this.VMax[2], position.Z);
                this.VMin[0] = Math.Min(this.VMin[0], position.X);
                this.VMin[1] = Math.Min(this.VMin[1], position.Y);
                this.VMin[2] = Math.Min(this.VMin[2], position.Z);

                Buffer.BlockCopy(BitConverter.GetBytes((float)normal.X), 0, allNormals, normalStep * i, sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes((float)normal.Y), 0, allNormals, normalStep * i + sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes((float)normal.Z), 0, allNormals, normalStep * i + sizeof(float) * 2, sizeof(float));
                this.NMax[0] = Math.Max(this.NMax[0], normal.X);
                this.NMax[1] = Math.Max(this.NMax[1], normal.Y);
                this.NMax[2] = Math.Max(this.NMax[2], normal.Z);
                this.NMin[0] = Math.Min(this.NMin[0], normal.X);
                this.NMin[1] = Math.Min(this.NMin[1], normal.Y);
                this.NMin[2] = Math.Min(this.NMin[2], normal.Z);

                Buffer.BlockCopy(BitConverter.GetBytes((float)uv.U), 0, allUvs, uvStep * i, sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes((float)uv.V), 0, allUvs, uvStep * i + sizeof(float), sizeof(float));
                this.UVMax[0] = Math.Max(this.UVMax[0], uv.U);
                this.UVMax[1] = Math.Max(this.UVMax[1], uv.V);
                this.UVMin[0] = Math.Min(this.UVMin[0], uv.U);
                this.UVMin[1] = Math.Min(this.UVMin[1], uv.V);

                if (color.HasValue)
                {
                    hasVertexColors = true;
                    Buffer.BlockCopy(BitConverter.GetBytes((float)color.Value.Red), 0, allColors, colorStep * i, sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes((float)color.Value.Green), 0, allColors, colorStep * i + sizeof(float), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes((float)color.Value.Blue), 0, allColors, colorStep * i + sizeof(float) * 2, sizeof(float));
                    this.CMax[0] = Math.Max(this.CMax[0], color.Value.Red);
                    this.CMax[1] = Math.Max(this.CMax[1], color.Value.Green);
                    this.CMax[2] = Math.Max(this.CMax[2], color.Value.Blue);
                    this.CMin[0] = Math.Min(this.CMin[0], color.Value.Red);
                    this.CMin[1] = Math.Min(this.CMin[1], color.Value.Green);
                    this.CMin[2] = Math.Min(this.CMin[2], color.Value.Blue);
                }
            }

            this.Vertices.AddRange(allPositions);
            this.Normals.AddRange(allNormals);
            this.UVs.AddRange(allUvs);
            if (hasVertexColors)
            {
                this.Colors.AddRange(allColors);
            }
        }

        /// <summary>
        /// Add an index to the graphics buffers.
        /// </summary>
        /// <param name="index">The index to add.</param>
        public void AddIndex(ushort index)
        {
            this.Indices.AddRange(BitConverter.GetBytes(index));
            this.IMax = Math.Max(this.IMax, index);
            this.IMin = Math.Min(this.IMin, index);
        }

        /// <summary>
        /// Add indices to the graphics buffers.
        /// </summary>
        /// <param name="indices">The indices to add.</param>
        public void AddIndices(IList<ushort> indices)
        {
            var newRange = new byte[indices.Count * sizeof(ushort)];
            for (var i = 0; i < indices.Count; i++)
            {
                var index = indices[i];
                this.IMax = Math.Max(this.IMax, index);
                this.IMin = Math.Min(this.IMin, index);
                var bytes = BitConverter.GetBytes(index);
                Buffer.BlockCopy(bytes, 0, newRange, i * sizeof(ushort), sizeof(ushort));
            }
            this.Indices.AddRange(newRange);
        }

        internal static int PreallocationSize()
        {
            // Assume a fully vertex-colored mesh of the size required to contain 30k vertices.
            // Postion, Normal, Color, Index, UV
            var floatSize = sizeof(float);
            var intSize = sizeof(int);
            return (floatSize * 3 + floatSize * 3 + floatSize * 4 + intSize + floatSize * 2) * _preallocationVertexCount;
        }

        public void Initialize(int vertexCount = _preallocationVertexCount, int indexCount = _preallocationVertexCount)
        {
            // Initialize everything
            this.Vertices = new List<byte>(_preallocationVertexCount * sizeof(float) * 3);
            this.Normals = new List<byte>(_preallocationVertexCount * sizeof(float) * 3);
            this.Indices = new List<byte>(_preallocationVertexCount * sizeof(int));
            this.UVs = new List<byte>(_preallocationVertexCount * sizeof(float) * 2);
            this.Colors = new List<byte>(_preallocationVertexCount * sizeof(float) * 4);

            this.CMin = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };
            this.CMax = new double[3] { double.MinValue, double.MinValue, double.MinValue };

            this.VMax = new double[3] { double.MinValue, double.MinValue, double.MinValue };
            this.VMin = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };

            this.NMin = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };
            this.NMax = new double[3] { double.MinValue, double.MinValue, double.MinValue };

            this.UVMin = new double[2] { double.MaxValue, double.MaxValue };
            this.UVMax = new double[2] { double.MinValue, double.MinValue };
        }
    }
}