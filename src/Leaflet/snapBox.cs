using System;
using System.ComponentModel;
using System.Diagnostics;
using georaster_layer_for_leaflet_dot_net_core;

namespace Leaflet
{
    public class snapBox
    {
        public double[] bbox_in_coordinate_system { get; set; }
        public double[] bbox_in_grid_cells { get; set; }

        public static snapBox snap(GeoExtent bbox, double[] container, bool debug, SimplePoint origin, SimplePoint? padding, SimplePoint scale)
        {
            if (debug) Console.WriteLine("[snap-bbox] starting");
            if (debug) Console.WriteLine("[snap-bbox] bbox:", bbox);
            if (debug) Console.WriteLine("[snap-bbox] debug:", debug);
            if (debug) Console.WriteLine("[snap-bbox] origin:", origin);
            if (debug) Console.WriteLine("[snap-bbox] padding:", padding);
            if (debug) Console.WriteLine("[snap-bbox] scale:", scale);

            //var  [originX, originY] = origin;
            double originX = origin.x, originY = origin.y;

            if (debug) Console.WriteLine("[snap-bbox] originX:", originX);
            if (debug) Console.WriteLine("[snap-bbox] originY:", originY);

            double padX = padding == null ? 0 : padding.x, padY = padding == null ? 0 : padding.y;
            if (debug) Console.WriteLine("[snap-bbox] padX:", padX);
            if (debug) Console.WriteLine("[snap-bbox] padY:", padY);

            double scale_x = scale.x, scale_y = scale.y;
            if (debug) Console.WriteLine("[snap-bbox] scale_x:", scale_x);
            if (debug) Console.WriteLine("[snap-bbox] scale_y:", scale_y);

            // if sign is -1 then x/y value decreases
            // as grid cell number increases
            var sign_scale_x = Math.Sign(scale_x);
            var sign_scale_y = Math.Sign(scale_y);
            if (debug) Console.WriteLine("[snap-bbox] sign_scale_x:", sign_scale_x);
            if (debug) Console.WriteLine("[snap-bbox] sign_scale_y:", sign_scale_y);

            double xmin = bbox.xmin, ymin = bbox.ymin, xmax = bbox.xmax, ymax = bbox.ymax;
            if (debug) Console.WriteLine("[snap-bbox] xmin:", xmin);
            if (debug) Console.WriteLine("[snap-bbox] ymin:", ymin);
            if (debug) Console.WriteLine("[snap-bbox] xmax:", xmax);
            if (debug) Console.WriteLine("[snap-bbox] ymax:", ymax);

            var left = (xmin - originX) / scale_x;
            var right = (xmax - originX) / scale_x;
            var top = (ymax - originY) / scale_y;
            var bottom = (ymin - originY) / scale_y;
            if (debug) Console.WriteLine("[snap-bbox] left:", left);
            if (debug) Console.WriteLine("[snap-bbox] right:", right);
            if (debug) Console.WriteLine("[snap-bbox] top:", top);
            if (debug) Console.WriteLine("[snap-bbox] bottom:", bottom);

            // we're rounding here, so we don't ask for half a pixel
            var left_int = Math.Floor(left) - padX;
            var right_int = Math.Ceiling(right) + padX;

            // top_int is the number of pixels from the top edge of the grid
            // so we want to subtract the padding
            var top_int = Math.Floor(top) - padY;

            // bottom_int is the number of pixels from the top edge of the edge
            // so we want to increase the padding
            var bottom_int = Math.Ceiling(bottom) + padY;
            if (debug) Console.WriteLine("[snap-bbox] left_int:", left_int);
            if (debug) Console.WriteLine("[snap-bbox] right_int:", right_int);
            if (debug) Console.WriteLine("[snap-bbox] top_int:", top_int);
            if (debug) Console.WriteLine("[snap-bbox] bottom_int:", bottom_int);

            if (container != null)
            {
                if (debug) Console.WriteLine("[snap-bbox] container:", container);
                var min_left = (container[0] - originX) / scale_x;
                var max_right = (container[2] - originX) / scale_x;
                var min_top = (container[3] - originY) / scale_y;
                var max_bottom = (container[1] - originY) / scale_y;
                if (debug) Console.WriteLine("[snap-bbox] min_left:", min_left);
                if (debug) Console.WriteLine("[snap-bbox] max_right:", max_right);
                if (debug) Console.WriteLine("[snap-bbox] min_top:", min_top);
                if (debug) Console.WriteLine("[snap-bbox] max_bottom:", max_bottom);

                var min_left_int = Math.Ceiling(min_left);
                var max_right_int = Math.Floor(max_right);
                var min_top_int = Math.Ceiling(min_top);
                var max_bottom_int = Math.Floor(max_bottom);
                if (debug) Console.WriteLine("[snap-bbox] min_left_int:", min_left_int);
                if (debug) Console.WriteLine("[snap-bbox] max_right_int:", max_right_int);
                if (debug) Console.WriteLine("[snap-bbox] min_top_int:", min_top_int);
                if (debug) Console.WriteLine("[snap-bbox] max_bottom_int:", max_bottom_int);

                left_int = Math.Max(left_int, min_left_int);
                right_int = Math.Min(right_int, max_right_int);
                top_int = Math.Max(top_int, min_top_int);
                bottom_int = Math.Min(bottom_int, max_bottom_int);
                if (debug)
                    Console.WriteLine("[snap-bbox] after containment, left_int:", left_int);
                if (debug)
                    Console.WriteLine("[snap-bbox] after containment, right_int:", right_int);
                if (debug) Console.WriteLine("[snap-bbox] after containment, top_int:", top_int);
                if (debug)
                    Console.WriteLine("[snap-bbox] after containment, bottom_int:", bottom_int);
            }

            // need ternary expresssions below because
            // top_int is sometimes -0, which fails
            // some NodeJS strict equality tests with 0
            // however, 0 == -0 evaluates to true in NodeJS
            var bbox_in_grid_cells = new double[] {
              left_int == 0 ? 0 : left_int,
                bottom_int == 0 ? 0 : bottom_int,
                right_int == 0 ? 0 : right_int,
                top_int == 0 ? 0 : top_int
            };
            if (debug) Console.WriteLine("[snap-bbox] bbox_in_grid_cells:", bbox_in_grid_cells);

            var bbox_in_coordinate_system = new double[] {
              originX + left_int * scale_x, // xmin
                originY + bottom_int * scale_y, // ymin
                originX + right_int * scale_x, // xmax
                originY + top_int * scale_y // ymax
            };

            return new snapBox() { bbox_in_coordinate_system = bbox_in_coordinate_system, bbox_in_grid_cells = bbox_in_grid_cells };


        }
    }
}
