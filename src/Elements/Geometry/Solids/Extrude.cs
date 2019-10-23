namespace Elements.Geometry.Solids
{
    /// <summary>
    /// An extrusion operation.
    /// </summary>
    public partial class Extrude
    {
        private double _rotation = 0.0;

        /// <summary>
        /// An extrusion defined by a profile, a height, and a direction.
        /// </summary>
        /// <param name="profile">The profile to be extruded.</param>
        /// <param name="height">The height of the extrusion.</param>
        /// <param name="direction">The direction of the extrusion.</param>
        /// <param name="rotation">An optional rotation of the extrusion's profile.</param>
        /// <param name="isVoid">Is the extrusion operation a void?</param>
        public Extrude(Profile profile, double height, Vector3 direction = null, double rotation=0.0, bool isVoid = false): base(isVoid)
        {
            this.Profile = profile;
            this.Height = height;
            this.Direction = direction ?? Vector3.ZAxis;
            this._rotation = rotation;
        }

        /// <summary>
        /// Get the updated solid representation of the frame.
        /// </summary>
        internal override Solid GetSolid()
        {            
            return Kernel.Instance.CreateExtrude(this.Profile, this.Height, this.Direction, this._rotation);
        }
    }
}