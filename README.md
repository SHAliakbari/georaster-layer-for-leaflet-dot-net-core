# georaster-layer-for-leaflet-dot-net-core
Hosts a C# .NET Core 8 project that migrates the functionality of the existing georaster-layer-for-leaflet library into a server-side processing application. 

GeoRasterLayer-for-Leaflet-Dot-Net-Core


GeoRasterLayer-for-Leaflet-Dot-Net-Core is a C# .NET Core 8 class library that simplifies rendering GeoTIFFs and other raster data on a Leaflet map within an ASP.NET Core backend. Whether you’re building GIS applications, visualizing satellite imagery, or working with geospatial data, this library provides a powerful foundation.

Features
GeoRaster Rendering: Convert GeoTIFFs into a format compatible with GeoRasterLayer.
Server-Side Backend: Utilize ASP.NET Core for handling requests and serving geospatial data.
Leaflet Integration: Display raster layers on a Leaflet map.
Custom Rendering Options: Customize colors, context drawing, and more.

Installation

Install the NuGet package from the NuGet Gallery (In comming versions):

	

Usage

Server-Side Configuration:
Add the necessary services and middleware in your ASP.NET Core application.
Initialize the GeoRasterLayer and serve the tile to leaflet request.

by default the endpoint will be : 
	

Frontend Integration:
Create a Leaflet map and add simple layer to server endpoint . 


Examples:
Explore the examples folder for sample code snippets.
Use simple HTML, JavaScript, and CSS to create interactive maps.


Contributing
Contributions are welcome! If you find a bug, have an enhancement idea, or want to add more features, feel free to submit a pull request.

License
This project is licensed under the MIT License. See the LICENSE file for details.
