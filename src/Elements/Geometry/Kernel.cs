using System.Linq;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Solids;
using Elements.Interfaces;

namespace Elements.Geometry
{
    /// <summary>
    /// 
    /// </summary>
    public class Kernel
    {
        private static Kernel _instance;

        /// <summary>
        /// The Kernel singleton.
        /// </summary>
        public static Kernel Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new Kernel();
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Create a sweep along a curve.
        /// </summary>
        /// <param name="sweep">The ISweepAlongCurve object.</param>
        /// <returns>A solid.</returns>
        public Solid CreateSweepAlongCurve(ISweepAlongCurve sweep)
        {
            return Solid.SweepFaceAlongCurve(sweep.Profile.Perimeter, sweep.Profile.Voids, sweep.Curve, sweep.StartSetback, sweep.EndSetback);
        }

        /// <summary>
        /// Create an extrude.
        /// </summary>
        /// <param name="extrude">The IExtrude object.</param>
        /// <returns>A solid.</returns>
        public Solid CreateExtrude(IExtrude extrude)
        {
            if(extrude is IHasOpenings)
            {
                var o = (IHasOpenings)extrude;
                // The voids in the profiles are concatenated with the
                // voids provided by the openings.
                Polygon[] voids = null;
                if(o.Openings != null && o.Openings.Count > 0)
                {
                    if(extrude.Profile.Voids == null)
                    {
                        voids = o.Openings.Select(op=>op.Profile.Perimeter).ToArray();
                    }
                    else
                    {
                        voids = extrude.Profile.Voids.Concat(o.Openings.Select(op=>op.Profile.Perimeter)).ToArray();
                    }
                }

                return Solid.SweepFace(extrude.Profile.Perimeter, voids, extrude.ExtrudeDirection, extrude.ExtrudeDepth, extrude.BothSides);
            }
            else
            {
                return Solid.SweepFace(extrude.Profile.Perimeter, null, extrude.ExtrudeDirection, extrude.ExtrudeDepth, extrude.BothSides);
            }
        }

        /// <summary>
        /// Create a lamina.
        /// </summary>
        /// <param name="lamina">The ILamina object.</param>
        /// <returns>A solid.</returns>
        public Solid CreateLamina(ILamina lamina)
        {
            return Solid.CreateLamina(lamina.Perimeter.Vertices);
        }
    }
}