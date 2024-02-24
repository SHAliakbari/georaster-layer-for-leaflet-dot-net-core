using georaster_layer_for_leaflet_dot_net_core;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {

        private readonly ILogger<HomeController> _logger;
        private readonly GeoLayers geoLayers;
        private readonly IWebHostEnvironment webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, GeoLayers geoLayers,IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            this.geoLayers=geoLayers;
            this.webHostEnvironment=webHostEnvironment;
        }



        [HttpGet("{x}/{y}/{z}.png")]
        [ResponseCache(Duration = 60 * 60, Location = ResponseCacheLocation.Any)]
        public IActionResult Tile(int x, int y, int z)
        {
            //string baseAdd = System.IO.Path.GetTempPath();
            string directoryName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "tempTiles", z.ToString());
            var fileName = System.IO.Path.Combine(directoryName, ($"{x}-{y}.png"));

            if (!System.IO.File.Exists(fileName))
            {
                string geoTiffPath = System.IO.Path.Combine( webHostEnvironment.ContentRootPath, "tiffFiles", "T17TPJ_20240121T161559_TCI.jp2");
                if (System.IO.File.Exists(geoTiffPath))
                {
                    ///this is caching mechanism for loading large geotiff files (geoLayers)
                    gdal2tiles gdal2Tiles = new gdal2tiles(geoTiffPath, geoLayers);
                    var bitmap = gdal2Tiles.CreateTile(z, x, y);
                    if (bitmap != null)
                    {
                        if (!System.IO.Directory.Exists(directoryName)) System.IO.Directory.CreateDirectory(directoryName);
                        System.IO.File.WriteAllBytes(fileName, bitmap);
                    }
                }
            }
            if (System.IO.File.Exists(fileName))
            {
                return PhysicalFile(fileName, "image/jpeg");
            }
            return NotFound();
        }
    }
}
