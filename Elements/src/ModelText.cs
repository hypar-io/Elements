using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Elements.Geometry;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Elements
{
    /// <summary>
    /// Model text font sizes.
    /// </summary>
    public enum FontSize
    {
        /// <summary>
        /// 12 pt
        /// </summary>
        PT12 = 12,
        /// <summary>
        /// 24 pt
        /// </summary>
        PT24 = 24,
        /// <summary>
        /// 36 pt
        /// </summary>
        PT36 = 36
    }

    /// <summary>
    /// A collection of text tags which are visible in 3D.
    /// </summary>
    public class ModelText : MeshElement
    {
        private static Font _font12;
        private static Font _font24;
        private static Font _font36;
        private static string _labelsDirectory;
        private int _dpi = 72;
        private int _maxTextureSize = 2048;
        private List<(UV min, UV max, FontRectangle fontRect)> _textureAtlas;
        private DrawingOptions _options;
        private string _texturePath;

        /// <summary>
        /// A collection of text data objects which specify the location,
        /// direction, and content of the text.
        /// </summary>
        public IList<(Vector3 location, Vector3 direction, string text)> Texts { get; set; }

        /// <summary>
        /// The font size of the model text.
        /// </summary>
        public FontSize FontSize { get; set; }

        /// <summary>
        /// An additional scale to apply to the size of the text.
        /// Fonts will be drawn at the real world equivalent of 72 dpi at scale=1.0.
        /// </summary>
        public double Scale { get; set; }

        /// <summary>
        /// Create a set of text 
        /// </summary>
        /// <param name="texts">A collection of text data objects which specify the location,
        /// direction, and content of the text.</param>
        /// <param name="fontSize">The font size of the text.</param>
        /// <param name="scale">An additional scale to apply to the size of the text.
        /// Fonts will be drawn at the real world equivalent of 72 dpi at scale=1.0.</param>
        public ModelText(IList<(Vector3 location, Vector3 direction, string text)> texts, FontSize fontSize, double scale = 1.0)
        {
            this.Texts = texts != null ? texts : new List<(Vector3 location, Vector3 direction, string text)>();
            this.FontSize = fontSize;
            this.Scale = scale;

            Initialize();
            GenerateTextureAtlas();
            GenerateMesh();

            this.Material = new Material($"{Guid.NewGuid().ToString()}_texture_atlas", Elements.Geometry.Colors.White, 0.0, 0.0, texture: _texturePath, repeatTexture: false, unlit: true);
        }

        private void Initialize()
        {
            if (_font12 == null)
            {
                FontCollection collection = new FontCollection();
                FontFamily family = collection.Install("Fonts/Roboto-Medium.ttf");
                _font12 = family.CreateFont(12);
                _font24 = family.CreateFont(24);
                _font36 = family.CreateFont(36);
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

            _options = new DrawingOptions()
            {
                TextOptions = new TextOptions()
                {
                    // ApplyKerning = true,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    DpiX = _dpi,
                    DpiY = _dpi
                }
            };
        }

        private void GenerateTextureAtlas()
        {
            var font = _font12;
            switch (this.FontSize)
            {
                case FontSize.PT12:
                    font = _font12;
                    break;
                case FontSize.PT24:
                    font = _font24;
                    break;
                case FontSize.PT36:
                    font = _font36;
                    break;
            }
            var renderOptions = new RendererOptions(font, _dpi);

            this._textureAtlas = new List<(UV min, UV max, FontRectangle fontRect)>();

            var image = new Image<Rgba32>(this._maxTextureSize, this._maxTextureSize);

            var x = 0.0f;
            var y = 0.0f;

            _texturePath = Path.Combine(_labelsDirectory, $"{Guid.NewGuid()}.png");

            foreach (var t in this.Texts)
            {
                var fontRectangle = TextMeasurer.Measure(t.text, renderOptions);
                if (x + fontRectangle.Width > this._maxTextureSize)
                {
                    x = 0;
                    y += fontRectangle.Height;
                }

                if (y + fontRectangle.Height > this._maxTextureSize)
                {
                    throw new Exception("The model text could not be created. There is too much text. Try making multiple model texts.");
                    // Start a new texture;
                    // image.SaveAsPng(path);
                    // image.Dispose();

                    // image = new Image<Rgba32>(this._maxTextureSize, this._maxTextureSize);
                    // path = Path.Combine(_labelsDirectory, $"{Guid.NewGuid()}.png");

                    // x = 0.0f;
                    // y = 0.0f;
                }

                image.Mutate(o => o.DrawText(_options, t.text, font, SixLabors.ImageSharp.Color.Black, new PointF(x, y)));

                var minU = (x / this._maxTextureSize);
                var minV = 1 - (y / this._maxTextureSize);
                var maxU = ((x + fontRectangle.Width) / this._maxTextureSize);
                var maxV = 1 - ((y + fontRectangle.Height) / this._maxTextureSize);

                this._textureAtlas.Add((new UV(minU, minV), new UV(maxU, maxV), fontRectangle));

                x += fontRectangle.Width;
            }

            if (image != null)
            {
                image.Save(_texturePath);
                image.Dispose();
            }
        }

        private void GenerateMesh()
        {
            for (var i = 0; i < this.Texts.Count; i++)
            {
                var t = this.Texts[i];

                var td = t.direction.IsZero() ? Vector3.ZAxis : t.direction;
                var ta = this._textureAtlas[i];

                var sizeX = Units.InchesToMeters(ta.fontRect.Width / _dpi);
                var sizeY = Units.InchesToMeters(ta.fontRect.Height / _dpi);

                var tx = new Transform(t.location, td);

                var v1 = tx.OfPoint(new Vector3(-sizeX * this.Scale / 2, sizeY * this.Scale / 2));
                var v2 = tx.OfPoint(new Vector3(-sizeX * this.Scale / 2, -sizeY * this.Scale / 2));
                var v3 = tx.OfPoint(new Vector3(sizeX * this.Scale / 2, -sizeY * this.Scale / 2));
                var v4 = tx.OfPoint(new Vector3(sizeX * this.Scale / 2, sizeY * this.Scale / 2));

                var uv1 = new UV(ta.min.U, ta.min.V);
                var uv2 = new UV(ta.min.U, ta.max.V);
                var uv3 = new UV(ta.max.U, ta.max.V);
                var uv4 = new UV(ta.max.U, ta.min.V);
                // var uv1 = new UV(0, 0);
                // var uv2 = new UV(0, 1);
                // var uv3 = new UV(1, 1);
                // var uv4 = new UV(1, 0);

                var a = this.Mesh.AddVertex(v1, uv1);
                var b = this.Mesh.AddVertex(v2, uv2);
                var c = this.Mesh.AddVertex(v3, uv3);
                var d = this.Mesh.AddVertex(v4, uv4);

                var e = this.Mesh.AddVertex(v4, uv1);
                var f = this.Mesh.AddVertex(v3, uv2);
                var g = this.Mesh.AddVertex(v2, uv3);
                var h = this.Mesh.AddVertex(v1, uv4);

                this.Mesh.AddTriangle(a, b, c);
                this.Mesh.AddTriangle(a, c, d);
                this.Mesh.AddTriangle(e, f, g);
                this.Mesh.AddTriangle(e, g, h);
            }

            this.Mesh.ComputeNormals();
        }

    }
}