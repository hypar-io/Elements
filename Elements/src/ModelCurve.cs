using System;
using System.Collections.Generic;
using System.Linq;
using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// A curve which is visible in 3D.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/ModelCurveTests.cs?name=example)]
    /// </example>
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

        /// <summary>
        /// Convert a Bounding Box to a set of model curves.
        /// </summary>
        /// <param name="box">The bounding box to convert.</param>
        /// <param name="context">An optional transform in which these curves should be drawn.</param>
        /// <param name="material">An optional material to use for these curves.</param>
        public static IList<ModelCurve> ToModelCurves(this BBox3 box, Transform context = null, Material material = null)
        {
            var mat = material ?? BuiltInMaterials.Black;
            var min = box.Min;
            var max = box.Max;
            var a = new Vector3(min.X, min.Y, min.Z);
            var b = new Vector3(max.X, min.Y, min.Z);
            var c = new Vector3(max.X, max.Y, min.Z);
            var d = new Vector3(min.X, max.Y, min.Z);
            var e = new Vector3(min.X, min.Y, max.Z);
            var f = new Vector3(max.X, min.Y, max.Z);
            var g = new Vector3(max.X, max.Y, max.Z);
            var h = new Vector3(min.X, max.Y, max.Z);
            var mc = new List<ModelCurve> {
                new ModelCurve(new Line(a,b), mat, context),
                new ModelCurve(new Line(b,c), mat, context),
                new ModelCurve(new Line(c,d), mat, context),
                new ModelCurve(new Line(d,a), mat, context),
                new ModelCurve(new Line(e,f), mat, context),
                new ModelCurve(new Line(f,g), mat, context),
                new ModelCurve(new Line(g,h), mat, context),
                new ModelCurve(new Line(h,e), mat, context),
                new ModelCurve(new Line(a,e), mat, context),
                new ModelCurve(new Line(b,f), mat, context),
                new ModelCurve(new Line(c,g), mat, context),
                new ModelCurve(new Line(d,h), mat, context),
            };
            return mc;
        }

        /// <summary>
        /// Convert a profile to a set of model curves.
        /// </summary>
        /// <param name="p">The profile to convert.</param>
        /// <param name="context">An optional transform in which these curves should be drawn.</param>
        /// <param name="material">An optional material to use for these curves.</param>
        public static IList<ModelCurve> ToModelCurves(this Profile p, Transform context = null, Material material = null)
        {
            var mat = material ?? BuiltInMaterials.Black;
            var xform = context ?? new Transform();
            var mc = new List<ModelCurve>() {
              new ModelCurve(p.Perimeter, mat, xform)
            };
            if (p.Voids != null)
            {
                mc.AddRange(p.Voids.Select(v => new ModelCurve(v, mat, xform)));
            }
            return mc;
        }
    }
}