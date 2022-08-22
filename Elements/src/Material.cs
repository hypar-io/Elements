using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Elements.Geometry;
using Elements.Validators;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Elements
{
    /// <summary>
    /// A material with red, green, blue, alpha, and metallic factor components.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../Elements/test/MaterialTests.cs?name=example)]
    /// </example>
    public class Material : Element
    {
        /// <summary>
        /// The texture image.
        /// </summary>
        internal Image<Rgba32> _texture;

        /// <summary>
        /// The normal texture image.
        /// </summary>
        internal Image<Rgba32> _normalTexture;

        /// <summary>
        /// The emissive texture image.
        /// </summary>
        internal Image<Rgba32> _emissiveTexture;

        /// <summary>The material's color.</summary>
        [JsonProperty("Color", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public Geometry.Color Color { get; set; } = new Geometry.Color();

        /// <summary>The specular factor between 0.0 and 1.0.</summary>
        [JsonProperty("SpecularFactor", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Range(0.0D, 1.0D)]
        public double SpecularFactor { get; set; } = 0.1D;

        /// <summary>The glossiness factor between 0.0 and 1.0.</summary>
        [JsonProperty("GlossinessFactor", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Range(0.0D, 1.0D)]
        public double GlossinessFactor { get; set; } = 0.1D;

        /// <summary>Is this material affected by lights?</summary>
        [JsonProperty("Unlit", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool Unlit { get; set; } = false;

        /// <summary>A relative file path to an image file to be used as a texture.</summary>
        [JsonProperty("Texture", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Texture { get; set; }

        /// <summary>Is this material to be rendered from both sides?</summary>
        [JsonProperty("DoubleSided", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool DoubleSided { get; set; } = false;

        /// <summary>Should the texture be repeated? The RepeatTexture property determines whether textures are clamped in the [0,0]-&gt;[1,1] range or repeat continuously.</summary>
        [JsonProperty("RepeatTexture", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool RepeatTexture { get; set; } = true;

        /// <summary>A relative path to a jpg or png image file to be used as a normal texture.</summary>
        [JsonProperty("NormalTexture", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string NormalTexture { get; set; }

        /// <summary>Should the texture colors be interpolated between pixels? If false, renders hard pixels in the texture rather than fading between adjacent pixels.</summary>
        [JsonProperty("InterpolateTexture", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        public bool InterpolateTexture { get; set; } = true;

        /// <summary>A relative path to a jpg or png image file to be used as an emissive texture.</summary>
        [JsonProperty("EmissiveTexture", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string EmissiveTexture { get; set; }

        /// <summary>
        /// The scale, between 0.0 and 1.0, of the emissive texture's components.
        /// </summary>
        [JsonProperty("EmissiveFactor", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public double EmissiveFactor { get; set; }

        /// <summary>
        /// Should objects with this material be drawn in front of all other objects?
        /// </summary>
        [JsonProperty("Draw In Front", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool DrawInFront { get; set; } = false;

        /// <summary>
        /// If provided, this controls how curves and lines will be drawn in the 3D view for supported viewers. This will not affect mesh / solid-based elements.
        /// </summary>
        public EdgeDisplaySettings EdgeDisplaySettings { get; set; } = null;

        /// <summary>
        /// Construct a material.
        /// </summary>
        public Material()
        {
            this.Color = Colors.Gray;
        }

        /// <summary>
        /// Construct a material.
        /// </summary>
        /// <param name="color">The color component.</param>
        /// <param name="specularFactor">The specular component.</param>
        /// <param name="glossinessFactor">The glossiness factor.</param>
        /// <param name="unlit">Does this material have a constant color?</param>
        /// <param name="texture">A path to an image texture for texture mapping.</param>
        /// <param name="doubleSided">Is this material double sided?</param>
        /// <param name="repeatTexture">Does the texture repeat?</param>
        /// <param name="normalTexture">A path to an image texture for normal mapping.</param>
        /// <param name="interpolateTexture">Should the texture be interpolated?</param>
        /// <param name="emissiveTexture">A path to an emissive image texture.</param>
        /// <param name="emissiveFactor">The scale, between 0.0 and 1.0, of the emissive texture's components.</param>
        /// <param name="drawInFront">Should objects with this material be drawn in front of all other objects?</param>
        /// <param name="id">The id of the material.</param>
        /// <param name="name">The name of the material.</param>
        [JsonConstructor]
        public Material(Geometry.Color @color,
                        double @specularFactor,
                        double @glossinessFactor,
                        bool @unlit,
                        string @texture,
                        bool @doubleSided,
                        bool @repeatTexture,
                        string @normalTexture,
                        bool @interpolateTexture,
                        string @emissiveTexture,
                        double @emissiveFactor,
                        bool @drawInFront,
                        Guid @id = default,
                        string @name = null)
            : base(id, name)
        {
            if (texture != null)
            {
                // https://weblog.west-wind.com/posts/2019/Aug/20/UriAbsoluteUri-and-UrlEncoding-of-Local-File-Urls
                _texture = Image.CreateFromUri(ConstructUri(texture));
                if (_texture == null)
                {
                    throw new ArgumentException("No image data could be found for the specified texture path.");
                }
            }

            if (normalTexture != null)
            {
                _normalTexture = Image.CreateFromUri(ConstructUri(normalTexture));
                if (_normalTexture == null)
                {
                    throw new ArgumentException("No image data could be found for the specified normal texture path.");
                }
            }

            if (emissiveTexture != null)
            {
                _emissiveTexture = Image.CreateFromUri(ConstructUri(emissiveTexture));
                if (_emissiveTexture == null)
                {
                    throw new ArgumentException("No image data could be found for the specified emissive texture path.");
                }
            }

            if (!Validator.DisableValidationOnConstruction)
            {
                if (specularFactor < 0.0 || glossinessFactor < 0.0)
                {
                    throw new ArgumentOutOfRangeException("The material could not be created. Specular and glossiness values must be less greater than 0.0.");
                }

                if (specularFactor > 1.0 || glossinessFactor > 1.0)
                {
                    throw new ArgumentOutOfRangeException("The material could not be created. Color, specular, and glossiness values must be less than 1.0.");
                }
            }

            this.Color = @color;
            this.SpecularFactor = @specularFactor;
            this.GlossinessFactor = @glossinessFactor;
            this.Unlit = @unlit;
            this.Texture = @texture;
            this.DoubleSided = @doubleSided;
            this.RepeatTexture = @repeatTexture;
            this.NormalTexture = @normalTexture;
            this.InterpolateTexture = @interpolateTexture;
            this.EmissiveTexture = @emissiveTexture;
            this.EmissiveFactor = emissiveFactor;
            this.DrawInFront = drawInFront;
        }

        private Uri ConstructUri(string path)
        {
            if (path.StartsWith("https://") || path.StartsWith("http://") || path.StartsWith("file://"))
            {
                return new Uri(path);
            }
            else
            {
                return new Uri($"file://{Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), path)}");
            }
        }

        /// <summary>
        /// Construct a material.
        /// </summary>
        /// <param name="name">The name of the material.</param>
        /// <param name="id">The id of the material.</param>
        public Material(string name, Guid id = default) : base(id, name)
        {
            this.Color = Colors.Gray;
        }

        /// <summary>
        /// Construct a material.
        /// </summary>
        /// <param name="name">The identifier of the material. Identifiers should be unique within a model.</param>
        /// <param name="color">The RGBA color of the material.</param>
        /// <param name="specularFactor">The specular component of the color. Between 0.0 and 1.0.</param>
        /// <param name="glossinessFactor">The glossiness component of the color. Between 0.0 and 1.0.</param>
        /// <param name="texture">A path to a jpg or png image file to be used as a texture. The path will be
        /// converted to a uri. It can be a local file path, a path relative to the executing assembly, or a url.</param>
        /// <param name="unlit">Is this material affected by lights?</param>
        /// <param name="doubleSided">Is this material to be rendered from both sides?</param>
        /// <param name="repeatTexture">Should the texture be repeated? The RepeatTexture property determines whether textures are clamped in the [0,0]->[1,1] range or repeat continuously.</param>
        /// <param name="normalTexture">A relative path to a jpg or png image file to be used as a normal texture.</param>
        /// <param name="interpolateTexture">Should the texture colors be interpolated between pixels? If false, renders hard pixels in the texture rather than fading between adjacent pixels.</param>
        /// <param name="emissiveTexture">A relative path to a jpg or png image file to be used as en emissive texture.</param>
        /// <param name="emissiveFactor">The scale, between 0.0 and 1.0, of the emissive texture's components.</param>
        /// <param name="drawInFront">Should objects with this material be drawn in front of all other objects?</param>
        /// <param name="id">The id of the material.</param>
        /// <exception>Thrown when the specular or glossiness value is less than 0.0.</exception>
        /// <exception>Thrown when the specular or glossiness value is greater than 1.0.</exception>
        public Material(string name,
                        Geometry.Color color,
                        double specularFactor = 0.1,
                        double glossinessFactor = 0.1,
                        string texture = null,
                        bool unlit = false,
                        bool doubleSided = false,
                        bool repeatTexture = true,
                        string normalTexture = null,
                        bool interpolateTexture = true,
                        string emissiveTexture = null,
                        double emissiveFactor = 1.0,
                        bool drawInFront = false,
                        Guid id = default) :
            this(color,
                 specularFactor,
                 glossinessFactor,
                 unlit,
                 texture,
                 doubleSided,
                 repeatTexture,
                 normalTexture,
                 interpolateTexture,
                 emissiveTexture,
                 emissiveFactor,
                 drawInFront,
                 id != default ? id : Guid.NewGuid(),
                 name)
        { }

        /// <summary>
        /// Construct a material.
        /// </summary>
        /// <param name="name">The name of the material.</param>
        /// <param name="color">The color of the material.</param>
        /// <param name="specularFactor">The specular component of the color. Between 0.0 and 1.0.</param>
        /// <param name="glossinessFactor">The glossiness component of the color. Between 0.0 and 1.0.</param>
        /// <param name="texture">The texture image.</param>
        /// <param name="unlit">Is this material affected by lights?</param>
        /// <param name="doubleSided">Is this material to be rendered from both sides?</param>
        /// <param name="repeatTexture">Should the texture be repeated? The RepeatTexture property determines whether textures are clamped in the [0,0]->[1,1] range or repeat continuously.</param>
        /// <param name="normalTexture">The normal texture image.</param>
        /// <param name="interpolateTexture">Should the texture colors be interpolated between pixels? If false, renders hard pixels in the texture rather than fading between adjacent pixels.</param>
        /// <param name="emissiveTexture">The emissive texture image.</param>
        /// <param name="emissiveFactor">The scale, between 0.0 and 1.0, of the emissive texture's components.</param>
        /// <param name="drawInFront">Should objects with this material be drawn in front of all other objects?</param>
        /// <param name="id">The id of the material.</param>
        public Material(string name,
                        Geometry.Color color,
                        Image<Rgba32> texture,
                        double specularFactor = 0.1,
                        double glossinessFactor = 0.1,
                        bool unlit = false,
                        bool doubleSided = false,
                        bool repeatTexture = true,
                        Image<Rgba32> normalTexture = null,
                        bool interpolateTexture = true,
                        Image<Rgba32> emissiveTexture = null,
                        double emissiveFactor = 1.0,
                        bool drawInFront = false,
                        Guid id = default) :

            this(color,
                 specularFactor,
                 glossinessFactor,
                 unlit,
                 null,
                 doubleSided,
                 repeatTexture,
                 null,
                 interpolateTexture,
                 null,
                 emissiveFactor,
                 drawInFront,
                 id != default ? id : Guid.NewGuid(),
                 name)
        {
            _texture = texture;
            _normalTexture = normalTexture;
            _emissiveTexture = emissiveTexture;
        }

        public Material(string name,
                        Geometry.Color color,
                        byte[] texture,
                        double specularFactor = 0.1,
                        double glossinessFactor = 0.1,
                        bool unlit = false,
                        bool doubleSided = false,
                        bool repeatTexture = true,
                        byte[] normalTexture = null,
                        bool interpolateTexture = true,
                        byte[] emissiveTexture = null,
                        double emissiveFactor = 1.0,
                        bool drawInFront = false,
                        Guid id = default) :

            this(color,
                 specularFactor,
                 glossinessFactor,
                 unlit,
                 null,
                 doubleSided,
                 repeatTexture,
                 null,
                 interpolateTexture,
                 null,
                 emissiveFactor,
                 drawInFront,
                 id != default ? id : Guid.NewGuid(),
                 name)
        {
            _texture = Image.CreateFromBytes(texture);
            _normalTexture = Image.CreateFromBytes(normalTexture);
            _emissiveTexture = Image.CreateFromBytes(emissiveTexture);
        }

        /// <summary>
        /// Finalize the model text.
        /// </summary>
        ~Material()
        {
            if (_texture != null)
            {
                _texture.Dispose();
            }

            if (_normalTexture != null)
            {
                _normalTexture.Dispose();
            }

            if (_emissiveTexture != null)
            {
                _emissiveTexture.Dispose();
            }
        }

        /// <summary>
        /// Is this material equal to the provided material?
        /// </summary>
        /// <param name="obj"></param>
        public override bool Equals(object obj)
        {
            if (!(obj is Material m))
            {
                return false;
            }
            return this.Color.Equals(m.Color) && this.SpecularFactor == m.SpecularFactor && this.GlossinessFactor == m.GlossinessFactor && this.Name == m.Name;
        }

        /// <summary>
        /// Get the hash code for the material.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return new ArrayList() { this.Name, this.Color, this.SpecularFactor, this.GlossinessFactor }.GetHashCode();
        }
    }
}