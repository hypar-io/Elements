using System;
using System.Collections.Generic;
using Elements.Geometry;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System.IO;

namespace Elements.Analysis
{
    /// <summary>
    /// A visualization of computed values at locations in space.
    /// Use this instead of AnalysisMesh to create a lightweight mesh with an image texture,
    /// rather than mesh faces for each pixel.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/AnalysisImageTests.cs?name=example)]
    /// </example>
    public class AnalysisImage : AnalysisMesh
    {
        // https://stackoverflow.com/questions/466204/rounding-up-to-next-power-of-2
        private static int NextPowerOfTwo(double num)
        {
            var v = (long)num;
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return (int)v;
        }

        /// <summary>
        /// Should the texture be interpolated?
        /// False by default.
        /// </summary>
        /// <value>If false, renders hard pixels in the texture rather than fading between adjacent pixels.</value>
        public bool InterpolateTexture { get; set; } = false;

        /// <summary>
        /// An AnalysisImage is similar to an AnalysisMesh in that it renders a mesh with analysis colors.
        /// However, it uses a mapped image texture rather than mesh vertex colors to lighten the resulting geometry.
        /// </summary>
        /// <param name="perimeter"></param>
        /// <param name="uLength"></param>
        /// <param name="vLength"></param>
        /// <param name="colorScale"></param>
        /// <param name="analyze"></param>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public AnalysisImage(Polygon perimeter,
                            double uLength,
                            double vLength,
                            ColorScale colorScale,
                            Func<Vector3, double> analyze,
                            Guid id = default(Guid),
                            string name = null) : base(perimeter, uLength, vLength, colorScale, analyze, id, name) { }

        /// <summary>
        /// Gives an element with a mapped texture.
        /// </summary>
        public MeshElement GetMeshElement()
        {
            var mesh = new Mesh();
            var perimBounds = new BBox3(new[] { this.Perimeter });
            var perimW = perimBounds.Max.X - perimBounds.Min.X;
            var perimH = perimBounds.Max.Y - perimBounds.Min.Y;

            var numPixelsX = perimW / ULength;
            var numPixelsY = perimH / VLength;

            var imgPixels = Math.Max(NextPowerOfTwo(numPixelsX), NextPowerOfTwo(numPixelsY));

            var image = new Image<Rgba32>(imgPixels, imgPixels);

            foreach (var result in this._results)
            {
                var center = result.cell.Center();
                if (this.Perimeter.Contains(center))
                {
                    var vertexColor = this.ColorScale.GetColor(result.value);
                    var u = (result.cell.Min.X - perimBounds.Min.X) / perimW * (numPixelsX / imgPixels);
                    var v = (result.cell.Min.Y - perimBounds.Min.Y) / perimH * (numPixelsY / imgPixels);

                    var pX = (int)Math.Round(u * imgPixels); // pixels in world coordinates
                    var pY = (int)Math.Round(v * imgPixels); // pixels in world coordinates

                    var x = pX;
                    var y = image.Height - pY - 1; // flip pixel adddress for y: images start at top not bottom

                    x = Math.Max(0, x);
                    x = Math.Min(imgPixels - 1, x);

                    y = Math.Max(0, y);
                    y = Math.Min(imgPixels - 1, y);

                    var rgbaColor = new Rgba32((float)vertexColor.Red, (float)vertexColor.Green, (float)vertexColor.Blue, (float)vertexColor.Alpha);
                    image[x, y] = rgbaColor;

                    // Extend this color to the right
                    if (Math.Abs(result.cell.Max.X - perimBounds.Max.X) < Vector3.EPSILON)
                    {
                        while (x < imgPixels - 1)
                        {
                            image[x, y] = rgbaColor;
                            x++;
                        }
                    }

                    // Extend this color to the top
                    if (Math.Abs(result.cell.Max.Y - perimBounds.Max.Y) < Vector3.EPSILON)
                    {
                        while (y >= 0)
                        {
                            image[x, y] = rgbaColor;
                            y--;
                        }
                    }
                }
            }

            var imagePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
            image.Save(imagePath);

            var material = new Material($"Analysis_{Guid.NewGuid().ToString()}", Colors.White, 0, 0, null, true, true, Guid.NewGuid());

            material.Texture = imagePath;
            material.InterpolateTexture = this.InterpolateTexture;

            var meshVertices = new List<Vertex>();

            var i = 0;

            while (i < this.Perimeter.Vertices.Count)
            {
                var v = this.Perimeter.Vertices[i];
                var uv = new UV((v.X - perimBounds.Min.X) / perimW * (numPixelsX / imgPixels), (v.Y - perimBounds.Min.Y) / perimH * (numPixelsX / imgPixels));
                meshVertices.Add(mesh.AddVertex(v, uv));

                if (i >= 2)
                {
                    mesh.AddTriangle(new Triangle(meshVertices[0], meshVertices[i - 1], meshVertices[i]));
                }
                i++;
            }

            return new MeshElement(mesh, material);
        }

    }
}