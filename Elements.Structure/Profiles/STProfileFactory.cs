namespace Elements.Geometry.Profiles
{
    /// <summary>
    /// A factory for creation L profiles.
    /// </summary>
    public class STProfileFactory : ParametricProfileFactory<STProfileType, STProfile>
    {
        /// <summary>
        /// Create an ST profile factory.
        /// </summary>
        public STProfileFactory() : base("./ProfileData/ST.csv", Units.InchesToMeters(1)) { }
    }
}