using System.Collections.Generic;
using Elements.Geometry.Solids;

namespace Elements.Geometry
{
    /// <summary>
    /// The geometry kernel.
    /// </summary>
    internal class Kernel
    {
        private static Kernel _instance;

        /// <summary>
        /// The Kernel singleton.
        /// </summary>
        public static Kernel Instance
        {
            get
            {
                if (_instance == null)
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
        public Solid CreateSweepAlongCurve(Profile profile,
                                           BoundedCurve curve,
                                           double startSetback,
                                           double endSetback,
                                           double profileRotation,
                                           double minimumChordLength = 0.01)
        {
            return Solid.SweepFaceAlongCurve(profile.Perimeter,
                                             profile.Voids != null && profile.Voids.Count > 0 ? profile.Voids : null,
                                             curve,
                                             startSetback,
                                             endSetback,
                                             profileRotation,
                                             minimumChordLength);
        }

        /// <summary>
        /// Create an extrude.
        /// </summary>
        /// <returns>A solid.</returns>
        public Solid CreateExtrude(Profile profile, double depth, Vector3 direction, bool flipped = false)
        {
            if (profile.Perimeter.Normal().Dot(direction) < 0 != flipped)
            {
                profile = profile.Reversed();
            }
            return Solid.SweepFace(profile.Perimeter, profile.Voids, direction, depth, false);
        }

        /// <summary>
        /// Create a lamina from a polygon.
        /// </summary>
        /// <returns>A solid.</returns>
        public Solid CreateLamina(Polygon perimeter)
        {
            return Solid.CreateLamina(perimeter.Vertices);
        }

        /// <summary>
        /// Create a lamina from a polygon and voids.
        /// </summary>
        /// <returns>A solid.</returns>
        public Solid CreateLamina(Polygon perimeter, IList<Polygon> voids)
        {
            return Solid.CreateLamina(perimeter, voids);
        }

        /// <summary>
        /// Create a lamina from a profile.
        /// </summary>
        /// <returns>A solid.</returns>
        public Solid CreateLamina(Profile profile)
        {
            return Solid.CreateLamina(profile);
        }

    }
}