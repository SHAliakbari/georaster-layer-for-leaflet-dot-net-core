using System;
using System.Drawing.Printing;

namespace Leaflet
{
    public class Util
    {


        // @function wrapNum(num: Number, range: Number[], includeMax?: Boolean): Number
        // Returns the number `num` modulo `range` in such a way so it lies within
        // `range[0]` and `range[1]`. The returned value will be always smaller than
        // `range[1]` unless `includeMax` is set to `true`.
        public static double wrapNum(double x, double[] range,bool? includeMax)
        {
            var max = range[1];
            var min = range[0];
               var d = max - min;
            return (x == max && includeMax.GetValueOrDefault(false)) ? x : ((x - min) % d + d) % d + min;
        }

        // @function formatNum(num: Number, precision?: Number|false): Number
        // Returns the number `num` rounded with specified `precision`.
        // The default `precision` value is 6 decimal places.
        // `false` can be passed to skip any processing (can be useful to avoid round-off errors).
        public static double FormatNum(double num,bool  precision)
        {
            if (precision == false) { return num; }
            return num;
        }

        public static double FormatNum(double num, int? precision)
        {
            
            var pow = Math.Pow(10, (!precision.HasValue) ? 6 : precision.Value);
            return Math.Round(num * pow) / pow;
        }

        // @function isArray(obj): Boolean
        // Compatibility polyfill for [Array.isArray](https://developer.mozilla.org/docs/Web/JavaScript/Reference/Global_Objects/Array/isArray)
        public static bool IsArray(object obj)
        {
            //return (Object.prototype.toString.call(obj) === '[object Array]');
            return obj.GetType().Name.IndexOf("array") >=0;
        }

    }
}
