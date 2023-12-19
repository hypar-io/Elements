namespace Elements.Geometry.Profiles
{
    /// <summary>
    /// A factory for creation MC profiles.
    /// </summary>
    public class MCProfileFactory : ParametricProfileFactory<MCProfileType, MCProfile>
    {
        /// <summary>
        /// Create an MC profile factory.
        /// </summary>
        public MCProfileFactory() : base("./ProfileData/MC.csv", Units.InchesToMeters(1)) { }
    }
}