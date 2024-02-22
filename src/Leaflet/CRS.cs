using Leaflet.geometry;
using Leaflet.Projection;
using System.Runtime.CompilerServices;

namespace Leaflet
{
    public abstract class CRS
    {
        public const double LN2 = 0.6931471805599453;
        public virtual Projection.Projection projection { get;  }

        public virtual bool infinite { get => false; }

        // @method latLngToPoint(latlng: LatLng, zoom: Number): Point
        // Projects geographical coordinates into pixel coordinates for a given zoom.
        public Point latLngToPoint(LatLng latlng, int zoom)
        {
            var projectedPoint = this.projection.project(latlng);
            var scale = this.scale(zoom);

            return this.transformation._transform(projectedPoint, scale);
        }

        // @method pointToLatLng(point: Point, zoom: Number): LatLng
        // The inverse of `latLngToPoint`. Projects pixel coordinates on a given
        // zoom into geographical coordinates.
        public LatLng pointToLatLng(Point point, int zoom)
        {
            var scale = this.scale(zoom);
                var untransformedPoint = this.transformation.untransform(point, scale);

            return this.projection.unproject(untransformedPoint);
        }

        // @method project(latlng: LatLng): Point
        // Projects geographical coordinates into coordinates in units accepted for
        // this CRS (e.g. meters for EPSG:3857, for passing it to WMS services).
        public Point project(LatLng latlng)
        {
            return this.projection.project(latlng);
        }

        // @method unproject(point: Point): LatLng
        // Given a projected coordinate returns the corresponding LatLng.
        // The inverse of `project`.
        public LatLng unproject(Point point)
        {
            return this.projection.unproject(point);
        }

        // @method scale(zoom: Number): Number
        // Returns the scale used when transforming projected coordinates into
        // pixel coordinates for a particular zoom. For example, it returns
        // `256 * 2^zoom` for Mercator-based CRS.
        public virtual double scale(double zoom)
        {
            return 256 * Math.Pow(2, zoom);
        }

        // @method zoom(scale: Number): Number
        // Inverse of `scale()`, returns the zoom level corresponding to a scale
        // factor of `scale`.
        public virtual double zoom(double scale)
        {
            return Math.Log(scale / 256) / LN2;
        }

        // @method getProjectedBounds(zoom: Number): Bounds
        // Returns the projection's bounds scaled and transformed for the provided `zoom`.
        public Bounds getProjectedBounds(double zoom)
        {
            if (this.infinite) { return null; }

            var b = this.projection.Bounds;
            var s = this.scale(zoom);
            var min = this.transformation.transform(b.min, s);
            var max = this.transformation.transform(b.max, s);

            return new Bounds(min, max);
        }

        public virtual Transformation transformation { get; }

        // @method distance(latlng1: LatLng, latlng2: LatLng): Number
        // Returns the distance between two geographical coordinates.
        public virtual double distance(LatLng latLng1, LatLng latLng2) { return 0; }

        // @property code: String
        // Standard code name of the CRS passed into WMS services (e.g. `'EPSG:3857'`)
        public virtual  string code { get; }
        // @property wrapLng: Number[]
        // An array of two numbers defining whether the longitude (horizontal) coordinate
        // axis wraps around a given range and how. Defaults to `[-180, 180]` in most
        // geographical CRSs. If `undefined`, the longitude axis does not wrap around.
        //
        public virtual double[] wrapLng { get; }
        public virtual double[] wrapLat { get; }
        // @property wrapLat: Number[]
        // Like `wrapLng`, but for the latitude (vertical) axis.

        // wrapLng: [min, max],
        // wrapLat: [min, max],

        // @property infinite: Boolean
        // If true, the coordinate space will be unbounded (infinite in both axes)

