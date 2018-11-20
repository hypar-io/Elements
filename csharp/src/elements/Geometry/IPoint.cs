namespace Hypar.Geometry
{
    /// <summary>
    /// The IPoint interface is implemented by classes which wish to
    /// visualize a point.
    /// </summary>
    public interface IPoint
    {
        /// <summary>
        /// The location of the IPoint.
        /// </summary>
        Vector3 Location{get;}
    }
}