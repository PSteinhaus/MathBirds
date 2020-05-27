using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
//using MathNet.Numerics.Interpolation;
using TestMySpline;

namespace CocosSharpMathGame
{
    /// <summary>
    /// A polygon where some points are connected through splines
    /// </summary>
    internal class PolygonWithSplines : Polygon
    {
        /// <summary>
        ///  0 means "no spline"
        /// -1 means "start/continue spline"
        /// 
        /// >0 means "stop spline"
        /// <-1 means "stop spline but also start a new spline from here"
        /// the abs(value) of these two cases sets how many segments will be used
        /// </summary>
        private int[] SplineControl { get; set; }
        private static readonly int SplineYes = -1;
        /// <summary>
        /// These points specify the overall structure of the polygon.
        /// The final polygon is created based on these points and the spline control information.
        /// </summary>
        internal CCPoint[] ControlPoints {
            get
            {
                return ControlPolygon.Points;
            }
            set
            {
                ControlPolygon.Points = value;
            }
        }
        private Polygon ControlPolygon { get; set; }
        internal PolygonWithSplines(CCPoint[] controlPoints, int[] splineControl = null) : base(controlPoints)
        {
            SplineControl = splineControl!=null ? splineControl : new int[controlPoints.Length];
            ControlPolygon = new Polygon(controlPoints);
            ConstructPolygon();
        }

        /// <summary>
        /// Draw a Spline through points of the polygon.
        /// </summary>
        /// <param name="startIndex">the starting point (control point) index</param>
        /// <param name="stopIndex">the stopping point (control point) index</param>
        /// <param name="segments">how many lines are used to draw splines between to control points</param>
        internal void SpecifySpline(int startIndex, int stopIndex, int segments)
        {
            for (int i = startIndex; i < stopIndex; i++)
            {
                SplineControl[i] = -1; // means "start/continue spline"
            }
            SplineControl[stopIndex] = segments;
        }

        internal override void MoveBy(float dx, float dy)
        {
            base.MoveBy(dx, dy);
            // also update the Control Polygon
            MovePointsBy(ControlPoints, dx, dy);
        }

        internal override void RotateBy(float degree)
        {
            base.RotateBy(degree);
            // also update the Control Polygon
            RotatePointsBy(ControlPoints, PivotPoint, degree);
        }

        public new object Clone()
        {
            var clonePoints = new CCPoint[Points.Length];
            var cloneControlPoints = new CCPoint[ControlPoints.Length];
            var cloneSplineControl = new int[SplineControl.Length];
            Array.Copy(Points, clonePoints, Points.Length);
            Array.Copy(ControlPoints, cloneControlPoints, ControlPoints.Length);
            Array.Copy(SplineControl, cloneSplineControl, SplineControl.Length);
            var clone = new PolygonWithSplines(cloneControlPoints, cloneSplineControl);
            clone.Points = clonePoints;
            clone.PivotPoint = PivotPoint;
            return clone;
        }

        internal void ConstructPolygon()
        {
            List<CCPoint> newPoints = new List<CCPoint>();
            List<float> splineControlPointsX = new List<float>();
            List<float> splineControlPointsY = new List<float>();
            // go through all control points
            // generate and collect the new points of the polygon
            for (int i=0; i<ControlPoints.Length; i++)
            {
                // search for the start of a spline
                if (SplineControl[i]==SplineYes)
                {
                    // add this point to the list of what will belong to this spline
                    splineControlPointsX.Add(ControlPoints[i].X);
                    splineControlPointsY.Add(ControlPoints[i].Y);
                    continue;
                }
                // search for the end of a spline
                else if (SplineControl[i] > 0 || SplineControl[i] < -1)
                {
                    // end the spline
                    // first also add the point as end point of the spline
                    splineControlPointsX.Add(ControlPoints[i].X);
                    splineControlPointsY.Add(ControlPoints[i].Y);
                    // get how many segments are intended between two control points
                    int segments = Math.Abs(SplineControl[i]);
                    // then calc the spline
                    CubicSpline.FitParametric(splineControlPointsX.ToArray(), splineControlPointsY.ToArray(), segments,
                        out float[] splineX, out float[] splineY);
                    // now add all these points to the list of new points
                    for (int j=0; j<splineX.Length-1; j++)
                        newPoints.Add(new CCPoint(splineX[j], splineY[j]));
                    // check if the last point should really be added (usual case)
                    // or not (in case another spline is starting there (because it will be added then)
                    if (SplineControl[i] > 0)
                        newPoints.Add(new CCPoint(splineX[splineX.Length-1], splineY[splineY.Length - 1]));
                    // finally clear the list of collected control points
                    splineControlPointsX.Clear();
                    splineControlPointsY.Clear();
                    continue;
                }
                // else you are really not inside a spline and can just add the control point to the Points as usual
                else
                {
                    newPoints.Add(ControlPoints[i]);
                    continue;
                }
            }
            // finally give the polygon its new form
            Points = newPoints.ToArray();
        }

        new internal void MirrorOnXAxis()
        {
            base.MirrorOnXAxis();
            ControlPolygon.MirrorOnXAxis();
        }
    }
}
