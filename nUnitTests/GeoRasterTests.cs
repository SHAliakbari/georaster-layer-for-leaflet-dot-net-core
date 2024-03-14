using georaster_layer_for_leaflet_dot_net_core;

namespace nUnitTests
{
    public class GeoRasterTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestNegetiveValueForHeightAndWidth()
        {
            var tmp = new GeoRaster();
            try
            {
                tmp.height = -1;
                tmp.width = -1;
            }
            catch
            {
                //TODO : change variables to properties with validation
                //Assert.Fail();
            }

        }
    }
}