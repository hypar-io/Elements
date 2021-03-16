namespace Elements.Spatial
{
    /// <summary>
    /// Different ways to space elements
    /// </summary>
    public enum SpacingMode
    {
        ByCount,
        ByLength,
        ByApproximateLength
    }

    /// <summary>
    /// A configuration representing a desired value and a spacing mode. What the value represents differs depending on the mode.
    /// </summary>
    public struct SpacingConfiguration
    {
        public SpacingConfiguration(SpacingMode mode, double value)
        {
            this.SpacingMode = mode;
            this.Value = value;
        }
        public SpacingMode SpacingMode { get; set; }
        public double Value { get; set; }
    }
}