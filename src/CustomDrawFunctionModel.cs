using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace georaster_layer_for_leaflet_dot_net_core
{
    public class CustomDrawFunctionModel
    {
        public SKCanvas  Canvas { get; set; }
        public byte[] Values { get; set; }
        public double x { get; set; }
        public double y { get; set; }

        public double width { get; set; }
        public double height { get; set; }

        public int rasterX { get; set; }
        public int rasterY { get; set; }

        public int sampleX { get; set; }
        public int sampleY { get; set; }

        public byte[][][] sampledRaster { get; set; }
    }
}
