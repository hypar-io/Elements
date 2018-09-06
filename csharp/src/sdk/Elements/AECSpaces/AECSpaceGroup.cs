using System;
using System.Collections.Generic;
using System.Drawing;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace AECSpaces
{
    public class AECSpaceGroup
    {
        private readonly string id;
        private string name;
        private List<AECSpace> spaces = new List<AECSpace>();

        public AECSpaceGroup()
        {
            id = Guid.NewGuid().ToString();
            name = "";
            spaces = new List<AECSpace>();
        }//constructor

        public AECSpaceGroup(AECSpace newSpace)
        {
            id = Guid.NewGuid().ToString();
            name = "";
            spaces = new List<AECSpace>
            {
                newSpace
            };
        }//constructor

        public AECSpaceGroup(List<AECSpace> newSpaces)
        {
            id = Guid.NewGuid().ToString();
            name = "";
            spaces = newSpaces;
        }//constructor

        /// <summary>
        /// Returns the aggregate area of all spaces in the group.
        /// </summary>
        public double Area
        {
            get
            {
                double area = 0;
                foreach (AECSpace space in spaces)
                {
                    area += space.Area;
                }//foreach
                return area;
            }//get
        }//property

        /// <summary>
        /// Returns the list of spaces sorted by level, ascending or descending.
        /// </summary>
        public List<AECSpace> ByLevel
        {
            get
            {
                List<AECSpace> byLevel = new List<AECSpace>(spaces);
                byLevel.Sort((a, b) => a.Level.CompareTo(b.Level));
                return byLevel;
            }//get
        }//property

        /// <summary>
        /// Returns the numboer of spaces in the group.
        /// </summary>
        public int Count
        {
            get { return spaces.Count; }
        }//property

        /// <summary>
        /// Returns the unique id.
        /// </summary>
        public string ID
        {
            get { return id; }
        }//property

        /// <summary>
        /// Returns a list of meshes compatible with GLTF environments.
        /// </summary>
        ///
        public List<Hypar.Elements.Mass> MeshesGLTF
        {
            get
            {
                List<Hypar.Elements.Mass> masses = new List<Hypar.Elements.Mass>();
                foreach (AECSpace space in spaces)
                {
                    masses.Add(space.MeshGLTF);
                }//foreach
                return masses;
            }//get

        }//property

        /// <summary>
        /// Name of the space group.
        /// </summary>
        ///
        public string Name
        {
            get => name;
            set => name = value;
        }//property

        /// <summary>
        /// Returns the list of spaces.
        /// </summary>
        public List<AECSpace> Spaces
        {
            get { return spaces; }
            set
            {
                spaces = value;
            }//set
        }//property

        /// <summary>
        /// Returns the aggregate volume of all spaces in the group.
        /// </summary>
        public double Volume
        {
            get
            {
                double volume = 0;
                foreach (AECSpace space in spaces)
                {
                    volume += space.Volume;
                }//foreach
                return volume;
            }//get
        }//property

        /// <summary>
        /// Returns the aggregate convex hull of all spaces in the group as anti-clockwise series of points.
        /// </summary>
        public List<AECPoint> Wrap
        {
            get
            {
                List<AECPoint> points = new List<AECPoint>();
                foreach (AECSpace space in spaces)
                {
                    points.AddRange(space.PointsFloor);
                }//foreach

                Coordinate[] coords = AECGeometry.PointsToCoordsLoop(points);
                GeometryFactory factory = new GeometryFactory();
                ConvexHull convexHull = new ConvexHull(coords, factory);
                Polygon hull = (Polygon)convexHull.GetConvexHull();
                return AECGeometry.CoordsToPoints(hull.Coordinates);
            }//get
        }//property

        /// <summary>
        /// Adds a single space to the end of the group list.
        /// </summary>
        public void Add(AECSpace space)
        {
            spaces.Add(space);
        }//method

        /// <summary>
        /// Adds a list of spaces to the end of the group list.
        /// </summary>
        public void Add(List<AECSpace> newSpaces)
        {
            spaces.AddRange(newSpaces);
        }//method

        /// <summary>
        /// Clears the space list.
        /// </summary>
        public void Clear()
        {
            spaces.Clear();
        }//method

        /// <summary>
        /// Removes the indexed space from the list.
        /// </summary>
        public bool Delete(int index)
        {
            if (index < 0 || index >= Count) return false;
            spaces.RemoveAt(index);
            return true;
        }//method

        /// <summary>
        /// Retrieves the space at the supplied index.
        /// </summary>
        public AECSpace GetSpace(int index)
        {
            if (index < 0 || index >= Count) return null;
            return spaces[index];
        }//method

        /// <summary>
        /// Retrieves the space with the supplied ID.
        /// Returns null if no such space exists within the group.
        /// </summary>
        public AECSpace GetSpaceByID(string id)
        {
            int index = 0;
            while (index < spaces.Count)
            {
                if (spaces[index].ID == id) return spaces[index];
                index++;
            }//while
            return null;
        }//method

        /// <summary>
        /// Retrieves the first space with the supplied name.
        /// </summary>
        public AECSpace GetSpaceByName(string name)
        {
            int index = 0;
            while (index < spaces.Count)
            {
                if (spaces[index].Name == name) return spaces[index];
                index++;
            }//while
            return null;
        }//method

        /// <summary>
        /// Retrieves all spaces with the supplied department.
        /// </summary>
        public List<AECSpace> GetSpacesByDepartment(string department)
        {
            List<AECSpace> deptSpaces = new List<AECSpace>();
            int index = 0;
            while (index < spaces.Count)
            {
                if (spaces[index].Name == name) deptSpaces.Add(spaces[index]);
                index++;
            }//while
            if (deptSpaces.Count > 0) return deptSpaces;
            return null;
        }//method

        /// <summary>
        /// Retrieves all spaces with the supplied name.
        /// </summary>
        public List<AECSpace> GetSpacesByName(string name)
        {
            List<AECSpace> nameSpaces = new List<AECSpace>();
            int index = 0;
            while (index < spaces.Count)
            {
                if (spaces[index].Name == name) nameSpaces.Add(spaces[index]);
                index++;
            }//while
            if (nameSpaces.Count > 0) return nameSpaces;
            return null;
        }//method

        /// <summary>
        /// Retrieves all spaces with the supplied type.
        /// </summary>
        public List<AECSpace> GetSpacesByType(string spaceType)
        {
            List<AECSpace> typeSpaces = new List<AECSpace>();
            int index = 0;
            while (index < spaces.Count)
            {
                if (spaces[index].Type == spaceType) typeSpaces.Add(spaces[index]);
                index++;
            }//while
            if (typeSpaces.Count > 0) return typeSpaces;
            return null;
        }//method

        /// <summary>
        /// Moves each space by the delivered displacement values.
        /// </summary>
        public void MoveBy(double x = 0, double y = 0, double z = 0)
        {
            foreach (AECSpace space in spaces)
            {
                space.MoveBy(xDelta: x, yDelta: y, zDelta: z);
            }//foreach
        }//method

        /// <summary>
        /// Moves each space from one point to another.
        /// </summary>
        public void MoveTo(AECPoint from, AECPoint to)
        {
            foreach (AECSpace space in spaces)
            {
                space.MoveTo(from: from, to: to);
            }//foreach
        }//method

        /// <summary>
        /// Rotates each space around the delivered point or the centroid of each space.
        /// </summary>
        public void Rotate(double angle, AECPoint point = null)
        {
            foreach (AECSpace space in spaces)
            {
                if (point == null) space.Rotate(angle, space.CentroidFloor); 
                else space.Rotate(angle, point);
            }//foreach
        }//method

        /// <summary>
        /// Rotates each space around the delivered point or the centroid of each space.
        /// </summary>
        public void Scale(double xScale = 1, double yScale = 1, double zScale = 1, AECPoint point = null)
        {
            foreach (AECSpace space in spaces)
            {
                if (point == null)
                    space.Scale(xScale: xScale, yScale: yScale, zScale: zScale, point: point);
                else
                    space.Scale(xScale: xScale, yScale: yScale, zScale: zScale, point: space.CentroidFloor);
            }//foreach
        }//method

        /// <summary>
        /// Uniformly sets the color of all the spaces in the group.
        /// </summary>
        public void SetColor(AECColor color)
        {
            foreach (AECSpace space in spaces)
            {
                space.Color = color;
            }//foreach
        }//method

        /// <summary>
        /// Uniformly sets the department identifier of all the spaces in the group.
        /// </summary>
        public void SetDepartment(string department)
        {
            foreach (AECSpace space in spaces)
            {
                space.Department = department;
            }//foreach
        }//method

        /// <summary>
        /// Uniformly sets the height of all the spaces in the group.
        /// </summary>
        public void SetHeight(double height)
        {
            foreach (AECSpace space in spaces)
            {
                space.Height = height;
            }//foreach
        }//method

        /// <summary>
        /// Uniformly sets the space type identifier of all the spaces in the group.
        /// </summary>
        public void SetType(string spaceType)
        {
            foreach (AECSpace space in spaces)
            {
                space.Type = spaceType;
            }//foreach
        }//method

    }//class

}//namespace
