using System.Collections.Generic;
using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// A generic container for graphics data. This is broken out primarily to facilitate
    /// simpler testing of graphics buffers.
    /// </summary>
    internal interface IGraphicsBuffers
    {
        /// <summary>
        /// Initialize a graphics buffer to a sepcific vertex size.
        /// </summary>
        /// <param name="vertexCount">The number of vertices.</param>
        /// <param name="indexCount">The number of indices.</param>
        void Initialize(int vertexCount, int indexCount);

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
        /// Add vertices to the graphics buffers
        /// </summary>
        void AddVertices(IList<(Vector3 position, Vector3 normal, UV uv, Color? color)> vertices);

        /// <summary>
        /// Add an index to the graphics buffers.
        /// </summary>
        /// <param name="index">The index to add.</param>
        void AddIndex(ushort index);

        /// <summary>
        /// Add multiple indices to the graphics buffers.
        /// </summary>
        /// <param name="indices">The indices to add.</param>
        void AddIndices(IList<ushort> indices);
    }
}