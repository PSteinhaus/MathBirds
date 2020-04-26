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
        /// the center of rotation (for now always the first point)
        /// </summary>
        internal CCPoint PivotPoint
        {
            get
            {
                return Points[0];
            }
        }
        internal Polygon(CCPoint[] points)
        {
            Points = points;
        }

        internal void MoveBy(float dx, float dy)
        {
            for (int i=0; i<Points.Length; i++)
            {
                Points[i].X += dx;
                Points[i].Y += dy;
            }
        }

        internal void RotateBy(float degree)
        {
            // first translate the degree into radians
            float radians = (float)((degree/180f) * Math.PI);
            for (int i = 0; i < Points.Length; i++)
            {
                // "-radians" because "RotateByAngle" rotates counter-clockwise, while CocosSharp-Rotation is clockwise
                Points[i] = CCPoint.RotateByAngle(Points[i], PivotPoint, -radians);
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
    }
}
