using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// Built in materials.
    /// </summary>
    public static class BuiltInMaterials
    {
        private static Material _glass = new Material("glass", new Color(1.0f, 1.0f, 1.0f, 0.2f), 1.0f, 1.0f);
        private static Material _steel = new Material("steel", new Color(0.6f, 0.5f, 0.5f, 1.0f), 0.0f, 0.0f);
        private static Material _default = new Material("default", new Color(1.0f, 1.0f, 1.0f, 1.0f), 0.0f, 0.0f);
        private static Material _concrete = new Material("concrete", new Color(0.5f,0.5f,0.5f,1.0f), 0.0f, 0.0f);
        private static Material _mass = new Material("mass", new Color(0.5f, 0.5f, 1.0f, 0.2f), 0.0f, 0.0f);
        private static Material _wood = new Material("wood", new Color(0.94f, 0.94f, 0.94f, 1.0f), 0.0f, 0.0f);
        private static Material _black = new Material("black", new Color(0.0f, 0.0f, 0.0f, 1.0f), 0.0f, 0.0f);
        private static Material _edges = new Material("edges", new Color(0.1f, 0.1f, 0.1f, 1.0f), 0.0f, 0.0f);
        private static Material _topography = new Material("topography", new Color(0.59f, 0.59f, 0.39f, 1.0f), 0.0f, 0.0f);
        private static Material _edgesHighlighted = new Material("edge_highlighted", new Color(1.0f, 1.0f, 0.0f, 1.0f), 0.0f, 0.0f);
        private static Material _void = new Material("void", new Color(Colors.Lime.Red, Colors.Lime.Green, Colors.Lime.Blue, 0.1f), 0.1f, 0.1f);
        private static Material _xAxis = new Material("x_axis", new Color(1.0f, 0.0f, 0.0f, 1.0f), 0.1f, 0.1f);
        private static Material _yAxis = new Material("x_axis", new Color(0.0f, 1.0f, 0.0f, 1.0f), 0.1f, 0.1f);
        private static Material _zAxis = new Material("x_axis", new Color(0.0f, 0.0f, 1.0f, 1.0f), 0.1f, 0.1f);

        /// <summary>
        /// Glass.
        /// </summary>
        public static Material Glass => _glass;

        /// <summary>
        /// Steel.
        /// </summary>
        public static Material Steel => _steel;

        /// <summary>
        /// The default material.
        /// </summary>
        public static Material Default => _default;

        /// <summary>
        /// Concrete.
        /// </summary>
        public static Material Concrete = _concrete;

        /// <summary>
        /// Default material used to represent masses.
        /// </summary>
        public static Material Mass => _mass;

        /// <summary>
        /// Wood.
        /// </summary>
        public static Material Wood => _wood;

        /// <summary>
        /// Black
        /// </summary>
        public static Material Black => _black;

        /// <summary>
        /// Edges
        /// </summary>
        public static Material Edges => _edges;

        /// <summary>
        /// Topography
        /// </summary>
        public static Material Topography => _topography;

        /// <summary>
        /// Edges Highlighted
        /// </summary>
        public static Material EdgesHighlighted => _edgesHighlighted ;

        /// <summary>
        /// Void
        /// </summary>
        public static Material Void => _void;

        /// <summary>
        /// X Axis
        /// </summary>
        public static Material XAxis => _xAxis;

        /// <summary>
        /// Y Axis
        /// </summary>
        public static Material YAxis => _yAxis; 

        /// <summary>
        /// Z Axis
        /// </summary>
        public static Material ZAxis => _zAxis;
    }
}