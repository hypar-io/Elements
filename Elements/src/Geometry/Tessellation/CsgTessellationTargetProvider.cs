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
                // We used the polygon's shared tag, which seems to 
                // work for planar solids turned into csgs as a discriminator,
                // but this may break in the future.
                yield return new CsgPolygonTessAdapter(p, (uint)p.Shared.Tag, solidId);
            }
        }
    }
}