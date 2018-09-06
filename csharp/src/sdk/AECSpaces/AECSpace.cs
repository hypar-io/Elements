using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace AECSpaces
{
    /// <summary>
    /// Space represents an airtight volume defined by a list of 2D anti-clockwise points, a level, and a height.
    /// Spaces floors and ceilings are parallel to the coordinate plane, walls are vertical.
    /// Curved walls are rationalized to a series of planes.
    /// </summary>
    public class AECSpace
    {
        private AECAddress address;
        private AECGeometry aecGeometry;
        private Polygon boundary;
        private AECColor color;
        private bool convex;
        private Dictionary<string, string> data;
        private string department;
        private GeometryFactory geoFactory;
        private double height;
        private readonly string id;
        private double level;
        private string name;
        private int occupancy;
        private string spaceType;
        private string tag;
        
        private List<AECPoint> points;

        /// <summary>
        /// Contructor accepts a list of aecSpace.Point instances to represent a new boundary.
        /// </summary>
        public AECSpace(List<AECPoint> flrPoints = null)
        {

            Address = new AECAddress(X: 0, Y: 0, Z: 0);
            aecGeometry = new AECGeometry();
            Boundary = new Polygon(null);
            color.RGB(AECColor.White);
            color.A = AECColor.Opaque;

            Department = "";
            data = new Dictionary<string, string>();
            geoFactory = new GeometryFactory();
            Height = 1.0;
            id = Guid.NewGuid().ToString();
            Level = 0;
            Name = "";
            Occupancy = 0;
            spaceType = "";
            Tag = "";
            if (flrPoints == null)
            {
                flrPoints = new List<AECPoint>
                {
                    new AECPoint(X: 0, Y: 0, Z: Level),
                    new AECPoint(X: 1, Y: 0, Z: Level),
                    new AECPoint(X: 1, Y: 1, Z: Level),
                    new AECPoint(X: 0, Y: 1, Z: Level)
                };
            }// if
            SetBoundary(flrPoints);
        }//contructor

        /* -------- Begin Properties -------- */

        /// <summary>
        /// A 3-digit coordinate indicating the position of a space in a grid.
        /// </summary>
        public AECAddress Address
        {
            get => address;
            set => address = value;
        }//property

        /// <summary>
        /// Returns the area of the space.
        /// </summary>
        public double Area
        {
            get => boundary.Area;
        }//property       

        /// <summary>
        /// Returns the longer of the two orthogonal bounding box axes as two endpoints. If both axes are the same length, returns the x-axis endpoints.
        /// </summary>
        public List<AECPoint> AxisMajor
        {
            get
            {
                AECBox box = PointsBox;
                List<AECPoint> axis = new List<AECPoint>();
                double xDelta = Math.Abs(box.SE.X - box.SW.X);
                double yDelta = Math.Abs(box.NE.Y - box.SE.Y);
                if (xDelta >= yDelta) return AxisX;
                else return AxisY;
            }//get
        }//property

        /// <summary>
        /// Returns the shorter of the two orthogonal bounding box axes as two endpoints. If both axes are the same length, returns the y-axis endpoints.
        /// </summary>
        public List<AECPoint> AxisMinor
        {
            get
            {
                AECBox box = PointsBox;
                List<AECPoint> axis = new List<AECPoint>();
                double xDelta = Math.Abs(box.SE.X - box.SW.X);
                double yDelta = Math.Abs(box.NE.Y - box.SE.Y);
                if (xDelta < yDelta) return AxisX;
                else return AxisY;
            }//get
        }//property

        /// <summary>
        /// Returns the central x-axis of the bounding box as two Points, minimum x value followed by maximum x value..
        /// </summary>
        public List<AECPoint> AxisX
        {
            get
            {
                AECBox box = PointsBox;
                List<AECPoint> axis = new List<AECPoint>
                {
                    AECGeometry.MidPoint(box.SW, box.NW),
                    AECGeometry.MidPoint(box.SE, box.NE)
                };
                return axis;
            }//get
        }//property

        /// <summary>
        /// Returns the central y-axis of the bounding box as two Points, minimum y value followed by maximum y value.
        /// </summary>
        public List<AECPoint> AxisY
        {
            get
            {
                AECBox box = PointsBox;
                List<AECPoint> axis = new List<AECPoint>
                {
                    AECGeometry.MidPoint(box.SW, box.SE),
                    AECGeometry.MidPoint(box.NW, box.NE)
                };
                return axis;
            }//get
        }//property

        /// <summary>
        /// A representation of the boundary as a Polygon.
        /// </summary>
        public Polygon Boundary
        {
            get => boundary;
            set => boundary = value;
        }//property

        /// <summary>
        /// A representation of the bounding box as a Polygon.
        /// </summary>
        public Polygon BoundingBox
        {
            get
            {
                AECBox box = PointsBox;
                Coordinate[] coords = new Coordinate[5];
                coords[0] = box.SW.CoordinateXY;
                coords[1] = box.SE.CoordinateXY;
                coords[2] = box.NE.CoordinateXY;
                coords[3] = box.NW.CoordinateXY;
                coords[4] = box.SW.CoordinateXY;
                return (Polygon)geoFactory.CreatePolygon(new LinearRing(coords));
            }//get
        }//property

        /// <summary>
        /// Returns the center Point of the ceiling bounding box.
        /// </summary>
        public AECPoint CenterCeiling
        {
            get
            {
                AECPoint center = CenterFloor;
                center.Z = Elevation;
                return center;
            }//get
        }//property

        /// <summary>
        /// Returns the center Point of the floor bounding box.
        /// </summary>
        public AECPoint CenterFloor
        {
            get
            {
                AECBox box = PointsBox;
                AECPoint center = AECGeometry.MidPoint(box.SW, box.NE);
                center.Z = Level;
                return center;
            }//get
        }//property

        /// <summary>
        /// Returns the center Point of the space bounding cube.
        /// </summary>
        public AECPoint CenterSpace
        {
            get
            {
                AECPoint center = CenterFloor;
                center.Z = Level + (Height * 0.5);
                return center;
            }//get
        }//property

        /// <summary>
        /// Returns the centroid Point of the ceiling boundary.
        /// </summary>
        public AECPoint CentroidCeiling => new AECPoint(boundary.Centroid.X, boundary.Centroid.Y, Elevation); //property

        /// <summary>
        /// Returns the centroid Point of the floor boundary.
        /// </summary>
        public AECPoint CentroidFloor => new AECPoint(boundary.Centroid.X, boundary.Centroid.Y, Level); //property

        /// <summary>
        /// Returns the centroid..
        /// </summary>
        public AECPoint CentroidSpace => new
                AECPoint(boundary.Centroid.X, boundary.Centroid.Y, Level + (Height * 0.5));//property

        /// <summary>
        /// Returns the circumference.
        /// </summary>
        public double Circumference => boundary.Length; //property

        /// <summary>
        /// The requested rendering color.
        /// </summary>
        public AECColor Color
        {
            get => color;
            set => color = value;
        }//property

        /// <summary>
        /// Returns the convex status of the boundary.
        /// </summary>
        public bool Convex { get => convex; }

        /// <summary>
        /// Sets and gets the department name.
        /// </summary>
        public string Department
        {
            get => department;
            set => department = value;
        }//property

        /// <summary>
        /// Returns the Level + Height.
        /// </summary>
        public double Elevation
        {
            get { return Level + Height; }
        }//property

        /// <summary>
        /// Returns the name of the function.
        /// </summary>
        public string Function
        {
            get => spaceType;
            set => spaceType = value;
        }//property

        /// <summary>
        /// Positive elevation of the space upper surface in relation to its Level.
        /// </summary>
        public double Height
        {
            get => height;
            set => height = Math.Abs(value);
        }//property

        /// <summary>
        /// Returns the unique id.
        /// </summary>
        public string ID
        {
            get { return id; }
        }//ID

        /// <summary>
        /// Returns the location of the space above the zero plane.
        /// </summary>
        public double Level
        {
            get => level;
            set => level = value;
        }//property

        /// <summary>
        /// Returns a mesh representation of the space composed of points, indices, and normals.
        /// </summary>
        public AECMesh Mesh
        {
            get
            {
                AECMesh mesh = new AECMesh();
                AECMesh2D flrMesh = MeshFloor;
                AECMesh2D clgMesh = MeshCeiling;
                List<AECMesh2D> sidMeshes = MeshesSides;
                mesh.indices = new List<AECAddress>(flrMesh.indices.ToArray());
                foreach (AECAddress address in clgMesh.indices)
                {
                    address.Offset(flrMesh.vertices.Count);
                    mesh.indices.Add(address);
                }//foreach
                mesh.vertices = new List<AECPoint>(flrMesh.vertices.ToArray());
                mesh.vertices.AddRange(new List<AECPoint>(clgMesh.vertices.ToArray()));
                foreach (AECPoint vertex in flrMesh.vertices)
                {
                    mesh.normals.Add(flrMesh.normal);
                }//foreach
                foreach (AECPoint vertex in clgMesh.vertices)
                {
                    mesh.normals.Add(clgMesh.normal);
                }//foreach            
                foreach (AECMesh2D sidMesh in sidMeshes)
                {
                    foreach (AECPoint vertex in sidMesh.vertices)
                    {
                        mesh.normals.Add(sidMesh.normal);
                    }//foreach
                    foreach (AECAddress address in sidMesh.indices)
                    {
                        address.Offset(mesh.vertices.Count);
                        mesh.indices.Add(address);
                    }//foreach
                    mesh.vertices.AddRange(sidMesh.vertices.ToArray());
                }//foreach         
                return mesh;
            }//get
        }//property

        /// <summary>
        /// Returns a mesh representation of the ceiling plane composed of points and indices.
        /// </summary>
        public AECMesh2D MeshCeiling
        {
            get
            {
                AECMesh2D mesh = aecGeometry.Mesh2D(PointsCeiling);
                mesh.normal = NormalCeiling;
                return mesh;
            }//get
        }//property

        /// <summary>
        /// Returns a mesh representation of the floor plane composed of points and indices.
        /// </summary>
        public AECMesh2D MeshFloor
        {
            get
            {
                AECMesh2D mesh = aecGeometry.Mesh2D(PointsFloor);
                mesh.normal = NormalFloor;
                return mesh;
            }//get
        }//property

        /// <summary>
        /// Returns a series of mesh representations of the side planes.
        /// </summary>
        public List<AECMesh2D> MeshesSides
        {
            get
            {
                List<AECBox> sides = PointsSides;
                List<AECMesh2D> meshes = new List<AECMesh2D>();
                AECMesh2D mesh = new AECMesh2D();
                foreach(AECBox side in sides)
                {
                    mesh = aecGeometry.MeshBox(side);
                    mesh.normal = AECGeometry.Normal(side);
                    meshes.Add(mesh);
                }//foreach
                return meshes;
            }//get
        }//property

        /// <summary>
        /// Returns the GLTF compatible mesh form of the space.
        /// </summary>
        public Hypar.Elements.Mass MeshGLTF
        {
            get
            {
                List<Hypar.Geometry.Vector3> vertices = new List<Hypar.Geometry.Vector3>();
                foreach (AECPoint point in PointsFloor)
                {
                    vertices.Add(new Hypar.Geometry.Vector3(point.X, point.Y, point.Z));
                }//foreach
                Hypar.Geometry.Polyline boundary = new Hypar.Geometry.Polyline(vertices);
                Hypar.Elements.Mass mass = Hypar.Elements.Mass.WithBottomProfile(boundary)
                                                         .WithTopAtElevation(Elevation)
                                                         .WithTopProfile(boundary)
                                                         .WithBottomAtElevation(Level);
                return mass;
            }//get
        }//property

        /// <summary>
        /// Returns a mesh representation as a structure of flat lists of doubles indicating #D vertex coordinates, triangle indices, and 3D normal vectors for each point.
        /// </summary>
        public AECMeshGraphic MeshGraphic
        {
            get
            {
                AECMesh mesh = Mesh;
                AECMeshGraphic meshGraphic = new AECMeshGraphic();
                foreach (AECPoint vertex in mesh.vertices)
                {
                    meshGraphic.vertices.Add(vertex.X);
                    meshGraphic.vertices.Add(vertex.Y);
                    meshGraphic.vertices.Add(vertex.Z);
                }//foreach
                foreach (AECAddress address in mesh.indices)
                {
                    meshGraphic.indices.Add(address.x);
                    meshGraphic.indices.Add(address.y);
                    meshGraphic.indices.Add(address.z);
                }//foreach
                foreach (AECVector vector in mesh.normals)
                {
                    meshGraphic.normals.Add(vector.X);
                    meshGraphic.normals.Add(vector.Y);
                    meshGraphic.normals.Add(vector.Z);
                }//foreach
                return meshGraphic;
            }//get
        }//property

        /// <summary>
        /// Name of the space.
        /// </summary>
        ///
        public string Name
        {
            get => name;
            set => name = value;
        }//property

        /// <summary>
        /// Returns a point representing the ceiling normal.
        /// </summary>
        public AECVector NormalCeiling
        {
            get
            {
                AECBox box = PointsBox;
                AECVector normal = AECGeometry.Normal(box);
                return normal;
            }//get
        }//property

        /// <summary>
        /// Returns a point representing the floor normal.
        /// </summary>
        public AECVector NormalFloor
        {
            get
            {
                AECBox box = PointsBox;
                AECVector normal = AECGeometry.Normal(box);
                normal.Multiply(-1);
                return normal;
            }//get
        }//property

        /// <summary>
        /// Returns a list of structures representing points defining sides of the space, each with an associated normal.
        /// </summary>
        public List<AECSpaceSide> NormalSides
        {
            get
            {
                List<AECSpaceSide> sideNormals = new List<AECSpaceSide>();
                List<AECBox> sides = PointsSides;
                AECVector normal = new AECVector();
                foreach(AECBox side in sides)
                {
                    normal = AECGeometry.Normal(side);
                    sideNormals.Add(new AECSpaceSide(side, normal));
                }//foreach
                return sideNormals;
            }//get
        }//property

        /// <summary>
        /// Sets or gets the occupancy.
        /// </summary>
        public int Occupancy
        {
            get => occupancy;
            set => occupancy = value;
        }//property

        /// <summary>
        /// Returns a random point within the space boundary at the ceiling level.
        /// </summary>
        public AECPoint PointCeiling
        {
            get
            {
                NetTopologySuite.Geometries.Point intPoint = 
                    (NetTopologySuite.Geometries.Point)Boundary.PointOnSurface;
                return new AECPoint(intPoint.X, intPoint.Y, Elevation);
            }//get
        }//property

        /// <summary>
        /// Returns a random point within the space boundary at the floor level.
        /// </summary>
        ///
        public AECPoint PointFloor
        {
            get
            {
                NetTopologySuite.Geometries.Point intPoint =
                    (NetTopologySuite.Geometries.Point)Boundary.PointOnSurface;
                return new AECPoint(intPoint.X, intPoint.Y, Level);
            }//get
        }//property

        /// <summary>
        /// Returns a random point within the space at a random elevation.
        /// </summary>
        ///
        public AECPoint PointSpace
        {
            get
            {
                NetTopologySuite.Geometries.Point intPoint =
                    (NetTopologySuite.Geometries.Point)Boundary.PointOnSurface;
                return new AECPoint(intPoint.X, intPoint.Y, AECGeometry.RandomDouble(Level, Elevation));
            }//get
        }//property

        /// <summary>
        /// Returns the Points of the bounding box.
        /// </summary>
        ///
        public AECBox PointsBox
        {
            get
            {
                Coordinate[] coordinates = boundary.Envelope.Coordinates;
                AECBox box;
                box.SW = new AECPoint(coordinates[0].X, coordinates[0].Y, Level);
                box.NW = new AECPoint(coordinates[1].X, coordinates[1].Y, Level);
                box.NE = new AECPoint(coordinates[2].X, coordinates[2].Y, Level);
                box.SE = new AECPoint(coordinates[3].X, coordinates[3].Y, Level);
                return box;
            }//get
        }//property

        /// <summary>
        /// Returns an anti-clockwise list of Points describing the ceiling boundary of the space.
        /// </summary>
        ///
        public List<AECPoint> PointsCeiling
        {
            get
            {
                List<AECPoint> retPoints = new List<AECPoint>();
                foreach (AECPoint point in points)
                {
                    retPoints.Add(new AECPoint(point.X, point.Y, Elevation));
                }//foreach
                return retPoints;
            }//get
        }//property

        /// <summary>
        /// Returns an anti-clockwise list of Points describing the floor boundary of the space.
        /// </summary>
        ///
        public List<AECPoint> PointsFloor
        {
            get
            {
                List<AECPoint> retPoints = new List<AECPoint>();
                foreach (AECPoint point in points)
                {
                    retPoints.Add(new AECPoint(point.X, point.Y, Level));
                }//foreach
                return retPoints;
            }//get
            set => SetBoundary(value);
        }//property

        /// <summary>
        /// Returns a list of Box structures describing each side of the space.
        /// </summary>
        ///
        public List<AECBox> PointsSides
        {
            get
            {
                List<AECPoint> clgPoints = PointsCeiling;
                List<AECPoint> flrPoints = PointsFloor;
                List<AECBox> sides = new List<AECBox>();
                int index = 0;
                int next = 0;
                int count = clgPoints.Count;
                while (index < count)
                {
                    next = AECGeometry.Mod((index + 1), count);
                    sides.Add(new AECBox(sw: flrPoints[index],
                                         se: flrPoints[next],
                                         ne: clgPoints[next],
                                         nw: clgPoints[index]));
                    index++;
                }//while
                return sides;
            }//get
        }//property

        /// <summary>
        /// Returns the x-axis size of the bounding box.
        /// </summary>
        ///  
        public double SizeX
        {
            get
            {
                AECBox box = PointsBox;
                return Math.Abs(box.SE.X - box.SW.X);
            }//get
        }//property

        /// <summary>
        /// Returns the y-axis size of the bounding box.
        /// </summary>
        /// 
        public double SizeY
        {
            get
            {
                AECBox box = PointsBox;
                return Math.Abs(box.NW.Y - box.SW.Y);
            }//get
        }//property

        /// <summary>
        /// Sets and gets the tag.
        /// </summary>
        public string Tag
        {
            get => tag;
            set => tag = value;
        }//property

        /// <summary>
        /// Sets and gets the spaceType.
        /// </summary>
        public string Type
        {
            get => spaceType;
            set => spaceType = value;
        }//property

        /// <summary>
        /// Returns the volume of the space.
        /// </summary>
        public double Volume
        {
            get { return Math.Abs(Boundary.Area * Height); }
        }//property

        /* -------- End Properties -------- */

        /// <summary>
        /// Combines a new set of points representing a closed boundary to the existing boundary to create a union.
        /// Returns True on success
        /// </summary>
        public bool Add(List<AECPoint> points)
        {
            if (points.Count < 3) return false;
            Coordinate[] coords = AECGeometry.PointsToCoordsLoop(points);
            Polygon thisBoundary = Boundary;
            Polygon thatBoundary = (Polygon)geoFactory.CreatePolygon(new LinearRing(coords));
            if(thisBoundary.Union(thatBoundary).GeometryType != "Polygon") return false;
            Polygon newBoundary = (Polygon)thisBoundary.Union(thatBoundary);
            newBoundary.Normalize();
            points = AECGeometry.CoordsToPoints(newBoundary.Coordinates);
            points.Reverse();
            return SetBoundary(aecGeometry.RemoveColinear(points));
        }//method

        /// <summary>
        /// Returns the endpoints of a line from the center of the bounding box to the specified corresponding compass as defined by a interval along the box perimeter.
        /// </summary>
        public List<AECPoint> CompassLine(AECGeometry.Compass orient)
        {
            if (orient < AECGeometry.Compass.N || orient > AECGeometry.Compass.NNW) return null;
            return AECGeometry.CompassLine(PointsBox, orient);
        }//method

        /// <summary>
        /// Returns the specified corresponding compass as defined by a interval along the bounding box perimeter.
        /// </summary>
        public AECPoint CompassPoint(AECGeometry.Compass orient)
        {
            if (orient < AECGeometry.Compass.N || orient > AECGeometry.Compass.NNW) return null;
            return AECGeometry.CompassPoint(PointsBox, orient);
        }//method

        /// <summary>
        /// If possible, reconfigures the boundary to conform to the boundary of the supplied space.
        /// </summary>
        public bool Contain(AECSpace container)
        {
            if (Boundary.Disjoint(container.Boundary)) return false;
            Polygon newBoundary = (Polygon)Boundary.Intersection(container.Boundary);
            return SetBoundary(AECGeometry.CoordsToPoints(newBoundary.Coordinates));
        }//method

        /// <summary>
        /// Returns true if the space boundary is contained by the supplied space as compared on the shared zero plane..
        /// </summary>
        public bool ContainedBy(AECSpace space)
        {
            return space.boundary.Covers(Boundary);
        }//method

        /// <summary>
        /// Returns true if the boundary contains the Point on the shared zero plane.
        /// </summary>
        public bool Contains(AECPoint point)
        {
            Coordinate coordinate = point.CoordinateXY;
            NetTopologySuite.Geometries.Point tstPoint = new NetTopologySuite.Geometries.Point(coordinate);
            return Boundary.Contains(tstPoint);
        }//method

        /// <summary>
        /// Returns true if the space boundary contains the shape as defined by the delivered points compared on the shared zero plane.
        /// </summary>
        public bool Contains(AECSpace space)
        {
             return Boundary.Covers(space.boundary);
        }//method

        /// <summary>
        /// Returns an exact copy of this space with a new ID.
        /// </summary>
        public AECSpace Copy()
        {
            AECSpace newSpace = new AECSpace(PointsFloor)
            {
                Color = Color,
                Height = Height,
                Level = Level,
                Name = Name
            };
            return newSpace;
        }//method

        /// <summary>
        /// Returns a copy of this space displaced along the supplied x, y, and z values.
        /// </summary>
        public AECSpace CopyMove(double xDelta = 0,
                                 double yDelta = 0,
                                 double zDelta = 0)
        {
            AECSpace newSpace = Copy();
            newSpace.MoveBy(xDelta: xDelta, yDelta: yDelta, zDelta: zDelta);
            return newSpace;
        }//method

        /// <summary>
        /// Returns this space and multiple copies of itself displaced along the supplied x, y, and z values, with each copy displaced from the previous space.
        /// </summary>
        public AECSpaceGroup CopyPlace(int copies = 1, 
                                       double xDelta = 0, 
                                       double yDelta = 0, 
                                       double zDelta = 0)
        {
            int index = 0;
            AECSpace newSpace;
            double displaceX = xDelta;
            double displaceY = yDelta;
            double displaceZ = zDelta;

            AECSpaceGroup spaceGroup = new AECSpaceGroup(this);
            while (index < copies)
            {
                newSpace = Copy();
                newSpace.MoveBy(xDelta: displaceX, yDelta: displaceY, zDelta: displaceZ);
                spaceGroup.Add(newSpace);
                displaceX += xDelta;
                displaceY += yDelta;
                displaceZ += zDelta;
                index++;
            }//while
            return spaceGroup;
        }//method

        /// <summary>
        /// Returns this space and multiple copies of itself displaced along the x or y axis by a delta determined by the relevant bounding box dimension added to the supplied gap value.
        /// </summary>
        public AECSpaceGroup CopyRow(int copies = 1, double gap = 0, bool xAxis = true)
        {
            if (xAxis) return CopyPlace(copies, xDelta: SizeX + gap);
            else return CopyPlace(copies, yDelta: SizeY + gap);
        }//method

        /// <summary>
        /// Returns this space and multiple copies of itself displaced along the z axis to arrive at or greater than a target area.
        /// </summary>
        public AECSpaceGroup CopyRowToArea(double targetArea, double gap = 0, bool xAxis = true)
        {
            int copies = (int)Math.Ceiling(targetArea / Area) - 1;
            return CopyRow(copies, gap: Height + gap, xAxis: xAxis);
        }//method

        /// <summary>
        /// Returns this space and multiple copies of itself displaced along the z axis by a delta determined by the height added to the supplied gap value.
        /// </summary>
        public AECSpaceGroup CopyStack(int copies = 1, double gap = 0)
        {
            return CopyPlace(copies, zDelta: Height + gap);
        }//method

        /// <summary>
        /// Returns this space and multiple copies of itself displaced along the z axis to arrive at or greater than a target area.
        /// </summary>
        public AECSpaceGroup CopyStackToArea(double targetArea, double gap = 0)
        {
            int copies = (int)Math.Ceiling(targetArea / Area) - 1;
            return CopyStack(copies, gap: gap);
        }//method

        /// <summary>
        /// If available, retrieves a string value from the data dictionary.
        /// </summary>
        public string DataGet(string key)
        {
            if (data.ContainsKey(key)) return data[key];
            return "";
        }//method

        /// <summary>
        /// If possible, sets a string value in the data dictionary.
        /// </summary>
        public void DataSet(string key, string value)
        {
            data.Add(key, value);
        }//method

        /// <summary>
        /// Returns one or more spaces representing the difference between this space and the delivered space.
        /// </summary>
        public AECSpaceGroup Difference(AECSpace thatSpace)
        {
            AECSpaceGroup diff = new AECSpaceGroup();
            if (Boundary.Disjoint(thatSpace.Boundary) || 
                Boundary.Touches(thatSpace.Boundary)) return diff;
            Geometry tstDiff = (Geometry)Boundary.Difference(thatSpace.Boundary);
            Polygon polygon;
            if (tstDiff.NumGeometries == 1)
            {
                polygon = (Polygon)tstDiff.GetGeometryN(0);
                diff.Add(new AECSpace(AECGeometry.CoordsToPoints(polygon.Coordinates)));
            }//if
            if (tstDiff.NumGeometries > 1)
            {
                int index = 0;
                while (index < tstDiff.NumGeometries)
                {
                    polygon = (Polygon)tstDiff.GetGeometryN(index);
                    diff.Add(new AECSpace(AECGeometry.CoordsToPoints(polygon.Coordinates)));
                    index++;
                }//while
            }//if
            return diff;
        }//method

        /// <summary>
        /// If possible, reconfigures the boundary, level and height to conform to the limits of the supplied space.
        /// </summary>
        public bool Enclose(AECSpace enclosure)
        {
            if (!Contain(enclosure)) return false;
            Level = enclosure.Level;
            if (Height > enclosure.Height) Height = enclosure.Height;
            return true;
        }//method

        /// <summary>
        /// Returns true if the space boundary fully encloses the delivered space within its boundary and between its level and height.
        /// </summary>
        public bool EnclosedBy(AECSpace space)
        {
            if (!space.Contains(this) || 
                space.Level > Level || 
                space.Height < Elevation)
                return false;
            return true;
        }//method

        /// <summary>
        /// Returns true if the boundary contains the Point between the Level and Elevation.
        /// </summary>
        public bool Encloses(AECPoint point)
        {
            if (!Contains(point) || 
                point.Z < Level || 
                point.Z > Elevation)
                return false;
            return true;
        }//method

        /// <summary>
        /// Returns true if the space boundary fully encloses the delivered space within its boundary and between its level and height.
        /// </summary>
        public bool Encloses(AECSpace space)
        {
            if (!Contains(space)) return false;
            if (Level <= space.Level && Height >= space.Height) return true;
            return false;
        }//method

        /// <summary>
        /// Returns one or more spaces representing the intersection between this space and the delivered space.
        /// </summary>
        public AECSpaceGroup Intersection(AECSpace thatSpace)
        {
            AECSpaceGroup diff = new AECSpaceGroup();
            if (Boundary.Disjoint(thatSpace.Boundary) ||
                Boundary.Touches(thatSpace.Boundary)) return diff;
            Geometry tstDiff = (Geometry)Boundary.Intersection(thatSpace.Boundary);
            Polygon polygon;
            if (tstDiff.NumGeometries == 1)
            {
                polygon = (Polygon)tstDiff.GetGeometryN(0);
                diff.Add(new AECSpace(AECGeometry.CoordsToPoints(polygon.Coordinates)));
            }//if
            if (tstDiff.NumGeometries > 1)
            {
                int index = 0;
                while (index < tstDiff.NumGeometries)
                {
                    polygon = (Polygon)tstDiff.GetGeometryN(index);
                    diff.Add(new AECSpace(AECGeometry.CoordsToPoints(polygon.Coordinates)));
                    index++;
                }//while
            }//if
            return diff;
        }//method

        /// <summary>
        /// Returns whether this space is exactly adjacent to the supplied space.
        /// </summary>
        public bool IsAdjacentTo(AECSpace space)
        {
            return Boundary.Touches(space.Boundary);
        }//method

        /// <summary>
        /// Returns whether this space is near to the supplied point as defined by the delivered distance compared to the distance between the centroid of this space and the supplied space.
        /// </summary>
        public bool IsNearTo(AECPoint thatPoint, double distance)
        {
            if (AECGeometry.Distance(CentroidFloor, thatPoint) <= distance)
                return true;
            return false;
        }//method

        /// <summary>
        /// Returns whether this space is near to the supplied space as defined by the delivered distance compared to the distance between the centroids of this space and the supplied space.
        /// </summary>
        public bool IsNearTo(AECSpace thatSpace, double distance)
        {
            if (AECGeometry.Distance(CentroidFloor, thatSpace.CentroidFloor) <= distance)
                return true;
            return false;
        }//method

        /// <summary>
        /// Horizontally mirrors the space around a line specified by two points. If two points aren't specified, the space mirrors around its y-axis.
        /// </summary>
        public bool Mirror(AECPoint thisPoint = null, AECPoint thatPoint = null)
        {
            if (thisPoint == null || thatPoint == null)
            {
                thisPoint = AxisY[0];
                thatPoint = AxisY[1];
            }//if
            AffineTransformation mirror = 
                AffineTransformation.ReflectionInstance(thisPoint.X, thisPoint.Y,
                                                        thatPoint.X, thatPoint.Y);
            Polygon newBoundary = (Polygon)mirror.Transform(Boundary);
            return SetBoundary(AECGeometry.CoordsToPoints(newBoundary.Coordinates));
        }//method

        /// <summary>
        /// Moves the space by moving each floor boundary point by the delivered vector and reconstructing the space.
        /// </summary>
        public bool MoveBy(double xDelta = 0, double yDelta = 0, double zDelta = 0)
        {
            List<AECPoint> points = new List<AECPoint>();
            foreach(AECPoint point in PointsFloor)
            {
                point.MoveBy(xDelta, yDelta);
                points.Add(point);
            }//foreach
            Level += zDelta;
            return SetBoundary(points);
        }//method

        /// <summary>
        /// Moves the space from a point to another point by moving each floor boundary point by a constructed vector and reconstructing the space.
        /// </summary>
        public bool MoveTo(AECPoint from, AECPoint to)
        {
            double x = to.X - from.X;
            double y = to.Y - from.Y;
            double z = to.Z - from.Z;
            return MoveBy(x, y, z);
        }//method

        /// <summary>
        /// Attempts to place this space at a random point within the boundary of the supplied space without changing either space.
        /// </summary>
        public bool PlaceWithin(AECSpace container)
        {
            if (Area > container.Area) return false;
            int trial = 0;
            AECPoint point = new AECPoint();
            AECBox box = container.PointsBox;
            AECSpace trialSpace = Copy();
            while(trial < 25)
            {
                point.X = AECGeometry.RandomDouble(box.SW.X, box.SE.X);
                point.Y = AECGeometry.RandomDouble(box.SW.Y, box.NW.Y);
                if (container.Contains(point))
                {
                    trialSpace.MoveTo(trialSpace.CentroidFloor, point);
                    if (trialSpace.ContainedBy(container))
                    {
                        MoveTo(CentroidFloor, point);
                        return true;
                    }//if
                }//if
                trial++;
            }//while
            return false;
        }//method

        /// <summary>
        /// Attepts to place this space at a random point within the supplied space along the specified compass line.
        /// </summary>
        public bool PlaceWithinCompass(AECSpace container, AECGeometry.Compass orient)
        {
            if (Area > container.Area) return false;
            int trial = 0;
            double fraction = 0;
            AECPoint point = new AECPoint();
            AECSpace trialSpace = Copy();
            List<AECPoint> line = AECGeometry.CompassLine(container.PointsBox, orient);
            while (trial < 25)
            {
                fraction = AECGeometry.RandomDouble(0, 1);
                point = AECGeometry.PointAlong(line[0], line[1], fraction);
                if (container.Contains(point))
                {
                    trialSpace.MoveTo(trialSpace.CentroidFloor, point);
                    if (trialSpace.ContainedBy(container))
                    {
                        MoveTo(CentroidFloor, point);
                        return true;
                    }//if
                }//if
                trial++;
            }//while
            return false;
        }//method

        /// <summary>
        /// Configures the boundary as a box whose southwest corner lies at the origin.
        /// </summary>
        public bool PlanBox(double xSize = 1, double ySize = 1)
        {
            if (xSize == 0) xSize = 1;
            else xSize = Math.Abs(xSize);
            if (ySize == 0) ySize = 1;
            else ySize = Math.Abs(ySize);
            return SetBoundary
            (
                new List<AECPoint>
                {
                    new AECPoint(0, 0, 0),
                    new AECPoint(xSize, 0, 0),
                    new AECPoint(xSize, ySize, 0),
                    new AECPoint(0, ySize, 0)
                }
            );
        }//method

        /// <summary>
        /// Configures the boundary as a polygon approximation of a circle whose center is coincident with the origin.
        /// </summary>
        public bool PlanCircle(double radius = 1)
        {
            if (radius <= 0) radius = 1;
            return PlanPolygon(radius: radius, sides: 72);
        }//method

        /// <summary>
        /// Configures the boundary as an E shape whose southwest bounding box corner is coincident with the origin. xWidth and YWidth define the widths of the x and y axis arms.
        /// </summary>
        public bool PlanE(double xSize = 1,
                          double ySize = 1,
                          double xWidth = -1,
                          double yWidthN = -1,
                          double yWidthM = -1,
                          double yWidthS = -1)
        {
            List<AECPoint> restorePoints = new List<AECPoint>(PointsFloor);
            try
            {
                if (xWidth <= 0 || xWidth > xSize) xWidth = Math.Round(xSize * 0.5, 8);
                if (yWidthN <= 0 || yWidthN > ySize * AECGeometry.oneThird)
                    yWidthN = Math.Round(ySize * 0.2, 8);
                if (yWidthM <= 0 || yWidthM > ySize * AECGeometry.oneThird)
                    yWidthM = Math.Round(ySize * 0.2, 8);
                if (yWidthS <= 0 || yWidthS > ySize * AECGeometry.oneThird)
                    yWidthS = Math.Round(ySize * 0.2, 8);

                if (!PlanBox(xWidth, ySize)) return false;

                AECBox box = AECGeometry.Box(new AECPoint(0, (ySize - yWidthN)), xSize, yWidthN);
                if (!Add(new List<AECPoint> { box.SW, box.SE, box.NE, box.NW })) return false;

                box = AECGeometry.Box(new AECPoint(0, (ySize * 0.5) - (yWidthM * 0.5)), xSize, yWidthM);
                if (!Add(new List<AECPoint> { box.SW, box.SE, box.NE, box.NW })) return false;

                box = AECGeometry.Box(new AECPoint(), xSize, yWidthS);
                return Add(new List<AECPoint> { box.SW, box.SE, box.NE, box.NW });
            }//try
            catch
            {
                SetBoundary(restorePoints);
                return false;
            }//catch
        }//method

        /// <summary>
        /// Configures the boundary as an F shape whose southwest bounding box corner is coincident with the origin. xWidth and YWidth define the widths of the x and y axis arms.
        /// </summary>
        public bool PlanF(double xSize = 1,
                          double ySize = 1,
                          double xWidth = -1,
                          double yWidthN = -1,
                          double yWidthM = -1)
        {
            List<AECPoint> restorePoints = new List<AECPoint>(PointsFloor);
            try
            {
                if (xWidth <= 0 || xWidth > xSize) xWidth = Math.Round(xSize * 0.5, 8);
                if (yWidthN <= 0 || yWidthN > ySize * AECGeometry.oneThird)
                    yWidthN = Math.Round(ySize * 0.2, 8);
                if (yWidthM <= 0 || yWidthM > ySize * AECGeometry.oneThird)
                    yWidthM = Math.Round(ySize * 0.2, 8);

                if (!PlanBox(xWidth, ySize)) return false;

                AECBox box = AECGeometry.Box(new AECPoint(0, (ySize - yWidthN)), xSize, yWidthN);
                if (!Add(new List<AECPoint> { box.SW, box.SE, box.NE, box.NW })) return false;

                box = AECGeometry.Box(new AECPoint(0, (ySize * 0.5) - (yWidthM * 0.5)), xSize, yWidthM);
                return Add(new List<AECPoint> { box.SW, box.SE, box.NE, box.NW });
            }//try
            catch
            {
                SetBoundary(restorePoints);
                return false;
            }//catch
        }//method

        /// <summary>
        /// Configures the boundary as an H shape whose southwest bounding box corner is coincident with the origin. xWidthWest, xWidthEast and YWidth define the widths of the x and y axis arms.
        /// </summary>
        public bool PlanH(double xSize = 1,
                          double ySize = 1,
                          double xWidthW = -1,
                          double xWidthE = -1,
                          double yWidth = -1,
                          double yOffset = -1)
        {
            List<AECPoint> restorePoints = new List<AECPoint>(PointsFloor);
            try
            {
                //Data validation
                if (xWidthW <= 0 || xWidthW > xSize * 0.5)
                xWidthW = Math.Round(xSize * AECGeometry.oneThird, 8);
                if (xWidthE <= 0 || xWidthE > xSize * 0.5)
                    xWidthE = Math.Round(xSize * AECGeometry.oneThird, 8);
                if (yWidth <= 0 || yWidth > ySize)
                    yWidth = Math.Round(ySize * AECGeometry.oneThird, 8);
                if (yOffset < 0) yOffset = Math.Round(ySize * AECGeometry.oneThird, 8);
                if (yOffset < 0 || ySize - yOffset < 0)
                    yOffset = Math.Round(ySize * AECGeometry.oneThird, 8);

                if (!PlanX(xSize: xSize,
                               ySize: ySize,
                               xWidth: xWidthW,
                               yWidth: yWidth,
                               xOffset: 0,
                               yOffset: yOffset))
                    return false;
                List<AECPoint> pointsEast = new List<AECPoint>()
                {
                    new AECPoint(xSize - xWidthE, 0, 0),
                    new AECPoint(xSize, 0, 0),
                    new AECPoint(xSize, ySize, 0),
                    new AECPoint(xSize - xWidthE, ySize, 0)
                };//contructor
                return Add(pointsEast);
            }//try
            catch
            {
                SetBoundary(restorePoints);
                return false;
            }//catch
        }//method

        /// <summary>
        /// Configures the boundary as an L shape whose southwest bounding box corner is coincident with the origin. xWidth and YWidth define the widths of the x and y axis arms.
        /// </summary>
        public bool PlanL(double xSize = 1,
                          double ySize = 1,
                          double xWidth = -1,
                          double yWidth = -1)
        {
            List<AECPoint> restorePoints = new List<AECPoint>(PointsFloor);
            try
            {
                if (xWidth <= 0 || xWidth > xSize) xWidth = Math.Round(xSize * 0.5, 8);
                if (yWidth <= 0 || yWidth > ySize) yWidth = Math.Round(ySize * 0.5, 8);

                return PlanX(xSize: xSize,
                                 ySize: ySize,
                                 xWidth: xWidth,
                                 yWidth: yWidth,
                                 xOffset: 0,
                                 yOffset: 0);
            }//try
            catch
            {
                SetBoundary(restorePoints);
                return false;
            }//catch
        }//method

        /// <summary>
        /// Configures the boundary as a regular polygon of the specified number of sides whose center is coincident with the origin.
        /// </summary>
        public bool PlanPolygon(double radius = 1, int sides = 3)
        {
            if (radius <= 0) radius = 1;
            if (sides < 3) sides = 3;
            double angle = Math.PI * 0.5;
            double incAngle = (Math.PI * 2) / sides;
            double x, y;
            List<AECPoint> points = new List<AECPoint>();
            int count = 0;
            while (count < sides)
            {
                x = (radius * Math.Cos(angle));
                y = (radius * Math.Sin(angle));
                points.Add(new AECPoint(x, y, 0));
                angle += incAngle;
                count++;
            }//while
            return SetBoundary(points);
        }//method

        /// <summary>
        /// Configures the boundary as an T shape whose southwest bounding box corner is coincident with the origin. xWidth and YWidth define the widths of the x and y axis arms.
        /// </summary>
        public bool PlanT(double xSize = 1,
                          double ySize = 1,
                          double xWidth = -1,
                          double yWidth = -1)
        {
            if (xWidth <= 0) xWidth = Math.Round(xSize * 0.5, 8);
            if (yWidth <= 0) yWidth = Math.Round(ySize * 0.5, 8);

            return PlanX(xSize: xSize,
                             ySize: ySize,
                             xWidth: xWidth,
                             yWidth: yWidth,
                             xOffset: ((xSize - xWidth) * 0.5),
                             yOffset: (ySize - yWidth));
        }//method

        /// <summary>
        /// Configures the boundary as a U shape whose southwest bounding box corner is coincident with the origin. xWidthWest, xWidthEast and YWidth define the widths of the x and y axis arms.
        /// </summary>
        public bool PlanU(double xSize = 1,
                          double ySize = 1,
                          double xWidthW = -1,
                          double xWidthE = -1,
                          double yWidth = -1)
        {
            if (xWidthW <= 0) xWidthW = Math.Round(xSize * AECGeometry.oneThird, 8);
            if (xWidthE <= 0) xWidthE = Math.Round(xSize * AECGeometry.oneThird, 8);
            if (yWidth <= 0) yWidth = Math.Round(ySize * AECGeometry.oneThird, 8);

            return PlanH(xSize: xSize,
                         ySize: ySize,
                         xWidthW: xWidthW,
                         xWidthE: xWidthE,
                         yWidth: yWidth,
                         yOffset: 0);
        }//method

        /// <summary>
        /// Configures the boundary as a cross whose southwest bounding box corner is coincident with the origin. xWidth and YWidth define the widths of the x and y axis arms. xOffset and yOffset determine the position of the lowest x and y points from the y and x zero coordinates respectively.
        /// </summary>
        public bool PlanX(double xSize = 1,
                          double ySize = 1,
                          double xWidth = -1,
                          double yWidth = -1,
                          double xOffset = -1,
                          double yOffset = -1)
        {
            List<AECPoint> restorePoints = new List<AECPoint>(PointsFloor);
            try
            {
                //Set bounding box size values to positive numbers
                if (xSize == 0) xSize = 1;
                else xSize = Math.Abs(xSize);

                if (ySize == 0) ySize = 1;
                else ySize = Math.Abs(ySize);

                //If widths or offsets are set to defaults, set to 1/3 of bounding box dimensions

                if (xWidth <= 0) xWidth = Math.Round(xSize * AECGeometry.oneThird, 8);
                if (yWidth <= 0) yWidth = Math.Round(ySize * AECGeometry.oneThird, 8);

                if (xOffset < 0) xOffset = Math.Round(xSize * AECGeometry.oneThird, 8);
                if (yOffset < 0) yOffset = Math.Round(ySize * AECGeometry.oneThird, 8);

                //Check and correct dimensions that would exceed the bounding box size

                if ((xWidth + xOffset) > xSize)
                {
                    xWidth = Math.Round(xSize * AECGeometry.oneThird, 8);
                    xOffset = Math.Round(xSize * AECGeometry.oneThird, 8);
                }//if

                if ((yWidth + yOffset) > ySize)
                {
                    yWidth = Math.Round(ySize * AECGeometry.oneThird, 8);
                    yOffset = Math.Round(ySize * AECGeometry.oneThird, 8);
                }//if

                if (!PlanBox(xSize: xSize, ySize: yWidth)) return false;
                MoveBy(yDelta: yOffset);
                AECBox box = AECGeometry.Box(new AECPoint(), new AECPoint(xWidth, ySize));
                box.SW.MoveBy(xDelta: xOffset);
                box.SE.MoveBy(xDelta: xOffset);
                box.NE.MoveBy(xDelta: xOffset);
                box.NW.MoveBy(xDelta: xOffset);
                return Add(new List<AECPoint> { box.SW, box.SE, box.NE, box.NW });
            }//try
            catch
            {
                SetBoundary(restorePoints);
                return false;
            }//catch
        }//method

        /// <summary>
        /// Horizontally rotates the space anti-clockwise for positive values around a specifed point. If no point is specified, the space rotates around its centroid.
        /// </summary>
        public bool Rotate(double angle = 0, AECPoint point = null)
        {
            if (point == null) point = CentroidFloor;
            List<AECPoint> points = new List<AECPoint>();
            foreach (AECPoint flrPoint in PointsFloor)
            {
                flrPoint.Rotate(angle, point);
                points.Add(flrPoint);
            }//foreach
            return SetBoundary(points);
        }//method

        /// <summary>
        /// Horizontally scales the space from the specified 2D point. If no point is specified, the space scales from its centroid. Chaanges Height according to the zScale multiplier.
        /// </summary>
        public bool Scale(double xScale = 1, double yScale = 1, double zScale = 1, AECPoint point = null)
        {
            if (point == null) point = CentroidFloor;
            AffineTransformation scale = AffineTransformation.ScaleInstance(xScale, yScale, point.X, point.Y);
            Polygon newBoundary = (Polygon)scale.Transform(Boundary);
            Height *= zScale;
            return SetBoundary(AECGeometry.CoordsToPoints(newBoundary.Coordinates));
        }//method

        /// <summary>
        /// Sets the space boundary from an anti-clockwise series of points.
        /// </summary>
        protected bool SetBoundary(List<AECPoint> points = null)
        {
            try
            {
                if (points == null) { return false; }
                points = aecGeometry.RemoveColinear(points);
                if (points.Count < 3)
                { throw (new ExceptionPointCount("Need at least three non-colinear points.")); }

                Coordinate[] coords = AECGeometry.PointsToCoordsLoop(points);
                boundary = (Polygon)geoFactory.CreatePolygon(new LinearRing(coords));
                if (boundary is Polygon)
                {
                    this.points = points;
                    convex = AECGeometry.IsConvexPolygon(points);
                }//if
                else
                {
                    throw (new ExceptionBoundaryInvalid("Unable to create valid boundary from point list."));
                }//else
                return true;
            }//try
            catch(ExceptionPointCount e)
            {
                Debug.WriteLine("Exception | Point Count: {0}", e.Message);
                return false;
            }//catch
            catch (ExceptionBoundaryInvalid e)
            {
                Debug.WriteLine("Exception | Boundary Formation: {0}", e.Message);
                return false;
            }//catch
            catch
            {
                Debug.WriteLine("Undefined Exception");
                return false;
            }//catch
        }//method 

        /// <summary>
        /// Reconfigures the boundary to a convex hull wrapping its current points.
        /// </summary>
        public bool Wrap()
        {
            Polygon newBoundary = (Polygon)Boundary.ConvexHull();
            return SetBoundary(AECGeometry.CoordsToPoints(newBoundary.Coordinates));
        }//method
    }//class
}//namespace
