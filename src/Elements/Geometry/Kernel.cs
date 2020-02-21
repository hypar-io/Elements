using System;
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
                if(_instance == null)
                {
                    _instance = new Kernel();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Create a solid from a mesh.
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns>A solid.</returns>
        public Solid CreateMeshSolid(Mesh mesh)
        {
            return Solid.CreateMesh(mesh);
        }

        /// <summary>
        /// Create a sweep along a curve.
        /// </summary>
        /// <returns>A solid.</returns>
        public Solid CreateSweepAlongCurve(Profile profile, Curve curve, double startSetback, double endSetback)
        {
            return Solid.SweepFaceAlongCurve(profile.Perimeter, profile.Voids, curve, startSetback, endSetback);
        }

        /// <summary>
        /// Create an extrude.
        /// </summary>
        /// <returns>A solid.</returns>
        public Solid CreateExtrude(Profile profile, double depth, Vector3 direction)
        {
            return Solid.SweepFace(profile.Perimeter, profile.Voids, direction, depth, false);
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