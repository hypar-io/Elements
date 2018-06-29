namespace Hypar.Elements
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
        public static Material Glass = new Material("glass", 1.0f, 1.0f, 1.0f, 0.2f, 1.0f, 1.0f);

        /// <summary>
        /// Steel.
        /// </summary>
        /// <returns></returns>
        public static Material Steel = new Material("steel", 0.5f, 0.5f, 0.5f, 1.0f, 0.0f, 0.0f);

        /// <summary>
        /// The default material.
        /// </summary>
        /// <returns></returns>
        public static Material Default = new Material("default", 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f);

        /// <summary>
        /// Concrete.
        /// </summary>
        /// <returns></returns>
        public static Material Concrete = new Material("concrete", 0.5f,0.5f,0.5f,1.0f,0.0f,0.0f);

        /// <summary>
        /// Default material used to represent masses.
        /// </summary>
        /// <returns></returns>
        public static Material Mass = new Material("mass", 0.5f, 0.5f, 1.0f, 0.2f, 0.0f, 0.0f);

        /// <summary>
        /// Wood.
        /// </summary>
        /// <returns></returns>
        public static Material Wood = new Material("wood", 0.94f, 0.94f, 0.94f, 1.0f, 0.0f, 0.0f);
    }
}