using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Interfaces;
using LibTessDotNet.Double;

namespace Elements.Serialization.glTF
{
    /// <summary>
    /// A renderer which generates GlRenderData objects for various types.
    /// </summary>
    public class GlRenderer : IRenderer
    {
        private List<GlMeshRenderData> meshRenderDatas = new List<GlMeshRenderData>();
        private List<GlPointRenderData> pointRenderDatas = new List<GlPointRenderData>();

        /// <summary>
        /// Render a mesh.
        /// </summary>
        /// <param name="mesh"></param>
        public void Render(Mesh mesh)
        {
            var floatSize = sizeof(float);
            var ushortSize = sizeof(ushort);

            var vertexBuffer = new byte[mesh.Vertices.Count * floatSize * 3];
            var normalBuffer = new byte[mesh.Vertices.Count * floatSize * 3];
            var indexBuffer = new byte[mesh.Triangles.Count * ushortSize * 3];
            var uvBuffer = new byte[mesh.Vertices.Count * floatSize * 2];

            byte[] colorBuffer;
            float[] cMin;
            float[] cMax;

            if (!mesh.Vertices[0].Color.Equals(default(Color)))
            {
                colorBuffer = new byte[mesh.Vertices.Count * floatSize * 3];
                cMin = new float[] { float.MaxValue, float.MaxValue, float.MaxValue };
                cMax = new float[] { float.MinValue, float.MinValue, float.MinValue };
            }
            else
            {
                colorBuffer = new byte[0];
                cMin = new float[0];
                cMax = new float[0];
            }

            var vMax = new double[3] { double.MinValue, double.MinValue, double.MinValue };
            var vMin = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };
            var nMin = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };
            var nMax = new double[3] { double.MinValue, double.MinValue, double.MinValue };
            var uvMax = new double[2] { double.MinValue, double.MinValue };
            var uvMin = new double[2] { double.MaxValue, double.MaxValue };

            var iMax = ushort.MinValue;
            var iMin = ushort.MaxValue;

            var vi = 0;
            var ii = 0;
            var ci = 0;
            var uvi = 0;

