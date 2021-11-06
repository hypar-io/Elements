namespace Elements.Geometry.Profiles
{
    /// <summary>
    /// A factory for creation WT profiles.
    /// </summary>
    public class WTProfileFactory : ParametricProfileFactory<WTProfileType, WTProfile>
    {
        /// <summary>
        /// Create an WT profile factory.
        /// </summary>
        public WTProfileFactory() : base("./ProfileData/WT.csv", Units.InchesToMeters(1)) { }
    }
}