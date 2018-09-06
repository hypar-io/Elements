using System.Collections.Generic;

namespace AECSpaces
{
    /// <summary>
    /// Represents a position with x, y, and z positions.
    /// </summary>
    ///
    public struct AECAddress
    {
        public int x;
        public int y;
        public int z;

        public AECAddress(int X, int Y, int Z)
        {
            x = X;
            y = Y;
            z = Z;
        }//constructor

        /// <summary>
        /// Offsets the address by a specified amount.
        /// </summary>
        /// 
        public void Offset(int offset)
        {
            x += offset;
            y += offset;
            z += offset;
        }//method

        /// <summary>
        /// Offsets the address by a specified amount.
        /// </summary>
        /// 
        public void Offset(int X, int Y, int Z)
        {
            x += X;
            y += Y;
            z += Z;
        }//method
    }//struct

    /// <summary>
    /// Four Points typically used to define quadrangles.
    /// </summary>
    /// 
    public struct AECBox
    {
        public AECPoint SW;
        public AECPoint SE;
        public AECPoint NE;
        public AECPoint NW;

        public AECBox(AECPoint sw, AECPoint se, AECPoint ne, AECPoint nw)
        {
            SW = sw;
            SE = se;
            NE = ne;
            NW = nw;
        }//constructor
    }//struct

    public struct AECColor
    {
        public byte A;
        public byte R;
        public byte G;
        public byte B;

        //Alpha Convenience Constants
        public static readonly byte Opaque = 255;
        public static readonly byte Translucent = 127;
        public static readonly byte Transparent = 255;

        //Color Convenience Constants
        public static readonly byte[] Aqua = new byte[] { 77, 184, 100 };
        public static readonly byte[] Beige = { 255, 250, 200 };
        public static readonly byte[] Black = { 0, 0, 0 };
        public static readonly byte[] Blue = { 0, 100, 255 };
        public static readonly byte[] Brown = { 170, 110, 40 };
        public static readonly byte[] Coral = { 255, 215, 180 };
        public static readonly byte[] Cyan = { 70, 240, 240 };
        public static readonly byte[] Darkgray = { 64, 64, 64 };
        public static readonly byte[] Green = { 60, 180, 75 };
        public static readonly byte[] Granite = { 60, 60, 60 };
        public static readonly byte[] Gray = { 128, 128, 128 };
        public static readonly byte[] Lavender = { 230, 190, 255 };
        public static readonly byte[] Lime = { 210, 245, 60 };
        public static readonly byte[] Magenta = { 240, 50, 230 };
        public static readonly byte[] Maroon = { 128, 0, 0 };
        public static readonly byte[] Mint = { 170, 255, 195 };
        public static readonly byte[] Navy = { 0, 0, 128 };
        public static readonly byte[] Olive = { 128, 128, 0 };
        public static readonly byte[] Orange = { 255, 115, 15 };
        public static readonly byte[] Pink = { 255, 66, 138 };
        public static readonly byte[] Purple = { 191, 2, 255 };
        public static readonly byte[] Red = { 255, 0, 0 };
        public static readonly byte[] Sand = { 255, 215, 96 };
        public static readonly byte[] Stone = { 20, 20, 20 };
        public static readonly byte[] Teal = { 0, 128, 128 };
        public static readonly byte[] White = new byte[] { 255, 255, 255 };
        public static readonly byte[] Yellow = { 255, 239, 17 };      

        public AECColor(byte[] newColor)
        {
            if (newColor.Length < 3)
            {
                newColor = new byte[] { 255, 255, 255 };
            }//if
            A = Translucent;
            R = newColor[0];
            G = newColor[1];
            B = newColor[2];
        }//constructor

        public void RGB (byte[] newColor)
        {
            if (newColor.Length < 3)
            {
                newColor = new byte[] { 255, 255, 255 };
            }//if
            R = newColor[0];
            G = newColor[1];
            B = newColor[2];
        }//property
    }//struct

    /// <summary>
    /// Represents a position in space with x,y, and z double values.
    /// </summary>
    public struct AECCoords
    {
        public double x, y, z;
        public AECCoords(double X, double Y, double Z)
        {
            x = X;
            y = Y;
            z = Z;
        }//Constructor
    }//Coords

    /// <summary>
    /// Lists of vertices and indices describing a 3D mesh.
    /// </summary>
    /// 
    public class AECMesh
    {
        public List<AECPoint> vertices = new List<AECPoint>();
        public List<AECAddress> indices = new List<AECAddress>();
        public List<AECVector> normals = new List<AECVector>();
    }//class

    /// <summary>
    /// Lists of vertices and indices describing a 2D mesh.
    /// </summary>
    /// 
    public class AECMesh2D
    {
        public List<AECPoint> vertices = new List<AECPoint>();
        public List<AECAddress> indices = new List<AECAddress>();
        public AECVector normal = new AECVector();
    }//class

    /// <summary>
    /// Lists of vertices, indices, and normals describing a 3D mesh suitable for graphics rendering.
    /// </summary>
    public class AECMeshGraphic
    {
        public List<double> vertices = new List<double>();
        public List<double> indices = new List<double>();
        public List<double> normals = new List<double>();
    }//class

    /// <summary>
    /// Represents a corridor segment as its centerline endpoints a width, and a height.
    /// </summary>
    public struct AECPassage
    {
        public AECPoint thisPoint;
        public AECPoint thatPoint;
        public double height;
        public double width;
    }//struct

    /// <summary>
    /// A container for space side characteristics.
    /// </summary>
    ///
    public struct AECSpaceSide
    {
        public AECBox side;
        public AECVector normal;

        public AECSpaceSide(AECBox Side, AECVector Normal)
        {
            side = Side;
            normal = Normal;
        }//constructor
    }//struct

    /// <summary>
    /// Represents an exterior and interior angle and convexity with regard to an anti-clockwise series of points.
    /// </summary>
    ///
    public struct AECVertexAngles
    {
        public bool convex;
        public double exterior;
        public double interior;

        public AECVertexAngles(bool Convex, double Exterior, double Interior)
        {
            convex = Convex;
            exterior = Exterior;
            interior = Interior;
        }//constructor
    }//struct
}
