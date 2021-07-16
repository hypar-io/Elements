using System.Collections.Generic;
using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// Utilities for drawing.
    /// </summary>
    public static class Draw
    {
        private static MeshElement _arrowDefinition;

        /// <summary>
        /// A small cube marker.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="label"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Mass Marker(Vector3 location, string label, double size = 0.4)
        {
            return new Mass(Polygon.Rectangle(size, size), size, BuiltInMaterials.YAxis, new Transform(location - new Vector3(0, 0, size / 2)), name: label);
        }

        /// <summary>
        /// An arrow along a curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="material"></param>
        /// <param name="arrowWidth"></param>
        /// <param name="arrowLength"></param>
        /// <param name="arrowHeadAtStart"></param>
        /// <param name="arrowHeadAtEnd"></param>
        /// <returns></returns>
        public static List<Element> Arrow(Curve curve,
                                          Material material,
                                          double arrowWidth = 0.1,
                                          double arrowLength = 0.3,
                                          bool arrowHeadAtStart = false,
                                          bool arrowHeadAtEnd = true)
        {
            var t1 = curve.TransformAt(0.0);
            var t2 = curve.TransformAt(1.0);

            var elements = new List<Element>();
            elements.Add(new ModelCurve(curve, material));

            if (arrowHeadAtStart)
            {
                elements.Add(ArrowHead(t1.Origin, t1.ZAxis, arrowWidth, arrowLength));
            }

            if (arrowHeadAtEnd)
            {
                elements.Add(ArrowHead(t2.Origin, t2.ZAxis.Negate(), arrowWidth, arrowLength));
            }
            return elements;
        }

        private static ElementInstance ArrowHead(Vector3 location,
                                                 Vector3 direction,
                                                 double coneWidth = 0.1,
                                                 double coneHeight = 0.3)
        {
            if (_arrowDefinition == null)
            {
                var cone = new Mesh();
                var vertices = new List<Vertex> {
                    new Vertex(new Vector3(-coneWidth, -coneWidth)),
                    new Vertex(new Vector3(coneWidth, -coneWidth)),
                    new Vertex(new Vector3(coneWidth, coneWidth)),
                    new Vertex(new Vector3(-coneWidth, coneWidth)),
                    new Vertex(new Vector3(0, 0, coneHeight))
                };
                vertices.ForEach((v) => cone.AddVertex(v));
                cone.AddTriangle(vertices[0], vertices[1], vertices[4]);
                cone.AddTriangle(vertices[1], vertices[2], vertices[4]);
                cone.AddTriangle(vertices[2], vertices[3], vertices[4]);
                cone.AddTriangle(vertices[3], vertices[0], vertices[4]);
                _arrowDefinition = new MeshElement(cone, BuiltInMaterials.ZAxis)
                {
                    IsElementDefinition = true
                };
                cone.ComputeNormals();
            }

            return _arrowDefinition.CreateInstance(new Transform(location, direction), null);
        }
    }
}