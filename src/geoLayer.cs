using static georaster_layer_for_leaflet_dot_net_core.GeoRasterLayerOptions;
using Leaflet.geometry;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;
using ProjNet;
using SkiaSharp;
using Leaflet;
using System.Collections.Concurrent;

namespace georaster_layer_for_leaflet_dot_net_core
{
    public delegate string PixelValuesToColorFn(double[] values);

    public class GeoRasterLayerOptions : IDisposable
    {
        public enum ResampleMethod
        {
            bilinear,
            near
        }

        public double resolution;
        public int? debugLevel;
        public PixelValuesToColorFn? pixelValuesToColorFn;
        public LatLngBounds? bounds;
        //public Action? proj4;
        public ResampleMethod? resampleMethod;
        public GeoRaster[]? georasters;
        public GeoRaster? georaster;
        public bool noWrap;
        public Action<CustomDrawFunctionModel> customDrawFunction;

        public void Dispose()
        {
            georaster?.Dispose();
            if (georasters != null && georasters.Length > 0)
                georasters.ToList().ForEach(x => x?.Dispose());
        }
        //mask?: Mask;
        //mask_srs?: string | number;
        //mask_strategy?: MaskStrategy;
        //updateWhenIdle?: boolean; // inherited from LeafletJS
        //updateWhenZooming?: boolean; // inherited from LeafletJS
        //keepBuffer?; // inherited from LeafletJS
    }

    public class DoneCallback
    {
        public Exception error { get; set; }
        public object res { get; set; }
    }

    public class GetValuesOptions
    {
        public double? bottom;
        public double height;
        public double? left;
        public double? right;
        public double? top;
        public double width;
        public string resampleMethod;
    };

    public class GeoRaster : IDisposable
    {
        public Func<GetValuesOptions?, byte[][][]> getValues;
        public double height;
        public double noDataValue;
        public double numberOfRasters;
        public string[] palette;
        public double pixelHeight;
        public double pixelWidth;
        public double projection;
        public string rasterType;
        public string sourceType;
        //toCanvas: (e: any) => HTMLCanvasElement;
        public byte[][][] values;
        public double width;
        public double xmax;
        public double xmin;
        public double ymax;
        public double ymin;
        public bool _blob_is_available;
        public string _data;
        //_geotiff: Record<string, unknown> | undefined;
        public bool cache;
        public double firstIFDOffset;

        public void Dispose()
        {
            values = null;

        }
        //ghostValues: null;
        //ifdRequests: Promise<any>[];
        //littleEndian: boolean;
        //_url: string;
        //_url_is_available: boolean;
        //_web_worker_is_available: boolean;
    }

    public class cacheClass
    {
        public cacheClass()
        {
        }

        public Dictionary<string, object> innerTile = new Dictionary<string, object>();
        public Dictionary<string, object> tile = new Dictionary<string, object>();
    }

    public class SimplePoint
    {
        public double x { get; set; }
        public double y { get; set; }
    }

    public class Coords : SimplePoint
    {
        public double z { get; set; }
    }

    public class GetRasterOptions
    {
        public SimplePoint innerTileTopLeftPoint;
        public double heightOfSampleInScreenPixels;
        public double widthOfSampleInScreenPixels;
        public double zoom;
        public double numberOfSamplesAcross;
        public double numberOfSamplesDown;
        public double ymax;
        public double xmin;
    }

    public class geoLayer
    {
        int EPSG4326 = 4326;
        int[] PROJ4_SUPPORTED_PROJECTIONS = new int[] { 3785, 3857, 4269, 4326, 900913, 102113 };
        int MAX_NORTHING = 1000;
        int MAX_EASTING = 1000;
        int[] ORIGIN = new int[] { 0, 0 };

        public GeoRaster[]? georasters;
        public ResampleMethod resampleMethod = ResampleMethod.near;

        public double height;
        public double noDataValue;
        public string[] palette;
        public double pixelHeight;
        public double pixelWidth;
        public double projection;
        public string rasterType;
        public string sourceType;
        public double width;
        public double xmax;
        public double xmin;
        public double ymax;
        public double ymin;
        public GeoExtent extent;
        double ratio;
        private byte[][][] rasters;
        double numBands;
        int debugLevel = 0;

        public cacheClass _cache { get; set; }

        public geoLayer()
        {

        }

        public void Dispose()
        {
            rasters = null;
            if (georasters != null && georasters.Length > 0)
                georasters.ToList().ForEach(x => x.Dispose());
        }


        public bool isSimpleCRS(CRS crs)
        {
            return crs.GetType() == typeof(Simple) ||
              string.IsNullOrEmpty(crs.code) &&
                  crs.infinite &&
                  crs?.transformation?._a == 1 &&
                  crs?.transformation?._b == 0 &&
                  crs?.transformation?._c == -1 &&
                  crs?.transformation?._d == 0;
        }


        public List<object[]> zip(object[] a, object[] b) => a.Select((it, i) => new object[] { it, b[i] }).ToList();

        GeoRasterLayerOptions options;

        private int tileHeight;
        private int tileWidth;
        private bool calcStats;
        private double yMaxOfLayer;
        private double yMinOfLayer;
        private double xMaxOfLayer;
        private double xMinOfLayer;
        private MathTransform _projector;
        private LatLngBounds _bounds;
        private CRS _map;

        public geoLayer(GeoRasterLayerOptions options)
        {
            this.options = options;

        }

