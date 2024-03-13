namespace Elements.Geometry.Profiles
{
    /// <summary>
    /// A factory for creation L profiles.
    /// </summary>
    public class HSSProfileFactory : ParametricProfileFactory<HSSProfileType, HSSProfile>
    {
        /// <summary>
        /// Create an C profile factory.
        /// </summary>
        public HSSProfileFactory() : base("./ProfileData/HSS.csv", Units.InchesToMeters(1)) { }
    }
}