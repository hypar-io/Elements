using Hypar.Geometry;

namespace Hypar.Elements
{
    /// <summary>
    /// A proxy element allows the user to supply whatever visualization they want for the element.
    /// </summary>
    public class Proxy : Element, ITessellate<Mesh>
    {
        /// <summary>
        /// Construct a proxy.
        /// </summary>
        /// <param name="material"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public Proxy(Material material = null, Transform transform = null) : base(material, transform){}

        /// <summary>
        /// Tessellate the proxy.
        /// Override this in inherited classes to define custom tessellation behavior.
        /// </summary>
        /// <returns></returns>
        public virtual Mesh Tessellate()
        {
            return null;
        }
    }
}