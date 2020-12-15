using System;
using System.Collections.Generic;
using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// A curve which is visible in 3D.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/ModelCurveTests.cs?name=example)]
    /// </example>
    [UserElement]
    public class ModelCurve : GeometricElement
    {
        /// <summary>
        /// The curve.
        /// </summary>
        public Curve Curve { get; set; }

        /// <summary>
        /// Create a model curve.
        /// </summary>
        /// <param name="curve">The curve.</param>
        /// <param name="material">The material. Specular and glossiness components will be ignored.</param>
        /// <param name="transform">The model curve's transform.</param>
        /// <param name="representations">The curve's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the model curve.</param>
        /// <param name="name">The name of the model curve.</param>
        public ModelCurve(Curve curve,
                          Material material = null,
                          Transform transform = null,
                          IList<Representation> representations = null,
                          bool isElementDefinition = false,
                          Guid id = default(Guid),
                          string name = null) : base(transform != null ? transform : new Transform(),
                                                     representations != null ? representations : new[] { new CurveRepresentation(
                                                         curve,material != null ? material : BuiltInMaterials.Edges) },
                                                     isElementDefinition,
                                                     id != default(Guid) ? id : Guid.NewGuid(),
                                                     name)
        {
            this.Curve = curve;
        }
    }

    /// <summary>
    /// Extension methods for model curves.
    /// </summary>
    public static class ModelCurveExtensions
    {
        /// <summary>
        /// Convert a transform to a set of model curves.
        /// </summary>
        /// <param name="t">The transform to convert.</param>
        /// <param name="context">An optional transform in which these curves should be drawn.</param>
        public static IList<ModelCurve> ToModelCurves(this Transform t, Transform context = null)
        {
            var mc = new List<ModelCurve>();
            var x = new ModelCurve(new Line(t.Origin, t.XAxis, 1.0), BuiltInMaterials.XAxis, context);
            var y = new ModelCurve(new Line(t.Origin, t.YAxis, 1.0), BuiltInMaterials.YAxis, context);
            var z = new ModelCurve(new Line(t.Origin, t.ZAxis, 1.0), BuiltInMaterials.ZAxis, context);
            mc.AddRange(new[] { x, y, z });
            return mc;
        }
    }
}