        public void initialize(GeoRasterLayerOptions options)
        {
            //debugger;
            try
            {
                if (options.georasters != null)
                {
                    georasters = options.georasters;
                }
                else if (options.georaster != null)
                {
                    georasters = new GeoRaster[] { options.georaster };
                }
                else
                {
                    throw new Exception("You initialized a GeoRasterLayer without a georaster or georasters value.");
                }

                //if (this.sourceType === "url")
                //{
                //    options.updateWhenIdle = false;
                //    options.updateWhenZooming = true;
                //    options.keepBuffer = 16;
                //}

                if (options.resampleMethod != null)
                {
                    resampleMethod = options.resampleMethod.Value;
                }




                //if (this.georasters.Length > 1)
                {
                    height = georasters[0].height;
                    noDataValue = georasters[0].height;
                    palette = georasters[0].palette;
                    pixelHeight = georasters[0].pixelHeight;
                    pixelWidth = georasters[0].pixelWidth;
                    projection = georasters[0].projection;
                    rasterType = georasters[0].rasterType;
                    sourceType = georasters[0].sourceType;
                    width = georasters[0].width;
                    xmax = georasters[0].xmax;
                    xmin = georasters[0].xmin;
                    ymax = georasters[0].ymax;
                    ymin = georasters[0].ymin;
                }


                _cache = new cacheClass();

                extent = new GeoExtent(new double[] { xmin, ymin, xmax, ymax }, projection);

                // used later if simple projection
                ratio = height / width;

                //this.debugLevel = options.debugLevel;
                //if (this.debugLevel >= 1) log({ options });

                rasters = georasters[0].values;

                //if (this.georasters.every((georaster: GeoRaster) => typeof georaster.values === "object"))
                //{
                //    this.rasters = this.georasters.reduce((result: number[][][], georaster: GeoRaster) => {
                //        // added double-check of values to make typescript linter and compiler happy
                //        if (georaster.values)
                //        {
                //            result = result.concat(georaster.values);
                //            return result;
                //        }
                //    }, []);
                //    if (this.debugLevel > 1) Console.WriteLine("this.rasters:", this.rasters);
                //}

                //if (options.mask)
                //{
                //    if (typeof options.mask === "string")
                //    {
                //        this.mask = fetch(options.mask).then(r => r.json()) as Promise<Mask>;
                //    }
                //    else if (typeof options.mask === "object")
                //    {
                //        this.mask = Promise.resolve(options.mask);
                //    }

                //    // default mask srs is the EPSG:4326 projection used by GeoJSON
                //    this.mask_srs = options.mask_srs || "EPSG:4326";
                //}

                //this.mask_strategy = (options.mask_strategy || "outside") as MaskStrategy;

                //this.chroma = chroma;
                //this.scale = chroma.scale();

                // could probably replace some day with a simple
                // (for var k in options) { this.options[k] = options[k]; }
                // but need to find a way around TypeScript any issues
                //L.Util.setOptions(this, options);

                /*
                    Caching the var ant tile size, so we don't recalculate everytime we
                    create a new tile
                */
                var tileSize = getTileSize();
                tileHeight = tileSize.Item1;
                tileWidth = tileSize.Item2;

                //if (this.georasters.length >= 4 && !options.pixelValuesToColorFn)
                //{
                //    throw "you must pass in a pixelValuesToColorFn if you are combining rasters";
                //}

                // total number of bands across all georasters
                //this.numBands = this.georasters.reduce((total: number, g: GeoRaster) => total + g.numberOfRasters, 0);
                //if (this.debugLevel > 1) Console.WriteLine("this.numBands:", this.numBands);

                numBands = georasters.Sum(x => x.numberOfRasters);

                //// in-case we want to track dynamic/running stats of all pixels fetched
                //this.currentStats = {
                //mins: new Array(this.numBands),
                //maxs: new Array(this.numBands),
                //ranges: new Array(this.numBands)
                //};

                // using single-band raster as grayscale
                // or mapping 2 or 3 rasters to rgb bands
                //if (
                //    [1, 2, 3].includes(this.georasters.length) &&
                //    this.georasters.every((g: GeoRaster) => g.sourceType === "url") &&
                //    this.georasters.every((g: GeoRaster) => g.numberOfRasters === 1) &&
                //    !options.pixelValuesToColorFn
                //)
                //{
                //    try
                //    {
                //        this.calcStats = true;
                //        this._dynamic = true;
                //        this.options.pixelValuesToColorFn = (values: number[]) => {
                //           var haveDataForAllBands = values.every(value => value !== undefined && value !== this.noDataValue);
                //            if (haveDataForAllBands)
                //            {
                //                return this.rawToRgb(values);
                //            }
                //        };
                //    }
                //    catch (error)
                //    {
                //        console.error("[georaster-layer-for-leaflet]", error);
                //    }
                //}

                // if you haven't specified a pixelValuesToColorFn
                // and the image is YCbCr, add a function to convert YCbCr
                //                this.checkIfYCbCr = new Promise(async resolve =>
                //                {
                //                if (this.options.pixelValuesToColorFn) return resolve(true);
                //                if (this.georasters.length === 1 && this.georasters[0].numberOfRasters === 3)
                //                {
                //                   var image = await this.georasters[0]._geotiff?.getImage();
                //                    if (image?.fileDirectory?.PhotometricInterpretation === 6)
                //                    {
                //                        this.options.pixelValuesToColorFn = (values: number[]) => {
                //                           var r = Math.round(values[0] + 1.402 * (values[2] - 0x80));
                //                           var g = Math.round(values[0] - 0.34414 * (values[1] - 0x80) - 0.71414 * (values[2] - 0x80));
                //                           var b = Math.round(values[0] + 1.772 * (values[1] - 0x80));
                //                            return `rgb(${ r},${ g},${ b})`;
                //            };
                //        }
                //    }
                //                return resolve(true);
                //});
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("ERROR initializing GeoTIFFLayer", ex.Message);
            }
        }

