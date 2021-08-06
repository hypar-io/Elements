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
        /// Draw text at the specified location.
        /// </summary>
        /// <param name="text">The text to draw.</param>
        /// <param name="location">The center point of a square mesh that will contain the text.</param>
        /// <param name="direction">The facing direction of the text</param>
        /// <param name="size">The side length of the square mesh that will contain the text.</param>
        public static MeshElement Text(string text, Vector3 location, Vector3 direction, double size = 1.0)
        {
            if (direction.IsZero())
            {
                direction = Vector3.ZAxis;
            }

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
                var v1 = new Vector3(-size / 2, size / 2);
                var v2 = new Vector3(-size / 2, -size / 2);
                var v3 = new Vector3(size / 2, -size / 2);
                var v4 = new Vector3(size / 2, size / 2);
                var a = mesh.AddVertex(v1, new UV(0, 1));
                var b = mesh.AddVertex(v2, new UV(0, 0));
                var c = mesh.AddVertex(v3, new UV(1, 0));
                var d = mesh.AddVertex(v4, new UV(1, 1));
                var e = mesh.AddVertex(v4, new UV(0, 1));
                var f = mesh.AddVertex(v3, new UV(0, 0));
                var g = mesh.AddVertex(v2, new UV(1, 0));
                var h = mesh.AddVertex(v1, new UV(1, 1));
                mesh.AddTriangle(a, b, c);
                mesh.AddTriangle(a, c, d);
                mesh.AddTriangle(e, f, g);
                mesh.AddTriangle(e, g, h);
                mesh.ComputeNormals();
                panel = new MeshElement(mesh, m, new Transform(location, direction));
            }
            return panel;
        }
    }
}