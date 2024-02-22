using ProjNet.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;
using System.Text.RegularExpressions;
using ProjNet;
using System.Collections.Concurrent;
using georaster_layer_for_leaflet_dot_net_core;

namespace Leaflet
{
    public class GeoExtent
    {
        public double xmin, xmax, ymin, ymax, width, height, area, perimeter;
        public double? srs;
        List<double[]> leafletBounds;
        SimplePoint bottomLeft, bottomRight, topLeft, topRight, center;
        public double[] bbox;
        string str;

        public GeoExtent(LatLngBounds bounds, double srs = 0)
            : this(new double[] { bounds.getWest(), bounds.getSouth(), bounds.getEast(), bounds.getNorth() }, srs)
        {

        }

        public GeoExtent(GeoExtent geoExtent)
            : this(new double[] { geoExtent.xmin, geoExtent.ymin, geoExtent.xmax, geoExtent.ymax }, geoExtent.srs)
        {
        }

        public GeoExtent(double[] corners, double? srs = 0)
        {

            string xmin_str, xmax_str, ymin_str, ymax_str;

            this.srs = srs;

            xmin = corners[0];
            ymin = corners[1];
            xmax = corners[2];
            ymax = corners[3];

            this.width = xmax-xmin;
            this.height = ymax-ymin;

            this.bottomLeft = new SimplePoint() { x= xmin, y= ymin };
            this.bottomRight = new SimplePoint() { x= xmax, y= ymin };
            this.topLeft = new SimplePoint() { x= xmin, y= ymax };
            this.topRight = new SimplePoint() { x= xmax, y= ymax };

            this.leafletBounds = new List<double[]> {
                new double[]{this.ymin, this.xmin },
                new double[]{this.ymax, this.xmax }
                };

            this.area = width*height;
            this.perimeter = width*2+height*2;

            this.bbox = new double[] { xmin, ymin, xmax, ymax };

            this.center = new SimplePoint() { x= (xmin+xmax)/2, y= (ymin+ymax)/2 };

            this.str = string.Join(",", this.bbox.Select(x => x.ToString()));
        }

        public GeoExtent clone()
        {
            return new GeoExtent(this);
        }

        public bool isDef(double? num) => num!= null;

        public GeoExtent[] _pre(GeoExtent _this, GeoExtent _other)
        {
            // convert other to an extent instance (if not already)
            _other = new GeoExtent(_other);

            if (!isDef(_this.srs) && !isDef(_other.srs))
            {
                // assume same/no projection
            }
            else if (isDef(_this.srs) && !isDef(_other.srs))
            {
                // assume other is the same srs as this
                _other = new GeoExtent(new double[] { _other.xmin, _other.ymin, _other.xmax, _other.ymax }, _this.srs);
            }
            else if (!isDef(_this.srs) && isDef(_other.srs))
            {
                // assume this' srs is the same as other
                _this = new GeoExtent(new double[] { _this.xmin, _this.ymin, _this.xmax, _this.ymax }, srs: _other.srs);
            }
            else if (isDef(_this.srs) && isDef(_other.srs) && _this.srs != _other.srs)
            {
                _other = _other.reproj(_this.srs.GetValueOrDefault());
            }
            else if (isDef(_this.srs) && isDef(_other.srs) && _this.srs == _other.srs)
            {
                // same projection, so no reprojection necessary
            }
            else
            {
                throw new Exception("UH OH");
            }
            return new GeoExtent[] { _this, _other };
        }

        public bool contains(GeoExtent other)
        {
            GeoExtent _this, _other;
            var tmp = this._pre(this, other);
            _this = tmp[0]; _other= tmp[1];

            var xContains = _other.xmin >= _this.xmin && _other.xmax <= _this.xmax;
            var yContains = _other.ymin >= _this.ymin && _other.ymax <= _this.ymax;

            return xContains && yContains;
        }

