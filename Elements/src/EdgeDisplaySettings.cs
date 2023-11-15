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

        /// <summary>
        /// Whether and how to display dashes along the line.
        /// </summary>
        public EdgeDisplayDashMode DashMode { get; set; } = EdgeDisplayDashMode.None;

        /// <summary>
        /// The size of the dash. If Mode is set to None, this value will be ignored. Note that the units for this value (screen vs world) are affected by the choice of Dash Mode.
        /// </summary>
        public double DashSize { get; set; } = 1;

        /// <summary>
        /// The size of the gaps between dashes. If Mode is set to None, this value will be ignored. If this value is set to null, DashSize will be used. Note that the units for this value (screen vs world) are affected by the choice of Dash Mode.
        /// </summary>
        public double? GapSize { get; set; } = 1;
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

    /// <summary>
    /// Different ways to interpret the Width property of a EdgeDisplaySettings.
    /// </summary>
    public enum EdgeDisplayDashMode
    {
        /// <summary>
        /// Dashed display is not enabled. Dash size is ignored.
        /// </summary>
        None = 0,
        /// <summary>
        /// Dash sizes are specified in pixels, and maintain a constant size when zooming.
        /// </summary>
        ScreenUnits = 1,
        /// <summary>
        /// Dash sizes are specified in meters, and maintain a constant size relative to the model.
        /// </summary>
        WorldUnits = 2,
    }
}