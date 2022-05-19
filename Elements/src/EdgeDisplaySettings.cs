namespace Elements
{
    /// <summary>
    /// Settings for how a curve or line should be displayed. 
    /// </summary>
    public class EdgeDisplaySettings
    {
        /// <summary>
        /// The width of the line. If Mode is set to Screen Units, this will be in pixels (and rounded to the nearest integer). 
        /// If set to World Units, this will be in meters. 
        /// </summary>
        public double LineWidth { get; set; } = 1;
        /// <summary>
        /// How the Width should be interpreted. If set to Screen Units, Width is interpreted as a constant pixel width (and rounded to the nearest integer). If set to World Units, Width is interpreted as a constant meter width.
        /// </summary>
        public EdgeDisplayWidthMode WidthMode { get; set; } = EdgeDisplayWidthMode.ScreenUnits;
    }

    /// <summary>
    /// Different ways to interpret the Width property of a EdgeDisplaySettings.
    /// </summary>
    public enum EdgeDisplayWidthMode
    {
        /// <summary>
        /// The Width property is interpreted as a constant pixel width (and rounded to the nearest integer).
        /// </summary>
        ScreenUnits = 0,
        /// <summary>
        /// The Width property is interpreted as a constant meter width.
        /// </summary>
        WorldUnits = 1,
    }
}