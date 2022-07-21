using Elements;
using Elements.Geometry;
using Elements.Spatial.AdaptiveGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Spatial.AdaptiveGrid
{
    /// <summary>
    /// 
    /// </summary>
    public class Obstacle
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="column"></param>
        /// <param name="offset"></param>
        /// <param name="perimeter"></param>
        /// <returns></returns>
        public static Obstacle FromColumn(Column column, double offset = 0, bool perimeter = false)
        {
            var p = column.Profile.Perimeter.TransformedPolygon(
                new Transform(column.Location));
            List<Vector3> points = new List<Vector3>();
            points.AddRange(p.Vertices);
            points.AddRange(p.Vertices.Select(
                v => new Vector3(v.X, v.Y, v.Z + column.Height)));
            return new Obstacle(points, offset, perimeter, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="offset"></param>
        /// <param name="perimeter"></param>
        /// <returns></returns>
        public static Obstacle FromWall(StandardWall wall, double offset = 0, bool perimeter = false)
        {
            var ortho = wall.CenterLine.Direction().Cross(Vector3.ZAxis);
            List<Vector3> points = new List<Vector3>();
            points.Add(wall.CenterLine.Start + ortho * wall.Thickness / 2);
            points.Add(wall.CenterLine.End + ortho * wall.Thickness / 2);
            points.Add(wall.CenterLine.Start - ortho * wall.Thickness / 2);
            points.Add(wall.CenterLine.End - ortho * wall.Thickness / 2);
            points.AddRange(points.Select(v => new Vector3(v.X, v.Y, v.Z + wall.Height)).ToArray());
            var transfrom = new Transform(Vector3.Origin,
                wall.CenterLine.Direction(), ortho, Vector3.ZAxis);
            return new Obstacle(points, offset, perimeter, transfrom);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="box"></param>
        /// <param name="offset"></param>
        /// <param name="perimeter"></param>
        /// <returns></returns>
        public static Obstacle FromBBox(BBox3 box, double offset = 0, bool perimeter = false)
        {
            List<Vector3> points = new List<Vector3>() { box.Min, box.Max };
            return new Obstacle(points, offset, perimeter, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polyon"></param>
        /// <param name="height"></param>
        /// <param name="offset"></param>
        /// <param name="perimeter"></param>
        /// <returns></returns>
        public static Obstacle From2DPolygon(Polygon polyon, double height, double offset = 0, bool perimeter = false)
        {
            List<Vector3> points = new List<Vector3>();
            points.AddRange(polyon.Vertices.Select(p => new Vector3(p.X, p.Y)));
            points.AddRange(polyon.Vertices.Select(p => new Vector3(p.X, p.Y, height)));
            return new Obstacle(points, offset, perimeter, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="offset"></param>
        /// <param name="perimeter"></param>
        /// <returns></returns>
        public static Obstacle FromLine(Line line, double offset = 0, bool perimeter = false)
        {
            List<Vector3> points = new List<Vector3>();
            points.Add(line.Start);
            points.Add(line.End);

            Transform frame = null;
            var forward = line.Direction();
            if (!forward.IsParallelTo(Vector3.ZAxis))
            {
                var rigth = forward.Cross(Vector3.ZAxis);
                var up = forward.Cross(rigth);
                frame = new Transform(Vector3.Origin, forward, rigth, up);
            }

            return new Obstacle(points, offset, perimeter, frame);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="offset"></param>
        /// <param name="perimeter"></param>
        /// <param name="transformation"></param>
        public Obstacle(List<Vector3> points, double offset, bool perimeter, Transform transformation)
        {
            Points = points;
            Offset = offset;
            Perimeter = perimeter;
            Transform = transformation;
        }

        /// <summary>
        /// 
        /// </summary>
        public List<Vector3> Points { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double Offset { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Perimeter { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Transform Transform { get; private set; }
    }
}
