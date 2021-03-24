namespace Elements.Geometry.Solids
{
    /// <summary>
    /// Create a custom SolidOperation from imported geometry.
    /// </summary>
    public class ConstructedSolid : SolidOperation
    {
        /// <summary>
        /// Create an import solid.
        /// </summary>
        /// <param name="solid">The solid which was imported.</param>
        /// <param name="isVoid">Is the operation a void?</param>
        public ConstructedSolid(Solid solid, bool isVoid = false) : base(isVoid)
        {
            this._solid = solid;
            this._csg = solid.ToCsg();
        }
    }
}