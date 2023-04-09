using System.Collections.Generic;
using Elements.Geometry.Solids;

namespace Elements.Geometry.Tessellation
{
    /// <summary>
    /// An object which provides tessellation targets for a solid.
    /// </summary>
    internal class SolidTesselationTargetProvider : ITessellationTargetProvider
    {
        private readonly Solid solid;
        private readonly Transform transform;
        private int offset;

        /// <summary>
        /// Construct a SolidTesselationTargetProvider.
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="transform"></param>
        public SolidTesselationTargetProvider(Solid solid, int offset, Transform transform = null)
        {
            this.solid = solid;
            this.transform = transform;
            this.offset = offset;
        }

        /// <summary>
        /// Get the tessellation targets.
        /// </summary>
        public IEnumerable<ITessAdapter> GetTessellationTargets()
        {
            foreach (var f in solid.Faces.Values)
            {
                yield return new SolidFaceTessAdapter(f, offset, transform);
            }
        }
    }
}