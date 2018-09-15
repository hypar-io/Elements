using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Hypar.Geometry;

namespace Hypar.Elements
{
    /// <summary>
    /// A linear structural element with a cross section.
    /// </summary>
    public abstract class StructuralFraming : Element, ITessellate<Mesh>
    {
        /// <summary>
        /// The cross-section profile of the beam.
        /// </summary>
        [JsonProperty("profile")]
        public IEnumerable<Polygon> Profile{get;}
        
        /// <summary>
        /// The up axis of the beam.
        /// </summary>
        [JsonProperty("up_axis")]
        public Vector3 UpAxis{get;}

        /// <summary>
        /// The center line of the beam.
        /// </summary>
        [JsonProperty("location")]
        public Line CenterLine{get;}

        /// <summary>
        /// Construct a beam.
        /// </summary>
        /// <param name="centerLine">The center line of the beam.</param>
        /// <param name="profile">The structural profile of the beam.</param>
        /// <param name="material">The beam's material.</param>
        /// <param name="up">The up axis of the beam.</param>
        public StructuralFraming(Line centerLine, IEnumerable<Polygon> profile, Material material = null, Vector3 up = null)
        {
            this.Profile = profile;
            this.CenterLine = centerLine;
            this.Material = material == null ? BuiltInMaterials.Steel : material;

            var t = centerLine.GetTransform(up);
            this.UpAxis = up == null ? t.YAxis : up;
            this.Transform = t;
        }

        /// <summary>
        /// Tessellate the beam.
        /// </summary>
        /// <returns>A mesh representing the tessellated beam.</returns>
        public Mesh Tessellate()
        {
            return Mesh.ExtrudeAlongLine(this.CenterLine, this.Profile);
        }
    }

    /// <summary>
    /// A beam is a structural framing element which is often horizontal.
    /// </summary>
    public class Beam : StructuralFraming
    {
        /// <summary>
        /// Construct a beam.
        /// </summary>
        /// <param name="centerLine">The beam's center line.</param>
        /// <param name="profile">The beam's profile.</param>
        /// <param name="material">The beam's material.</param>
        /// <param name="up">The beam's up axis.</param>
        /// <returns></returns>
        public Beam(Line centerLine, IList<Polygon> profile, Material material = null, Vector3 up = null) : base(centerLine, profile, material, up){}
    }

    /// <summary>
    /// A column is a structural framing element which is often vertical.
    /// </summary>
    public class Column : StructuralFraming
    {
        /// <summary>
        /// Construct a column.
        /// </summary>
        /// <param name="location">The location of the base of the column.</param>
        /// <param name="height">The column's height.</param>
        /// <param name="profile">The column's profile.</param>
        /// <param name="material">The column's material.</param>
        /// <returns></returns>
        public Column(Vector3 location, double height, IList<Polygon> profile, Material material = null) : base(new Line(location, new Vector3(location.X, location.Y, location.Z + height)), profile, material){}

        /// <summary>
        /// Construct a column.
        /// </summary>
        /// <param name="centerLine">The Column's center line</param>
        /// <param name="profile">The column's profile.</param>
        /// <param name="material">The column's material.</param>
        /// <returns></returns>
        public Column(Line centerLine, IList<Polygon> profile, Material material) : base(centerLine, profile, material){}
    }

    /// <summary>
    /// A brace is a structural framing element which is often diagonal.
    /// </summary>
    public class Brace : StructuralFraming
    {
        /// <summary>
        /// Construct a brace.
        /// </summary>
        /// <param name="centerLine">The brace's center line.</param>
        /// <param name="profile">The brace's profile.</param>
        /// <param name="material">The brace's material.</param>
        /// <param name="up">The brace's up axis.</param>
        /// <returns></returns>
        public Brace(Line centerLine, IList<Polygon> profile, Material material = null, Vector3 up = null) : base(centerLine, profile, material, up){}
    }
}