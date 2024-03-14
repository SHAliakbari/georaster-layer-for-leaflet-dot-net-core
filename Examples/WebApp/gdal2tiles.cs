using georaster_layer_for_leaflet_dot_net_core;
using OSGeo.GDAL;
using OSGeo.OSR;
using SkiaSharp;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;

namespace WebApp
{
   

    public class gdal2tiles : IDisposable
    {
        GeoRasterLayerOptions geoRasterLayerOptions;
        geoLayer geoLayer;

        public gdal2tiles(string geoTiffPath, GeoLayers geoLayers)
        {
            geoLayer = geoLayers.Get(geoTiffPath, () =>
            {
                using Dataset dataset = Gdal.Open(geoTiffPath, Access.GA_ReadOnly);
                if (dataset == null)
                {
                    throw new Exception("Failed to open the GeoTIFF dataset.");
                }

                double[] geoTransform = new double[6];
                dataset.GetGeoTransform(geoTransform);

                GeoTiffPosition position = new GeoTiffPosition
                {
                    MinX = geoTransform[0],
                    MaxX = geoTransform[0] + geoTransform[1] * dataset.RasterXSize,
                    MaxY = geoTransform[3],
                    MinY = geoTransform[3] + geoTransform[5] * dataset.RasterYSize
                };
                using SpatialReference srsRaster = new SpatialReference(dataset.GetProjection());
                double noData = 0;
                List<byte[][]> bytes = new List<byte[][]>();
                for (int bandNo = 1; bandNo <= dataset.RasterCount; bandNo++)
                {
                    //var boundaries = ExtractGeoTiffPositionInMeter(dataset);
                    Band band = dataset.GetRasterBand(bandNo);
                    double val;
                    int hasval;
                    band.GetMinimum(out val, out hasval);
                    if (hasval != 0) Console.WriteLine("   Minimum: " + val.ToString());
                    band.GetMaximum(out val, out hasval);
                    if (hasval != 0) Console.WriteLine("   Maximum: " + val.ToString());

                    band.GetNoDataValue(out noData, out hasval);
                    if (hasval != 0) Console.WriteLine("   NoDataValue: " + val.ToString());
                    band.GetOffset(out val, out hasval);
                    if (hasval != 0) Console.WriteLine("   Offset: " + val.ToString());
                    band.GetScale(out val, out hasval);
                    if (hasval != 0) Console.WriteLine("   Scale: " + val.ToString());



                    int rasterWidth = dataset.RasterXSize;
                    int rasterHeight = dataset.RasterYSize;

                    byte[] buffer = new byte[rasterWidth * rasterHeight];

                    var res = band.ReadRaster(0, 0, rasterWidth, rasterHeight, buffer, rasterWidth, rasterHeight, 0, 0);
                    byte[][] result = new byte[rasterHeight][];
                    for (int i = 0; i < rasterHeight; i++)
                    {
                        result[i] = new byte[rasterWidth];
                        for (int j = 0; j < rasterWidth; j++)
                        {
                            result[i][j] = buffer[i * rasterWidth + j];
                        }
                    }
                    bytes.Add(result);
                    buffer = null;
                }


                geoRasterLayerOptions = new GeoRasterLayerOptions()
                {

                    georaster= new GeoRaster
                    {
                        cache = true,
                        height = dataset.RasterYSize,
                        width = dataset.RasterXSize,
                        numberOfRasters = 1,
                        noDataValue =noData,
                        xmin = position.MinX,
                        xmax = position.MaxX,
                        ymin = position.MinY,
                        ymax = position.MaxY,
                        pixelHeight = Math.Abs(geoTransform[5]),
                        pixelWidth = Math.Abs(geoTransform[1]),
                        projection = int.Parse(srsRaster.GetAttrValue("AUTHORITY", 1)),
                        values = bytes.ToArray()

                    },
                    customDrawFunction= (CustomDrawFunctionModel model) =>
                    {
                        SKColor color = SKColors.Transparent;

                        if (model.Values.Length == 3)
                        {
                            color = new SKColor((byte)model.Values[0], (byte)model.Values[1], (byte)model.Values[2]);
                        }
                        if (model.Values.Length == 1)
                        {
                            color = new SKColor(model.Values[0]);
                        }

                        using SKPaint sKPaint = new SKPaint()
                        {
                            Color = color,
                            IsAntialias = true,
                            Style = SKPaintStyle.Fill
                        };
                        model.Canvas.DrawRect((int)model.x, (int)model.y, (int)model.width, (int)model.height, sKPaint);

                    },
                    debugLevel=4,
                    resolution= 256 // optional parameter for adjusting display resolution
                };

                geoLayer tmp = new geoLayer(geoRasterLayerOptions);
                tmp.initialize(geoRasterLayerOptions);
                return tmp;

            });
        }


        public void Dispose()
        {
        }


        public byte[] CreateTile(int zoom, int x, int y)
        {
            if (geoLayer.IsValidTile(new Coords { x=x, y=y, z=zoom }))
            {
                //Console.WriteLine($"Generating z={zoom} x={x} y={y}");
                return geoLayer.createTile(new Coords() { x = x, y = y, z = zoom }, null);
            }
            return null;
        }


    }
}
