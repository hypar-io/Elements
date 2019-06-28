using System.Linq;
using Elements.Geometry.Interfaces;
using Elements.Geometry.Solids;
using Hypar.Elements.Interfaces;

namespace Elements.Geometry
{
    public class Kernel
    {
        private static Kernel _instance;

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

        public Solid CreateSweepAlongCurve(ISweepAlongCurve sweep)
        {
            return Solid.SweepFaceAlongCurve(sweep.Profile.Perimeter, sweep.Profile.Voids, sweep.Curve, sweep.StartSetback, sweep.EndSetback);
        }

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
    }
}