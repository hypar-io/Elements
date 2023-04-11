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
        private uint solidId;

        /// <summary>
        /// Construct a SolidTesselationTargetProvider.
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="solidId"></param>
        /// <param name="transform"></param>
        public SolidTesselationTargetProvider(Solid solid, uint solidId, Transform transform = null)
        {
            this.solid = solid;
            this.transform = transform;
            this.solidId = solidId;
        }

        /// <summary>
        /// Get the tessellation targets.
        /// </summary>
        public IEnumerable<ITessAdapter> GetTessellationTargets()
        {
            foreach (var f in solid.Faces.Values)
            {
                yield return new SolidFaceTessAdapter(f, solidId, transform);
            }
        }
    }
}