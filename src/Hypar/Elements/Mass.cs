using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hypar.Geometry;

namespace Hypar.Elements
{
    public class Mass : Element, IMeshProvider
    {
        private List<Polygon3>m_sides = new List<Polygon3>();

        /// <summary>
        /// The bottom perimeter of the Mass.
        /// </summary>
        /// <returns></returns>
        public Polygon2 Bottom{get;}

        public double BottomElevation{get;}

        /// <summary>
        /// The top perimeter of the Mass.
        /// </summary>
        /// <returns></returns>
        public Polygon2 Top{get;}

        public double TopElevation{get;}

        /// <summary>
        /// The faces of the mass.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Polygon3> Faces
        {
            get{return m_sides;}
        }

        public Mass(Polygon2 bottom, double bottomElevation, Polygon2 top, double topElevation, Material material, Transform transform = null) : base(material, transform)
        {
            if(bottom.Vertices.Count() != top.Vertices.Count())
            {
                throw new ArgumentException("The top and bottom boundaries must have the same number of vertices.");
            }

            if(topElevation <= bottomElevation)
            {
                throw new ArgumentOutOfRangeException("topElevation","The top elevation must be greater than the bottom elevation.");
            }

            this.Top = top;
            this.Bottom = bottom;
            this.BottomElevation = bottomElevation;
            this.TopElevation = topElevation;

            var b = bottom.Vertices.ToArray();
            var t = top.Vertices.ToArray();

            for(var i=0; i<b.Length; i++)
            {
                var next = i+1;
                if(i == b.Length-1)
                {
                    next = 0;
                }
                var v1 = b[i];
                var v2 = b[next];
                var v3 = t[next];
                var v4 = t[i];
                var v1n = new Vector3(v1.X, v1.Y, bottomElevation);
                var v2n = new Vector3(v2.X, v2.Y, bottomElevation);
                var v3n = new Vector3(v3.X, v3.Y, topElevation);
                var v4n =new Vector3(v4.X, v4.Y, topElevation);
                var side = new Polygon3(new[]{v1n,v2n,v3n,v4n});
                m_sides.Add(side);
            }
        }

        public Mesh Tessellate()
        {
            var mesh = new Mesh();
            foreach(var s in m_sides)
            {
                mesh.AddQuad(s.ToArray());
            }
 
            mesh.AddTesselatedFace(new[]{this.Bottom}, this.BottomElevation);
            mesh.AddTesselatedFace(new []{this.Top}, this.TopElevation, true);
            return mesh;
        }
    }
}