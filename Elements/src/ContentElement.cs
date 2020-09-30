using System.IO;

namespace Elements
{
    public partial class ContentElement
    {
        public override void UpdateRepresentations()
        {
            var exists = File.Exists(this.GltfLocation);
        }
    }
}