        public byte[][][] getRasters(GetRasterOptions options)
        {
            //var  {
            //    innerTileTopLeftPoint,
            //heightOfSampleInScreenPixels,
            //widthOfSampleInScreenPixels,
            //zoom,
            //numberOfSamplesAcross,
            //numberOfSamplesDown,
            //ymax,
            //xmin
            //} = options;
            //if (this.debugLevel >= 1) Console.WriteLine("starting getRasters with options:", options);

            // called if georaster was var ructed from URL and we need to get
            // data separately for each tile
            // aka 'COG mode'

            /*
              This function takes in coordinates in the rendered image inner tile and
              returns the y and x values in the original raster
            */
            var rasterCoordsForTileCoords = new Func<double, double, SimplePoint>((h, w) =>
            {

                var xInMapPixels = options.innerTileTopLeftPoint.x + w * options.widthOfSampleInScreenPixels;
                var yInMapPixels = options.innerTileTopLeftPoint.y + h * options.heightOfSampleInScreenPixels;

                var mapPoint = new SimplePoint() { x = xInMapPixels, y = yInMapPixels };
                //if (this.debugLevel >= 1) log({ mapPoint });

                var latlng = getMap().pointToLatLng(new Leaflet.geometry.Point(mapPoint.x, mapPoint.y), (int)options.zoom);

                double lat = latlng.Lat, lng = latlng.Lng;

                if (projection == EPSG4326)
                {
                    return new SimplePoint()
                    {
                        y = Math.Round((ymax - lat) / pixelHeight),
                        x = Math.Round((lng - xmin) / pixelWidth)
                    };
                }
                else if (getProjector() != null)
                {
                    /* source raster doesn't use latitude and longitude,
                       so need to reproject point from lat/long to projection of raster
                    */
                    double[] tt = { lng, lat };
                    tt = getProjector().Inverse().Transform(tt);

                    var x = tt[0];
                    var y = tt[1];

                    //if (x === Infinity || y === Infinity)
                    //{
                    //    if (this.debugLevel >= 1) console.error("projector converted", [lng, lat], "to", [x, y]);
                    //}
                    return new SimplePoint()
                    {
                        y = Math.Round((ymax - y) / pixelHeight),
                        x = Math.Round((x - xmin) / pixelWidth)
                    };
                }
                else
                {
                    return null;
                }
            });

            // careful not to flip min_y/max_y here
            var topLeft = rasterCoordsForTileCoords(0, 0);
            var bottomRight = rasterCoordsForTileCoords(options.numberOfSamplesDown, options.numberOfSamplesAcross);

            var getValuesOptions = new GetValuesOptions()
            {
                bottom = bottomRight?.y,
                height = options.numberOfSamplesDown,
                left = topLeft?.x,
                right = bottomRight?.x,
                top = topLeft?.y,
                width = options.numberOfSamplesAcross,
                resampleMethod = resampleMethod.ToString()// || "nearest"
            };
            //todo aliakbari
            var values = georasters[0].getValues(getValuesOptions);
            return values;


            //if (!Object.values(getValuesOptions).every(it => it !== undefined && isFinite(it))) {
            //    console.error("getRasters failed because not all values are finite:", getValuesOptions);
            //} else {
            //    // !note: The types need confirmation - SFR 2021-01-20
            //    return Promise.all(
            //        this.georasters.map((georaster: GeoRaster) =>
            //            georaster.getValues({ ...getValuesOptions, resampleMethod: this.resampleMethod || "nearest" })
            //        )
            //    ).then(valuesByGeoRaster =>
            //        valuesByGeoRaster.reduce((result: number[][][], values) => {
            //            result = result.concat(values as number[][]);
            //            return result;
            //        }, [])
            //    );
            //}


        }


        public byte[] createTile(Coords coords, DoneCallback done)
        {
            /* This tile is the square piece of the Leafvar map that we draw on */
            //var  tile = L.DomUtil.create("canvas", "leaflet-tile") as HTMLCanvasElement;
            //debugger;
            //// we do this because sometimes css normalizers will set * to box-sizing: border-box
            //tile.style.boxSizing = "content-box";

            //// start tile hidden
            //tile.style.visibility = "hidden";

            //var  context = tile.getContext("2d");

            // note that we aren't setting the tile height or width here
            // drawTile dynamically sets the width and padding based on
            // how much the georaster takes up the tile area
            return drawTile(coords, done);

            //if (this.options.tileReadyCallback)
            //    this.options.tileReadyCallback({ coords, tile });

            //return tile;
        }


