namespace Elements
{
    /// <summary>
    /// The base class for all mapping classes.
    /// </summary>
    public class MappingBase : Element
    { }

    /// <summary>
    /// The supported MappingContexts.
    /// </summary>
    public enum MappingContext
    {
        /// <summary>
        /// The Revit Context.
        /// </summary>
        [System.Runtime.Serialization.EnumMember(Value = @"Revit")]
        Revit
    }
}