using Newtonsoft.Json;

namespace Elements.Geometry
{
    /// <summary>
    /// A PlanarFace bound by 4 edges.
    /// </summary>
    public class QuadFace : PlanarFace
    {
        /// <summary>
        /// Construct a QuadFace.
        /// </summary>
        /// <param name="vertices"></param>
        [JsonConstructor]
        public QuadFace(Vector3[] vertices) : base(vertices) { }

        /// <summary>
        /// Compute the Mesh for the QuadFace.
        /// </summary>
        /// <param name="mesh"></param>
        public override void Tessellate(Mesh mesh)
        {
            mesh.AddQuad(this.Vertices);
        }
    }
}