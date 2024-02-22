namespace Leaflet.geometry
{

    /*
     * @class Bounds
     * @aka L.Bounds
     *
     * Represents a rectangular area in pixel coordinates.
     *
     * @example
     *
     * ```js
     * var p1 = L.point(10, 10),
     * p2 = L.point(40, 60),
     * bounds = L.bounds(p1, p2);
     * ```
     *
     * All Leaflet methods that accept `Bounds` objects also accept them in a simple Array form (unless noted otherwise), so the bounds example above can be passed like this:
     *
     * ```js
     * otherBounds.intersects([[10, 10], [40, 60]]);
     * ```
     *
     * Note that `Bounds` does not inherit from Leaflet's `Class` object,
     * which means new classes can't inherit from it, and new methods
     * can't be added to it with the `include` function.
     */
    public class Bounds
    {
        // @method extend(point: Point): this
        // Extends the bounds to contain the given point.

        // @alternative
        // @method extend(otherBounds: Bounds): this
        // Extend the bounds to contain the given bounds

        // @property min: Point
        // The top left corner of the rectangle.
        // @property max: Point
        // The bottom right corner of the rectangle.
        public Point min;
        public Point max;

        public Bounds(Point a, Point b)
        {
            var points = b != null ? new Point[] { a, b } : new Point[] { a };

            for (var i = 0; i < points.Length; i++)
            {
                this.extend(points[i]);
            }
        }

        public Bounds extend(Bounds obj)
        {
            // @property min: Point
            // The top left corner of the rectangle.
            // @property max: Point
            // The bottom right corner of the rectangle.
            obj = toBounds(obj);
            var min2 = obj.min;
            var max2 = obj.max;

            if (min2 == null || max2 == null) { return this; }
            if (this.min == null && this.max == null)
            {
                this.min = min2.clone();
                this.max = max2.clone();
            }
            else
            {
                this.min.x = Math.Min(min2.x, this.min.x);
                this.max.x = Math.Max(max2.x, this.max.x);
                this.min.y = Math.Min(min2.y, this.min.y);
                this.max.y = Math.Max(max2.y, this.max.y);
            }
            return this;
        }

        public Bounds extend(Point obj)
        {
            Point min2, max2;

            min2 = max2 =Point.toPoint(obj);

            // @property min: Point
            // The top left corner of the rectangle.
            // @property max: Point
            // The bottom right corner of the rectangle.
            if (this.min != null && this.max != null)
            {
                this.min = min2.clone();
                this.max = max2.clone();
            }
            else
            {
                this.min.x = Math.Min(min2.x, this.min.x);
                this.max.x = Math.Max(max2.x, this.max.x);
                this.min.y = Math.Min(min2.y, this.min.y);
                this.max.y = Math.Max(max2.y, this.max.y);
            }
            return this;
        }

        // @method getCenter(round?: Boolean): Point
        // Returns the center point of the bounds.
        public Point getCenter(bool round)
        {
            return Point.toPoint(
                    (this.min.x + this.max.x) / 2,
                    (this.min.y + this.max.y) / 2, round);
        }

        // @method getBottomLeft(): Point
        // Returns the bottom-left point of the bounds.
        public Point getBottomLeft()
        {
            return toPoint(this.min.x, this.max.y);
        }

        private Point toPoint(double x, double y)
        {
            return Point.toPoint(x, y);
        }

        // @method getTopRight(): Point
        // Returns the top-right point of the bounds.
        public Point getTopRight()
        { // -> Point
            return toPoint(this.max.x, this.min.y);
        }

        // @method getTopLeft(): Point
        // Returns the top-left point of the bounds (i.e. [`this.min`](#bounds-min)).
        public Point getTopLeft()
        {
            return this.min; // left, top
        }

        // @method getBottomRight(): Point
        // Returns the bottom-right point of the bounds (i.e. [`this.max`](#bounds-max)).
        public Point getBottomRight()
        {
            return this.max; // right, bottom
        }

        // @method getSize(): Point
        // Returns the size of the given bounds
        public Point getSize()
        {
            return this.max.subtract(this.min);
        }

        // @method contains(otherBounds: Bounds): Boolean
        // Returns `true` if the rectangle contains the given one.
        // @alternative
        // @method contains(point: Point): Boolean
        // Returns `true` if the rectangle contains the given point.
        public bool contains(Bounds obj)
        {
            Point min, max;

            min = obj.min;
            max = obj.max;

            return (min.x >= this.min.x) &&
                   (max.x <= this.max.x) &&
                   (min.y >= this.min.y) &&
                   (max.y <= this.max.y);
        }

        public bool contains(Point obj)
        {
            Point min, max;

            //if (typeof obj[0] === 'number' || obj instanceof Point) {
            //    obj = toPoint(obj);
            //} else
            //{
            //    obj = toBounds(obj);
            //}

            //if (obj instanceof Bounds) {
            //    min = obj.min;
            //    max = obj.max;
            //} else
            //{
            min = max = obj;
            //}

            return (min.x >= this.min.x) &&
                   (max.x <= this.max.x) &&
                   (min.y >= this.min.y) &&
                   (max.y <= this.max.y);
        }

        // @method intersects(otherBounds: Bounds): Boolean
        // Returns `true` if the rectangle intersects the given bounds. Two bounds
        // intersect if they have at least one point in common.
        public bool intersects(Bounds bounds)
        { // (Bounds) -> Boolean
            bounds = toBounds(bounds);

            var min = this.min;
            var max = this.max;
            var min2 = bounds.min;
            var max2 = bounds.max;
            var xIntersects = (max2.x >= min.x) && (min2.x <= max.x);
            var yIntersects = (max2.y >= min.y) && (min2.y <= max.y);

            return xIntersects && yIntersects;
        }

        // @method overlaps(otherBounds: Bounds): Boolean
        // Returns `true` if the rectangle overlaps the given bounds. Two bounds
        // overlap if their intersection is an area.
        public bool overlaps(Bounds bounds)
        { // (Bounds) -> Boolean
            bounds = toBounds(bounds);

            var min = this.min;
            var max = this.max;
            var min2 = bounds.min;
            var max2 = bounds.max;
            var xOverlaps = (max2.x > min.x) && (min2.x < max.x);
            var yOverlaps = (max2.y > min.y) && (min2.y < max.y);

            return xOverlaps && yOverlaps;
        }

        // @method isValid(): Boolean
        // Returns `true` if the bounds are properly initialized.
        public bool isValid()
        {
            return (this.min != null && this.max != null);
        }


        // @method pad(bufferRatio: Number): Bounds
        // Returns bounds created by extending or retracting the current bounds by a given ratio in each direction.
        // For example, a ratio of 0.5 extends the bounds by 50% in each direction.
        // Negative values will retract the bounds.
        public Bounds pad(double bufferRatio)
        {
            var min = this.min;
            var max = this.max;
            var heightBuffer = Math.Abs(min.x - max.x) * bufferRatio;
            var widthBuffer = Math.Abs(min.y - max.y) * bufferRatio;


            return toBounds(
                toPoint(min.x - heightBuffer, min.y - widthBuffer),
                toPoint(max.x + heightBuffer, max.y + widthBuffer));
        }


        // @method equals(otherBounds: Bounds): Boolean
        // Returns `true` if the rectangle is equivalent to the given bounds.
        public bool equals(Bounds bounds)
        {
            if (bounds == null) { return false; }

            bounds = toBounds(bounds);

            return this.min.equals(bounds.getTopLeft()) &&
                this.max.equals(bounds.getBottomRight());
        }

        public static Bounds toBounds(Bounds a)
        {
            return a;
        }

        public static Bounds toBounds(Point a, Point b)
        {

            return new Bounds(a, b);
        }

    }
}
