namespace Elements.Serialization.SVG
{
    /// <summary>
    /// The set of save options for saving a document to PDF.
    /// </summary>
    public class PdfSaveOptions
    {
        /// <summary>
        /// What should the margin of the PDF be?.
        /// </summary>
        public float Margin { get; set; } = 15;

        /// <summary>
        /// What is the width of the PDF?
        /// </summary>
        public float PageWidth { get; set; } = 840;

        /// <summary>
        /// What is the height of the PDF?
        /// </summary>
        public float PageHeight { get; set; } = 1188;
    }
}