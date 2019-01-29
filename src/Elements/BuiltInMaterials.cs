using Elements.Geometry;

namespace Elements
{
    /// <summary>
    /// Built in materials.
    /// </summary>
    public static class BuiltInMaterials
    {
        /// <summary>
        /// Glass.
        /// </summary>
        /// <returns></returns>
        public static Material Glass = new Material("glass", new Color(1.0f, 1.0f, 1.0f, 0.2f), 1.0f, 1.0f);

        /// <summary>
        /// Steel.
        /// </summary>
        /// <returns></returns>
        public static Material Steel = new Material("steel", new Color(0.6f, 0.5f, 0.5f, 1.0f), 0.0f, 0.0f);

        /// <summary>
        /// The default material.
        /// </summary>
        /// <returns></returns>
        public static Material Default = new Material("default", new Color(1.0f, 1.0f, 1.0f, 1.0f), 0.0f, 0.0f);

        /// <summary>
        /// Concrete.
        /// </summary>
        /// <returns></returns>
        public static Material Concrete = new Material("concrete", new Color(0.5f,0.5f,0.5f,1.0f), 0.0f, 0.0f);

        /// <summary>
        /// Default material used to represent masses.
        /// </summary>
        /// <returns></returns>
        public static Material Mass = new Material("mass", new Color(0.5f, 0.5f, 1.0f, 0.2f), 0.0f, 0.0f);

        /// <summary>
        /// Wood.
        /// </summary>
        /// <returns></returns>
        public static Material Wood = new Material("wood", new Color(0.94f, 0.94f, 0.94f, 1.0f), 0.0f, 0.0f);

        /// <summary>
        /// Black
        /// </summary>
        public static Material Black = new Material("black", new Color(0.0f, 0.0f, 0.0f, 1.0f), 0.0f, 0.0f);

        /// <summary>
        /// Edges
        /// </summary>
        public static Material Edges = new Material("edges", new Color(0.1f, 0.1f, 0.1f, 1.0f), 0.0f, 0.0f);

        /// <summary>
        /// Edges Highlighted
        /// </summary>
        public static Material EdgesHighlighted = new Material("edge_highlighted", new Color(1.0f, 1.0f, 0.0f, 1.0f), 0.0f, 0.0f);

        /// <summary>
        /// Void
        /// </summary>
        public static Material Void = new Material("void", new Color(Colors.Lime.Red, Colors.Lime.Green, Colors.Lime.Blue, 0.1f), 0.1f, 0.1f);

        /// <summary>
        /// X Axis
        /// </summary>
        public static Material XAxis = new Material("x_axis", new Color(1.0f, 0.0f, 0.0f, 1.0f), 0.1f, 0.1f);

        /// <summary>
        /// Y Axis
        /// </summary>
        public static Material YAxis = new Material("x_axis", new Color(0.0f, 1.0f, 0.0f, 1.0f), 0.1f, 0.1f);

        /// <summary>
        /// Z Axis
        /// </summary>
        public static Material ZAxis = new Material("x_axis", new Color(0.0f, 0.0f, 1.0f, 1.0f), 0.1f, 0.1f);
    }
}