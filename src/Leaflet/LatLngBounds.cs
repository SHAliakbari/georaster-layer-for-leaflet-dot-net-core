
namespace Leaflet
{
    public class LatLngBounds
    {
        private LatLng _southWest;
        private LatLng _northEast;

        public LatLngBounds(LatLng corner1, LatLng corner2)
        {
            if (corner1 == null) { return; }
            LatLng[] latlngs = corner2 != null ? new LatLng[] { corner1, corner2 } : new LatLng[] { corner1 };
            foreach (LatLng latlng in latlngs)
            {
                Extend(latlng);
            }
        }

        public LatLngBounds Extend(object obj)
        {
            LatLng sw = _southWest;
            LatLng ne = _northEast;
            LatLng sw2, ne2;
            if (obj is LatLng)
            {
                sw2 = (LatLng)obj;
                ne2 = (LatLng)obj;
            }
            else if (obj is LatLngBounds)
            {
                LatLngBounds bounds = (LatLngBounds)obj;
                sw2 = bounds._southWest;
                ne2 = bounds._northEast;
                if (sw2 == null || ne2 == null) { return this; }
            }
            else
            {
                return obj != null ? Extend(obj is LatLng ? LatLng.ToLatLng(obj) : toLatLngBounds((LatLngBounds)obj)) : this;

                //return obj != null ? Extend(LatLng.ToLatLng(obj) ?? toLatLngBounds((LatLngBounds)obj)) : this;
            }
            if (sw == null && ne == null)
            {
                _southWest = new LatLng(sw2.Lat, sw2.Lng);
                _northEast = new LatLng(ne2.Lat, ne2.Lng);
            }
            else
            {
                sw.Lat = Math.Min(sw2.Lat, sw.Lat);
                sw.Lng = Math.Min(sw2.Lng, sw.Lng);
                ne.Lat = Math.Max(ne2.Lat, ne.Lat);
                ne.Lng = Math.Max(ne2.Lng, ne.Lng);
            }
            return this;
        }

        public LatLngBounds pad(int bufferRatio)
        {
            var sw = this._southWest;

            var ne = this._northEast;
            var heightBuffer = Math.Abs(sw.Lat - ne.Lat) * bufferRatio;
            var widthBuffer = Math.Abs(sw.Lng - ne.Lng) * bufferRatio;

            return new LatLngBounds(
                    new LatLng(sw.Lat - heightBuffer, sw.Lng - widthBuffer),
                    new LatLng(ne.Lat + heightBuffer, ne.Lng + widthBuffer));
        }

        // @method getCenter(): LatLng
        // Returns the center point of the bounds.
        public LatLng getCenter()
        {
            return new LatLng(
                    (this._southWest.Lat + this._northEast.Lat) / 2,
                    (this._southWest.Lng + this._northEast.Lng) / 2);
        }

        // @method getSouthWest(): LatLng
        // Returns the south-west point of the bounds.
        public LatLng getSouthWest()
        {
            return this._southWest;
        }

        // @method getNorthEast(): LatLng
        // Returns the north-east point of the bounds.
        public LatLng getNorthEast()
        {
            return this._northEast;
        }

        // @method getNorthWest(): LatLng
        // Returns the north-west point of the bounds.
        public LatLng getNorthWest()
        {
            return new LatLng(this.getNorth(), this.getWest());
        }

        // @method getSouthEast(): LatLng
        // Returns the south-east point of the bounds.
        public LatLng getSouthEast()
        {
            return new LatLng(this.getSouth(), this.getEast());
        }

        // @method getWest(): Number
        // Returns the west longitude of the bounds
        public double getWest()
        {
            return this._southWest.Lng;
        }

        // @method getSouth(): Number
        // Returns the south latitude of the bounds
        public double getSouth()
        {
            return this._southWest.Lat;
        }

        // @method getEast(): Number
        // Returns the east longitude of the bounds
        public double getEast()
        {
            return this._northEast.Lng;
        }

        // @method getNorth(): Number
        // Returns the north latitude of the bounds
        public double getNorth()
        {
            return this._northEast.Lat;
        }

