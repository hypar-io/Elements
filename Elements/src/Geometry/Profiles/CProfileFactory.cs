namespace Elements.Geometry.Profiles
{
    /// <summary>
    /// A factory for creation L profiles.
    /// </summary>
    public class CProfileFactory : ParametricProfileFactory<CProfileType, CProfile>
    {
        /// <summary>
        /// Create an L profile factory.
        /// </summary>
        public CProfileFactory() : base("./ProfileData/C.csv", Units.InchesToMeters(1)) { }
    }
}