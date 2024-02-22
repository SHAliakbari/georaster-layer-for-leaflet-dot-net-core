using Leaflet.geometry;
using System.Runtime.InteropServices;

namespace Leaflet.Projection
{

    public abstract class Projection
    {
        public abstract Point project(LatLng latlng);
        public abstract LatLng unproject(Point point);
        public abstract Bounds Bounds { get; }
    }

    public class LonLat : Projection
    {
        public override Point project(LatLng latlng)
        {
            return new Point(latlng.Lng, latlng.Lat);
        }

        public override LatLng unproject(Point point)
        {
            return new LatLng(point.y, point.x);
        }

        public override Bounds Bounds { get => new Bounds(new Point(-180, -90), new Point(180, 90)); }
    }

    public class Mercator : Projection
    {
        public const double R = 6378137;
        public const double R_MINOR = 6356752.314245179;

        public override Bounds Bounds { get => new Bounds(new Point(-20037508.34279, -15496570.73972), new Point(20037508.34279, 18764656.23138)); }

        public override Point project(LatLng latlng)
        {
            var d = Math.PI / 180;
            var r = Mercator.R;
            var y = latlng.Lat * d;
            var tmp = Mercator.R_MINOR / r;
            var e = Math.Sqrt(1 - tmp * tmp);
            var con = e * Math.Sin(y);

            var ts = Math.Tan(Math.PI / 4 - y / 2) / Math.Pow((1 - con) / (1 + con), e / 2);
            y = -r * Math.Log(Math.Max(ts, 1E-10));

            return new Point(latlng.Lng * d * r, y);
        }

        public override LatLng unproject(Point point)
        {
            var d = 180 / Math.PI;
            var r = Mercator.R;
            var tmp = Mercator.R_MINOR / r;
            var e = Math.Sqrt(1 - tmp * tmp);
            var ts = Math.Exp(-point.y / r);
            var phi = Math.PI / 2 - 2 * Math.Atan(ts);

            var dphi = 0.1;
            double con;
            for (var i = 0; i < 15 && Math.Abs(dphi) > 1e-7; i++)
            {
                con = e * Math.Sin(phi);
                con = Math.Pow((1 - con) / (1 + con), e / 2);
                dphi = Math.PI / 2 - 2 * Math.Atan(ts * con) - phi;
                phi += dphi;
            }

            return new LatLng(phi * d, point.x * d / r);
        }
    }

    public class SphericalMercator : Projection
    {
        public const double R = 6378137;
        public const double MAX_LATITUDE = 85.0511287798;

        public override Point project(LatLng latlng)
        {
            var d = Math.PI / 180;
            var max = MAX_LATITUDE;
            var lat = Math.Max(Math.Min(max, latlng.Lat), -max);
            var sin = Math.Sin(lat * d);

            return new Point(
                R * latlng.Lng * d,
                R * Math.Log((1 + sin) / (1 - sin)) / 2);
        }

        public override LatLng unproject(Point point)
        {
            var d = 180 / Math.PI;

            return new LatLng(
                (2 * Math.Atan(Math.Exp(point.y / R)) - (Math.PI / 2)) * d,
                point.x * d /R);
        }

        public override Bounds Bounds
        {
            get
            {
                var d = earthRadius * Math.PI;
                return new Bounds(new Point(-d, -d), new Point(d, d));
            }
        }

        double earthRadius = 6378137;
    }
}