        // should return null if no overlap
        public GeoExtent? crop(GeoExtent other)
        {
            other = new GeoExtent(other);

            // if really no overlap then return null
            if (this.overlaps(other, true) == false && other.overlaps(this, true) == false) return null;

            // first check if other fully contains this extent
            // in which case, we don't really need to crop
            // and can just return the extent of this
            if (other.contains(this)) return this.clone();

            // check if special case where other crosses 180th meridian
            if (other.srs == 4326 && (other.xmin < -180 || other.xmax > 180))
            {
                var parts = other.unwrap();

                var cropped = parts.Select(it => this.crop(it)).ToList();

                // filter out any parts that are null (didn't overlap)
                cropped = cropped.Where(x => x != null).ToList();

                // no overlap
                if (cropped.Count == 0) return null;

                var combo = cropped[0];
                for (var i = 1; i < cropped.Count; i++) combo = combo.combine(cropped[i]);

                return combo;
            }

            // if both this and other have srs defined reproject
            // otherwise, assume they are the same projection
            var another = isDef(this.srs) && isDef(other.srs) ? other.reproj(this.srs.GetValueOrDefault(), true) : other.clone();
            if (another != null)
            {
                if (!this.overlaps(another)) return null;
                var xmin = Math.Max(this.xmin, another.xmin);
                var ymin = Math.Max(this.ymin, another.ymin);
                var xmax = Math.Min(this.xmax, another.xmax);
                var ymax = Math.Min(this.ymax, another.ymax);
                return new GeoExtent(new double[] { xmin, ymin, xmax, ymax }, this.srs);
            }

            // fall back to converting everything to 4326 and cropping there
            var this4326 = isDef(this.srs) ? this.reproj(4326) : this;
            var other4326 = isDef(other.srs) ? other.reproj(4326) : other;
            var tmp = this4326.bbox;
            var aMinLon = tmp[0];
            var aMinLat = tmp[1];
            var aMaxLon = tmp[2];
            var aMaxLat = tmp[3];
            tmp = other4326.bbox;
            var bMinLon = tmp[0];
            var bMinLat = tmp[1];
            var bMaxLon = tmp[2];
            var bMaxLat = tmp[3];

            if (!this4326.overlaps(other4326)) return null;

            var minLon = Math.Max(aMinLon, bMinLon);
            var minLat = Math.Max(aMinLat, bMinLat);
            var maxLon = Math.Min(aMaxLon, bMaxLon);
            var maxLat = Math.Min(aMaxLat, bMaxLat);
            return new GeoExtent(new double[] { minLon, minLat, maxLon, maxLat }, 4326).reproj(this.srs.GetValueOrDefault());
        }
        // add two extents together
        // result is a new extent in the projection of this
        public GeoExtent combine(GeoExtent other)
        {
            if (isDef(this.srs) && isDef(other.srs))
            {
                other = other.reproj(this.srs.GetValueOrDefault());
            }

            var xmin = Math.Min(this.xmin, other.xmin);
            var xmax = Math.Max(this.xmax, other.xmax);
            var ymin = Math.Min(this.ymin, other.ymin);
            var ymax = Math.Max(this.ymax, other.ymax);

            return new GeoExtent(new double[] { xmin, xmax, ymin, ymax }, this.srs);
        }

        public bool equals(GeoExtent other, double digits, bool strict /*, { digits = 13, strict = true } = { digits: 13, strict: true }*/)
        {
            // convert other to GeoExtent if necessary
            other = new GeoExtent(other);

            if (isDef(this.srs) && isDef(other.srs))
            {
                other = other.reproj(this.srs.GetValueOrDefault());
            }
            else if (strict && isDef(this.srs) != !isDef(this.srs))
            {
                return false;
            }
            var str1 = string.Join(",", this.bbox.Select(n => Math.Round(digits)));
            var str2 = string.Join(",", other.bbox.Select(n => Math.Round(digits)));
            return str1 == str2;
        }

