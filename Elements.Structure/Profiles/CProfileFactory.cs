namespace Elements.Geometry.Profiles
{
    /// <summary>
    /// A factory for creation C profiles.
    /// </summary>
    public class CProfileFactory : ParametricProfileFactory<CProfileType, CProfile>
    {
        /// <summary>
        /// Create an C profile factory.
        /// </summary>
        public CProfileFactory() : base("./ProfileData/C.csv", Units.InchesToMeters(1)) { }
    }
}