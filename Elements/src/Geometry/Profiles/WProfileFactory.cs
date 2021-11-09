namespace Elements.Geometry.Profiles
{
    /// <summary>
    /// A factory for creation W profiles.
    /// </summary>
    public class WProfileFactory : ParametricProfileFactory<WProfileType, WProfile>
    {
        /// <summary>
        /// Create an L profile factory.
        /// </summary>
        public WProfileFactory() : base("./ProfileData/W.csv", Units.InchesToMeters(1)) { }
    }
}