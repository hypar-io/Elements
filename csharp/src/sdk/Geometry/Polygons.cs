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

        // public static Polygon HSS(double d, double b, double t, 
        //                             VerticalAlignment verticalAlignment = VerticalAlignment.Center, 
        //                             HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center, 
        //                             double verticalOffset = 0.0, double horizontalOffset = 0.0)
        // {
        //     var height = d;
        //     var width = b;

        //     if(verticalOffset == 0.0)
        //     {
        //         switch(verticalAlignment)
        //         {
        //             case VerticalAlignment.Top:
        //                 verticalOffset = height/2;
        //                 break;
        //             case VerticalAlignment.Center:
        //                 verticalOffset = 0.0;
        //                 break;
        //             case VerticalAlignment.Bottom:
        //                 verticalOffset = -height/2;
        //             break;
        //         }
        //     }

        //     var o1 = new Vector3(- width/2 + horizontalOffset, - height/2 + verticalOffset);
        //     var o2 = new Vector3(width/2 + horizontalOffset, - height/2 + verticalOffset);
        //     var o3 = new Vector3(width/2 + horizontalOffset, height/2 + verticalOffset);
        //     var o4 = new Vector3(width/2 + horizontalOffset, height/2 + verticalOffset);

        //     var i1 = o1 + new Vector3(t, t);
        //     var i2 = o2 + new Vector3(-t, t);
        //     var i3 = o3 + new Vector3(-t, -t);
        //     var i4 = o4 + new Vector3(t, -t);

        //     return new Polygon(new []{o1, o2, o3, o4});
        // }
    }
}