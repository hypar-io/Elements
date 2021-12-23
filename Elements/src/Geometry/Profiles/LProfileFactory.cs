namespace Elements.Geometry.Profiles
{
    /// <summary>
    /// A factory for creation L profiles.
    /// </summary>
    public class LProfileFactory : ParametricProfileFactory<LProfileType, LProfile>
    {
        /// <summary>
        /// Create an L profile factory.
        /// </summary>
        public LProfileFactory() : base("./ProfileData/L.csv", Units.InchesToMeters(1)) { }
    }
}