        /*
   shouldn't accept GeoJSON as input because the extent created from a GeoJSON
   might overlap, but the actual polygon wouldn't.
   Or at least make the user have to be explicit about the functionality via
   a flag like overlaps(geojson, { strict: false })
 */
        public bool overlaps(GeoExtent other, bool quiet = true)
        {
            try
            {
                GeoExtent _this, _other;
                var tmp = this._pre(this, other);
                _this = tmp[0]; _other= tmp[1];

                var yOverlaps = _other.ymin <= _this.ymax && _other.ymax >= _this.ymin;
                var xOverlaps = _other.xmin <= _this.xmax && _other.xmax >= _this.xmin;

                return xOverlaps && yOverlaps;
            }
            catch
            {
                if (quiet) return false;
                else throw;
            }
        }


        private static CoordinateSystemServices db = new CoordinateSystemServices();
        private static CoordinateTransformationFactory ctFactory = new CoordinateTransformationFactory();
        private static CoordinateSystemFactory csFact = new CoordinateSystemFactory();
        private static ConcurrentDictionary<int, CoordinateSystem> coordinateSystems = new ConcurrentDictionary<int, CoordinateSystem>();
        private static ConcurrentDictionary<string, ICoordinateTransformation> coordinateTransformations = new ConcurrentDictionary<string, ICoordinateTransformation>();
        public static object _lock = new object();


        private CoordinateSystem GetCoordinateSystem(int id)
        {
            CoordinateSystem coordinateSystem;
            if (!coordinateSystems.TryGetValue(id, out coordinateSystem))
            {
                lock (_lock)
                {
                    if (!coordinateSystems.TryGetValue(id, out coordinateSystem))
                    {
                        try
                        {
                            coordinateSystem= db.GetCoordinateSystem($"EPSG", id);
                        }
                        catch (Exception)
                        {

                        }
                        if (coordinateSystem == null)
                        {
                            using OSGeo.OSR.SpatialReference srcS = new OSGeo.OSR.SpatialReference(null);
                            srcS.ImportFromEPSG(id);
                            srcS.ExportToWkt(out string tmp, null);
                            coordinateSystem = csFact.CreateFromWkt(tmp);
                            srcS.Dispose();
                        }
                        if(coordinateSystem != null)
                        {
                            coordinateSystems.TryAdd(id, coordinateSystem);
                        }
                    }
                }
            }

            return coordinateSystem;
        }


        private ICoordinateTransformation GetCoordinateTransformation(int from , int to)
        {
            ICoordinateTransformation coordinateTransformation;
            var key = $"{from}_{to}";
            if (!coordinateTransformations.TryGetValue(key, out coordinateTransformation))
            {
                lock (_lock)
                {
                    if (!coordinateTransformations.TryGetValue(key, out coordinateTransformation))
                    {
                        var src = GetCoordinateSystem((int)from);
                        var trg = GetCoordinateSystem((int)to);

                        if (src == null || trg == null)
                            throw new Exception("GeoExtend SRC OR TRG is null");

                        // Create a coordinate transformation from the source to target coordinate system
                        coordinateTransformation = ctFactory.CreateFromCoordinateSystems(src, trg);
                        if (coordinateTransformation != null)
                        {
                            coordinateTransformations.TryAdd(key, coordinateTransformation);
                        }
                    }
                }
            }

            return coordinateTransformation;
        }

        public double[] reprojectBoundingBox(GeoExtent bbox, double from, double to)
        {
            string tmp = "";
            try
            {
                var ct = GetCoordinateTransformation((int)from, (int)to);

                double[] topLeft = { bbox.xmin, bbox.ymax };
                topLeft = ct.MathTransform.Transform(topLeft);


                double[] topright = { bbox.xmax, bbox.ymax };
                topright = ct.MathTransform.Transform(topright);

                double[] bottomleft = { bbox.xmin, bbox.ymin };
                bottomleft = ct.MathTransform.Transform(bottomleft);
                //transform.TransformPoint(bottomleft);

                double[] bottomright = { bbox.xmax, bbox.ymin };
                bottomright = ct.MathTransform.Transform(bottomright);
                //transform.TransformPoint(bottomright);


                var corners = new double[][] { topLeft, topright, bottomleft, bottomright };

                var xs = corners.Select((corner) => corner[0]);
                var ys = corners.Select((corner) => corner[1]);

                ct = null;

                return new double[] { xs.Min(), ys.Min(), xs.Max(), ys.Max() };
            }
            catch
            {
                throw;
            }
            finally
            {
                //db = null;
                //csFact = null;
                //ctFactory = null;
            }
        }

