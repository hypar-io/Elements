namespace Hypar.Elements
{
    public static class BuiltInMaterials
    {
        public const string GLASS = "glass";
        public const string STEEL = "steel";
        public const string DEFAULT = "default";
        public const string CONCRETE = "concrete";
    }

    public static class Materials
    {
        public static Material Glass()
        {
            return new Material(BuiltInMaterials.GLASS, 1.0f, 1.0f, 1.0f, 0.2f, 1.0f, 1.0f);
        }

        public static Material Steel()
        {
            return new Material(BuiltInMaterials.STEEL, 0.5f, 0.5f, 0.5f, 1.0f, 0.0f, 0.0f);
        }

        public static Material Default()
        {
            return new Material(BuiltInMaterials.DEFAULT, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f);
        }

        public static Material Concrete()
        {
            return new Material(BuiltInMaterials.CONCRETE, 0.5f,0.5f,0.5f,1.0f,0.0f,0.0f);
        }
    }
}