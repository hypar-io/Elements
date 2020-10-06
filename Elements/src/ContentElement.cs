using System.Collections.Generic;
using System.IO;
using Elements.Geometry;
using Elements.Geometry.Solids;

namespace Elements
{
    public partial class ContentElement
    {
        /// <summary>
        /// Update the ContentElement representation with a solid of the
        /// Bounding Box.  This is used in the absense of finding a the
        /// Gltf for import.
        /// </summary>
        public override void UpdateRepresentations()
        {
            if (BoundingBox.Min.X == BoundingBox.Max.X
                || BoundingBox.Min.Y == BoundingBox.Max.Y
                || BoundingBox.Min.Z == BoundingBox.Max.Z)
            {
                throw new System.ArgumentException("The bounding box will have zero volume, please ensure that the Min and Max don't have any identical vertex values.");
            }
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