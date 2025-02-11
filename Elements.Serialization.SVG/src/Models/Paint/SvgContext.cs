using Elements.Geometry;
using System;
using Svg;
using System.Linq;

namespace Elements.Serialization.SVG
{
    /// <summary>
    /// The style and color information about how to draw geometries and text.
    /// </summary>
    public class SvgContext
    {
        /// <summary>
        /// Initializes a new instance of SvgContext class.
        /// </summary>
        public SvgContext() : this(Colors.White, Colors.Black, 0.01)
        {
        }

        /// <summary>
        /// Initializes a new instance of SvgContext class.
        /// </summary>
        /// <param name="strokeColor">The paint's stroke color.</param>
        /// <param name="strokeWidth">The paint's stroke width.</param>
        /// <param name="dashIntervals">The definition of the dash pattern via an even number of entries.</param>
        public SvgContext(Color strokeColor, double strokeWidth, double[] dashIntervals) : this(Colors.White, strokeColor, strokeWidth)
        {
            ElementStroke.CreateDash(dashIntervals, 0);
            // TODO: delete
            _strokeDashArray = new SvgUnitCollection();
            foreach (var interval in dashIntervals)
            {
                _strokeDashArray.Add(new SvgUnit(SvgUnitType.User, (float)interval));
            }
        }

        /// <summary>
        /// Initializes a new instance of SvgContext class.
        /// </summary>
        /// <param name="strokeColor">The paint's stroke color.</param>
        /// <param name="strokeWidth">The paint's stroke width.</param>
        public SvgContext(Color strokeColor, double strokeWidth) : this(Colors.White, strokeColor, strokeWidth) { }

        /// <summary>
        /// Initializes a new instance of SvgContext class.
        /// </summary>
        /// <param name="color">The paint's foreground color.</param>
        /// <param name="strokeColor">The paint's stroke color.</param>
        /// <param name="strokeWidth">The paint's stroke width.</param>
        public SvgContext(Color color, Color strokeColor, double strokeWidth)
        {
            _elementStroke = new SvgStroke();
            _font = new SvgFont("Arial");

            Color = color;
            ElementStroke.Color = strokeColor;
            ElementStroke.Width = strokeWidth;
            // TODO: delete
            _stroke = new SvgColourServer(ToDrawingColor(strokeColor));
            _strokeWidth = new SvgUnit(SvgUnitType.User, (float)strokeWidth);
        }

        /// <summary>
        /// Initializes a new instance of SvgContext class.
        /// </summary>
        /// <param name="fontFamily">The font family name.</param>
        /// <param name="fontSize">The font size.</param>
        public SvgContext(string fontFamily, double fontSize) : this(Colors.White, Colors.Black, 0)
        {
            Font.FamilyName = fontFamily;
            Font.Size = fontSize;
        }

        /// <summary>
        /// Initializes a new instance of SvgContext class.
        /// </summary>
        /// <param name="color">The paint's foreground color.</param>
        /// <param name="strokeColor">The paint's stroke color.</param>
        /// <param name="strokeWidth">The paint's stroke width.</param>
        public SvgContext(System.Drawing.Color color, System.Drawing.Color strokeColor, double strokeWidth) :
        this(new Color(color), new Color(strokeColor), strokeWidth)
        { }

        /// <summary>
        /// Initializes a new instance of SvgContext class.
        /// </summary>
        /// <param name="strokeColor">The paint's stroke color.</param>
        /// <param name="strokeWidth">The paint's stroke width.</param>
        public SvgContext(System.Drawing.Color strokeColor, double strokeWidth) :
        this(new Color(strokeColor), strokeWidth)
        { }

        /// <summary>
        /// Initializes a new instance of SvgContext class.
        /// </summary>
        /// <param name="strokeColor">The paint's stroke color.</param>
        /// <param name="strokeWidth">The paint's stroke width.</param>
        /// <param name="dashIntervals">The definition of the dash pattern via an even number of entries.</param>
        public SvgContext(System.Drawing.Color strokeColor, double strokeWidth, double[] dashIntervals) :
        this(new Color(strokeColor), strokeWidth, dashIntervals)
        { }


        [Obsolete("Fill is deprecated, please use Color instead.")]
        public SvgColourServer? Fill
        {
            get { return _colourServer; }
            set
            {
                _colourServer = value;
                _color = value == null ? null : new Color(value.Colour);
            }
        }

        [Obsolete("Stroke is deprecated, please use ElementStroke.Color instead.")]
        public SvgColourServer? Stroke
        {
            get { return _stroke; }
            set
            {
                if (value == null)
                {
                    _stroke = null;
                    _elementStroke.Color = null;
                }
                else
                {
                    _stroke = value;
                    _elementStroke.Color = new Color(value.Colour);
                }
            }
        }

        [Obsolete("StrokeWidth is deprecated, please use ElementStroke.Width instead.")]
        public SvgUnit StrokeWidth
        {
            get { return _strokeWidth; }
            set
            {
                _strokeWidth = value;
                _elementStroke.Width = value.Value;
            }
        }

        [Obsolete("StrokeDashArray is deprecated, please use ElementStroke.CreateDash instead.")]
        public SvgUnitCollection? StrokeDashArray
        {
            get { return _strokeDashArray; }
            set
            {
                if (value == null)
                {
                    _strokeDashArray = null;
                    _elementStroke.DeleteDash();
                }
                else
                {
                    _strokeDashArray = value;
                    var intervals = _strokeDashArray.Select(ar => (double)ar.Value);
                    _elementStroke.CreateDash(intervals.ToArray(), 0);
                }
            }
        }

        /// <summary>
        /// The paint's foreground color.
        /// </summary>
        public Color? Color
        {
            get { return _color; }
            set
            {
                _color = value;
                _colourServer = _color.HasValue ? new SvgColourServer(ToDrawingColor(_color.Value)) : null;
            }
        }

        /// <summary>
        /// The stroke style information.
        /// </summary>
        public SvgStroke ElementStroke => _elementStroke;

        /// <summary>
        /// The font style information.
        /// </summary>
        public SvgFont Font => _font;

        private static System.Drawing.Color ToDrawingColor(Geometry.Color color)
        {
            return System.Drawing.Color.FromArgb((int)(color.Alpha * 255), (int)(color.Red * 255), (int)(color.Green * 255), (int)(color.Blue * 255));
        }

        private Color? _color;
        private readonly SvgStroke _elementStroke;
        private SvgFont _font;

        // TODO: delete
        private SvgColourServer? _colourServer;
        private SvgColourServer? _stroke;
        private SvgUnit _strokeWidth = new SvgUnit(SvgUnitType.User, 0.01f);
        private SvgUnitCollection? _strokeDashArray;
    }
}