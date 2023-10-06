namespace Elements.Serialization.SVG
{
    /// <summary>
    /// The font family and size information
    /// </summary>
    public class SvgFont
    {
        /// <summary>
        /// Initializes a new instance of SvgFont.
        /// </summary>
        /// <param name="familyName">The family name.</param>
        public SvgFont(string familyName)
        {
            FamilyName = familyName;
        }

        /// <summary>
        /// The font family name.
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// The font size
        /// </summary>
        public double Size { get; set; } = 0.7;
    }
}