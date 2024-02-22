namespace Leaflet
{
    public class LatLng
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
        public double Alt { get; set; }

        Earth earth = new Earth();

        public LatLng(double lat, double lng, double alt = 0)
        {
            if (double.IsNaN(lat) || double.IsNaN(lng))
            {
                throw new Exception($"Invalid LatLng object: ({lat}, {lng})");
            }

            Lat = lat;
            Lng = lng;
            Alt = alt;
        }

        public bool Equals(LatLng otherLatLng, double maxMargin = 1.0E-9)
        {
            if (otherLatLng == null) { return false; }
            var margin = Math.Max(
                Math.Abs(this.Lat - otherLatLng.Lat),
                Math.Abs(this.Lng - otherLatLng.Lng));
            return margin <= maxMargin;
        }

        public string ToString(int precision)
        {
            return $"LatLng({Util.FormatNum(this.Lat, precision)}, {Util.FormatNum(this.Lng, precision)})";
        }

        public double DistanceTo(LatLng other)
        {
            return earth.distance(this, other);
        }

        public LatLng Wrap()
        {
            return earth.wrapLatLng(this);
        }

        public LatLngBounds ToBounds(double sizeInMeters)
        {
            double latAccuracy = 180 * sizeInMeters / 40075017;
            double lngAccuracy = latAccuracy / Math.Cos((Math.PI / 180) * this.Lat);
            return LatLngBounds.toLatLngBounds(
                new LatLng( this.Lat - latAccuracy, this.Lng - lngAccuracy ),
                new LatLng(this.Lat + latAccuracy, this.Lng + lngAccuracy ));
        }

        public LatLng Clone()
        {
            return new LatLng(this.Lat, this.Lng, this.Alt);
        }

        public static LatLng ToLatLng(object a, object b = null, object c = null)
        {
            if (a is LatLng)
            {
                return (LatLng)a;
            }
            if (Util.IsArray(a) && !(a is Array && ((Array)a).Length > 0 && ((Array)a).GetValue(0) is object))
            {
                if (((Array)a).Length == 3)
                {
                    return new LatLng((double)((Array)a).GetValue(0), (double)((Array)a).GetValue(1), (double)((Array)a).GetValue(2));
                }
                if (((Array)a).Length == 2)
                {
                    return new LatLng((double)((Array)a).GetValue(0), (double)((Array)a).GetValue(1));
                }
                return null;
            }
            if (a == null)
            {
                return null;
            }
            if (a is object && ((dynamic)a).lat != null)
            {
                return new LatLng((double)((dynamic)a).lat, ((dynamic)a).lng != null ? (double)((dynamic)a).lng : (double)((dynamic)a).lon, ((dynamic)a).alt != null ? (double)((dynamic)a).alt : 0);
            }
            if (b == null)
            {
                return null;
            }
            return new LatLng((double)a, (double)b, c != null ? (double)c : 0);
        }
    }
}
