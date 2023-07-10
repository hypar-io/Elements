using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;

namespace Elements.Serialization.SVG
{
    /// <summary>
    /// Base class for SVG documents
    /// </summary>
    public abstract class SvgBaseDrawing
    {
        /// <summary>
        /// Scale applied to the model.
        /// Usually it's a PageHeight to a ViewBoxHeight ratio (or a PageWidth to a ViewBoxWidth)
        /// </summary>
        public double Scale { get; protected set; }
        /// <summary>
        /// The width of the model bounding box.
        /// </summary>
        public float ViewBoxWidth
        {
            get { return _viewBoxWidth; }
            protected set { _viewBoxWidth = value; }
        }
        /// <summary>
        /// The height of the model bounding box.
        /// </summary>
        public float ViewBoxHeight
        {
            get { return _viewBoxHeight; }
            protected set { _viewBoxHeight = value; }
        }

        /// <summary>
        /// Get the scene bounds.
        /// </summary>
        public BBox3 SceneBounds
        {
            get { return _sceneBounds; }
            protected set { _sceneBounds = value; }
        }

        /// <summary>
        /// Computes scene bounds
        /// </summary>
        /// <param name="models">The set of the models</param>
        /// <returns>Returns the boundinf box around the input models</returns>
        public static BBox3 ComputeSceneBounds(IList<Model> models)
        {
            var bounds = new BBox3(Vector3.Max, Vector3.Min);
            foreach (var model in models)
            {
                foreach (var element in model.Elements)
                {
                    if (element.Value is GeometricElement geo)
                    {
                        geo.UpdateRepresentations();
                        if (geo.Representation == null || geo.Representation.SolidOperations.All(v => v.IsVoid))
                        {
                            continue;
                        }
                        geo.UpdateBoundsAndComputeSolid();

                        var bbMax = geo.Transform.OfPoint(geo._bounds.Max);
                        var bbMin = geo.Transform.OfPoint(geo._bounds.Min);
                        bounds.Extend(new[] { bbMax, bbMin });
                    }
                }
            }

            return bounds;
        }

        /// <summary>
        /// Gets the rotation angle that must be applied to the plan.
        /// </summary>
        /// <param name="models">The set of the models.</param>
        /// <param name="rotation">The orientation for a plan relative to the page.</param>
        /// <param name="angle">The angle value that must be applied if rotation is Angle</param>
        /// <returns>The angle in degrees</returns>
        protected static double GetRotationValueForPlan(IList<Model> models, PlanRotation rotation, double angle)
        {
            if (rotation == PlanRotation.Angle)
            {
                return angle;
            }

            var grids = models.SelectMany(m => m.AllElementsOfType<GridLine>()).Select(gl => gl.Curve).Where(gl => gl is Line).ToList();
            if (!grids.Any())
            {
                return 0.0;
            }

            var longest = (Line)grids.OrderBy(g => g.Length()).First();

            return rotation switch
            {
                PlanRotation.LongestGridHorizontal => -longest.Direction().PlaneAngleTo(Vector3.YAxis),
                PlanRotation.LongestGridVertical => -longest.Direction().PlaneAngleTo(Vector3.XAxis),
                PlanRotation.Angle => angle,
                PlanRotation.None => 0.0,
                _ => 0.0,
            };
        }

        private BBox3 _sceneBounds;
        private float _viewBoxHeight;
        private float _viewBoxWidth;
    }
}