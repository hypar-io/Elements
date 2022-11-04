using Elements.Geometry;
using Svg;
using Svg.Transforms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Colors = System.Drawing.Color;

namespace Elements.Serialization.SVG
{
    /// <summary>
    /// Orientations for a plan relative to the page.
    /// </summary>
    public enum PlanRotation
    {
        /// <summary>
        /// Align the longest grid along the long axis of the page.
        /// </summary>
        LongestGridHorizontal,
        /// <summary>
        /// Align the longest grid along the short axis of the page.
        /// </summary>
        LongestGridVertical,
        /// <summary>
        /// Do not reorient the drawing on the page.
        /// </summary>
        None,
        /// <summary>
        /// Rotate the drawing by a specific angle.
        /// </summary>
        Angle,
    }

    /// <summary>
    /// 
    /// </summary>
    public static class SvgSection
    {
        /// <summary>
        /// Create a plan of a model at the provided elevation and save the 
        /// resulting section to the provided path.
        /// </summary>
        /// <param name="models">A collection of models to include in the plan.</param>
        /// <param name="elevation">The elevation at which the plan will be cut.</param>
        /// <param name="frontContext">An svg context which defines settings for elements cut by the cut plane.</param>
        /// <param name="backContext">An svg context which defines settings for elements behind the cut plane.</param>
        /// <param name="path">The location on disk to write the SVG file.</param>
        /// <param name="showGrid">If gridlines exist, should they be shown in the plan?</param>
        /// <param name="gridHeadExtension">The extension of the grid head past the bounds of the drawing in the created plan.</param>
        /// <param name="gridHeadRadius">The radius of grid heads in the created plan.</param>
        /// <param name="planRotation">How should the plan be rotated relative to the page?</param>
        /// <param name="planRotationDegrees">An additional amount to rotate the plan.</param>
        public static void CreateAndSavePlanFromModels(IList<Model> models,
                                                double elevation,
                                                SvgContext frontContext,
                                                SvgContext backContext,
                                                string path,
                                                bool showGrid = true,
                                                double gridHeadExtension = 2.0,
                                                double gridHeadRadius = 0.5,
                                                PlanRotation planRotation = PlanRotation.Angle,
                                                double planRotationDegrees = 0.0)
        {
            var doc = CreatePlanFromModels(models, elevation, frontContext, backContext, showGrid, gridHeadExtension, gridHeadRadius, planRotation, planRotationDegrees);
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            doc.Write(stream);
        }

        /// <summary>
        /// Generate a plan of a model at the provided elevation.
        /// </summary>
        /// <param name="models">A collection of models to include in the plan.</param>
        /// <param name="elevation">The elevation at which the plan will be cut.</param>
        /// <param name="frontContext">An svg context which defines settings for elements cut by the cut plane.</param>
        /// <param name="backContext">An svg context which defines settings for elements behind the cut plane.</param>
        /// <param name="showGrid">If gridlines exist, should they be shown in the plan?</param>
        /// <param name="gridHeadExtension">The extension of the grid head past the bounds of the drawing in the created plan.</param>
        /// <param name="gridHeadRadius">The radius of grid heads in the created plan.</param>
        /// <param name="planRotation">How should the plan be rotated relative to the page?</param>
        /// <param name="planRotationDegrees">An additional amount to rotate the plan.</param>
        public static SvgDocument CreatePlanFromModels(IList<Model> models,
                                                double elevation,
                                                SvgContext frontContext,
                                                SvgContext backContext,
                                                bool showGrid = true,
                                                double gridHeadExtension = 2.0,
                                                double gridHeadRadius = 0.5,
                                                PlanRotation planRotation = PlanRotation.Angle,
                                                double planRotationDegrees = 0.0)
        {
            return CreatePlanFromModels(models, elevation, frontContext, backContext, out _, showGrid, gridHeadExtension, gridHeadRadius,
                    planRotation, planRotationDegrees);
        }

        /// <summary>
        /// Generate a plan of a model at the provided elevation.
        /// </summary>
        /// <param name="models">A collection of models to include in the plan.</param>
        /// <param name="elevation">The elevation at which the plan will be cut.</param>
        /// <param name="frontContext">An svg context which defines settings for elements cut by the cut plane.</param>
        /// <param name="backContext">An svg context which defines settings for elements behind the cut plane.</param>
        /// <param name="sceneBounds">The scene bounds. It can be used to add elements to the output document</param>
        /// <param name="showGrid">If gridlines exist, should they be shown in the plan?</param>
        /// <param name="gridHeadExtension">The extension of the grid head past the bounds of the drawing in the created plan.</param>
        /// <param name="gridHeadRadius">The radius of grid heads in the created plan.</param>
        /// <param name="planRotation">How should the plan be rotated relative to the page?</param>
        /// <param name="planRotationDegrees">An additional amount to rotate the plan.</param>
        public static SvgDocument CreatePlanFromModels(IList<Model> models,
                                                double elevation,
                                                SvgContext frontContext,
                                                SvgContext backContext,
                                                out BBox3 sceneBounds,
                                                bool showGrid = true,
                                                double gridHeadExtension = 2.0,
                                                double gridHeadRadius = 0.5,
                                                PlanRotation planRotation = PlanRotation.Angle,
                                                double planRotationDegrees = 0.0)
        {
            var drawingPlan = new SvgDrawingPlan(models, elevation);
            drawingPlan.BackContext = backContext;
            drawingPlan.FrontContext = frontContext;
            drawingPlan.ShowGrid = showGrid;
            drawingPlan.GridHeadExtension = gridHeadExtension;
            drawingPlan.GridHeadRadius = gridHeadRadius;
            drawingPlan.PlanRotation = planRotation;
            drawingPlan.PlanRotationDegrees = planRotationDegrees;

            var doc = drawingPlan.CreateSvgDocument();
            sceneBounds = drawingPlan.GetSceneBounds();
            return doc;
        }

        internal static double GetRotationValueForPlan(IList<Model> models, PlanRotation rotation, double angle)
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

        internal static BBox3 ComputeSceneBounds(IList<Model> models)
        {
            var max = new Vector3(double.MinValue, double.MinValue);
            var min = new Vector3(double.MaxValue, double.MaxValue);

            foreach (var model in models)
            {
                foreach (var element in model.Elements)
                {
                    if (element.Value is GeometricElement geo)
                    {
                        geo.UpdateRepresentations();
                        geo.UpdateBoundsAndComputeSolid();

                        if (geo._bounds.Max.X > max.X)
                        {
                            max.X = geo._bounds.Max.X;
                        }
                        if (geo._bounds.Max.Y > max.Y)
                        {
                            max.Y = geo._bounds.Max.Y;
                        }
                        if (geo._bounds.Min.X < min.X)
                        {
                            min.X = geo._bounds.Min.X;
                        }
                        if (geo._bounds.Min.Y < min.Y)
                        {
                            min.Y = geo._bounds.Min.Y;
                        }
                    }
                }
            }

            return new BBox3(min, max);
        }
    }
}
