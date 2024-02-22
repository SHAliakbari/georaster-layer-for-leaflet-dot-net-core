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

        public HomeController(ILogger<HomeController> logger, GeoLayers geoLayers)
        {
            _logger = logger;
            this.geoLayers=geoLayers;
        }



        [HttpGet("{x}/{y}/{z}.png")]
        [ResponseCache(Duration = 60 * 60, Location = ResponseCacheLocation.Any)]
        public IActionResult Tile(int x, int y, int z)
        {
            string baseAdd = System.IO.Path.GetTempPath();
            string directoryName = System.IO.Path.Combine(baseAdd, "tempTiles", z.ToString());
            var fileName = System.IO.Path.Combine(directoryName, ($"{x}-{y}.png"));

            if (!System.IO.File.Exists(fileName))
            {
                string geoTiffPath = "image.tif";
                if (System.IO.File.Exists(geoTiffPath))
                {
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
