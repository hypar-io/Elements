namespace Elements.Geometry.Solids
{
    public partial class Lamina
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="isVoid"></param>
        /// <param name="perimeter"></param>
        public static void ValidateConstructorParameters(Polygon @perimeter, bool @isVoid)
        {
            return;
        }

        /// <summary>
        /// Create a lamina.
        /// </summary>
        /// <param name="perimeter">The polygon to convert into a lamina.</param>
        public Lamina(Polygon perimeter): base(false)
        {
            this.Perimeter = perimeter;
        }

        /// <summary>
        /// Get the updated solid representation of the frame.
        /// </summary>
        /// <returns></returns>
        internal override Solid GetSolid()
        {
            return Kernel.Instance.CreateLamina(this.Perimeter);
        }
    }
}