namespace Elements.Geometry.Profiles
{
    /// <summary>
    /// A factory for creation L profiles.
    /// </summary>
    public class WTProfileFactory : ParametricProfileFactory<WTProfileType, WTProfile>
    {
        /// <summary>
        /// Create an L profile factory.
        /// </summary>
        public WTProfileFactory() : base("./ProfileData/WT.csv", Units.InchesToMeters(1)) { }
    }
}