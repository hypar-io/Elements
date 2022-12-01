namespace Elements.Serialization.SVG
{
    /// <summary>
    /// The set of save options for saving a document to PDF.
    /// </summary>
    public class PdfSaveOptions
    {
        /// <summary>
        /// The margin.
        /// </summary>
        public float Margin { get; set; } = 15;

        /// <summary>
        /// The page width.
        /// </summary>
        public float PageWidth { get; set; } = 840;

        /// <summary>
        /// The page height.
        /// </summary>
        public float PageHeight { get; set; } = 1188;
    }
}