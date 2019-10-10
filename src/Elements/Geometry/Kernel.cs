using Elements.Geometry.Solids;

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
        /// <returns>A solid.</returns>
        public Solid CreateSweepAlongCurve(Profile profile, Curve curve, double startSetback, double endSetback, double rotation = 0.0)
        {
            return Solid.SweepFaceAlongCurve(profile.Perimeter, profile.Voids, curve, startSetback, endSetback, rotation);
        }

        /// <summary>
        /// Create an extrude.
        /// </summary>
        /// <returns>A solid.</returns>
        public Solid CreateExtrude(Profile profile, double depth, Vector3 direction, double rotation = 0.0)
        {
            return Solid.SweepFace(profile.Perimeter, profile.Voids, direction, depth, false, rotation);
        }

        /// <summary>
        /// Create a lamina.
        /// </summary>
        /// <returns>A solid.</returns>
        public Solid CreateLamina(Polygon perimeter)
        {
            return Solid.CreateLamina(perimeter.Vertices);
        }
    }
}