using Hypar.Geometry;

namespace Hypar.Elements
{
    /// <summary>
    /// Box represents a unit square box.
    /// </summary>
    public class Box: Element, ITessellate<Mesh>
    {
        /// <summary>
        /// Construct a unit square box.
        /// </summary>
        public Box()
        {
            this.Material = new Material("box", 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f);
        }

        /// <summary>
        /// Tessellate the box.
        /// </summary>
        /// <returns>A mesh representing the tessellated box.</returns>
        public Mesh Tessellate()
        {
            return Mesh.Extrude(new[]{Profiles.Rectangular()},1);
        }
    }
}