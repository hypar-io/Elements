namespace Elements.Annotations
{
    /// <summary>
    /// An annotation.
    /// </summary>
    public abstract class Annotation : Element
    {
        /// <summary>
        /// Text to be displayed in place of the annotation's value.
        /// </summary>
        public string DisplayValue { get; set; }

        /// <summary>
        /// Text to be displayed before the annotation's value.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Text to be displayed after the annotation's value.
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// Create a annotation.
        /// </summary>
        public Annotation() { }
    }
}