using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
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
        PT36 = 36,
        /// <summary>
        /// 48 pt
        /// </summary>
        PT48 = 48,
        /// <summary>
        /// 60 pt
        /// </summary>
        PT60 = 60,
        /// <summary>
        /// 72 pt
        /// </summary>
        PT72 = 72
    }

    /// <summary>
    /// A collection of text tags which are visible in 3D.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/ModelTextTests.cs?name=example)]
    /// </example>
    public class ModelText : MeshElement
    {
        private static Font _font12;
        private static Font _font24;
        private static Font _font36;
        private static Font _font48;
        private static Font _font60;
        private static Font _font72;
        private static string _labelsDirectory;
        private int _dpi = 72;
        private int _maxTextureSize = 2048;
        private List<(UV min, UV max, FontRectangle fontRect)> _textureAtlas;
        private DrawingOptions _options;
        private string _texturePath;

        /// <summary>
        /// A collection of text data objects which specify the location,
        /// direction, content, and color of the text.
        /// </summary>
        public IList<(Vector3 location, Vector3 facingDirection, Vector3 lineDirection, string text, Geometry.Color? color)> Texts { get; set; }

        /// <summary>
        /// The font size of the model text.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
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
        public ModelText(IList<(Vector3 location, Vector3 facingDirection, Vector3 lineDirection, string text, Geometry.Color? color)> texts,
                         FontSize fontSize,
                         double scale = 10.0)
        {
            this.Texts = texts != null ? texts : new List<(Vector3 location, Vector3 facingDirection, Vector3 lineDirection, string text, Geometry.Color? color)>();
            this.FontSize = fontSize;
            this.Scale = scale;

            Initialize();
            GenerateTextureAtlas();
            GenerateMesh();

            this.Material = new Material($"{Guid.NewGuid()}_texture_atlas",
                                         Elements.Geometry.Colors.White,
                                         0.0,
                                         0.0,
                                         texture: _texturePath,
                                         repeatTexture: false,
                                         unlit: true);
        }

        private void Initialize()
        {
            if (_font12 == null)
            {
                var asmDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var fontPath = Path.Combine(asmDir, "Fonts/Roboto-Medium.ttf");
                var collection = new FontCollection();
                var family = collection.Install(fontPath);
                _font12 = family.CreateFont(12);
                _font24 = family.CreateFont(24);
                _font36 = family.CreateFont(36);
                _font48 = family.CreateFont(48);
                _font60 = family.CreateFont(60);
                _font72 = family.CreateFont(72);
            }

            if (_labelsDirectory == null)
            {
                var tempDir = Path.GetTempPath();
                _labelsDirectory = Path.Combine(tempDir, $"Labels");
                if (!Directory.Exists(_labelsDirectory))
                {
                    Directory.CreateDirectory(_labelsDirectory);
                }
            }

            _options = new DrawingOptions()
            {
                TextOptions = new TextOptions()
                {
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
                case FontSize.PT48:
                    font = _font48;
                    break;
                case FontSize.PT60:
                    font = _font60;
                    break;
                case FontSize.PT72:
                    font = _font72;
                    break;
            }
            var renderOptions = new RendererOptions(font, _dpi);

            this._textureAtlas = new List<(UV min, UV max, FontRectangle fontRect)>();

            var image = new Image<Rgba32>(this._maxTextureSize, this._maxTextureSize);

            var x = 0.0f;
            var y = 0.0f;

            _texturePath = Path.Combine(_labelsDirectory, $"{Guid.NewGuid()}.png");

            var textureLookup = new Dictionary<int, (UV min, UV max, FontRectangle fontRect)>();

            foreach (var t in this.Texts)
            {
                // TODO: Investigate why System.HashCode isn't available.
                // https://stackoverflow.com/questions/892618/create-a-hashcode-of-two-numbers/16651358#16651358
                int hash = 23;
                hash = hash * 31 + t.text.GetHashCode();
                hash = hash * 31 + t.color.GetHashCode();

                if (textureLookup.ContainsKey(hash))
                {
                    this._textureAtlas.Add(textureLookup[hash]);
                    continue;
                }

                var fontRectangle = TextMeasurer.Measure(t.text, renderOptions);
                if (x + fontRectangle.Width > this._maxTextureSize)
                {
                    x = 0;
                    y += fontRectangle.Height;
                }

                if (y + fontRectangle.Height > this._maxTextureSize)
                {
                    throw new Exception("The model text could not be created. There is too much text. Try making multiple model texts.");

                    // TODO: Instead of throwing an exception, we could do the
                    // user a favor and just start a new texture. This makes the 
                    // element have multiple materials however and would need
                    // to be handled as a special case.
                }

                var color = t.color != null ? new SixLabors.ImageSharp.Color(new Rgba32((float)t.color.Value.Red, (float)t.color.Value.Green, (float)t.color.Value.Blue)) : SixLabors.ImageSharp.Color.Black;
                image.Mutate(o => o.DrawText(_options, t.text, font, color, new PointF(x, y)));

                var minU = (x / this._maxTextureSize);
                var minV = 1 - (y / this._maxTextureSize);
                var maxU = ((x + fontRectangle.Width) / this._maxTextureSize);
                var maxV = 1 - ((y + fontRectangle.Height) / this._maxTextureSize);

                var textureData = (new UV(minU, minV), new UV(maxU, maxV), fontRectangle);
                this._textureAtlas.Add(textureData);
                textureLookup.Add(hash, textureData);

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

                var td = t.facingDirection.IsZero() ? Vector3.ZAxis : t.facingDirection;
                var ta = this._textureAtlas[i];

                var sizeX = Units.InchesToMeters(ta.fontRect.Width / _dpi);
                var sizeY = Units.InchesToMeters(ta.fontRect.Height / _dpi);

                var tx = new Transform(t.location, t.lineDirection, td);

                var v1 = tx.OfPoint(new Vector3(-sizeX * this.Scale / 2, sizeY * this.Scale / 2));
                var v2 = tx.OfPoint(new Vector3(-sizeX * this.Scale / 2, -sizeY * this.Scale / 2));
                var v3 = tx.OfPoint(new Vector3(sizeX * this.Scale / 2, -sizeY * this.Scale / 2));
                var v4 = tx.OfPoint(new Vector3(sizeX * this.Scale / 2, sizeY * this.Scale / 2));

                var uv1 = new UV(ta.min.U, ta.min.V);
                var uv2 = new UV(ta.min.U, ta.max.V);
                var uv3 = new UV(ta.max.U, ta.max.V);
                var uv4 = new UV(ta.max.U, ta.min.V);

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