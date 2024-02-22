using System;
using System.Reflection.Metadata.Ecma335;

namespace Leaflet.geometry
{

    /*
     * @class Point
     * @aka L.Point
     *
     * Represents a point with `x` and `y` coordinates in pixels.
     *
     * @example
     *
     * ```js
     * var point = L.point(200, 300);
     * ```
     *
     * All Leaflet methods and options that accept `Point` objects also accept them in a simple Array form (unless noted otherwise), so these lines are equivalent:
     *
     * ```js
     * map.panBy([200, 300]);
     * map.panBy(L.point(200, 300));
     * ```
     *
     * Note that `Point` does not inherit from Leaflet's `Class` object,
     * which means new classes can't inherit from it, and new methods
     * can't be added to it with the `include` function.
     */
    public class Point
    {
        public double x;
        public double y;

        public Point(double x, double y, bool round = false)
        {
            this.x = (round ? Math.Round(x) : x);
            // @property y: Number; The `y` coordinate of the point
            this.y = (round ? Math.Round(y) : y);
        }

        public static double trunc(double v)
        {
            return v > 0 ? Math.Floor(v) : Math.Ceiling(v);
        }

        public Point clone()
        {
            return new Point(this.x, this.y);
        }

        // @method add(otherPoint: Point): Point
        // Returns the result of addition of the current and the given points.
        public Point add(Point point)
        {
            // non-destructive, returns a new point
            return this.clone()._add(toPoint(point));
        }

        public Point _add(Point point)
        {
            // destructive, used directly for performance in situations where it's safe to modify existing point
            this.x += point.x;
            this.y += point.y;
            return this;
        }

        // @method subtract(otherPoint: Point): Point
        // Returns the result of subtraction of the given point from the current.
        public Point subtract(Point point)
        {
            return this.clone()._subtract(toPoint(point));
        }

        public Point _subtract(Point point)
        {
            this.x -= point.x;
            this.y -= point.y;
            return this;
        }

        // @method divideBy(num: Number): Point
        // Returns the result of division of the current point by the given number.
        public Point divideBy(double num)
        {
            return this.clone()._divideBy(num);
        }

        public Point _divideBy(double num)
        {
            this.x /= num;
            this.y /= num;
            return this;
        }

        // @method multiplyBy(num: Number): Point
        // Returns the result of multiplication of the current point by the given number.
        public Point multiplyBy(double num)
        {
            return this.clone()._multiplyBy(num);
        }

        public Point _multiplyBy(double num)
        {
            this.x *= num;
            this.y *= num;
            return this;
        }

        // @method scaleBy(scale: Point): Point
        // Multiply each coordinate of the current point by each coordinate of
        // `scale`. In linear algebra terms, multiply the point by the
        // [scaling matrix](https://en.wikipedia.org/wiki/Scaling_%28geometry%29#Matrix_representation)
        // defined by `scale`.
        public Point scaleBy(Point point)
        {
            return new Point(this.x * point.x, this.y * point.y);
        }

        // @method unscaleBy(scale: Point): Point
        // Inverse of `scaleBy`. Divide each coordinate of the current point by
        // each coordinate of `scale`.
        public Point unscaleBy(Point point)
        {
            return new Point(this.x / point.x, this.y / point.y);
        }

        // @method round(): Point
        // Returns a copy of the current point with rounded coordinates.
        public Point round()
        {
            return this.clone()._round();
        }

        public Point _round()
        {
            this.x = Math.Round(this.x);
            this.y = Math.Round(this.y);
            return this;
        }

        // @method floor(): Point
        // Returns a copy of the current point with floored coordinates (rounded down).
        public Point floor()
        {
            return this.clone()._floor();
        }

        public Point _floor()
        {
            this.x = Math.Floor(this.x);
            this.y = Math.Floor(this.y);
            return this;
        }

        // @method ceil(): Point
        // Returns a copy of the current point with ceiled coordinates (rounded up).
        public Point ceil()
        {
            return this.clone()._ceil();
        }

        public Point _ceil()
        {
            this.x = Math.Ceiling(this.x);
            this.y = Math.Ceiling(this.y);
            return this;
        }

        // @method trunc(): Point
        // Returns a copy of the current point with truncated coordinates (rounded towards zero).
        public Point trunc()
        {
            return this.clone()._trunc();
        }

        public Point _trunc()
        {
            this.x = trunc(this.x);
            this.y = trunc(this.y);
            return this;
        }

        // @method distanceTo(otherPoint: Point): Number
        // Returns the cartesian distance between the current and the given points.
        public double distanceTo(Point point)
        {
            point = toPoint(point);

            var x = point.x - this.x;
            var y = point.y - this.y;

            return Math.Sqrt((double)(x * x + y * y));
        }

        // @method equals(otherPoint: Point): Boolean
        // Returns `true` if the given point has the same coordinates.
        public bool equals(Point point)
        {
            point = toPoint(point);

            return point.x == this.x &&
                   point.y == this.y;
        }

        // @method contains(otherPoint: Point): Boolean
        // Returns `true` if both coordinates of the given point are less than the corresponding current point coordinates (in absolute values).
        public bool contains(Point point)
        {
            point = toPoint(point);

            return Math.Abs(point.x) <= Math.Abs(this.x) &&
                   Math.Abs(point.y) <= Math.Abs(this.y);
        }

        // @method toString(): String
        // Returns a string representation of the point for debugging purposes.
        public string toString()
        {
            return $"Point(${x},{y})";

        }

        public static Point toPoint(Point? x) => x;

        public static Point toPoint(double[] x) => new Point(x[0], x[1]);

        public static Point toPoint(double x, double y, bool round = false)
        {

            //if (typeof x === 'object' && 'x' in x && 'y' in x) {
            //    return new Point(x.x, x.y);
            //}
            return new Point(x, y, round);
        }


    }
}