using System.Collections.Generic;

namespace Hypar.Elements
{
    public static class ElementsFactory
    {
        /// <summary>
        /// Create a default mass.
        /// </summary>
        /// <returns></returns>
        public static Mass CreateMass()
        {
            var defaultProfile = Profiles.Square();
            var m = new Mass(defaultProfile, 0.0, defaultProfile, 1.0);
            return m;
        }

        /// <summary>
        /// Create a default grid.
        /// </summary>
        /// <returns></returns>
        public static Grid CreateGrid()
        {
            var perimeter = Profiles.Square();
            var g = new Grid(perimeter);
            return g;
        }

        public static IEnumerable<Grid> CreateGrids(int n)
        {
            var grids = new List<Grid>();
            for(var i=0; i<n; i++)
            {
                grids.Add(CreateGrid());
            }
            return grids;
        }
    }
}