using System;
using Elements.GeoJSON;
using Elements.Geometry;

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
        /// Convert Latitude and Longitude to meters relative to a base position.
        /// </summary>
        /// <param name="relativeToOrigin">A position marking the latitude and longitude of (0,0)</param>
        /// <param name="lat">The latitude in degrees</param>
        /// <param name="lon">The longitude in degrees</param>
        /// <returns>A Vector3 in meters specifying the offset from the origin for this location.</returns>
        public static Vector3 LatLonToMeters(Position relativeToOrigin, double lat, double lon)
        {
            var originX = LonToX(relativeToOrigin.Longitude);
            var originY = LatToY(relativeToOrigin.Latitude);
            var phi = Units.DegreesToRadians(relativeToOrigin.Latitude);
            var x = (LonToX(lon) - originX) * Math.Cos(phi);
            var y = (LatToY(lat) - originY) * Math.Cos(phi);
            return new Vector3(x, y);
        }

        /// <summary>
        /// Convert a position in space to a latitude and longitude.
        /// </summary>
        /// <param name="relativeToOrigin">A position marking the latitude and longitude of (0,0).</param>
        /// <param name="location">The position to convert to latitude and longitude.</param>
        /// <returns>A position indicating the latitude and longitude of the location.</returns>
        public static Position MetersToLatLon(Position relativeToOrigin, Vector3 location)
        {
            var phi = Units.DegreesToRadians(relativeToOrigin.Latitude);
            var locationX = location.X / Math.Cos(phi);
            var locationY = location.Y; // it's not entirely clear why the Y is not scaled by cos(phi) here — perhaps YToLat already takes this scaling into account?
            var lon = XToLon(locationX) + relativeToOrigin.Longitude;
            var lat = YToLat(locationY) + relativeToOrigin.Latitude;
            return new Position(lat, lon);
        }

        /// <summary>
        /// Get the coordinates of the longitude and latitude.
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        /// <returns>An array of doubles containing the x, and y coordintes, in the Mercator projection.</returns>
        public static double[] ToPixel(double lon, double lat)
        {
            return new double[] { LonToX(lon), LatToY(lat) };
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
        /// Get the x coordinate, in the Mercator projection, of the specified longitude. 
        /// The units will be in meters at the equator, and distorted elsewhere. Utilize LatLonToMeters() for a conversion
        /// relative to a basepoint.
        /// </summary>
        /// <param name="lon"></param>
        /// <returns></returns>
        public static double LonToX(double lon)
        {
            return R_MAJOR * Units.DegreesToRadians(lon);
        }

        /// <summary>
        /// Get the y coordinate, in the Mercator projection, of the specified latitude.
        /// The units will be in meters at the equator, and distorted elsewhere. Utilize LatLonToMeters() for a conversion
        /// relative to a basepoint.
        /// </summary>
        /// <param name="lat">The latitude to convert, within the range [-89.5, 89.5]. Values outside this range will be clamped.</param>
        /// <returns></returns>
        public static double LatToY(double lat)
        {
            lat = Math.Min(89.5, Math.Max(lat, -89.5));
            double phi = Units.DegreesToRadians(lat);
            double sinphi = Math.Sin(phi);
            double con = ECCENT * sinphi;
            con = Math.Pow(((1.0 - con) / (1.0 + con)), COM);
            double ts = Math.Tan(0.5 * ((Math.PI * 0.5) - phi)) / con;
            return 0 - R_MAJOR * Math.Log(ts);
        }

        /// <summary>
        /// Get the longitude of the specified x coordinate.
        /// </summary>
        /// <param name="x">The x coordinate in the Mercator projection.</param>
        /// <returns>The longitude in degrees.</returns>
        public static double XToLon(double x)
        {
            return Units.RadiansToDegrees(x) / R_MAJOR;
        }

        /// <summary>
        /// Get the latitude of the specified y coordinate.
        /// </summary>
        /// <param name="y">The y coordinate in the Mercator projection.</param>
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