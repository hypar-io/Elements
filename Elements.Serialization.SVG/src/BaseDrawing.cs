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
        public double Scale { get; protected set; }
        public float ViewBoxWidth => _viewBoxWidth;
        public float ViewBoxHeight => _viewBoxHeight;

        /// <summary>
        /// Get the scene bounds.
        /// </summary>
        public BBox3 GetSceneBounds()
        {
            return _sceneBounds;
        }

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

        protected BBox3 _sceneBounds;
        protected float _viewBoxWidth;
        protected float _viewBoxHeight;
    }
}