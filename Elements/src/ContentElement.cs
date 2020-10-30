using System.Collections.Generic;
using System.IO;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements
{
    public partial class ContentElement
    {
        public ContentElement(string @gltfLocation, BBox3 @boundingBox, double @gltfScaleToMeters, Vector3 @sourceDirection, Transform @transform, Material @material, Representation @representation, bool @isElementDefinition, System.Guid @id, string @name, string @additionalProperties)
        : this(@gltfLocation, @boundingBox, @gltfScaleToMeters, @sourceDirection, @transform, @material, @representation, @isElementDefinition, @id, @name)
        {
            this.AdditionalProperties = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(@additionalProperties);
        }


        /// <summary>
        /// Update the ContentElement representation with a solid of the
        /// Bounding Box.  This is used in the absence of finding a the
        /// Gltf for import.
        /// </summary>
        public override void UpdateRepresentations()
        {
            var vertices = new List<Vector3> { BoundingBox.Min, BoundingBox.Max };
            var bottomProfile = new Polygon(new List<Vector3>{
                            new Vector3(BoundingBox.Min.X, BoundingBox.Min.Y, BoundingBox.Min.Z),
                            new Vector3(BoundingBox.Min.X, BoundingBox.Max.Y, BoundingBox.Min.Z),
                            new Vector3(BoundingBox.Max.X, BoundingBox.Max.Y, BoundingBox.Min.Z),
                            new Vector3(BoundingBox.Max.X, BoundingBox.Min.Y, BoundingBox.Min.Z),
                        });

            var height = BoundingBox.Max.Z - BoundingBox.Min.Z;
            var boxSolid = new Extrude(bottomProfile, height, Vector3.ZAxis, false);
            this.Representation = new Representation(new List<SolidOperation> { boxSolid });
        }
    }
}