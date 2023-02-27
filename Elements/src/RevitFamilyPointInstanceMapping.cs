namespace Elements
{
    /// <summary>
    /// The mapping class for Revit
    /// </summary>
    public class RevitFamilyPointInstanceMapping : MappingBase
    {
        /// <summary>
        /// The Family name expected in Revit.
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// The FamilyType name expected in Revit.
        /// </summary>
        public string TypeName { get; set; }
    }
}