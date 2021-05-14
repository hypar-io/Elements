using System;
using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// A container for graphics data.
    /// The buffers used in this class align with webgl requirements.
    /// </summary>
    public class GraphicsBuffers
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
            this.Vertices.AddRange(BitConverter.GetBytes((float)position.X));
            this.Vertices.AddRange(BitConverter.GetBytes((float)position.Y));
            this.Vertices.AddRange(BitConverter.GetBytes((float)position.Z));

            this.Normals.AddRange(BitConverter.GetBytes((float)normal.X));
            this.Normals.AddRange(BitConverter.GetBytes((float)normal.Y));
            this.Normals.AddRange(BitConverter.GetBytes((float)normal.Z));

            this.UVs.AddRange(BitConverter.GetBytes((float)uv.U));
            this.UVs.AddRange(BitConverter.GetBytes((float)uv.V));

            this.VMax[0] = Math.Max(this.VMax[0], position.X);
            this.VMax[1] = Math.Max(this.VMax[1], position.Y);
            this.VMax[2] = Math.Max(this.VMax[2], position.Z);
            this.VMin[0] = Math.Min(this.VMin[0], position.X);
            this.VMin[1] = Math.Min(this.VMin[1], position.Y);
            this.VMin[2] = Math.Min(this.VMin[2], position.Z);

            this.NMax[0] = Math.Max(this.NMax[0], normal.X);
            this.NMax[1] = Math.Max(this.NMax[1], normal.Y);
            this.NMax[2] = Math.Max(this.NMax[2], normal.Z);
            this.NMin[0] = Math.Min(this.NMin[0], normal.X);
            this.NMin[1] = Math.Min(this.NMin[1], normal.Y);
            this.NMin[2] = Math.Min(this.NMin[2], normal.Z);

            this.UVMax[0] = Math.Max(this.UVMax[0], uv.U);
            this.UVMax[1] = Math.Max(this.UVMax[1], uv.V);
            this.UVMin[0] = Math.Min(this.UVMin[0], uv.U);
            this.UVMin[1] = Math.Min(this.UVMin[1], uv.V);

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
    }
}