using System;

namespace Elements.Spatial
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

        /// <summary>
        /// Get the coordinates of the longitude and latitude.
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        /// <returns>An array of doubles containing the x, and y coordinates, in meters.</returns>
        public static double[] ToPixel(double lon, double lat)
        {
            var (x, y) = LatLonToXY(lon, lat);
            return new double[] { x, y };
        }

        /// <summary>
        /// Get the latitude and longitude of the specified x and y coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>An array of doubles containing the longitude and latitude in degrees.</returns>
        public static double[] ToGeoCoord(double x, double y)
        {
            return new double[] { XToLon(x), YToLat(y) };
        }

        /// <summary>
        /// Get the x coordinate, in meters, of the specified longitude.
        /// </summary>
        /// <param name="lon">The longitude</param>
        /// <param name="lat">The latitude</param>
        public static double LonToX(double lon, double lat)
        {
            return R_MAJOR * Units.DegreesToRadians(lon) * Math.Sin(Units.DegreesToRadians(lat));
        }

        public static (double x, double y) LatLonToXY(double lat, double lon)
        {
            var x = LonToX(lon, lat);
            var y = LatToY(lat);
            return (x, y);
        }

        public static Elements.Geometry.Vector3 LatLonToVector3(double lat, double lon)
        {
            return new Elements.Geometry.Vector3(LonToX(lon, lat), LatToY(lat), 0);
        }

        /// <summary>
        /// Get the y coordinate, in meters, of the specified latitude.
        /// </summary>
        /// <param name="lat">The latitude</param>
        public static double LatToY(double lat)
        {
            lat = Math.Min(89.5, Math.Max(lat, -89.5));
            double phi = Units.DegreesToRadians(lat);
            double sinphi = Math.Sin(phi);
            double con = ECCENT * sinphi;
            con = Math.Pow(((1.0 - con) / (1.0 + con)), COM);
            double ts = Math.Tan(0.5 * ((Math.PI * 0.5) - phi)) / con;
            return (0 - R_MAJOR * Math.Log(ts)) * Math.Cos(phi);
        }

        /// <summary>
        /// Get the longitude of the specified x coordinate.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <returns>The longitude in degrees.</returns>
        public static double XToLon(double x)
        {
            return Units.RadiansToDegrees(x) / R_MAJOR;
        }

        /// <summary>
        /// Get the latitude of the specified y coordinate.
        /// </summary>
        /// <param name="y">The y coordinate.</param>
        /// <returns>The latitude in degrees.</returns>
        public static double YToLat(double y)
        {
            double ts = Math.Exp(-y / R_MAJOR);
            double phi = Units.PI_2 - 2 * Math.Atan(ts);
            double dphi = 1.0;
            int i = 0;
            while ((Math.Abs(dphi) > 0.000000001) && (i < 15))
            {
                double con = ECCENT * Math.Sin(phi);
                dphi = Units.PI_2 - 2 * Math.Atan(ts * Math.Pow((1.0 - con) / (1.0 + con), COM)) - phi;
                phi += dphi;
                i++;
            }
            return Units.RadiansToDegrees(phi);
        }
    }
}