namespace Hypar.Elements
{
    /// <summary>
    /// Represents an object which has a material.
    /// </summary>
    public interface IMaterialize
    {
        /// <summary>
        /// The material of the object.
        /// </summary>
        /// <value></value>
        Material Material{get; set;}
    }
}