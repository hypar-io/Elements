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
            var defaultProfile = Profiles.Rectangular();
            var m = new Mass(defaultProfile, 0.0, defaultProfile, 1.0);
            return m;
        }

        /// <summary>
        /// Create a default beam.
        /// </summary>
        /// <returns></returns>
        public static Beam CreateBeam()
        {
            var b = new Beam();
            return b;
        }

        public static IEnumerable<Beam> CreateBeams(int n)
        {
            var beams = new List<Beam>();
            for(var i=0; i<n; i++)
            {
                beams.Add(CreateBeam());
            }
            return beams;
        }

        /// <summary>
        /// Create a default grid.
        /// </summary>
        /// <returns></returns>
        public static Grid CreateGrid()
        {
            var perimeter = Profiles.Rectangular();
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