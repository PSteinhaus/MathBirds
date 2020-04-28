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
                float m = (Points[i].Y - Points[j].Y) / (Points[i].X - Points[j].X);
                // solve y = mx + n
                float n = Points[i].Y - m * Points[i].X;
                // solve point.Y = mp*point.X + np      (the p in mp and np stands for "perpendicular")
                // for np
                float mp = -1 / m;  // perpendicular slope
                float np = point.Y - mp * point.X;
                // solve  m*x + n = mp*x + np   for x
                // m*x - mp*x = np - n
                // x*(m - mp) = np - n
                float x = (np - n) / (m - mp);
                float y = m * x + n;
                // if this intersection point lies on the polygon boundary it is a quasi-closest-point
                // to check this simply check whether the rectangle created by the two points of the polygon contains the intersection point
                float smallerX, smallerY, largerX, largerY;
                if (Points[i].X < Points[j].X)
                {
                    smallerX = Points[i].X;
                    largerX  = Points[j].X;
                }
                else
                {
                    smallerX = Points[j].X;
                    largerX  = Points[i].X;
                }
                if (Points[i].Y < Points[j].Y)
                {
                    smallerY = Points[i].Y;
                    largerY  = Points[j].Y;
                }
                else
                {
                    smallerY = Points[j].Y;
                    largerY  = Points[i].Y;
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
