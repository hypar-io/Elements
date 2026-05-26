namespace Elements.Geometry.Tessellation
{
    /// <summary>
    /// Per-vertex data attached to <see cref="LibTessDotNet.Double.ContourVertex.Data"/>
    /// by the CSG and solid-face tessellation adapters, and consumed by
    /// <see cref="Tessellation.PackTessellationsIntoBuffers"/>.
    /// </summary>
    internal readonly struct CsgVertexData
    {
        public readonly UV Uv;
        public readonly uint Tag;
        public readonly uint FaceId;
        public readonly uint SolidId;

        public CsgVertexData(UV uv, uint tag, uint faceId, uint solidId)
        {
            Uv = uv;
            Tag = tag;
            FaceId = faceId;
            SolidId = solidId;
        }
    }
}
