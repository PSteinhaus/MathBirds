using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// a polygon consisting of CCPoints;
    /// it can be moved, rotated and drawn to the screen 
    /// </summary>
    internal class Polygon : ICloneable
    {
        internal CCPoint[] Points { get; set; }
        /// <summary>
        /// the center of rotation
        /// </summary>
        internal CCPoint PivotPoint { get; set; } = CCPoint.Zero;
        internal Polygon(CCPoint[] points)
        {
            Points = points;
        }

        internal virtual void MoveBy(float dx, float dy)
        {
            MovePointsBy(Points, dx, dy);
            // also move the PivotPoint
            PivotPoint = new CCPoint(PivotPoint.X + dx, PivotPoint.Y + dy);
        }

        internal virtual void RotateBy(float degree)
        {
            RotatePointsBy(Points, PivotPoint, degree);
        }

        internal static void MovePointsBy(CCPoint[] points, float dx, float dy)
        {
            for (int i = 0; i < points.Length; i++)
            {
                points[i].X += dx;
                points[i].Y += dy;
            }
        }

        internal static void RotatePointsBy(CCPoint[] points, CCPoint pivotPoint, float degrees)
        {
            // first translate the degree into radians
            float radians = Constants.DegreesToRadians(degrees);
            for (int i = 0; i < points.Length; i++)
            {
                // "-radians" because "RotateByAngle" rotates counter-clockwise, while CocosSharp-Rotation is clockwise
                points[i] = CCPoint.RotateByAngle(points[i], pivotPoint, -radians);
            }
        }
        /// <summary>
        /// Transforms the polygon.
        /// </summary>
        /// <param name="dx">the shift in x direction</param>
        /// <param name="dy">the shift in y direction</param>
        /// <param name="degree">the rotation that is applied at the end</param>
        internal void Transform(float dx, float dy, float degree)
        {
            MoveBy(dx, dy);
            RotateBy(degree);
        }

        /// <summary>
        /// Scale the Polygon, relative to the PivotPoint.
        /// </summary>
        /// <param name="scale"></param>
        internal void Scale(float scale)
        {
            for (int i=0; i<Points.Length; i++)
            {
                // get the point as vector relative to the pivot point
                CCPoint relativePoint = new CCPoint(PivotPoint.X - Points[i].X, PivotPoint.Y - Points[i].Y);
                // scale it
                CCPoint newRelativePoint = new CCPoint(relativePoint.X * scale, relativePoint.Y * scale);
                // reassign it the scaled point as the new point
                Points[i] = new CCPoint(PivotPoint.X + newRelativePoint.X, PivotPoint.Y + newRelativePoint.Y);
            }
        }

        /// <summary>
        /// Returns the closest point that is still inside the polygon
        /// </summary>
        internal CCPoint ClosestTo(CCPoint point)
        {
            if (ContainsPoint(point))
                return point;
            else
                return ClosestOnEdge(point);
        }

        /// <summary>
        /// Returns the closest point that lies on the edge/boundary of the polygon.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        internal CCPoint ClosestOnEdge(CCPoint point)
        {
            // find all quasi-closest-points
            // all Points of the polygon are quasi-closest-points
            List<CCPoint> quasiClosestPoints = new List<CCPoint>(Points);
            // all intersections of boundary-lines and perpendicular lines going through the point in question are also quasi-closest-points
            int j = Points.Length - 1;
            for (int i = 0; i < Points.Length; i++)
            {
                if (Points[i].Equals(Points[j])) continue;
                float iX = Points[i].X;
                float iY = Points[i].Y;
                float jX = Points[j].X;
                float jY = Points[j].Y;
                // edge cases: make sure there are no infinities
                if (iX == jX)
                    iX += 0.01f;   // fudge the coordinates a little
                else if (iY == jY)
                    iY += 0.01f;
                float m = (iY - jY) / (iX - jX);
                // solve y = mx + n
                float n = iY - m * iX;
                // solve point.Y = mp*point.X + np      (the p in mp and np stands for "perpendicular")
                // for np
                Console.WriteLine("m: " + m);
                float mp = -1 / m;  // perpendicular slope
                Console.WriteLine("mp: " + mp);
                float np = point.Y - mp * point.X;
                Console.WriteLine("np: " + np);
                // solve  m*x + n = mp*x + np   for x
                // m*x - mp*x = np - n
                // x*(m - mp) = np - n
                Console.WriteLine("(m - mp): " + (m - mp));
                Console.WriteLine("(np - n): " + (np - n));
                float x = (np - n) / (m - mp);
                float y = m * x + n;
                Console.WriteLine("(x,y): " + x+","+y);
                // if this intersection point lies on the polygon boundary it is a quasi-closest-point
                // to check this simply check whether the rectangle created by the two points of the polygon contains the intersection point
                float smallerX, smallerY, largerX, largerY;
                if (iX < jX)
                {
                    smallerX = iX;
                    largerX  = jX;
                }
                else
                {
                    smallerX = jX;
                    largerX  = iX;
                }
                if (iY < jY)
                {
                    smallerY = iY;
                    largerY  = jY;
                }
                else
                {
                    smallerY = jY;
                    largerY  = iY;
                }
                if (smallerX < x && x <= largerX && smallerY < y && y <= largerY)
                    quasiClosestPoints.Add(new CCPoint(x, y));
                j = i;
            }
            // now get the closest point
            var closestPoint = quasiClosestPoints[0];
            foreach (var quasiPoint in quasiClosestPoints)
                if ((quasiPoint.X - point.X) * (quasiPoint.X - point.X) + (quasiPoint.Y - point.Y) * (quasiPoint.Y - point.Y) <
                    (closestPoint.X - point.X) * (closestPoint.X - point.X) + (closestPoint.Y - point.Y) * (closestPoint.Y - point.Y))
                    closestPoint = quasiPoint;
            return closestPoint;
        }

        /// <summary>
        /// Returns whether the point is inside the polygon.
        /// Based on the ray-casting algorithm: https://en.wikipedia.org/wiki/Point_in_polygon
        /// </summary>
        /// <param name="testPoint">the point which is tested</param>
        /// <returns></returns>
        internal bool ContainsPoint(CCPoint testPoint)
        {
            int i, j;
            bool c = false;
            for (i = 0, j = Points.Length - 1; i < Points.Length; j = i++)
            {
                if (((Points[i].Y > testPoint.Y) != (Points[j].Y > testPoint.Y)) &&
                 (testPoint.X < (Points[j].X - Points[i].X) * (testPoint.Y - Points[i].Y) / (Points[j].Y - Points[i].Y) + Points[i].X))
                    c = !c;
            }
            return c;
            /*
            bool result = false;
            int j = Points.Length - 1;
            for (int i = 0; i < Points.Length; i++)
            {
                if (Points[i].Y < testPoint.Y && Points[j].Y >= testPoint.Y || Points[j].Y < testPoint.Y && Points[i].Y >= testPoint.Y)
                {
                    if (Points[i].X + (testPoint.Y - Points[i].Y) / (Points[j].Y - Points[i].Y) * (Points[j].X - Points[i].X) < testPoint.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
            */
        }

        /// <summary>
        /// returns a deep copy of the polygon
        /// </summary>
        /// <returns>a deep copy of the polygon</returns>
        public object Clone()
        {
            var clonePoints = new CCPoint[Points.Length];
            Array.Copy(Points, clonePoints, Points.Length);
            this.Clone();
            return new Polygon(clonePoints);
        }
        /// <summary>
        /// Returns a CCDrawNode that draws the polygon
        /// </summary>
        /// <returns></returns>
        internal CCDrawNode CreateDrawNode(CCColor4B fillColor, float borderWidth, CCColor4B borderColor, bool closePolygon=true)
        {
            var drawNode = new CCDrawNode();
            drawNode.DrawPolygon(Points, Points.Length, fillColor, borderWidth, borderColor, closePolygon);
            return drawNode;
        }

        internal CCDrawNode CreateDrawNode(bool closePolygon = true)
        {
            return CreateDrawNode(CCColor4B.Transparent, 2f, CCColor4B.White, closePolygon);
        }
    }
}
