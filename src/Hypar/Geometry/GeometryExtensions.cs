using System;
using System.Collections.Generic;

namespace Hypar.Geometry
{
    /// <summary>
    /// Extension methods used with geometry.
    /// </summary>
    public static class GeometryExtensions
    {
        /// <summary>
        /// Convert a Haxe array to a Vector3.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static Vector3 ToVector3(this haxe.root.Array<double> arr)
        {
            if(arr.length != 3)
            {
                throw new Exception($"I expected 3 components, but got {arr.length} components while converting an Array<double> to a Vector3.");
            }
            return new Vector3(arr[0], arr[1], arr[2]);
        }

        /// <summary>
        /// Convert an object to a Vector3.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static Vector3 ToVector3(this object arr)
        {
            if(arr is haxe.root.Array<double>)
            {
                var haxArr = (haxe.root.Array<double>)arr;
                return haxArr.ToVector3();
            }
            throw new Exception($"I expected a haxe.root.Array<double>, but got {arr.GetType().Name} when attempting to convert to a Vector3.");
        }

        /// <summary>
        /// Convert a Haxe array of objects to a collection of Vector3.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static IEnumerable<Vector3> ToVector3Collection(this haxe.root.Array<object> arr)
        {
            var result = new List<Vector3>();
            for(var i=0; i<arr.length; i++)
            {
                result.Add(arr[i].ToVector3());
            }   

            return result;
        }
    }
}