            for (var i = 0; i < mesh.Vertices.Count; i++)
            {
                var v = mesh.Vertices[i];

                System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Position.X), 0, vertexBuffer, vi, floatSize);
                System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Position.Y), 0, vertexBuffer, vi + floatSize, floatSize);
                System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Position.Z), 0, vertexBuffer, vi + 2 * floatSize, floatSize);

                System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Normal.X), 0, normalBuffer, vi, floatSize);
                System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Normal.Y), 0, normalBuffer, vi + floatSize, floatSize);
                System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Normal.Z), 0, normalBuffer, vi + 2 * floatSize, floatSize);

                System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.UV.U), 0, uvBuffer, uvi, floatSize);
                System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.UV.V), 0, uvBuffer, uvi + floatSize, floatSize);

                uvi += 2 * floatSize;
                vi += 3 * floatSize;

                vMax[0] = Math.Max(vMax[0], v.Position.X);
                vMax[1] = Math.Max(vMax[1], v.Position.Y);
                vMax[2] = Math.Max(vMax[2], v.Position.Z);
                vMin[0] = Math.Min(vMin[0], v.Position.X);
                vMin[1] = Math.Min(vMin[1], v.Position.Y);
                vMin[2] = Math.Min(vMin[2], v.Position.Z);

                nMax[0] = Math.Max(nMax[0], v.Normal.X);
                nMax[1] = Math.Max(nMax[1], v.Normal.Y);
                nMax[2] = Math.Max(nMax[2], v.Normal.Z);
                nMin[0] = Math.Min(nMin[0], v.Normal.X);
                nMin[1] = Math.Min(nMin[1], v.Normal.Y);
                nMin[2] = Math.Min(nMin[2], v.Normal.Z);

                uvMax[0] = Math.Max(uvMax[0], v.UV.U);
                uvMax[1] = Math.Max(uvMax[1], v.UV.V);
                uvMin[0] = Math.Min(uvMin[0], v.UV.U);
                uvMin[1] = Math.Min(uvMin[1], v.UV.V);

                iMax = Math.Max(iMax, (ushort)v.Index);
                iMin = Math.Min(iMin, (ushort)v.Index);

                if (!v.Color.Equals(default(Color)))
                {
                    cMax[0] = Math.Max(cMax[0], (float)v.Color.Red);
                    cMax[1] = Math.Max(cMax[1], (float)v.Color.Green);
                    cMax[2] = Math.Max(cMax[2], (float)v.Color.Blue);
                    cMin[0] = Math.Min(cMin[0], (float)v.Color.Red);
                    cMin[1] = Math.Min(cMin[1], (float)v.Color.Green);
                    cMin[2] = Math.Min(cMin[2], (float)v.Color.Blue);

                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Color.Red), 0, colorBuffer, ci, floatSize);
                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Color.Green), 0, colorBuffer, ci + floatSize, floatSize);
                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Color.Blue), 0, colorBuffer, ci + 2 * floatSize, floatSize);
                    ci += 3 * floatSize;
                }
            }

            for (var i = 0; i < mesh.Triangles.Count; i++)
            {
                var t = mesh.Triangles[i];

                System.Buffer.BlockCopy(BitConverter.GetBytes((ushort)t.Vertices[0].Index), 0, indexBuffer, ii, ushortSize);
                System.Buffer.BlockCopy(BitConverter.GetBytes((ushort)t.Vertices[1].Index), 0, indexBuffer, ii + ushortSize, ushortSize);
                System.Buffer.BlockCopy(BitConverter.GetBytes((ushort)t.Vertices[2].Index), 0, indexBuffer, ii + 2 * ushortSize, ushortSize);
                ii += 3 * ushortSize;
            }

            meshRenderDatas.Add(new GlMeshRenderData(vertexBuffer,
                                    indexBuffer,
                                    colorBuffer,
                                    uvBuffer,
                                    vMax,
                                    vMin,
                                    nMin,
                                    nMax,
                                    cMin,
                                    cMax,
                                    iMin,
                                    iMax,
                                    uvMin,
                                    uvMax));
        }

        /// <summary>
        /// Render a line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns>A GlPointRenderData object.</returns>
        public void Render(Line line)
        {
            pointRenderDatas.Add(new GlPointRenderData(new List<Vector3> { line.Start, line.End }));
        }

        /// <summary>
        /// Render a polyline.
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns>A GlPointRenderData object.</returns>
        public void Render(Polyline polyline)
        {
            pointRenderDatas.Add(new GlPointRenderData(polyline.Vertices));
        }

        /// <summary>
        /// Render a polygon.
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns>A GlPointRenderData object.</returns>
        public void Render(Polygon polygon)
        {
            var verts = new List<Vector3>(polygon.Vertices);
            verts.Add(polygon.Start);
            pointRenderDatas.Add(new GlPointRenderData(verts));
        }

        /// <summary>
        /// Render a bezier.
        /// </summary>
        /// <param name="bezier"></param>
        /// <returns>A GlPointRenderData object.</returns>
        public void Render(Bezier bezier)
        {
            var vertices = new List<Vector3>();
            // TODO: Implement adaptive sampling.
            var samples = 10;
            for (var i = 0; i <= samples; i++)
            {
                vertices.Add(bezier.PointAt(i * 1.0 / samples));
            }
            pointRenderDatas.Add(new GlPointRenderData(vertices));
        }

        /// <summary>
        /// Render a collection of solid operations.
        /// </summary>
        /// <param name="solids"></param>
        /// <returns></returns>
        public void Render(List<SolidOperation> solids)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Render an arc.
        /// </summary>
        /// <param name="arc"></param>
        public void Render(Arc arc)
        {
            var parameters = arc.GetSampleParameters();
            var vertices = new List<Vector3>();
            foreach (var p in parameters)
            {
                vertices.Add(arc.PointAt(p));
            }
            pointRenderDatas.Add(new GlPointRenderData(vertices));
        }

        /// <summary>
        /// Render a csg.
        /// </summary>
        /// <param name="csg"></param>
        public void Render(Csg.Solid csg)
        {
            var tessellations = new Tess[csg.Polygons.Count];

            var fi = 0;
            foreach (var p in csg.Polygons)
            {
                var tess = new Tess();
                tess.NoEmptyPolygons = true;
                tess.AddContour(p.Vertices.ToContourVertices());

                tess.Tessellate(WindingRule.Positive, LibTessDotNet.Double.ElementType.Polygons, 3);
                tessellations[fi] = tess;
                fi++;
            }

            var floatSize = sizeof(float);
            var ushortSize = sizeof(ushort);

            var vertexCount = tessellations.Sum(t => t.VertexCount);
            var indexCount = tessellations.Sum(t => t.Elements.Length);

            var vertexBuffer = new byte[vertexCount * floatSize * 3];
            var normalBuffer = new byte[vertexCount * floatSize * 3];
            var indexBuffer = new byte[indexCount * ushortSize];
            var uvBuffer = new byte[vertexCount * floatSize * 2];

            // Vertex colors are not used in this context currently.
            var colorBuffer = new byte[0];
            var cmin = new float[0];
            var cmax = new float[0];

            var vmax = new double[3] { double.MinValue, double.MinValue, double.MinValue };
            var vmin = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };
            var nmin = new double[3] { double.MaxValue, double.MaxValue, double.MaxValue };
            var nmax = new double[3] { double.MinValue, double.MinValue, double.MinValue };

            // TODO: Set this properly when solids get UV coordinates.
            var uvmin = new double[2] { 0, 0 };
            var uvmax = new double[2] { 0, 0 };

            var imax = ushort.MinValue;
            var imin = ushort.MaxValue;

            var vi = 0;
            var ii = 0;
            var uvi = 0;

            var iCursor = 0;

            for (var i = 0; i < tessellations.Length; i++)
            {
                var tess = tessellations[i];
                if (tess.ElementCount == 0)
                {
                    continue;
                }
                var a = tess.Vertices[tess.Elements[0]].Position.ToVector3();
                var b = tess.Vertices[tess.Elements[1]].Position.ToVector3();
                var c = tess.Vertices[tess.Elements[2]].Position.ToVector3();
                var n = (b - a).Cross(c - a).Unitized();

                for (var j = 0; j < tess.Vertices.Length; j++)
                {
                    var v = tess.Vertices[j];

                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Position.X), 0, vertexBuffer, vi, floatSize);
                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Position.Y), 0, vertexBuffer, vi + floatSize, floatSize);
                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)v.Position.Z), 0, vertexBuffer, vi + 2 * floatSize, floatSize);

                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)n.X), 0, normalBuffer, vi, floatSize);
                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)n.Y), 0, normalBuffer, vi + floatSize, floatSize);
                    System.Buffer.BlockCopy(BitConverter.GetBytes((float)n.Z), 0, normalBuffer, vi + 2 * floatSize, floatSize);

                    // TODO: Update Solids to use something other than UV = {0,0}.
                    System.Buffer.BlockCopy(BitConverter.GetBytes(0f), 0, uvBuffer, uvi, floatSize);
                    System.Buffer.BlockCopy(BitConverter.GetBytes(0f), 0, uvBuffer, uvi + floatSize, floatSize);

                    uvi += 2 * floatSize;
                    vi += 3 * floatSize;

                    vmax[0] = Math.Max(vmax[0], v.Position.X);
                    vmax[1] = Math.Max(vmax[1], v.Position.Y);
                    vmax[2] = Math.Max(vmax[2], v.Position.Z);
                    vmin[0] = Math.Min(vmin[0], v.Position.X);
                    vmin[1] = Math.Min(vmin[1], v.Position.Y);
                    vmin[2] = Math.Min(vmin[2], v.Position.Z);

                    nmax[0] = Math.Max(nmax[0], n.X);
                    nmax[1] = Math.Max(nmax[1], n.Y);
                    nmax[2] = Math.Max(nmax[2], n.Z);
                    nmin[0] = Math.Min(nmin[0], n.X);
                    nmin[1] = Math.Min(nmin[1], n.Y);
                    nmin[2] = Math.Min(nmin[2], n.Z);

                    // uvmax[0] = Math.Max(uvmax[0], 0);
                    // uvmax[1] = Math.Max(uvmax[1], 0);
                    // uvmin[0] = Math.Min(uvmin[0], 0);
                    // uvmin[1] = Math.Min(uvmin[1], 0);
                }

                for (var k = 0; k < tess.Elements.Length; k++)
                {
                    var t = tess.Elements[k];
                    var index = (ushort)(t + iCursor);
                    System.Buffer.BlockCopy(BitConverter.GetBytes(index), 0, indexBuffer, ii, ushortSize);
                    imax = Math.Max(imax, index);
                    imin = Math.Min(imin, index);
                    ii += ushortSize;
                }

                iCursor = imax + 1;
            }
        }
    }
}