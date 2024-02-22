namespace Leaflet.geometry
{

    /*
     * @class Transformation
     * @aka L.Transformation
     *
     * Represents an affine transformation: a set of coefficients `a`, `b`, `c`, `d`
     * for transforming a point of a form `(x, y)` into `(a*x + b, c*y + d)` and doing
     * the reverse. Used by Leaflet in its projections code.
     *
     * @example
     *
     * ```js
     * var transformation = L.transformation(2, 5, -1, 10),
     * 	p = L.point(1, 2),
     * 	p2 = transformation.transform(p), //  L.point(7, 8)
     * 	p3 = transformation.untransform(p2); //  L.point(1, 2)
     * ```
     */

    public class Transformation
    {
       public double _a, _b, _c, _d;

        public Transformation(double[] a)
        {
            _a=a[0];
            _b=a[1];
            _c=a[2];
            _d=a[3];
        }

        public Transformation(double a, double b, double c, double d)
        {
            _a=a;
            _b=b; _c=c; _d=d;
        }


        public static Transformation toTransformation(double a, double b, double c, double d)
        {
            return new Transformation(a, b, c, d);
        }


        // @method transform(point: Point, scale?: Number): Point
        // Returns a transformed point, optionally multiplied by the given scale.
        // Only accepts actual `L.Point` instances, not arrays.
        public Point transform(Point point, double? scale)
        { // (Point, Number) -> Point
            return this._transform(point.clone(), scale);
        }

        // destructive transform (faster)
        public Point _transform(Point point, double? scale)
        {
           var _scale = scale.GetValueOrDefault(1);
            point.x = _scale * (this._a * point.x + this._b);
            point.y = _scale * (this._c * point.y + this._d);
            return point;
        }

        // @method untransform(point: Point, scale?: Number): Point
        // Returns the reverse transformation of the given point, optionally divided
        // by the given scale. Only accepts actual `L.Point` instances, not arrays.
        public Point untransform(Point point, double? scale)
        {
           var _scale = scale.GetValueOrDefault(1);
            return new Point(
                    (point.x / _scale - this._b) / this._a,
                    (point.y / _scale - this._d) / this._c);
        }
    }
}
