using georaster_layer_for_leaflet_dot_net_core;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;
using OSGeo.GDAL;
using OSGeo.OSR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nUnitTests
{
    public class mainTests
    {
        //const string baseURL = "https://landsat-pds.s3.amazonaws.com/c1/L8/024/030/LC08_L1TP_024030_20180723_20180731_01_T1/LC08_L1TP_024030_20180723_20180731_01_T1_B.TIF";
        //readonly string band4URL = baseURL.Replace("B.TIF", "B4.TIF");
        //readonly string band5URL = baseURL.Replace("B.TIF", "B5.TIF");
        GeoRaster georaster;

        public mainTests()
        {

        }

        [SetUp]
        public void Setup()
        {
            Gdal.AllRegister();
            MaxRev.Gdal.Core.Proj.Configure();

            string fileName = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "files", "cea.tif");
            using Dataset dataset = Gdal.Open(fileName, Access.GA_ReadOnly);
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

            };
        }
        [TearDown]
        public void TearDown()
        {
            georaster.Dispose();
        }

        [Test]
        public void TestGeoRasterValues()
        {
            Assert.That(georaster.height, Is.EqualTo(515));
            Assert.That(georaster.width, Is.EqualTo(514));
            Assert.That(georaster.noDataValue, Is.EqualTo(0));
            Assert.That(Math.Round(georaster.xmin, 2), Is.EqualTo(-28493.17));
            Assert.That(Math.Round(georaster.xmax, 2), Is.EqualTo(2358.21));
            Assert.That(Math.Round(georaster.ymin, 2), Is.EqualTo(4224973.14));
            Assert.That(Math.Round(georaster.ymax, 2), Is.EqualTo(4255884.54));
            Assert.That(Math.Round(georaster.pixelHeight, 2), Is.EqualTo(60.02));
            Assert.That(Math.Round(georaster.pixelWidth, 2), Is.EqualTo(60.02));
            Assert.That(Math.Round(georaster.projection, 2), Is.EqualTo(4267));
        }

        [Test]
        public void TestBounds()
        {
            geoLayer tmp = new geoLayer(new GeoRasterLayerOptions()
            {
                georaster = georaster,
                resolution= 256 // optional parameter for adjusting display resolution
            });

            var bounds = tmp.getBounds();
            Assert.That(System.Math.Round(bounds.getSouthEast().Lat, 2), Is.EqualTo(-35.45));
            Assert.That(System.Math.Round(bounds.getSouthEast().Lng, 2), Is.EqualTo(-53.17));
            Assert.That(System.Math.Round(bounds.getNorthWest().Lat, 2), Is.EqualTo(13.14));
            Assert.That(System.Math.Round(bounds.getNorthWest().Lng, 2), Is.EqualTo(-161.79));
            tmp.Dispose();
        }

        [Test]
        public void TestBoundsInProj84()
        {
            geoLayer tmp = new geoLayer(new GeoRasterLayerOptions()
            {
                georaster = georaster,
                resolution= 256 // optional parameter for adjusting display resolution
            });

            SimplePoint bounds = tmp.getTopLeftPointInProj("8439",zoom = 8);

            Assert.That(System.Math.Round(bounds.getSouthEast().Lat, 2), Is.EqualTo(-35.45));
            Assert.That(System.Math.Round(bounds.getSouthEast().Lng, 2), Is.EqualTo(-53.17));
            Assert.That(System.Math.Round(bounds.getNorthWest().Lat, 2), Is.EqualTo(13.14));
            Assert.That(System.Math.Round(bounds.getNorthWest().Lng, 2), Is.EqualTo(-161.79));
            tmp.Dispose();
        }

        [Test]
        public void TestgetProjector()
        {
            geoLayer tmp = new geoLayer(new GeoRasterLayerOptions()
            {
                georaster = georaster,
                resolution= 256 // optional parameter for adjusting display resolution
            });

            var proj = tmp.getProjector();
            //TODO : check WKT value
            //Assert.That(proj.WKT, Is.EqualTo(""));
        }

        [Test]
        public void TestIsValidTile()
        {
            geoLayer tmp = new geoLayer(new GeoRasterLayerOptions()
            {
                georaster = georaster,
                resolution= 256 // optional parameter for adjusting display resolution
            });

            Assert.That(tmp.IsValidTile(new Coords { x = 199, y = 200, z = 8 }), Is.EqualTo(false));
            Assert.That(tmp.IsValidTile(new Coords { x = 199, y = 200, z = 8 }), Is.EqualTo(false));
            Assert.That(tmp.IsValidTile(new Coords { x = 199, y = 200, z = 8 }), Is.EqualTo(false));
            Assert.That(tmp.IsValidTile(new Coords { x = 199, y = 200, z = 8 }), Is.EqualTo(false));
            Assert.That(tmp.IsValidTile(new Coords { x = 199, y = 200, z = 8 }), Is.EqualTo(false));
        }
    }
}
