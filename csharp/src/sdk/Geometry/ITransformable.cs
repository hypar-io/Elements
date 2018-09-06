namespace Hypar.Geometry
{
    /// <summary>
    /// Represents an object which has a transform.
    /// </summary>
    public interface ITransformable
    {
        /// <summary>
        /// The object's transform.
        /// </summary>
        /// <value></value>
        Transform Transform{get;}
    }
}