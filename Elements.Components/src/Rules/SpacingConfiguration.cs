namespace Elements.Components
{
    /// <summary>
    /// Different ways to space elements
    /// </summary>
    public enum SpacingMode
    {
        /// <summary>
        /// Construct the array with a fixed count. Value = Count.
        /// </summary>    
        ByCount,
        /// <summary>
        /// Construct the array with a fixed length. Value = length.
        /// </summary>    
        ByLength,
        /// <summary>
        /// Construct the array with an approximate length. Value = target length.
        /// </summary>    
        ByApproximateLength
    }

    /// <summary>
    /// A configuration representing a desired value and a spacing mode. What the value represents differs depending on the mode.
    /// </summary>
    public struct SpacingConfiguration
    {
        /// <summary>
        /// Construct a spacing configuration.
        /// </summary>
        /// <param name="mode">How to space this array.</param>
        /// <param name="value">The driving value of the array. (The meaning of this value depends on the choice of spacing mode.)</param>
        public SpacingConfiguration(SpacingMode mode, double value)
        {
            this.SpacingMode = mode;
            this.Value = value;
        }
        /// <summary>
        /// How to space this array.
        /// </summary>
        public SpacingMode SpacingMode { get; set; }

        /// <summary>
        /// The driving value of the array (meaning of this value depends on the choice of spacing mode.).
        /// </summary>
        public double Value { get; set; }
    }
}