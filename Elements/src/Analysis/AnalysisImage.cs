using System;
using System.Collections.Generic;
using Elements.Geometry;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System.IO;
using System.Text.Json.Serialization;

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

        private BBox3 _perimBounds;
        private double _perimW;
        private double _perimH;
        private double _numPixelsX;
        private double _numPixelsY;
        private int _imgPixels;

        /// <summary>
        /// Create an analysis image.
        /// </summary>
        [JsonConstructor]
        public AnalysisImage()
        {

        }

        /// <summary>
        /// An AnalysisImage is similar to an AnalysisMesh in that it renders a mesh with analysis colors.
        /// However, it uses a mapped image texture rather than mesh vertex colors to lighten the resulting geometry.
        /// </summary>
        /// <param name="perimeter">The perimeter of the mesh image.</param>
        /// <param name="uLength">The number of divisions in the u direction.</param>
        /// <param name="vLength">The number of divisions in the v direction.</param>
        /// <param name="colorScale">The color scale to be used in the visualization.</param>
        /// <param name="analyze">A function which takes a location and computes a value.</param>
        /// <param name="id">The id of the analysis image.</param>
        /// <param name="name">The name of the analysis image.</param>
        /// <returns></returns>
        public AnalysisImage(Polygon perimeter,
                            double uLength,
                            double vLength,
                            ColorScale colorScale,
                            Func<Vector3, double> analyze,
                            Guid id = default(Guid),
                            string name = null) : base(perimeter, uLength, vLength, colorScale, analyze, id, name) { }

        /// <summary>
        /// Compute a value for each grid cell, and create the required material.
        /// </summary>
        public override void Analyze()
        {
            base.Analyze();

            _perimBounds = new BBox3(new[] { this.Perimeter });
            _perimW = _perimBounds.Max.X - _perimBounds.Min.X;
            _perimH = _perimBounds.Max.Y - _perimBounds.Min.Y;

            _numPixelsX = _perimW / ULength;
            _numPixelsY = _perimH / VLength;

            _imgPixels = Math.Max(NextPowerOfTwo(_numPixelsX), NextPowerOfTwo(_numPixelsY));

            var image = new Image<Rgba32>(_imgPixels, _imgPixels);

            foreach (var result in this._results)
            {
                var center = result.cell.Center();
                if (this.Perimeter.Contains(center))
                {
                    var vertexColor = this.ColorScale.GetColor(result.value);
                    var u = (result.cell.Min.X - _perimBounds.Min.X) / _perimW * (_numPixelsX / _imgPixels);
                    var v = (result.cell.Min.Y - _perimBounds.Min.Y) / _perimH * (_numPixelsY / _imgPixels);

                    var pX = (int)Math.Round(u * _imgPixels); // pixels in world coordinates
                    var pY = (int)Math.Round(v * _imgPixels); // pixels in world coordinates

                    var x = pX;
                    var y = image.Height - pY - 1; // flip pixel adddress for y: images start at top not bottom

                    x = Math.Max(0, x);
                    x = Math.Min(_imgPixels - 1, x);

                    y = Math.Max(0, y);
                    y = Math.Min(_imgPixels - 1, y);

                    var rgbaColor = new Rgba32((float)vertexColor.Red, (float)vertexColor.Green, (float)vertexColor.Blue, (float)vertexColor.Alpha);
                    image[x, y] = rgbaColor;

                    // Extend this color to the right
                    if (Math.Abs(result.cell.Max.X - _perimBounds.Max.X) < Vector3.EPSILON)
                    {
                        while (x < _imgPixels - 1)
                        {
                            image[x, y] = rgbaColor;
                            x++;
                        }
                    }

                    // Extend this color to the top
                    if (Math.Abs(result.cell.Max.Y - _perimBounds.Max.Y) < Vector3.EPSILON)
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

            this.Material = new Material($"Analysis_{Guid.NewGuid().ToString()}", Colors.White, 0, 0, imagePath, true, true, interpolateTexture: false, id: Guid.NewGuid());
        }

        /// <summary>
        /// Gives an element with a mapped texture.
        /// </summary>
        public override void Tessellate(ref Mesh mesh, Transform transform = null, Elements.Geometry.Color color = default(Elements.Geometry.Color))
        {
            var meshVertices = new List<Vertex>();

            var i = 0;

            while (i < this.Perimeter.Vertices.Count)
            {
                var v = this.Perimeter.Vertices[i];
                var uv = new UV((v.X - _perimBounds.Min.X) / _perimW * (_numPixelsX / _imgPixels), (v.Y - _perimBounds.Min.Y) / _perimH * (_numPixelsX / _imgPixels));
                meshVertices.Add(mesh.AddVertex(v, uv));

                if (i >= 2)
                {
                    mesh.AddTriangle(new Triangle(meshVertices[0], meshVertices[i - 1], meshVertices[i]));
                }
                i++;
            }
        }

    }
}