        public GeoExtent reproj(double to, bool quiet = false)
        {
            //to = normalize(to); // normalize srs

            // don't need to reproject, so just return a clone
            if (isDef(this.srs) && this.srs == to) return this.clone();

            if (!isDef(this.srs))
            {
                if (quiet) return null;
                throw new Exception($"[geo-extent] cannot reproject ${this.bbox} without a projection set");
            }

            // unwrap, reproject pieces, and combine
            if (this.srs == 4326 && (this.xmin < -180 || this.xmax > 180))
            {
                try
                {
                    var parts = this.unwrap().Select(ext => ext.reproj(to)).ToList();
                    var combo = parts[0];
                    for (var i = 1; i < parts.Count; i++) combo = combo.combine(parts[i]);
                    return combo;
                }
                catch
                {
                    if (quiet) return null;
                    throw;
                }
            }

            var reprojected = reprojectBoundingBox(new GeoExtent(this.bbox, null), this.srs.GetValueOrDefault(), to);

            if (reprojected==null)
            {
                if (quiet) return null;
                throw new Exception($"[geo-extent] failed to reproject ${this.bbox} from ${this.srs} to ${to}");
            }
            return new GeoExtent(reprojected, to);
        }

        public GeoExtent[] unwrap()
        {
            //const { xmin, ymin, xmax, ymax, srs } = this;

            // not in 4326, so just return a clone
            if (srs != 4326) return new GeoExtent[] { this.clone() };

            // extent is within the normal extent of the earth, so return clone
            if (xmin > -180 && xmax < 180) return new GeoExtent[] { this.clone() };

            // handle special case where extent overflows xmin and then overlaps itself
            if (xmin < -180 && xmax >= xmin + 360) return new GeoExtent[] { new GeoExtent(new double[] { -180, ymin, 180, ymax }, 4326) };

            if (xmax > 180 && xmin <= xmax - 360) return new GeoExtent[] { new GeoExtent(new double[] { -180, ymin, 180, ymax }, 4326) };

            var extents = new List<GeoExtent>();

            // extent overflows left edge of the world
            if (xmin < -180)
            {
                extents.Add(new GeoExtent(new double[] { xmin + 360, ymin, 180, ymax }, srs));
            }

            // add extent for part between -180 to 180 longitude
            extents.Add(new GeoExtent(new double[] { xmin < -180 ? -180 : xmin, ymin, xmax > 180 ? 180 : xmax, ymax }, srs));

            // extent overflows right edge of the world
            if (this.xmax > 180)
            {
                extents.Add(new GeoExtent(new double[] { -180, ymin, xmax - 360, ymax }, srs));
            }

            return extents.ToArray();
        }

        private string normalize(string srs)
        {
            if (string.IsNullOrEmpty(srs)) return srs;
            if (isStr(srs) && srs.StartsWith("EPSG:")) return srs;
            if (isStr(srs) && Regex.Match(srs, @"/^\d+$/").Success) return "EPSG:" + srs;
            else if (isNum(srs)) return "EPSG:" + srs;
            //var code = getEPSGCode(srs);
            //if (isNum(code)) return "EPSG:" + code;
            return srs;
        }

        bool isStr(object o) { return o.GetType() == typeof(string); }
        bool isNum(object o) { int tmp = 0; return int.TryParse(o.ToString(), out tmp); }

    }


}
