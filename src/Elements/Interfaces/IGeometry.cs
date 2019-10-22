#pragma warning disable CS1591

namespace Elements.Interfaces
{
    public interface IGeometry
    {
        Elements.Geometry.Geometry Geometry { get; }

        /// <summary>
        /// This method provides an opportunity for geometric elements
        /// to adjust their solid operations before tesselation. As an example,
        /// a floor might want to clip its opening profiles out of 
        /// the profile of the floor.
        /// </summary>
        void UpdateSolidOperations();
    }
}