        public bool contains(LatLng obj)
        {
            var sw = this._southWest;
            var ne = this._northEast;
            LatLng sw2, ne2;
            sw2 = ne2 = obj;
            return (sw2.Lat >= sw.Lat) && (ne2.Lat <= ne.Lat) &&
                  (sw2.Lng >= sw.Lng) && (ne2.Lng <= ne.Lng);
        }

        public bool contains(LatLngBounds obj)
        { // (LatLngBounds) or (LatLng) -> Boolean
            //if (typeof obj[0] === 'number' || obj instanceof LatLng || 'lat' in obj) {
            //    obj = toLatLng(obj);
            //} else
            //{
            //    obj = toLatLngBounds(obj);
            //}

            var sw = this._southWest;
            var ne = this._northEast;
            LatLng sw2, ne2;

            sw2 = obj.getSouthWest();
            ne2 = obj.getNorthEast();

            return (sw2.Lat >= sw.Lat) && (ne2.Lat <= ne.Lat) &&
                   (sw2.Lng >= sw.Lng) && (ne2.Lng <= ne.Lng);
        }

        public bool intersects(LatLngBounds bounds)
        {
            bounds = toLatLngBounds(bounds);

            var sw = this._southWest;
            var ne = this._northEast;
            var sw2 = bounds.getSouthWest();
            var ne2 = bounds.getNorthEast();

            var latIntersects = (ne2.Lat >= sw.Lat) && (sw2.Lat <= ne.Lat);
            var lngIntersects = (ne2.Lng >= sw.Lng) && (sw2.Lng <= ne.Lng);

            return latIntersects && lngIntersects;
        }

        public bool overlaps(LatLngBounds bounds)
        {
            bounds = toLatLngBounds(bounds);

            var sw = this._southWest;
            var ne = this._northEast;
            var sw2 = bounds.getSouthWest();
            var ne2 = bounds.getNorthEast();

            var latOverlaps = (ne2.Lat > sw.Lat) && (sw2.Lat < ne.Lat);
            var lngOverlaps = (ne2.Lng > sw.Lng) && (sw2.Lng < ne.Lng);

            return latOverlaps && lngOverlaps;
        }

        // @method toBBoxString(): String
        // Returns a string with bounding box coordinates in a 'southwest_lng,southwest_lat,northeast_lng,northeast_lat' format. Useful for sending requests to web services that return geo data.
        public string toBBoxString()
        {
            var tmp = new double[] { this.getWest(), this.getSouth(), this.getEast(), this.getNorth() };
            return string.Join(",", tmp.Select(x => x.ToString()));
        }

        // @method equals(otherBounds: LatLngBounds, maxMargin?: Number): Boolean
        // Returns `true` if the rectangle is equivalent (within a small margin of error) to the given bounds. The margin of error can be overridden by setting `maxMargin` to a small number.
        public bool equals(LatLngBounds bounds, int maxMargin)
        {

            bounds = toLatLngBounds(bounds);

            return this._southWest.Equals(bounds.getSouthWest(), maxMargin) &&
                   this._northEast.Equals(bounds.getNorthEast(), maxMargin);
        }

        // @method isValid(): Boolean
        // Returns `true` if the bounds are properly initialized.
        public bool isValid()
        {
            return !!(this._southWest != null && this._northEast != null);
        }


        // @factory L.latLngBounds(corner1: LatLng, corner2: LatLng)
        // Creates a `LatLngBounds` object by defining two diagonally opposite corners of the rectangle.

        // @alternative
        // @factory L.latLngBounds(latlngs: LatLng[])
        // Creates a `LatLngBounds` object defined by the geographical points it contains. Very useful for zooming the map to fit a particular set of locations with [`fitBounds`](#map-fitbounds).

        public static LatLngBounds toLatLngBounds(LatLng a, LatLng b)
        {
            return new LatLngBounds(a, b);
        }

        public static LatLngBounds toLatLngBounds(LatLngBounds a)
        {
                return a;
        }
    }
}
