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

        /// <summary>
        /// Set whether this model curve should be selectable in the web UI.
        /// </summary>
        /// <param name="selectable"></param>
        public void SetSelectable(bool selectable)
        {
            this._isSelectable = selectable;
        }

        internal GraphicsBuffers ToGraphicsBuffers()
        {
            return this.Curve.ToGraphicsBuffers();
        }

        /// <summary>
        /// Get graphics buffers and other metadata required to modify a GLB.
        /// </summary>
        /// <returns>
        /// True if there is graphicsbuffers data applicable to add, false otherwise.
        /// Out variables should be ignored if the return value is false.
        /// </returns>
        public override bool TryToGraphicsBuffers(out List<GraphicsBuffers> graphicsBuffers, out string id, out glTFLoader.Schema.MeshPrimitive.ModeEnum? mode)
        {
            id = this._isSelectable ? $"{this.Id}_curve" : $"unselectable_{this.Id}_curve";
            mode = glTFLoader.Schema.MeshPrimitive.ModeEnum.LINE_STRIP;
            graphicsBuffers = new List<GraphicsBuffers>() { this.ToGraphicsBuffers() };
            return true;
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
            var mc = new List<ModelCurve>();
            Action<Vector3, Vector3> tryAddLine = (Vector3 from, Vector3 to) =>
            {
                if (from.DistanceTo(to) > Vector3.EPSILON)
                {
                    mc.Add(new ModelCurve(new Line(from, to), mat, context));
                }
            };

            tryAddLine(a, b);
            tryAddLine(b, c);
            tryAddLine(c, d);
            tryAddLine(d, a);
            tryAddLine(e, f);
            tryAddLine(f, g);
            tryAddLine(g, h);
            tryAddLine(h, e);
            tryAddLine(a, e);
            tryAddLine(b, f);
            tryAddLine(c, g);
            tryAddLine(d, h);

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

        /// <summary>
        /// Convert a Grid2d to a set of model curves.
        /// </summary>
        /// <param name="grid">The grid to convert.</param>
        /// <param name="context">An optional transform to apply to these curves.</param>
        /// <param name="material">An optional material to use for these curves.</param>
        public static IEnumerable<ModelCurve> ToModelCurves(this Elements.Spatial.Grid2d grid, Transform context = null, Material material = null)
        {
            var mat = material ?? BuiltInMaterials.Black;
            var xform = context ?? new Transform();
            return grid.GetCells().SelectMany(c => c.GetTrimmedCellGeometry()).Select(c => new ModelCurve(c, material, xform));
        }
    }
}