        // @method wrapLatLng(latlng: LatLng): LatLng
        // Returns a `LatLng` where lat and lng has been wrapped according to the
        // CRS's `wrapLat` and `wrapLng` properties, if they are outside the CRS's bounds.
        public LatLng wrapLatLng(LatLng latlng)
        {
            var lng = this.wrapLng != null ? Util.wrapNum(latlng.Lng, this.wrapLng, true) : latlng.Lng;
            var lat = this.wrapLat != null ? Util.wrapNum(latlng.Lat, this.wrapLat, true) : latlng.Lat;
            var alt = latlng.Alt;

            return new LatLng(lat, lng, alt);
        }

        // @method wrapLatLngBounds(bounds: LatLngBounds): LatLngBounds
        // Returns a `LatLngBounds` with the same size as the given one, ensuring
        // that its center is within the CRS's bounds.
        // Only accepts actual `L.LatLngBounds` instances, not arrays.
        public LatLngBounds wrapLatLngBounds(LatLngBounds bounds)
        {
            var center = bounds.getCenter();
            var newCenter = this.wrapLatLng(center);
            var latShift = center.Lat - newCenter.Lat;
            var lngShift = center.Lng - newCenter.Lng;

            if (latShift == 0 && lngShift == 0)
            {
                return bounds;
            }
            var sw = bounds.getSouthWest();
            var ne = bounds.getNorthEast();
            var newSw = new LatLng(sw.Lat - latShift, sw.Lng - lngShift);
            var newNe = new LatLng(ne.Lat - latShift, ne.Lng - lngShift);

            return new LatLngBounds(newSw, newNe);
        }
    }

    public class Simple : CRS
    {
        Projection.Projection _projection = new Projection.LonLat();
        public override Projection.Projection projection { get => _projection; }
        public override double scale(double zoom)
        {
            return Math.Pow(2, zoom);
        }

        public override double zoom(double scale)
        {
            return Math.Log(scale) / LN2;
        }

        public override double distance(LatLng latLng1, LatLng latLng2)
        {
            var dx = latLng2.Lng - latLng1.Lng;
             var dy = latLng2.Lat - latLng1.Lat;

            return Math.Sqrt(dx * dx + dy * dy);
        }

        public override bool infinite => true;
    }

    public class Earth : CRS
    {
        public override double[] wrapLng => new double[] { -180, 180 };

        public double R = 6371000;

        public override double distance(LatLng latLng1, LatLng latLng2)
        {
            var rad = Math.PI / 180;
            var lat1 = latLng1.Lat * rad;
            var lat2 = latLng2.Lat * rad;
            var sinDLat = Math.Sin((latLng2.Lat - latLng1.Lat) * rad / 2);
            var sinDLon = Math.Sin((latLng2.Lng - latLng1.Lng) * rad / 2);
            var a = sinDLat * sinDLat + Math.Cos(lat1) * Math.Cos(lat2) * sinDLon * sinDLon;
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return this.R * c;
        }
    }

    public class EPSG4326:CRS
    {
        public override string code => "EPSG:4326";
        Projection.Projection _projection = new Projection.LonLat();
        public override Projection.Projection projection { get => _projection; }
        Transformation _transformation = new Transformation(1/180, 1, -1/180, 0.5);
        public override Transformation transformation => _transformation;

    }

    public class EPSG3395 : CRS
    {
        public EPSG3395()
        {
            var scale = 0.5 / (Math.PI * Mercator.R);
            _transformation =  new Transformation(scale, 0.5, -scale, 0.5);
        }

        public override string code => "EPSG:3395";
        Projection.Projection _projection = new Projection.Mercator();
        public override Projection.Projection projection { get => _projection; }
        Transformation _transformation;
        public override Transformation transformation => _transformation;

    }

    public class EPSG3857 : Earth
    {
        public EPSG3857()
        {
            var scale = 0.5 / (Math.PI * SphericalMercator.R);
            _transformation = new Transformation(scale, 0.5, -scale, 0.5);
        }

        public override string code => "EPSG:3857";
        Projection.Projection _projection = new Projection.SphericalMercator();
        public override Projection.Projection projection { get => _projection; }
        Transformation _transformation;
        public override Transformation transformation => _transformation;

    }

    public class EPSG900913 : EPSG3857 {

        public override string code => "EPSG:900913";
    }
}