        public byte[] drawTile(Coords coords, DoneCallback done)
        {
            try
            {
                //var  { debugLevel = 0 } = this;

                //if (debugLevel >= 2) Console.WriteLine("starting drawTile with", { tile, coords, context, done });

                Exception error;

                var zoom = coords.z;

                // stringified hash of tile coordinates for caching purposes
                var cacheKey = $"{coords.x},{coords.y},{coords.z}";
                //if (debugLevel >= 2) log({ cacheKey });

                var mapCRS = this.getMap();
                //if (debugLevel >= 2) log({ mapCRS });

                var inSimpleCRS = false;
                //TODO : need to check
                inSimpleCRS = isSimpleCRS(mapCRS);
                //if (debugLevel >= 2) log({ inSimpleCRS });

                // Unpacking values for increased speed
                //var  { rasters, xmin, xmax, ymin, ymax } = this;
                var rasterHeight = height;
                var rasterWidth = width;

                //xmin =  (xmax = this.getBounds().getEast()), (ymin =this.getBounds().getSouth()), (ymax = this.getBounds().getNorth())

                var extentOfLayer = new GeoExtent(new double[] { getBounds().getWest(), getBounds().getSouth(), getBounds().getEast(), getBounds().getNorth() }, inSimpleCRS ? 0 : 4326);
                //if (debugLevel >= 2) log({ extentOfLayer });

                var pixelHeight = inSimpleCRS ? extentOfLayer.height / rasterHeight : this.pixelHeight;
                var pixelWidth = inSimpleCRS ? extentOfLayer.width / rasterWidth : this.pixelWidth;
                //if (debugLevel >= 2) log({ pixelHeight, pixelWidth });

                // these values are used, so we don't try to sample outside of the raster
                //           var {
                //                xMinOfLayer, xMaxOfLayer, yMinOfLayer, yMaxOfLayer
                //} = this;
                var boundsOfTile = _tileCoordsToBounds(coords);
                //if (debugLevel >= 2) log({ boundsOfTile });

                //var { code } = mapCRS;
                //TODO : need to check
                var code = 3857;
                //code = mapCRS;
                //if (debugLevel >= 2) log({ code });
                var extentOfTile = new GeoExtent(new double[] { boundsOfTile.getWest(), boundsOfTile.getSouth(), boundsOfTile.getEast(), boundsOfTile.getNorth() }, inSimpleCRS ? 0 : 4326);
                //if (debugLevel >= 2) log({ extentOfTile });

                // create blue outline around tiles
                //if (debugLevel >= 4)
                //{
                //    if (!this._cache.tile[cacheKey])
                //    {
                //        this._cache.tile[cacheKey] = L.rectangle(extentOfTile.leafletBounds, { fillOpacity: 0 })
                //            .addTo(this.getMap())
                //            .bindTooltip(cacheKey, { direction: "center", permanent: true });
                //    }
                //}

                var extentOfTileInMapCRS = inSimpleCRS ? extentOfTile : extentOfTile.reproj(code);
                //if (debugLevel >= 2) log({ extentOfTileInMapCRS });

                //TODO : need to document for extent
                var extentOfInnerTileInMapCRS = extentOfTileInMapCRS.crop(inSimpleCRS ? extentOfLayer : extent);

                if (extentOfInnerTileInMapCRS == null) return null;

                //if (debugLevel >= 2)
                //    Console.WriteLine(
                //    "[georaster-layer-for-leaflet] extentOfInnerTileInMapCRS",
                //        extentOfInnerTileInMapCRS.reproj(inSimpleCRS ? "simple" : 4326)
                //    );
                //if (debugLevel >= 2) log({ coords, extentOfInnerTileInMapCRS, extent: this.extent });

                // create blue outline around tiles
                //if (debugLevel >= 4)
                //{
                //    if (!this._cache.innerTile[cacheKey])
                //    {
                //       var ext = inSimpleCRS ? extentOfInnerTileInMapCRS : extentOfInnerTileInMapCRS.reproj(4326);
                //        this._cache.innerTile[cacheKey] = L.rectangle(ext.leafletBounds, {
                //        color: "#F00",
                //        dashArray: "5, 10",
                //        fillOpacity: 0
                //                    }).addTo(this.getMap());
                //    }
                //}

                var widthOfScreenPixelInMapCRS = extentOfTileInMapCRS.width / tileWidth;
                var heightOfScreenPixelInMapCRS = extentOfTileInMapCRS.height / tileHeight;
                //if (debugLevel >= 3) log({ heightOfScreenPixelInMapCRS, widthOfScreenPixelInMapCRS });

                // expand tile sampling area to align with raster pixels
                var oldExtentOfInnerTileInRasterCRS = inSimpleCRS
                    ? extentOfInnerTileInMapCRS
                    : extentOfInnerTileInMapCRS.reproj(projection);
                var snapped = snapBox.snap(
                bbox: new GeoExtent(oldExtentOfInnerTileInRasterCRS.bbox, 0),
                    // pad xmax and ymin of container to tolerate ceil() and floor() in snap()
                    container: inSimpleCRS
                        ? new double[] {
                            extentOfLayer.xmin,
                            extentOfLayer.ymin - 0.25 * pixelHeight,
                            extentOfLayer.xmax + 0.25 * pixelWidth,
                            extentOfLayer.ymax
                        }
                        : new double[] { xmin, ymin - 0.25 * pixelHeight, xmax + 0.25 * pixelWidth, ymax },
                    debug: false, // debugLevel >= 2,
                    padding: null,
                    origin: inSimpleCRS ? new SimplePoint() { x = extentOfLayer.xmin, y = extentOfLayer.ymax } : new SimplePoint() { x = xmin, y = ymax },
                    scale: new SimplePoint() { x = pixelWidth, y = -pixelHeight } // negative because origin is at ymax
                            );
                var extentOfInnerTileInRasterCRS = new GeoExtent(snapped.bbox_in_coordinate_system, inSimpleCRS ? 0 : projection);

                var gridbox = snapped.bbox_in_grid_cells;
                var snappedSamplesAcross = Math.Abs(gridbox[2] - gridbox[0]);
                var snappedSamplesDown = Math.Abs(gridbox[3] - gridbox[1]);
                var rasterPixelsAcross = Math.Ceiling(oldExtentOfInnerTileInRasterCRS.width / pixelWidth);
                var rasterPixelsDown = Math.Ceiling(oldExtentOfInnerTileInRasterCRS.height / pixelHeight);
                var resolution = options.resolution;
                var layerCropExtent = inSimpleCRS ? extentOfLayer : extent;
                var recropTileOrig = oldExtentOfInnerTileInRasterCRS.crop(layerCropExtent); // may be null
                var maxSamplesAcross = 1;
                var maxSamplesDown = 1;
                if (recropTileOrig != null)
                {
                    var recropTileProj = inSimpleCRS ? recropTileOrig : recropTileOrig.reproj(code);
                    var recropTile = recropTileProj.crop(extentOfTileInMapCRS);
                    if (recropTile != null)
                    {
                        maxSamplesAcross = (int)Math.Ceiling(resolution * (recropTile.width / extentOfTileInMapCRS.width));
                        maxSamplesDown = (int)Math.Ceiling(resolution * (recropTile.height / extentOfTileInMapCRS.height));
                    }
                }

                var overdrawTileAcross = rasterPixelsAcross < maxSamplesAcross;
                var overdrawTileDown = rasterPixelsDown < maxSamplesDown;
                var numberOfSamplesAcross = overdrawTileAcross ? snappedSamplesAcross : maxSamplesAcross;
                var numberOfSamplesDown = overdrawTileDown ? snappedSamplesDown : maxSamplesDown;

                if (debugLevel >= 3)
                    Console.WriteLine(
                        "[georaster-layer-for-leaflet] extent of inner tile before snapping " +
                        extentOfInnerTileInMapCRS.reproj(inSimpleCRS ? 0 : 4326).bbox.ToString()
                    );

                // Reprojecting the bounding box back to the map CRS would expand it
                // (unless the projection is purely scaling and translation),
                // so instead just extend the old map bounding box proportionately.
                {
                    var oldrb = new GeoExtent(oldExtentOfInnerTileInRasterCRS.bbox);
                    var newrb = new GeoExtent(extentOfInnerTileInRasterCRS.bbox);
                    var oldmb = new GeoExtent(extentOfInnerTileInMapCRS.bbox);
                    if (oldrb.width != 0 && oldrb.height != 0)
                    {
                        var n0 = (newrb.xmin - oldrb.xmin) / oldrb.width * oldmb.width;
                        var n1 = (newrb.ymin - oldrb.ymin) / oldrb.height * oldmb.height;
                        var n2 = (newrb.xmax - oldrb.xmax) / oldrb.width * oldmb.width;
                        var n3 = (newrb.ymax - oldrb.ymax) / oldrb.height * oldmb.height;
                        if (!overdrawTileAcross)
                        {
                            n0 = Math.Max(n0, 0);
                            n2 = Math.Min(n2, 0);
                        }
                        if (!overdrawTileDown)
                        {
                            n1 = Math.Max(n1, 0);
                            n3 = Math.Min(n3, 0);
                        }
                        var newbox = new double[] { oldmb.xmin + n0, oldmb.ymin + n1, oldmb.xmax + n2, oldmb.ymax + n3 };
                        extentOfInnerTileInMapCRS = new GeoExtent(newbox, extentOfInnerTileInMapCRS.srs);
                    }
                }

                // create outline around raster pixels
                if (debugLevel >= 4)
                {
                    if (_cache.innerTile[cacheKey] != null)
                    {
                        var ext = inSimpleCRS ? extentOfInnerTileInMapCRS : extentOfInnerTileInMapCRS.reproj(4326);
                        //this._cache.innerTile[cacheKey] = L.rectangle(ext.leafletBounds, {
                        //color: "#F00",
                        //dashArray: "5, 10",
                        //fillOpacity: 0
                        //            }).addTo(this.getMap());
                    }
                }

                if (debugLevel >= 3)
                    Console.WriteLine(
                        "[georaster-layer-for-leaflet] extent of inner tile after snapping " +
                        extentOfInnerTileInMapCRS.reproj(inSimpleCRS ? 0 : 4326).bbox.ToString()
                    );

                // Note that the snapped "inner" tile may extend beyond the original tile,
                // in which case the padding values will be negative.

                // we round here because sometimes there will be slight floating arithmetic issues
                // where the padding is like 0.00000000000001
                var padding = new
                {
                    left = Math.Round((extentOfInnerTileInMapCRS.xmin - extentOfTileInMapCRS.xmin) / widthOfScreenPixelInMapCRS),
                    right = Math.Round((extentOfTileInMapCRS.xmax - extentOfInnerTileInMapCRS.xmax) / widthOfScreenPixelInMapCRS),
                    top = Math.Round((extentOfTileInMapCRS.ymax - extentOfInnerTileInMapCRS.ymax) / heightOfScreenPixelInMapCRS),
                    bottom = Math.Round((extentOfInnerTileInMapCRS.ymin - extentOfTileInMapCRS.ymin) / heightOfScreenPixelInMapCRS)
                };
                //if (debugLevel >= 3) log({ padding });

                var innerTileHeight = tileHeight - padding.top - padding.bottom;
                var innerTileWidth = tileWidth - padding.left - padding.right;
                //if (debugLevel >= 3) log({ innerTileHeight, innerTileWidth });

                if (debugLevel >= 4)
                {
                    var xMinOfInnerTileInMapCRS = extentOfTileInMapCRS.xmin + padding.left * widthOfScreenPixelInMapCRS;
                    var yMinOfInnerTileInMapCRS = extentOfTileInMapCRS.ymin + padding.bottom * heightOfScreenPixelInMapCRS;
                    var xMaxOfInnerTileInMapCRS = extentOfTileInMapCRS.xmax - padding.right * widthOfScreenPixelInMapCRS;
                    var yMaxOfInnerTileInMapCRS = extentOfTileInMapCRS.ymax - padding.top * heightOfScreenPixelInMapCRS;
                    //log({ xMinOfInnerTileInMapCRS, yMinOfInnerTileInMapCRS, xMaxOfInnerTileInMapCRS, yMaxOfInnerTileInMapCRS });
                }

                var canvasPadding = new
                {
                    left = Math.Max(padding.left, 0),
                    right = Math.Max(padding.right, 0),
                    top = Math.Max(padding.top, 0),
                    bottom = Math.Max(padding.bottom, 0)
                };
                var canvasHeight = tileHeight - canvasPadding.top - canvasPadding.bottom;
                var canvasWidth = tileWidth - canvasPadding.left - canvasPadding.right;

                // set padding and size of canvas tile
                //tile.style.paddingTop = canvasPadding.top + "px";
                //tile.style.paddingRight = canvasPadding.right + "px";
                //tile.style.paddingBottom = canvasPadding.bottom + "px";
                //tile.style.paddingLeft = canvasPadding.left + "px";

                //tile.height = canvasHeight;
                //tile.style.height = canvasHeight + "px";

                //tile.width = canvasWidth;
                //tile.style.width = canvasWidth + "px";
                if (debugLevel >= 3) Console.WriteLine("setting tile height to " + canvasHeight + "px");
                if (debugLevel >= 3) Console.WriteLine("setting tile width to " + canvasWidth + "px");

                // set how large to display each sample in screen pixels
                var heightOfSampleInScreenPixels = innerTileHeight / numberOfSamplesDown;
                var heightOfSampleInScreenPixelsInt = Math.Ceiling(heightOfSampleInScreenPixels);
                var widthOfSampleInScreenPixels = innerTileWidth / numberOfSamplesAcross;
                var widthOfSampleInScreenPixelsInt = Math.Ceiling(widthOfSampleInScreenPixels);

                //var map = getMap();
                var tileSize = getTileSize();

                // this converts tile coordinates (how many tiles down and right)
                // to pixels from left and top of tile pane
                //var tileNwPoint = coords.scaleBy(tileSize);
                var tileNwPoint = new Point(coords.x * tileSize.Item1, coords.y * tileSize.Item2); //coords.scaleBy(tileSize);


                //if (debugLevel >= 4) log({ tileNwPoint });
                var xLeftOfInnerTile = tileNwPoint.x + padding.left;
                var yTopOfInnerTile = tileNwPoint.y + padding.top;
                var innerTileTopLeftPoint = new SimplePoint() { x = xLeftOfInnerTile, y = yTopOfInnerTile };
                //if (debugLevel >= 4) log({ innerTileTopLeftPoint });

                // render asynchronously so tiles show up as they finish instead of all at once (which blocks the UI)
                //setTimeout(async () =>
                //{
                try
                {
                    // crate a surface
                    var info = new SKImageInfo(tileSize.Item1, tileSize.Item2);
                    using var surface = SKSurface.Create(info);

                    // the the canvas and properties
                    using var canvas = surface.Canvas;


                    //System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap((int)tileSize.Item1, (int)tileSize.Item2);
                    //using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        byte[][][] tileRasters = null;
                        if (rasters == null)
                        {
                            tileRasters = getRasters(new GetRasterOptions()
                            {
                                innerTileTopLeftPoint = innerTileTopLeftPoint,
                                heightOfSampleInScreenPixels = heightOfSampleInScreenPixels,
                                widthOfSampleInScreenPixels = widthOfSampleInScreenPixels,
                                zoom = zoom,
                                //pixelHeight,
                                //pixelWidth,
                                numberOfSamplesAcross = numberOfSamplesAcross,
                                numberOfSamplesDown = numberOfSamplesDown,
                                ymax = ymax,
                                xmin = xmin
                            });

                        }

                        //await this.checkIfYCbCr;

                        for (var h = 0; h < numberOfSamplesDown; h++)
                        {
                            var yCenterInMapPixels = yTopOfInnerTile + (h + 0.5) * heightOfSampleInScreenPixels;
                            var latWestPoint = new Point(xLeftOfInnerTile, yCenterInMapPixels);
                            var lat = mapCRS.pointToLatLng(Point.toPoint(latWestPoint), (int)zoom).Lat;
                            if (lat > yMinOfLayer && lat < yMaxOfLayer)
                            {
                                var yInTilePixels = Math.Round(h * heightOfSampleInScreenPixels) + Math.Min(padding.top, 0) + (padding.top > 0 ? padding.top : 0);

                                var yInRasterPixels = 0;
                                if (inSimpleCRS || projection == EPSG4326)
                                {
                                    yInRasterPixels = (int)Math.Floor((yMaxOfLayer - lat) / pixelHeight);
                                }

                                for (var w = 0; w < numberOfSamplesAcross; w++)
                                {
                                    var latLngPoint = new Point(
                                         xLeftOfInnerTile + (w + 0.5) * widthOfSampleInScreenPixels,
                                         yCenterInMapPixels
                                     );
                                    var xOfLayer = mapCRS.pointToLatLng(Leaflet.geometry.Point.toPoint(latLngPoint), (int)zoom).Lng;
                                    if (xOfLayer > xMinOfLayer && xOfLayer < xMaxOfLayer)
                                    {
                                        var xInRasterPixels = 0;
                                        if (inSimpleCRS || projection == EPSG4326)
                                        {
                                            xInRasterPixels = (int)Math.Floor((xOfLayer - xMinOfLayer) / pixelWidth);
                                        }
                                        else if (getProjector() != null)
                                        {
                                            double[] inverted = { xOfLayer, lat };
                                            //new SimplePoint { x= xOfLayer, y= lat }
                                            inverted = getProjector().Inverse().Transform(inverted);
                                            //todo aliakari
                                            var yInSrc = inverted[1];
                                            yInRasterPixels = (int)Math.Floor((ymax - yInSrc) / pixelHeight);
                                            if (yInRasterPixels < 0 || yInRasterPixels >= rasterHeight) continue;

                                            var xInSrc = inverted[0];
                                            xInRasterPixels = (int)Math.Floor((xInSrc - xmin) / pixelWidth);
                                            if (xInRasterPixels < 0 || xInRasterPixels >= rasterWidth) continue;
                                        }
                                        byte[] values = null;
                                        if (tileRasters != null)
                                        {
                                            // get value from array specific to this tile
                                            values = tileRasters.Select(band => band[h][w]).ToArray();
                                        }
                                        else if (rasters != null)
                                        {
                                            // get value from array with data for entire raster
                                            values = rasters.Select((band) =>
                                            {
                                                return band[yInRasterPixels][xInRasterPixels];
                                            }).ToArray();
                                        }
                                        else
                                        {
                                            //if(done != null) done( Error("no rasters are available for, so skipping value generation"));
                                            return null;
                                        }

                                        // x-axis coordinate of the starting point of the rectangle representing the raster pixel
                                        var x = Math.Round(w * widthOfSampleInScreenPixels) + Math.Min(padding.left, 0) + (padding.left > 0 ? padding.left : 0);

                                        // y-axis coordinate of the starting point of the rectangle representing the raster pixel
                                        var y = yInTilePixels;

                                        // how many real screen pixels does a pixel of the sampled raster take up
                                        var width = widthOfSampleInScreenPixelsInt;
                                        var height = heightOfSampleInScreenPixelsInt;

                                        if (options.customDrawFunction != null)
                                        {
                                            options.customDrawFunction(new CustomDrawFunctionModel
                                            {
                                                Canvas = canvas,
                                                Values = values,
                                                //context,
                                                x= x,
                                                y=y,
                                                width=width,
                                                height=height,
                                                rasterX = xInRasterPixels,
                                                rasterY = yInRasterPixels,
                                                sampleX = w,
                                                sampleY = h,
                                                sampledRaster = tileRasters
                                            });
                                        }
                                        //TODO : check and test
                                        //else
                                        //{
                                        //    var color = this.getColor(values);
                                        //    if (color && context)
                                        //    {
                                        //        context.fillStyle = color;
                                        //        context.fillRect(x, y, width, height);
                                        //    }
                                        //}
                                    }
                                }
                            }
                        }

                        // save the file
                        using var image = surface.Snapshot();
                        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

                        return data.ToArray();

                        //return bitmap;
                        //using (var stream = new MemoryStream())
                        //{
                        //    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        //    return stream.ToArray();
                        //}
                    }

                    //TODO : mask implementation
                    //if (this.mask)
                    //{
                    //    if (inSimpleCRS)
                    //    {
                    //        console.warn("[georaster-layer-for-leaflet] mask is not supported when using simple projection");
                    //    }
                    //    else
                    //    {
                    //        this.mask.then((mask: Mask) => {
                    //            geocanvas.maskCanvas({
                    //            canvas: tile,
                    //                                    // eslint-disable-next-line camelcase
                    //                                    canvas_bbox: extentOfInnerTileInMapCRS.bbox, // need to support simple projection too
                    //                                    // eslint-disable-next-line camelcase
                    //                                    canvas_srs: 3857, // default map crs, need to support simple
                    //                                    mask,
                    //                                    // eslint-disable-next-line camelcase
                    //                                    mask_srs: this.mask_srs,
                    //                                    strategy: this.mask_strategy // hide everything inside or outside the mask
                    //                                });
                    //        });
                    //    }
                    //}

                    //tile.style.visibility = "visible"; // set to default
                }
                catch (Exception e)
                {
                    //TODO : Exception handling
                    Console.Error.WriteLine(e);
                    error = e;
                }
                //if(done != null)  done(error, tile);
                //}, 0); //timeout end

                // return the tile so it can be rendered on screen

            }
            catch (Exception error)
            {
                //TODO : Exception handling
                Console.Error.WriteLine(error);
                //done && done(error, tile);
            }
            return null;
        }

