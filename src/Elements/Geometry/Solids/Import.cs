namespace Elements.Geometry.Solids
{
    internal class Import : SolidOperation
    {
        private Solid _solid;

        /// <summary>
        /// Create an import solid.
        /// </summary>
        /// <param name="solid">The solid which was imported.</param>
        /// <param name="isVoid">Is the operation a void?</param>
        public Import(Solid solid, bool isVoid = false): base(isVoid)
        {
            this._solid = solid;
            this.IsVoid = isVoid;
        }

        internal override Solid GetUpdatedSolid()
        {
            return this._solid;
        }
    }
}