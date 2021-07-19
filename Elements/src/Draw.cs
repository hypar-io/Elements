using System.Collections.Generic;
using Elements.Geometry;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Fonts;
using System;
using System.Reflection;
using System.IO;

namespace Elements
{
    /// <summary>
    /// Utilities for drawing.
    /// </summary>
    public static class Draw
    {
        private static Mesh _cone;
        private static Font _font;
        private static string _labelsDirectory;

        /// <summary>
        /// Draw a small cube.
        /// </summary>
        /// <param name="location">The location of the center of the cube.</param>
        /// <param name="label">A label on the cube.</param>
        /// <param name="material">The cube's material.</param>
        /// <param name="size">The size of the marker.</param>
        public static Mass Cube(Vector3 location, string label, Material material, double size = 0.4)
        {
            return new Mass(Geometry.Polygon.Rectangle(size, size), size, material, new Transform(location - new Vector3(0, 0, size / 2)), name: label);
        }

        /// <summary>
        /// Draw an arrow along a curve.
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

        /// <summary>
        /// Draw text at the specified location.
        /// </summary>
        /// <param name="text">The text to draw.</param>
        /// <param name="location">The center point of a square mesh that will contain the text.</param>
        /// <param name="direction">The facing direction of the text</param>
        /// <param name="size">The side length of the square mesh that will contain the text.</param>
        public static MeshElement Text(string text, Vector3 location, Vector3 direction, double size = 1.0)
        {
            if (_font == null)
            {
                FontCollection collection = new FontCollection();
                FontFamily family = collection.Install("Fonts/Roboto-Medium.ttf");
                _font = family.CreateFont(36);
            }

            if (_labelsDirectory == null)
            {
                var asmDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                _labelsDirectory = Path.Combine(asmDir, $"Labels");
                if (!Directory.Exists(_labelsDirectory))
                {
                    Directory.CreateDirectory(_labelsDirectory);
                }
            }

            var id = Guid.NewGuid();
            MeshElement panel;
            var width = 256;
            var height = 256;

            using (var image = new Image<Rgba32>(width, height))
            {
                var options = new DrawingOptions()
                {
                    TextOptions = new TextOptions()
                    {
                        ApplyKerning = true,
                        TabWidth = 8,
                        WrapTextWidth = width,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };

                image.Mutate(x => x.DrawText(options, text, _font, SixLabors.ImageSharp.Color.Black, new PointF(0, height / 2)));

                var path = Path.Combine(_labelsDirectory, $"{id}.png");
                image.SaveAsPng(path);

                var m = new Material(id.ToString(), Elements.Geometry.Colors.White, 0.0, 0.0, texture: path, repeatTexture: false, unlit: true);
                var mesh = new Mesh();
                var a = mesh.AddVertex(new Vector3(-size / 2, size / 2), new UV(0, 1));
                var b = mesh.AddVertex(new Vector3(-size / 2, -size / 2), new UV(0, 0));
                var c = mesh.AddVertex(new Vector3(size / 2, -size / 2), new UV(1, 0));
                var d = mesh.AddVertex(new Vector3(size / 2, size / 2), new UV(1, 1));
                mesh.AddTriangle(a, b, c);
                mesh.AddTriangle(a, c, d);
                mesh.ComputeNormals();
                panel = new MeshElement(mesh, m, new Transform(location, direction));
            }
            return panel;
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