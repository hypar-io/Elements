namespace Elements.Geometry.Profiles
{
    /// <summary>
    /// A factory for creation L profiles.
    /// </summary>
    public class MCProfileFactory : ParametricProfileFactory<MCProfileType, MCProfile>
    {
        /// <summary>
        /// Create an C profile factory.
        /// </summary>
        public MCProfileFactory() : base("./ProfileData/MC.csv", Units.InchesToMeters(1)) { }
    }
}