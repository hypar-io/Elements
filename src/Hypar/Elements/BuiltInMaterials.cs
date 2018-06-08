namespace Hypar.Elements
{
    public static class BuiltIntMaterials
    {
        public static Material Glass = new Material("glass", 1.0f, 1.0f, 1.0f, 0.2f, 1.0f, 1.0f);
        public static Material Steel = new Material("steel", 0.5f, 0.5f, 0.5f, 1.0f, 0.0f, 0.0f);
        public static Material Default = new Material("default", 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f);
        public static Material Concrete = new Material("concrete", 0.5f,0.5f,0.5f,1.0f,0.0f,0.0f);
        public static Material Mass = new Material("mass", 0.5f, 0.5f, 1.0f, 0.2f, 0.0f, 0.0f);
        public static Material Wood = new Material("wood", 0.94f, 0.94f, 0.94f, 1.0f, 0.0f, 0.0f);
    }
}