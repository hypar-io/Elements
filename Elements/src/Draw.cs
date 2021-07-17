using System.Collections.Generic;
using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// Utilities for drawing.
    /// </summary>
    public static class Draw
    {
        private static Mesh _cone;

        /// <summary>
        /// A small cube.
        /// </summary>
        /// <param name="location">The location of the center of the cube.</param>
        /// <param name="label">A label on the cube.</param>
        /// <param name="material">The cube's material.</param>
        /// <param name="size">The size of the marker.</param>
        public static Mass Cube(Vector3 location, string label, Material material, double size = 0.4)
        {
            return new Mass(Polygon.Rectangle(size, size), size, material, new Transform(location - new Vector3(0, 0, size / 2)), name: label);
        }

        /// <summary>
        /// An arrow along a curve.
        /// </summary>
        /// <param name="curve">The curve along which the arrows will be drawn.</param>
        /// <param name="material">The material to apply to the curve and the arrow head.</param>
        /// <param name="arrowWidth">The width of the arrow head.</param>
        /// <param name="arrowLength">The length of the arrow head.</param>
        /// <param name="arrowHeadAtStart">Should an arrow head be drawn at the start of the curve?</param>
        /// <param name="arrowHeadAtEnd">Should an arrow head be drawn at the end of the curve?</param>
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
                elements.Add(ArrowHead(t1.Origin, t1.ZAxis, material, arrowWidth, arrowLength));
            }

            if (arrowHeadAtEnd)
            {
                elements.Add(ArrowHead(t2.Origin, t2.ZAxis.Negate(), material, arrowWidth, arrowLength));
            }
            return elements;
        }

        private static MeshElement ArrowHead(Vector3 location,
                                                 Vector3 direction,
                                                 Material material,
                                                 double coneWidth = 0.1,
                                                 double coneHeight = 0.3)
        {
            if (_cone == null)
            {
                _cone = Cone(coneWidth, coneHeight);
            }

            return new MeshElement(_cone, material, new Transform(location, direction));
        }

        private static Mesh Cone(double coneWidth, double coneHeight)
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
            cone.ComputeNormals();

            return cone;
        }
    }
}