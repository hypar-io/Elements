using System;
using Elements.Geometry;
using Elements.Geometry.Interfaces;
using Xunit;
using static Elements.Units;

namespace Elements.Tests
{
    /// <summary>
    /// This is an example of a user element with a mesh representation.
    /// </summary>
    [UserElement]
    public class MeshElement : GeometricElement, ITessellate
    {
        private Mesh _mesh;

        internal MeshElement(Mesh mesh, 
                             Transform transform = null,
                             Material material = null,
                             bool isElementDefinition = false,
                             Guid id = default(Guid),
                             string name = null) : base(transform == null ? new Transform() : transform,
                                                        material == null ? BuiltInMaterials.Default : material,
                                                        null,
                                                        isElementDefinition,
                                                        id == default(Guid) ? Guid.NewGuid() : id,
                                                        name)
        {
            this._mesh = mesh;
        }

        public void Tessellate(ref Mesh mesh)
        {
            mesh = this._mesh;
        }
    }

    public class MeshTests : ModelTest
    {
        [Fact]
        public void FromSTL()
        {
            this.Name = "FromSTL";

            var path = "../../../models/STL/Hilti_2008782_Speed lock clevis hanger MH-SLC 2_ EG_2.stl";
            var mesh = Mesh.FromSTL(path, LengthUnit.Millimeter);
            var shiny = new Material("shiny", Colors.Red, 1.0, 0.9);
            var meshElement = new MeshElement(mesh, null, shiny, isElementDefinition: true);

            for(var u=0; u<360.0; u += 20)
            {
                var t = new Transform(new Vector3(1, 0, 0));
                t.Rotate(Vector3.ZAxis, u);
                this.Model.AddElements(t.ToModelCurves());
                var instance = meshElement.CreateInstance(t, $"Component_{u}");
                this.Model.AddElement(instance);
            }
        }
    }
}