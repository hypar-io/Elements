using System;
using Elements.Geometry;
using Elements.Interfaces;

namespace Elements
{
    /// <summary>
    /// A curve which is visible in 3D.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/ModelCurveTests.cs?name=example)]
    /// </example>
    public class ModelCurve : GeometricElement, IVisualizeCurves3d
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
        /// <param name="representation">The curve's representation.</param>
        /// <param name="isElementDefinition">Is this an element definition?</param>
        /// <param name="id">The id of the model curve.</param>
        /// <param name="name">The name of the model curve.</param>
        public ModelCurve(Curve curve,
                          Material material = null,
                          Transform transform = null,
                          Representation representation = null,
                          bool isElementDefinition = false,
                          Guid id = default(Guid),
                          string name = null) : base(transform != null ? transform : new Transform(),
                                                     material != null ? material : BuiltInMaterials.Concrete,
                                                     null,
                                                     isElementDefinition,
                                                     id != default(Guid) ? id : Guid.NewGuid(),
                                                     name)
        {
            this.Curve = curve;
            this.Material = material != null ? material : BuiltInMaterials.Edges;
        }

        /// <summary>
        /// Visualize this curve in 3d.
        /// </summary>
        /// <param name="lineLoop"></param>
        public GraphicsBuffers VisualizeCurves3d(bool lineLoop)
        {
            return this.Curve.RenderVertices().ToGraphicsBuffers(lineLoop);
        }
    }
}