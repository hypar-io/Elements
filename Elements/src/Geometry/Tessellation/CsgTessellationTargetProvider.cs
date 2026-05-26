using System.Collections.Generic;

namespace Elements.Geometry.Tessellation
{
    /// <summary>
    /// An object which provides tessellation targets for a csg solid.
    /// </summary>
    internal class CsgTessellationTargetProvider : ITessellationTargetProvider
    {
        private readonly Csg.Solid csg;
        private readonly uint solidId;

        /// <summary>
        /// Construct a CsgTessellationTargetProvider.
        /// </summary>
        /// <param name="csg"></param>
        /// <param name="solidId"></param>
        public CsgTessellationTargetProvider(Csg.Solid csg, uint solidId)
        {
            this.csg = csg;
            this.solidId = solidId;
        }

        /// <summary>
        /// Get the tessellation targets.
        /// </summary>
        public IEnumerable<ITessAdapter> GetTessellationTargets()
        {
            foreach (var p in csg.Polygons)
            {
                // Shared.Tag groups coplanar polygons so the pack shares vertices across
                // them; post-union collisions are caught by position-matched reuse downstream.
                yield return new CsgPolygonTessAdapter(p, (uint)p.Shared.Tag, solidId);
            }
        }
    }
}