using Hypar.Geometry;

namespace Hypar.Elements
{
    public class Box: Element, IMeshProvider
    {
        public Box()
        {
            this._material = new Material("box", 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f);
        }

        public Mesh Tessellate()
        {
            return Mesh.Extrude(new[]{Profiles.Rectangular()},1);
        }
    }
}