        // add in to ensure backwards compatability with Leaflet 1.0.3
        private LatLngBounds _tileCoordsToNwSe(Coords coords)
        {
            var map = getMap();
            var tileSize = getTileSize();
            var nwPoint = new Point(coords.x * tileSize.Item1, coords.y * tileSize.Item2);
            var sePoint = nwPoint.add(new Point(tileSize.Item1, tileSize.Item2));
            var nw = map.pointToLatLng(nwPoint, (int)coords.z);
            var se = map.pointToLatLng(sePoint, (int)coords.z);
            return new LatLngBounds(nw, se);
        }


        private LatLngBounds _tileCoordsToBounds(Coords coords)
        {
            var bounds = _tileCoordsToNwSe(coords);

            if (!options.noWrap)
            {
                //const { crs } = this.getMap().options;
                //bounds = crs.wrapLatLngBounds(bounds);

                var crs = getMap();
                bounds = crs.wrapLatLngBounds(bounds);
            }
            return bounds;
        }


        [Obsolete("Use IsValid method", true)]
        public bool _isValidTile(Coords coords)
        {
            return IsValidTile(coords);
        }
        public bool IsValidTile(Coords coords)
        {
            var crs = getMap();

            //if (!crs.infinite)
            //{
            //    // don't load tile if it's out of bounds and not wrapped
            //    var globalBounds = this._globalTileRange;
            //    if (
            //        (!crs.wrapLng && (coords.x < globalBounds.min.x || coords.x > globalBounds.max.x)) ||
            //        (!crs.wrapLat && (coords.y < globalBounds.min.y || coords.y > globalBounds.max.y))
            //    )
            //    {
            //        return false;
            //    }
            //}

            var bounds = getBounds();

            if (bounds == null)
            {
                return true;
            }

            double x = coords.x, y = coords.y, z = coords.z;

            // not sure what srs should be here when simple crs
            var layerExtent = new GeoExtent(bounds, 4326);

            var boundsOfTile = _tileCoordsToBounds(coords);

            var extentOfTile = new GeoExtent(new double[] { boundsOfTile.getWest(), boundsOfTile.getSouth(), boundsOfTile.getEast(), boundsOfTile.getNorth() }, 4326);

            // check given tile coordinates
            if (layerExtent.overlaps(extentOfTile)) return true;

            // if not within the original confines of the earth return false
            // we don't want wrapping if using Simple CRS
            if (isSimpleCRS(crs)) return false;

            // width of the globe in tiles at the given zoom level
            var width = Math.Pow(2, z);

            // check one world to the left
            var leftCoords = new Coords() { x = x - width, y = y };
            leftCoords.z = z;
            var leftBounds = _tileCoordsToBounds(leftCoords);
            if (layerExtent.overlaps(new GeoExtent(leftBounds, 4326))) return true;

            // check one world to the right
            var rightCoords = new Coords() { x = x + width, y = y };
            rightCoords.z = z;
            var rightBounds = _tileCoordsToBounds(rightCoords);
            if (layerExtent.overlaps(new GeoExtent(rightBounds, 4326))) return true;

            return false;
        }


