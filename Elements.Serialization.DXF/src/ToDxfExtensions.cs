using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Elements.Geometry;
using Elements.Geometry.Solids;
using IxMilia.Dxf;
using IxMilia.Dxf.Entities;

namespace Elements.Serialization.DXF.Extensions
{
    /// <summary>
    /// Extension methods for converting Element geometric primitives into DXF Entities and objects.
    /// </summary>
    public static class ToDxfExtensions
    {

        /// <summary>
        /// Convert a polyline to a DXF Polyline entity.
        /// </summary>
        public static DxfPolyline ToDxf(this Polyline polyline)
        {
            if (polyline == null || polyline.Vertices == null || polyline.Vertices.Count < 2)
            {
                return null;
            }
            var vertices = polyline.Vertices.Select(v => v.ToDxfVertex());
            var dxf = new DxfPolyline(vertices);
            dxf.IsClosed = polyline is Polygon;
            return dxf;
        }

        /// <summary>
        /// Convert a Vector3 to a DXF Vertex.
        /// </summary>
        public static DxfVertex ToDxfVertex(this Vector3 vector3)
        {
            return new DxfVertex(new DxfPoint(vector3.X, vector3.Y, vector3.Z));
        }

        /// <summary>
        /// Convert a Vector3 to a DXF Lightweight Polyline Vertex.
        /// </summary>
        public static DxfLwPolylineVertex ToDxfLwPolylineVertex(this Vector3 vector3)
        {
            var vertex = new DxfLwPolylineVertex();
            vertex.X = vector3.X;
            vertex.Y = vector3.Y;
            return vertex;
        }

        /// <summary>
        /// Convert an Elements Color to a DxfColor.
        /// </summary>
        public static DxfColor ToDxfColor(this Color color)
        {
            var r = (byte)Math.Round(color.Red * 255);
            var g = (byte)Math.Round(color.Green * 255);
            var b = (byte)Math.Round(color.Blue * 255);
            return DxfColorHelpers.GetClosestDefaultIndexColor(r, g, b);
        }

        /// <summary>
        /// Convert an Elements color to a 24-bit integer.
        /// </summary>
        public static int To24BitColor(this Color color)
        {
            var r = (byte)Math.Round(color.Red * 255);
            var g = (byte)Math.Round(color.Green * 255);
            var b = (byte)Math.Round(color.Blue * 255);
            int rgb = r;
            rgb = (rgb << 8) + g;
            rgb = (rgb << 8) + b;
            return rgb;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static string GetBlockName(this Element element)
        {
            return (element.Name != null ? $"{Regex.Replace(element.Name, @"[^A-Za-z0-9_-]", "")}_" : "") + element.Id;
        }

        /// <summary>
        /// Get DXF entities from the Representation of a GeometricElement.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static List<DxfEntity> GetEntitiesFromRepresentation(this GeometricElement element)
        {
            var list = new List<DxfEntity>();
            foreach (var solidOp in element.Representation.SolidOperations)
            {
                list.AddRange(solidOp.ToDxfEntities(element.Transform));
            }
            return list;
        }

        /// <summary>
        /// Get DXF entities from a SolidOperation.
        /// </summary>
        /// <param name="solidOp"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static List<DxfEntity> ToDxfEntities(this SolidOperation solidOp, Transform transform)
        {
            var list = new List<DxfEntity>();
            switch (solidOp)
            {
                case Extrude extrude:
                    {
                        var profile = extrude.Profile.Transformed(transform);
                        list.AddRange(profile.ToDxfEntities());
                        break;
                    }
                case Sweep sweep:
                    {
                        var profile = sweep.Profile.Transformed(sweep.Curve.TransformAt(sweep.StartSetback));
                        list.AddRange(profile.ToDxfEntities());
                        break;
                    }
                case Lamina lamina:
                    {
                        var profile = new Profile(lamina.Perimeter.TransformedPolygon(transform), lamina.Voids?.Select(v => v.TransformedPolygon(transform)).ToList() ?? new List<Polygon>());
                        list.AddRange(profile.ToDxfEntities());
                        break;
                    }
            }
            return list;
        }

        /// <summary>
        /// Get DXF entities from a Profile.
        /// </summary>
        /// <param name="profile"></param>
        public static List<DxfEntity> ToDxfEntities(this Profile profile)
        {
            var list = new List<DxfEntity>();
            list.Add(profile.Perimeter.ToDxf());
            if (profile.Voids != null)
            {
                list.AddRange(profile.Voids.Select(v => v.ToDxf()));
            }
            return list;
        }
        /// <summary>
        /// Get a DXFPoint from a Vector3.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static DxfPoint ToDxfPoint(this Vector3 vector)
        {
            return new DxfPoint(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// Get a DXFPoint from a transform.
        /// </summary>

        public static DxfPoint ToDxfPoint(this Transform transform, DxfRenderContext context)
        {
            // TODO: use the context to get correct orientation
            return transform.Origin.ToDxfPoint();
        }

        /// <summary>
        /// Get a rotation angle from a transform.
        /// </summary>
        public static double ToDxfAngle(this Transform transform, DxfRenderContext context)
        {
            // TODO: use the context to get correct orientation
            return Vector3.XAxis.PlaneAngleTo(transform.XAxis);
        }
    }
}