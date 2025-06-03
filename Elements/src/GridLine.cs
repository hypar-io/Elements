using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Elements.Geometry;
using Newtonsoft.Json;

namespace Elements
{
    /// <summary>
    /// An architectural or structural gridline.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/GridLineTests.cs?name=example)]
    /// </example>
    // TODO: remove IDeserializationCallback implementation once we remove the Line and Geometry properties.
    // Remove its usage from JsonInheritanceConverter..
    [JsonConverter(typeof(Serialization.JSON.JsonInheritanceConverter), "discriminator")]
    public class GridLine : GeometricElement, IDeserializationCallback
    {
        // TODO: Remove this flag once we remove the Line and Geometry properties.
        private bool deserializationIsInProgress = false;

        /// <summary>
        /// Initializes a new instance of the GridLine class.
        /// </summary>
        public GridLine() : base()
        {
            deserializationIsInProgress = false;
        }

        // TODO: Remove this constructor once we remove the Line and Geometry properties.
        [JsonConstructor]
        private GridLine(BoundedCurve curve, Polyline geometry, Line line,
        Transform @transform = null, Material @material = null, Representation @representation = null, bool @isElementDefinition = false, System.Guid @id = default, string @name = null)
            : base(@transform, @material, @representation, @isElementDefinition, @id, @name)
        {
            deserializationIsInProgress = true;
            if (curve != null)
            {
                Curve = curve;
            }
            else if (geometry != null)
            {
                Geometry = geometry;
            }
            else
            {
                Line = line;
            }
        }

        /// <summary>
        /// Line that runs from the start of the gridline to its end.
        /// </summary>
        [Obsolete("We now use 'Curve' instead.")]
        public Line Line
        {
            get { return this.Curve as Line; }
            set
            {
                if (!deserializationIsInProgress)
                {
                    this.Curve = value;
                }
            }
        }

        /// <summary>
        /// The polyline that runs from the start of the gridline to its end.
        /// </summary>
        [Obsolete("We now use 'Curve' instead.")]
        public Polyline Geometry
        {
            get { return this.Curve as Polyline; }
            set
            {
                if (!deserializationIsInProgress)
                {
                    this.Curve = value;
                }
            }
        }


        /// <summary>
        /// Curve that runs from the start of the gridline to its end.
        /// </summary>
        public BoundedCurve Curve { get; set; }

        /// <summary>
        /// Radius of the gridline head.
        /// </summary>
        public double Radius = 1;

        /// <summary>
        /// How far to extend the gridline from the beginning to the start of the circle.
        /// </summary>
        public double ExtensionBeginning = 1;

        /// <summary>
        /// How far to extend the gridline past the end of the circle.
        /// </summary>
        public double ExtensionEnd = 1;

        /// <summary>
        /// Get graphics buffers and other metadata required to modify a GLB.
        /// </summary>
        /// <returns>
        /// True if there is graphicsbuffers data applicable to add, false otherwise.
        /// Out variables should be ignored if the return value is false.
        /// </returns>
        public override Boolean TryToGraphicsBuffers(out List<GraphicsBuffers> graphicsBuffers, out string id, out glTFLoader.Schema.MeshPrimitive.ModeEnum? mode)
        {
            if (this.Curve == null)
            {
                return base.TryToGraphicsBuffers(out graphicsBuffers, out id, out mode);
            }

            id = $"{this.Id}_gridline";
            mode = glTFLoader.Schema.MeshPrimitive.ModeEnum.LINE_STRIP;
            graphicsBuffers = new List<GraphicsBuffers>();

            var renderVertices = new List<Vector3>();

            var start = GetPointAndDirectionAt(this.Curve.Domain.Min);
            var end = GetPointAndDirectionAt(this.Curve.Domain.Max);

            var circleVertexTransform = GetCircleTransform();
            var circle = new Arc(circleVertexTransform, Radius);

            renderVertices.AddRange(circle.RenderVertices());

            if (ExtensionBeginning > 0)
            {
                renderVertices.Add(start.point - start.dir * ExtensionBeginning);
            }

            renderVertices.AddRange(this.Curve.RenderVertices());

            if (ExtensionEnd > 0)
            {
                renderVertices.Add(end.point + end.dir * ExtensionEnd);
            }

            graphicsBuffers.Add(renderVertices.ToGraphicsBuffers());
            return true;
        }

        /// <summary>
        /// Get normal and direction at a point on the curve. The direction is the invert of TransformAt's Z Axis.
        /// </summary>
        private (Vector3 point, Vector3 dir) GetPointAndDirectionAt(double u)
        {
            var transform = this.Curve.TransformAt(u);
            var point = transform.Origin;
            var dir = transform.ZAxis;
            return (point, dir.Negate());
        }

        /// <summary>
        /// Get the transform of the circle created by the gridline.
        /// </summary>
        /// <returns></returns>
        public Transform GetCircleTransform()
        {
            var start = GetPointAndDirectionAt(this.Curve.Domain.Min);
            var normal = Vector3.ZAxis;
            if (normal.Dot(start.dir) > 1 - Vector3.EPSILON)
            {
                normal = Vector3.XAxis;
            }
            var circleCenter = start.point - start.dir * (ExtensionBeginning + Radius);
            var circleVertexTransform = new Transform(circleCenter, start.dir, normal);
            return circleVertexTransform;
        }

        /// <summary>
        /// Add gridline's text data to a text collection for insertion into ModelText.
        /// </summary>
        /// <param name="texts">Collection of texts to add to.</param>
        /// <param name="color">Color for this text.</param>
        public void AddTextToCollection(List<(Vector3 location, Vector3 facingDirection, Vector3 lineDirection, string text, Color? color)> texts, Color? color = null)
        {
            var circleCenter = this.GetCircleTransform();
            texts.Add((circleCenter.Origin, circleCenter.ZAxis, circleCenter.XAxis, this.Name, color));
        }

        /// <summary>
        /// This method is called after the object has been deserialized.
        /// </summary>
        public void OnDeserialization(object sender)
        {
            deserializationIsInProgress = false;
        }
    }
}