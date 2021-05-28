using System.Collections.Generic;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements.Geometry.Solids
{
    public partial class Lamina
    {
        /// <summary>
        /// Construct a lamina from a perimeter.
        /// </summary>
        /// <param name="perimeter">The lamina's perimeter</param>
        /// <param name="isVoid">Should the lamina be considered a void?</param>
        public Lamina(Polygon @perimeter, bool @isVoid = false) : this(@perimeter, new List<Polygon>(), @isVoid)
        {
            // This additional constructor is necessary for backwards compatibility with the old generated constructor.
        }

        /// <summary>
        /// Construct a lamina from a profile.
        /// </summary>
        /// <param name="profile">The profile of the lamina</param>
        /// <param name="isVoid">Should the lamina be considered a void?</param>
        /// <returns></returns>
        public Lamina(Profile profile, bool isVoid = false) : this(profile.Perimeter, profile.Voids, isVoid)
        {

        }
    }
}