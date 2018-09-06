using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace AECSpaces
{
    /// <summary>
    /// Collects a number of utilities for geometry inquiry and calculation.
    /// </summary>
    public class AECGeometry
    {
        public const double oneThird = 0.33333333333;

        public enum Compass
        {
            N, NNE, NE, ENE, E, ESE, SE, SSE, S, SSW, SW, WSW, W, WNW, NW, NNW
        };//enum
 
        private GeometryFactory geoFactory;

        /// <summary>
        /// Contructor creates instances of helpful classes.
        /// </summary>
        public AECGeometry()
        {
            geoFactory = new GeometryFactory();
        }//contructor

        /// <summary>
        /// Returns the angle in radians from two points.
        /// </summary>
        public double AngleFromPoints(AECPoint thisPoint, AECPoint thatPoint)
        {
            return Math.Atan2(thatPoint.Y - thisPoint.Y, thatPoint.X - thisPoint.X);              
        }//method

        /// <summary>
        /// Returns whether all delivered points are colinear.
        /// </summary>
        public bool AreColinear(AECPoint thisPoint, AECPoint thatPoint, AECPoint othrPoint)
        {
            double thisSlope = (thatPoint.Y - thisPoint.Y) / (thatPoint.X - thisPoint.X);
            double thatSlope = (othrPoint.Y - thatPoint.Y) / (othrPoint.X - thatPoint.X);
            if (thisSlope == thatSlope) return true;
            return false;
        }//method

        /// <summary>
        /// Returns whether boundaries constructed from two sequences of anticlockwise points overlap one another.
        /// </summary>
        public bool AreOverlapping(List<AECPoint> thisShape, List<AECPoint> thatShape)
        {
            Coordinate[] theseCoords = PointsToCoordsLoop(thisShape);
            Coordinate[] thoseCoords = PointsToCoordsLoop(thatShape);

            Polygon thisBoundary = (Polygon)geoFactory.CreatePolygon(new LinearRing(theseCoords));
            Polygon thatBoundary = (Polygon)geoFactory.CreatePolygon(new LinearRing(thoseCoords));
            return thisBoundary.Overlaps(thatBoundary);
        }//method

        /// <summary>
        /// Returns a box on the zero plane derived from two diagonal points.
        /// </summary>
        public static AECBox Box(AECPoint thisPoint, AECPoint thatPoint)
        {
            AECBox box = new AECBox();
            if (thisPoint.IsColocated(thatPoint) ||
                thisPoint.X == thatPoint.X ||
                thisPoint.Y == thatPoint.Y) return box;
            if (thisPoint.X < thatPoint.X)
            {
                box.SW = thisPoint;
                box.NE = thatPoint;
                box.NW = new AECPoint(thisPoint.X, thatPoint.Y);
                box.SE = new AECPoint(thatPoint.X, thisPoint.Y);
            }//end if
            else
            {
                box.SW = thatPoint;
                box.NE = thisPoint;
                box.NW = new AECPoint(thatPoint.X, thisPoint.Y);
                box.SE = new AECPoint(thisPoint.X, thatPoint.Y);
            }//end if
            return box;
        }//method

        /// <summary>
        /// Returns a box on the zero plane derived from an origin and x y distances.
        /// </summary>
        public static AECBox Box(AECPoint thisPoint, double xWidth, double yWidth)
        {
            return
            new AECBox
            {
                SW = thisPoint,
                SE = new AECPoint(thisPoint.X + xWidth, thisPoint.Y),
                NE = new AECPoint(thisPoint.X + xWidth, thisPoint.Y + yWidth),
                NW = new AECPoint(thisPoint.X, thisPoint.Y + yWidth)
            };
        }//method

        /// <summary>
        /// Returns the endpoints of a line from the center of a box to the specified corresponding compass as defined by a interval along the box point.
        /// </summary>
        public static List<AECPoint> CompassLine(AECBox box, Compass orient)
        {
            if (orient < Compass.N || orient > Compass.NNW) return null;
            AECPoint center = MidPoint(box.SW, box.NE);
            AECPoint point = CompassPoint(box, orient);
            List<AECPoint> line = new List<AECPoint>
            {
                center,
                point
            };
            return line;
        }//method

        /// <summary>
        /// Returns the specified corresponding compass point on a box as defined by an interval along the box boundary. North is the maximum Y side of the box, East the maximum X, South the minimum Y, West the minimum X.
        /// </summary>
        public static AECPoint CompassPoint(AECBox box, Compass orient)
        {

            if (orient < Compass.N || orient > Compass.NNW) return null;
            AECPoint point = new AECPoint();
            switch (orient)
            {
                case Compass.N: point = MidPoint(box.NW, box.NE); break;
                case Compass.E: point = MidPoint(box.NE, box.SE); break;
                case Compass.S: point = MidPoint(box.SW, box.SE); break;
                case Compass.W: point = MidPoint(box.SW, box.NW); break;
                case Compass.NE: point = box.NE; break;
                case Compass.SE: point = box.SE; break;
                case Compass.SW: point = box.SW; break;
                case Compass.NW: point = box.NW; break;
                case Compass.NNW: point = MidPoint(box.NW, MidPoint(box.NW, box.NE)); break;
                case Compass.NNE: point = MidPoint(box.NE, MidPoint(box.NW, box.NE)); break;
                case Compass.SSW: point = MidPoint(box.SW, MidPoint(box.SW, box.SE)); break;
                case Compass.SSE: point = MidPoint(box.SE, MidPoint(box.SW, box.SE)); break;
                case Compass.ENE: point = MidPoint(box.NE, MidPoint(box.NE, box.SE)); break;
                case Compass.ESE: point = MidPoint(box.SE, MidPoint(box.SE, box.NE)); break;
                case Compass.WNW: point = MidPoint(box.NW, MidPoint(box.SW, box.NW)); break;
                case Compass.WSW: point = MidPoint(box.SW, MidPoint(box.SW, box.NW)); break;
                default:
                    break;
            }//switch
            return point;
        }//method

        /// <summary>
        /// Returns an open list of AECPoints from NetTopologySuite Coordinates. Suitable for construction of AECSpace boundaries.
        /// </summary>
        public static List<AECPoint> CoordsToPoints(Coordinate[] coords)
        {
            int index = 0;
            List<AECPoint> points = new List<AECPoint>();
            while (index < coords.Length - 1)
            {
                if (coords[index] == null)
                {
                    index++;
                    continue;
                }//if
                points.Add(new AECPoint(coords[index].X, coords[index].Y, 0));
                index++;
            }//while
            return points;
        }//method

        /// <summary>
        /// Returns whether the boundary constructed from a sequence of anticlockwise points covers a point.
        /// </summary>
        public bool CoversPoint(List<AECPoint> shape, AECPoint point)
        {
            Coordinate[] coords = PointsToCoordsLoop(shape);
            Point tstPoint = new Point(point.CoordinateXYZ);
            Polygon boundary = (Polygon)geoFactory.CreatePolygon(new LinearRing(coords));
            return boundary.Covers(tstPoint);
        }//method

        /// <summary>
        /// Returns whether the first boundary constructed from a sequence of anticlockwise points contains a second boundary also constructed from a sequence of anticlockwise points.
        /// </summary>
        ///
        public bool CoversShape(List<AECPoint> thisShape, List<AECPoint> thatShape)
        {
            Coordinate[] theseCoords = PointsToCoordsLoop(thisShape);
            Coordinate[] thoseCoords = PointsToCoordsLoop(thatShape);
            Polygon thisBoundary = (Polygon)geoFactory.CreatePolygon(new LinearRing(theseCoords));
            Polygon thatBoundary = (Polygon)geoFactory.CreatePolygon(new LinearRing(thoseCoords));
            return thisBoundary.Covers(thatBoundary);
        }//method

        /// <summary>
        /// Returns the distance between two points..
        /// </summary>
        public static double Distance(AECPoint thisPoint, AECPoint thatPoint)
        {
            return Math.Sqrt(Math.Pow((thatPoint.X - thisPoint.X), 2) + Math.Pow((thatPoint.Y - thisPoint.Y), 2));
        }//method

        /// <summary>
        /// Determines whether a vertex point position represents a concave or convex angle relative to the adjacent 2D points.
        /// </summary>
        /// 
        public static bool IsConvexAngle(AECPoint vtxPoint, AECPoint prvPoint, AECPoint nxtPoint)
        {
            double inVectorX = vtxPoint.X - prvPoint.X;
            double inVectorY = vtxPoint.Y - prvPoint.Y;
            double outVectorX = nxtPoint.X - vtxPoint.X;
            double outVectorY = nxtPoint.Y - vtxPoint.Y;
            if (((inVectorX * outVectorY) - (inVectorY * outVectorX)) < 0) return false;
            return true;
        }//method

        /// <summary>
        /// Determines whether a 2D polygon is concave or convex.
        /// </summary>
        ///
        public static bool IsConvexPolygon(List<AECPoint> points)
        {
            int index = 0;
            int length = points.Count;
            while (index < length)
            {
                if (IsConvexAngle(vtxPoint: points[index],
                                  prvPoint: points[Mod(index - 1, length)],
                                  nxtPoint: points[Mod(index + 1, length)])
                                  == false)
                { return false; }
                index++;
            }//while
            return true;
        }//method

        /// <summary>
        /// Returns a 2D mesh of an arbitrary simple polygon boundary described by an anti-clockwise sequence of points.
        /// </summary>
        public AECMesh2D Mesh2D(List<AECPoint> points)
        {
            //put in too few points Exception

            AECMesh2D mesh = new AECMesh2D
            {
                vertices = new List<AECPoint>(points.ToArray())
            };
            if (points.Count == 3)
            {
                mesh.indices.Add(new AECAddress(0, 1, 2));
                return mesh;
            }//if
            
            List<AECPoint> tstPoints = new List<AECPoint>(points.ToArray());
            List<AECPoint> triangle = new List<AECPoint>();
            AECPoint point = new AECPoint();
            AECPoint prvPoint = new AECPoint();
            AECPoint nxtPoint = new AECPoint();
            while (tstPoints.Count >= 3)
            {
                int index = 0;
                bool convex = false;
                bool cvrShape = false;
                bool cvrPoint = true;
                while (!convex || !cvrShape || cvrPoint)
                {
                    index++;
                    point = tstPoints[index];
                    prvPoint = tstPoints[Mod(index - 1, tstPoints.Count)];
                    nxtPoint = tstPoints[Mod(index + 1, tstPoints.Count)];
                    convex = IsConvexAngle(point, prvPoint, nxtPoint);
                    if (!convex) continue;
                    if (tstPoints.Count > 3)
                    {
                        triangle.Clear();
                        triangle.Add(prvPoint);
                        triangle.Add(point);
                        triangle.Add(nxtPoint);
                        cvrShape = CoversShape(points, triangle);
                        if(!cvrShape) continue;
                        foreach (AECPoint tstPoint in tstPoints)
                        {
                            if (tstPoint == prvPoint || tstPoint == point || tstPoint == nxtPoint)
                            {
                                continue;
                            }//if
                            cvrPoint = CoversPoint(triangle, tstPoint);
                            if (cvrPoint) break;
                        }//foreach
                    }//if
                    else
                    {
                        convex = true;
                        cvrShape = true;
                        cvrPoint = false;
                    }//else
                }//while
                mesh.indices.Add(new AECAddress(X: points.IndexOf(point),
                                                  Y: points.IndexOf(nxtPoint),
                                                  Z: points.IndexOf(prvPoint)));
                tstPoints.RemoveAt(index);
            }//while
             return mesh;
        }//method

        /// <summary>
        /// Returns a mesh of a quadrilateral polygon boundary.
        /// </summary>
        ///
        public AECMesh2D MeshBox(AECBox box)
        {
            AECMesh2D mesh = new AECMesh2D();
            mesh.vertices.Add(box.SW);
            mesh.vertices.Add(box.SE);
            mesh.vertices.Add(box.NE);
            mesh.vertices.Add(box.NW);
            mesh.indices.Add(new AECAddress(0, 1, 2));
            mesh.indices.Add(new AECAddress(2, 3, 0));
            return mesh;
        }//method

        /// <summary>
        /// Returns the midpoint between two 3D points.
        /// </summary>
        ///
        public static AECPoint MidPoint(AECPoint start, AECPoint end)
        {
            double xCoord = (start.X + end.X) * 0.5;
            double yCoord = (start.Y + end.Y) * 0.5;
            double zCoord = (start.Z + end.Z) * 0.5;
            return new AECPoint(X: xCoord, Y: yCoord, Z: zCoord);
        }//method

        /// <summary>
        /// Returns an index within the delivered modulus.
        /// </summary>
        ///
        public static int Mod(int index, int modulus)
        {
            if (modulus == 0) return 0;
            return ((index % modulus) + modulus) % modulus;
        }//method

        /// <summary>
        /// Returns the normal of a plane determined by three points.
        /// </summary>
        public static AECVector Normal(AECBox box)
        {
            AECVector preVector = new AECVector(box.SE, box.SW);
            AECVector nxtVector = new AECVector(box.NW, box.SW);
            return preVector.CrossProduct(nxtVector).Unit;
        }//method

        /// <summary>
        /// Returns a point a percentage length along a line segment defined by two points.
        /// </summary>
        public static AECPoint PointAlong(AECPoint thisPoint, AECPoint thatPoint, double fraction)
        {
            fraction = Math.Abs(fraction);
            while (fraction > 1) fraction *= 0.1;
            List<AECPoint> points = new List<AECPoint>
            {
                thisPoint,
                thatPoint
            };
            Coordinate[] coords = PointsToCoordsLoop(points);
            LineSegment line = new LineSegment(coords[0], coords[1]);
            coords[0] = line.PointAlong(fraction);
            coords[1] = null;
            return CoordsToPoints(coords)[0];
        }//method

        /// <summary>
        /// Returns a list of NetTopologySuite Coordinates from a list of AECPoints.
        /// </summary>
        ///
        public static Coordinate[] PointsToCoords(List<AECPoint> points)
        {
            int index = 0;
            Coordinate[] coords = new Coordinate[points.Count];
            while (index < points.Count)
            {
                if (points[index] == null)
                {
                    index++;
                    continue;
                }//if
                coords[index] = points[index].CoordinateXYZ;
                index++;
            }//while
            return coords;
        }//method

        /// <summary>
        /// Returns a closed list of NetTopologySuite Coordinates bracketed by identical point values converted from a unrepeating list of AECPoints. Suitable for construction of NetTopologySuite Polygons.
        /// </summary>
        ///
        public static Coordinate[] PointsToCoordsLoop(List<AECPoint> points)
        {
            Coordinate[] coords = PointsToCoords(points);
            Coordinate[] coordsLoop = new Coordinate[points.Count + 1];
            coords.CopyTo(coordsLoop, 0);
            coordsLoop[coords.Length] = coords[0];
            return coordsLoop;
        }//method

        /// <summary>
        /// Returns a random double between the delivered minimum and maximum values.
        /// </summary>
        public static double RandomDouble(double min, double max)
        {
            Random rng = new Random();
            double half_min = min / 2.0;
            double half_max = max / 2.0;
            double average = half_min + half_max;
            double factor = max - average;
            return (2.0 * rng.NextDouble() - 1.0) * factor + average;
        }//method

        /// <summary>
        /// Returns a list of points with colinear points removed.
        /// </summary>
        ///
        public List<AECPoint> RemoveColinear(List<AECPoint> points)
        {
            if (points.Count <= 2) return points;
            int index = 0;
            List<int> indices = new List<int>();
            points.Reverse();
            for (int idx = 0; idx < 3; idx++)
            {
                while (index < points.Count)
                {
                    if (AreColinear(points[Mod(index, points.Count)],
                                    points[Mod(index + 1, points.Count)],
                                    points[Mod(index + 2, points.Count)]))
                    {
                        points.RemoveAt(index + 1);
                        index++;
                    }//if
                    index++;
                }//while
            }
            points.Reverse();
            return points;
        }//method

    }//class
}//namespace