using System;
using Hypar.Geometry;

namespace Hypar.GeoJSON
{
    // Originally provided in this gist:
    // https://gist.github.com/nagasudhirpulla/9b5a192ccaca3c5992e5d4af0d1e6dc4
    public class WebMercator
    {
        private const double EarthRadius = 6378137.0;
        private const double OriginShift = 2.0 * Math.PI * EarthRadius / 2.0;

        //Converts given lat/lon in WGS84 Datum to XY in Spherical Mercator EPSG:900913
        public static Vector3 LatLonToMeters(double lat, double lon)
        {
            var x = lon * OriginShift / 180.0;
            var y = Math.Log(Math.Tan((90.0 + lat) * Math.PI / 360.0)) / (Math.PI / 180.0);
            y = y * OriginShift / 180.0;
            return new Vector3(x, y);
        }

        //Converts XY point from (Spherical) Web Mercator EPSG:3785 (unofficially EPSG:900913) to lat/lon in WGS84 Datum
        public static Vector3 MetersToLatLon(Vector3 m)
        {
            var x = (m.X / OriginShift) * 180.0;
            var y = (m.Y / OriginShift) * 180.0;
            y = 180.0 / Math.PI * (2.0 * Math.Atan(Math.Exp(y * Math.PI / 180.0)) - Math.PI / 2.0);
            return new Vector3(x,y);
        }
    }
}