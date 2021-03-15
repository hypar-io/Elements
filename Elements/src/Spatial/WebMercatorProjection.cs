using System;
using Elements.Geometry;

namespace Elements.Spatial
{
    /// <summary>
    /// Methods for computing web mercator projection tiles and coordinates.
    /// </summary>
    public static class WebMercatorProjection
    {
        // Taken from Mapbox
        private const double EARTH_RADIUS = 6378137;
        private const double ORIGIN_SHIFT = 2 * Math.PI * EARTH_RADIUS / 2;
        private const double WEB_MERCATOR_MAX = 20037508.342789244;

        /// <summary>
        /// Get the tile size, in meters.
        /// </summary>
        /// <param name="lat">The latitude of the tile, in degrees.</param>
        /// <param name="zoom">The zoom level of the tile.</param>
        public static double GetTileSizeMeters(double lat, int zoom)
        {
            // Circumference of the earth * cos(latitude) divided by 2^zoom
            return (2 * Math.PI * EARTH_RADIUS * Math.Cos(Units.DegreesToRadians(lat))) / Math.Pow(2, zoom);
        }

        /// <summary>
        /// Get the center of the mercator web tile.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="zoom"></param>
        public static Vector3 TileIdToCenterWebMercator(int x, int y, int zoom)
        {
            var tileCnt = Math.Pow(2, zoom);
            var centerX = x + 0.5;
            var centerY = y + 0.5;

            centerX = ((centerX / tileCnt * 2) - 1) * WEB_MERCATOR_MAX;
            centerY = (1 - (centerY / tileCnt * 2)) * WEB_MERCATOR_MAX;
            return new Vector3(centerX, centerY);
        }
    }
}