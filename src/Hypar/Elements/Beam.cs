using Hypar.Geometry;

namespace Hypar.Elements
{
    public class Beam : Element, IMeshProvider
    {
        public Line CenterLine{get;}

        public Polygon2 Profile {get;}

        public Beam(Line centerLine, StructuralProfile profile, Material material, Vector3 up = null, Transform transform = null):base(material, transform)
        {
            this.Profile = profile.Profile;
            this.CenterLine = centerLine;
            this.Transform = centerLine.GetTransform(up);
        }

        public Mesh Tessellate()
        {
            return Mesh.ExtrudeAlongLine(this.CenterLine, new[]{this.Profile});
        }
    }
}