        // method from https://github.com/Leaflet/Leaflet/blob/bb1d94ac7f2716852213dd11563d89855f8d6bb1/src/layer/ImageOverlay.js
        public LatLngBounds getBounds()
        {
            initBounds();
            return _bounds;
        }


        public CRS getMap()
        {
            if (_map == null) _map = new EPSG3857();
            return _map;
            //return this._map || this._mapToAdd;
        }

        //public object getMapCRS()
        //{
        //    return this.getMap()?.options.crs || L.CRS.EPSG3857;
        //}

        public bool isSupportedProjection()
        {
            return true;
        }

        public int getZone(double projection)
        {
            return int.Parse(projection.ToString().Substring(3));
        }

        public string getHemisphere(double projection)
        {
            var projstr = projection.ToString();
            if (projstr.StartsWith("326"))
            {
                return "N";
            }
            else if (projstr.StartsWith("327"))
            {
                return "S";
            }
            return "";
        }

        public bool isUTM(double projection)
        {
            var projstr = projection.ToString();
            return projstr.StartsWith("326") || projstr.StartsWith("327");
        }

        public string getProjString(double projection)
        {
            var zone = getZone(projection);
            var hemisphere = getHemisphere(projection);
            return $"+proj=utm +zone={zone}{(hemisphere == "S" ? " +south " : " ")} " +
            "+ellps=WGS84 +datum=WGS84 +units=m +no_defs";
        }

