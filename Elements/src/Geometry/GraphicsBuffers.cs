using System;
using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// A generic container for graphics data. This is broken out primarily to facilitate
    /// simpler testing of graphics buffers.
    /// </summary>
    internal interface IGraphicsBuffers
    {
        /// <summary>
        /// Add a vertex to the graphics buffers.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="normal">The normal of the vertex.</param>
        /// <param name="uv">The UV of the vertex.</param>
        /// <param name="color">The vertex color.</param>
        void AddVertex(Vector3 position, Vector3 normal, UV uv, Color? color = null);

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
        void AddVertex(double x, double y, double z, double nx, double ny, double nz, double u, double v, Color? color = null);

        /// <summary>
        /// Add an index to the graphics buffers.
        /// </summary>
        /// <param name="index">The index to add.</param>
        void AddIndex(ushort index);
    }

    /// <summary>
    /// A container for graphics data.
    /// The buffers used in this class align with webgl requirements.
    /// </summary>
    public class GraphicsBuffers : IGraphicsBuffers
    {
        /// <summary>
        /// A collection of vertex positions stored as sequential bytes.
        /// </summary>
        public List<byte> Vertices { get; }

        /// <summary>
        /// A collection of indices stored as sequential bytes.
        /// </summary>
        public List<byte> Indices { get; }

        /// <summary>
        /// A collection of sequential normal values stored as sequential bytes.
        /// </summary>
        public List<byte> Normals { get; }

        /// <summary>
        /// A collection of sequential color values stored as sequential bytes.
        /// </summary>
        public List<byte> Colors { get; }

        /// <summary>
        /// A collection of UV values stored as sequential bytes.
        /// </summary>
        public List<byte> UVs { get; }

        /// <summary>
        /// The maximum of the axis-aligned bounding box of the data as [x,y,z].
        /// </summary>
        public double[] VMax { get; }

        /// <summary>
        /// The minimum of the axis-aligned bounding box of the data as [x,y,z].
        /// </summary>
        public double[] VMin { get; }

        /// <summary>
        /// The minimum normal of the data as [x,y,z].
        /// </summary>
        public double[] NMin { get; }

        /// <summary>
        /// The maximum normal of the data as [x,y,z].
        /// </summary>
        public double[] NMax { get; }

        /// <summary>
        /// The minimum color value as [r,g,b].
        /// </summary>
        public double[] CMin { get; }

        /// <summary>
        /// The maximum color value as [r,g,b].
        /// </summary>
        public double[] CMax { get; }

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
        public double[] UVMin { get; }

        /// <summary>
        /// The maximum UV value as [u,v].
        /// </summary>
        public double[] UVMax { get; }

        /// <summary>
        /// Construct an empty graphics buffers object.
        /// </summary>
        public GraphicsBuffers()
        {
            // Initialize everything
            this.Vertices = new List<byte>();
            this.Normals = new List<byte>();
            this.Indices = new List<byte>();
            this.UVs = new List<byte>();
            this.Colors = new List<byte>();

            this.CMin = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };
            this.CMax = new double[3] { double.MinValue, double.MinValue, double.MinValue };

            this.VMax = new double[3] { double.MinValue, double.MinValue, double.MinValue };
            this.VMin = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };

            this.NMin = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };
            this.NMax = new double[3] { double.MinValue, double.MinValue, double.MinValue };

            this.UVMin = new double[2] { double.MaxValue, double.MaxValue };
            this.UVMax = new double[2] { double.MinValue, double.MinValue };
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

            if (color.HasValue && color.Value != default(Color))
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
        /// Add an index to the graphics buffers.
        /// </summary>
        /// <param name="index">The index to add.</param>
        public void AddIndex(ushort index)
        {
            this.Indices.AddRange(BitConverter.GetBytes(index));
            this.IMax = Math.Max(this.IMax, index);
            this.IMin = Math.Min(this.IMin, index);
        }

        internal static int PreallocationSize()
        {
            // Assume a fully vertex-colored mesh of the size required to contain 30k vertices.
            // Postion, Normal, Color, Index, UV
            var floatSize = sizeof(float);
            var intSize = sizeof(int);
            return (floatSize * 3 + floatSize * 3 + floatSize * 4 + intSize + floatSize * 2) * 30000;
        }
    }
}