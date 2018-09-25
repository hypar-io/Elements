using Hypar.Geometry;
using System.Collections.Generic;
using System;

namespace Hypar.Geometry
{   
    /// <summary>
    /// Construct profiles.
    /// </summary>
    public partial class Polygon
    {
        /// <summary>
        /// Construct a rectangular profile
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="verticalOffset"></param>
        /// <param name="horizontalOffset"></param>
        /// <returns></returns>
        public static Polygon Rectangle(Vector3 origin = null, double width = 1.0, double height = 1.0, double verticalOffset = 0.0, double horizontalOffset = 0.0)
        {
            if(origin == null)
            {
                origin = Vector3.Origin;
            }

            var a = new Vector3(origin.X - width/2 + horizontalOffset, origin.Y - height/2 + verticalOffset);
            var b = new Vector3(origin.X + width/2 + horizontalOffset, origin.Y - height/2 + verticalOffset);
            var c = new Vector3(origin.X + width/2 + horizontalOffset, origin.Y + height/2 + verticalOffset);
            var d = new Vector3(origin.X - width/2 + horizontalOffset, origin.Y + height/2 + verticalOffset);

            return new Polygon(new []{a, b, c, d});
        }

        /// <summary>
        /// The vertical alignment of the profile.
        /// </summary>
        public enum VerticalAlignment
        {
            /// <summary>
            /// Align the profile along its top.
            /// </summary>
            Top,
            /// <summary>
            /// Align the profile along its center.
            /// </summary>
            Center,
            /// <summary>
            /// Align the profile along its bottom.
            /// </summary>
            Bottom
        }

        /// <summary>
        /// The horizontal alignment of the profile.
        /// </summary>
        public enum HorizontalAlignment
        {
            /// <summary>
            /// Align the profile along its left edge.
            /// </summary>
            Left,
            /// <summary>
            /// Align the profile along its center.
            /// </summary>
            Center, 
            /// <summary>
            /// Align the profile along its right edge.
            /// </summary>
            Right
        }

        /// <summary>
        /// Construct a wide-flange profile.
        /// </summary>
        /// <param name="bf">The width.</param>
        /// <param name="d">The depth.</param>
        /// <param name="tf">The thickness of the flange.</param>
        /// <param name="tw">The thickness of the web.</param>
        /// <param name="verticalAlignment"></param>
        /// <param name="horizontalAlignment"></param>
        /// <param name="verticalOffset"></param>
        /// <param name="horizontalOffset"></param>
        /// <returns></returns>
        public static Polygon WideFlange(double bf = 0.1, double d = 0.05, double tf = 0.005, double tw = 0.005, 
                                                        VerticalAlignment verticalAlignment = VerticalAlignment.Center, 
                                                        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center, 
                                                        double verticalOffset = 0.0, double horizontalOffset = 0.0)
        {
            var o = new Vector3();

            var height = d;
            var width = bf;
            var thicknessWeb = tw;
            var thicknessFlange = tf;

            if(verticalOffset == 0.0)
            {
                switch(verticalAlignment)
                {
                    case VerticalAlignment.Top:
                        verticalOffset = height/2;
                        break;
                    case VerticalAlignment.Center:
                        verticalOffset = 0.0;
                        break;
                    case VerticalAlignment.Bottom:
                        verticalOffset = -height/2;
                    break;
                }
            }

            if(horizontalOffset == 0.0)
            {
                switch(horizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        horizontalOffset = -width/2;
                        break;
                    case HorizontalAlignment.Center:
                        horizontalOffset = 0.0;
                        break;
                    case HorizontalAlignment.Right:
                        horizontalOffset = width/2;
                        break;
                }
            }

            // Left
            var a = new Vector3(o.X - width/2 + horizontalOffset, o.Y + height/2 + verticalOffset);
            var b = new Vector3(o.X - width/2 + horizontalOffset, o.Y + height/2 - thicknessFlange + verticalOffset);
            var c = new Vector3(o.X - thicknessWeb/2 + horizontalOffset, o.Y + height/2 - thicknessFlange + verticalOffset);
            var e = new Vector3(o.X - thicknessWeb/2 + horizontalOffset, o.Y - height/2 + thicknessFlange + verticalOffset);
            var f = new Vector3(o.X - width/2 + horizontalOffset, o.Y - height/2 + thicknessFlange + verticalOffset);
            var g = new Vector3(o.X - width/2 + horizontalOffset, o.Y - height/2 + verticalOffset);

            // Right
            var h = new Vector3(o.X + width/2 + horizontalOffset, o.Y - height/2 + verticalOffset);
            var i = new Vector3(o.X + width/2 + horizontalOffset, o.Y - height/2 + thicknessFlange + verticalOffset);
            var j = new Vector3(o.X + thicknessWeb/2 + horizontalOffset, o.Y - height/2 + thicknessFlange + verticalOffset);
            var k = new Vector3(o.X + thicknessWeb/2 + horizontalOffset, o.Y + height/2 - thicknessFlange + verticalOffset);
            var l = new Vector3(o.X + width/2 + horizontalOffset, o.Y + height/2 - thicknessFlange + verticalOffset);
            var m = new Vector3(o.X + width/2 + horizontalOffset, o.Y + height/2 + verticalOffset);

            return new Polygon(new []{a,b,c,e,f,g,h,i,j,k,l,m});
        }

        /// <summary>
        /// A circle.
        /// </summary>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="divisions">The number of divisions of the circle.</param>
        /// <returns></returns>
        public static Polygon Circle(double radius = 1.0, int divisions = 10)
        {
            var verts = new List<Vector3>();
            for(var i=0.0; i<Math.PI*2; i += Math.PI*2/divisions)
            {
                verts.Add(new Vector3(radius * Math.Cos(i), radius * Math.Sin(i)));
            }
            return new Polygon(verts);
        }
    }
}