        public string getProjectionString(double projection)
        {
            if (isUTM(projection))
            {
                return getProjString(projection);
            }
            return $"EPSG:{projection}";
        }


        private static CoordinateSystemServices db = new CoordinateSystemServices();
        public static object _lock = new object();
        private static CoordinateSystemFactory csFact = new CoordinateSystemFactory();
        private static ConcurrentDictionary<int, CoordinateSystem> coordinateSystems = new ConcurrentDictionary<int, CoordinateSystem>();
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
                            string tmp = "";
                            using OSGeo.OSR.SpatialReference srcS = new OSGeo.OSR.SpatialReference(null);
                            srcS.ImportFromEPSG((int)projection);
                            srcS.ExportToWkt(out tmp, null);
                            coordinateSystem = csFact.CreateFromWkt(tmp);
                            srcS.Dispose();
                        }
                        if (coordinateSystem != null)
                        {
                            coordinateSystems.TryAdd(id, coordinateSystem);
                        }
                    }
                }
            }

            return coordinateSystem;
        }

        public MathTransform getProjector()
        {
            if (isSupportedProjection() && _projector == null)
            {
                //string tmp = "";
                //var db = new CoordinateSystemServices();
                //var csFact = new CoordinateSystemFactory();
                var ctFactory = new CoordinateTransformationFactory();
                int targetProjection = 4326;
                CoordinateSystem src = GetCoordinateSystem((int)projection);

                CoordinateSystem trg = GetCoordinateSystem(targetProjection);

                _projector = ctFactory.CreateFromCoordinateSystems(src, trg).MathTransform;
            }

            return _projector;

        }

        public LatLngBounds initBounds(GeoRasterLayerOptions options = null)
        {
            if (options == null) options = this.options;
            if (_bounds == null)
            {

                if (false)
                {
                    if (height == width)
                    {
                        _bounds = new LatLngBounds(new LatLng(ORIGIN[0], ORIGIN[1]), new LatLng(MAX_NORTHING, MAX_EASTING));
                    }
                    else if (height > width)
                    {
                        _bounds = new LatLngBounds(new LatLng(ORIGIN[0], ORIGIN[1]), new LatLng(MAX_NORTHING, MAX_EASTING / ratio));
                    }
                    else if (width > height)
                    {
                        _bounds = new LatLngBounds(new LatLng(ORIGIN[0], ORIGIN[1]), new LatLng(MAX_NORTHING * ratio, MAX_EASTING));
                    }
                }
                else if (projection == EPSG4326)
                {
                    //if (debugLevel >= 1) console.log(`georaster projection is in ${ EPSG4326}`);
                    var minLatWest = new LatLng(ymin, xmin);
                    var maxLatEast = new LatLng(ymax, xmax);
                    _bounds = new LatLngBounds(minLatWest, maxLatEast);
                }
                else if (getProjector() != null)
                {
                    //if (debugLevel >= 1) console.log("projection is UTM or supported by proj4");
                    //var  bottomLeft = this.getProjector().forward({ x: xmin, y: ymin });
                    double[] bottomLeft = { xmin, ymin };
                    bottomLeft = getProjector().Transform(bottomLeft);
                    var minLatWest = new LatLng(bottomLeft[1], bottomLeft[0]);
                    double[] topRight = { xmax, ymax };
                    topRight = getProjector().Transform(topRight);
                    var maxLatEast = new LatLng(topRight[1], topRight[0]);
                    _bounds = new LatLngBounds(minLatWest, maxLatEast);
                }
                else
                {
                    //if (typeof proj4FullyLoaded !== "function")
                    //{
                    //    throw `You are using the lite version of georaster-layer-for-leaflet, which does not support rasters with the projection ${ projection}.  Please try using the default build or add the projection definition to your global proj4.`;
                    //    } else
                    //    {
                    //        throw `GeoRasterLayer does not provide built-in support for rasters with the projection ${ projection}.  Add the projection definition to your global proj4.`;
                    //    }
                    //}
                }
                // these values are used so we don't try to sample outside of the raster
                xMinOfLayer = _bounds.getWest();
                xMaxOfLayer = _bounds.getEast();
                yMaxOfLayer = _bounds.getNorth();
                yMinOfLayer = _bounds.getSouth();

                options.bounds = _bounds;
            }

            return _bounds;
        }

        //public bool same(GeoRaster[] array, key: GeoRasterKeys)
        //{
        //    return new Set(array.map(item => item[key])).size === 1;
        //}

        public (int, int) getTileSize() => (256, 256);
    }
}
