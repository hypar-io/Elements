using System;

namespace Elements.GeoJSON
{
    /// <summary>
    /// Methods for computing geographic coordinates using the Mercator projection.
    /// </summary>
    public static class MercatorProjection
    {
        private static readonly double R_MAJOR = 6378137.0;
        private static readonly double R_MINOR = 6356752.3142;
        private static readonly double RATIO = R_MINOR / R_MAJOR;
        private static readonly double ECCENT = Math.Sqrt(1.0 - (RATIO * RATIO));
        private static readonly double COM = 0.5 * ECCENT;

        private static readonly double DEG2RAD = Math.PI / 180.0;
        private static readonly double RAD2Deg = 180.0 / Math.PI;
        private static readonly double PI_2 = Math.PI / 2.0;

        /// <summary>
        /// Get the coordinates of the longitude and latitude.
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        /// <returns>An array of doubles containing the x, and y coordintes, in meters.</returns>
        public static double[] toPixel(double lon, double lat)
        {
            return new double[] { lonToX(lon), latToY(lat) };
        }
        
        /// <summary>
        /// Get the latitude and longitude of the specified x and y coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>An array of doubles containing the longitude and latitude in degrees.</returns>
        public static double[] toGeoCoord(double x, double y)
        {
            return new double[] { xToLon(x), yToLat(y) };
        }

        /// <summary>
        /// Get the x coordinate, in meters, of the specified longitude.
        /// </summary>
        /// <param name="lon"></param>
        /// <returns></returns>
        public static double lonToX(double lon)
        {
            return R_MAJOR * DegToRad(lon);
        }

        /// <summary>
        /// Get the y coordinate, in meters, of the specified latitude.
        /// </summary>
        /// <param name="lat"></param>
        /// <returns></returns>
        public static double latToY(double lat)
        {
            lat = Math.Min(89.5, Math.Max(lat, -89.5));
            double phi = DegToRad(lat);
            double sinphi = Math.Sin(phi);
            double con = ECCENT * sinphi;
            con = Math.Pow(((1.0 - con) / (1.0 + con)), COM);
            double ts = Math.Tan(0.5 * ((Math.PI * 0.5) - phi)) / con;
            return 0 - R_MAJOR * Math.Log(ts);
        }

        /// <summary>
        /// Get the longitude of the specified x coordinate.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <returns>The longitude in degrees.</returns>
        public static double xToLon(double x)
        {
            return RadToDeg(x) / R_MAJOR;
        }

        /// <summary>
        /// Get the latitude of the specified y coordinate.
        /// </summary>
        /// <param name="y">The y coordinate.</param>
        /// <returns>The latitude in degrees.</returns>
        public static double yToLat(double y)
        {
            double ts = Math.Exp(-y / R_MAJOR);
            double phi = PI_2 - 2 * Math.Atan(ts);
            double dphi = 1.0;
            int i = 0;
            while ((Math.Abs(dphi) > 0.000000001) && (i < 15))
            {
                double con = ECCENT * Math.Sin(phi);
                dphi = PI_2 - 2 * Math.Atan(ts * Math.Pow((1.0 - con) / (1.0 + con), COM)) - phi;
                phi += dphi;
                i++;
            }
            return RadToDeg(phi);
        }

        private static double RadToDeg(double rad)
        {
            return rad * RAD2Deg;
        }

        private static double DegToRad(double deg)
        {
            return deg * DEG2RAD;
        }
    }
}