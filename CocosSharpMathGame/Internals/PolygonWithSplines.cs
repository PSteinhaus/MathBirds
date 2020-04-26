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
        private CCPoint[] ControlPoints { get; set; }
        internal PolygonWithSplines(CCPoint[] controlPoints, int[] splineControl) : base(controlPoints)
        {
            SplineControl = splineControl;
            ControlPoints = controlPoints;
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
                    /*
                    //var spline = CubicSpline.InterpolateNatural(splineControlPointsX, splineControlPointsY);
                    // now for each control point
                    for (int j=0; j<splineControlPointsX.Count; j++)
                    {
                        // add the control point itself (but only if it isn't the last point AND a new spline is supposed to start here)
                        if (j != splineControlPointsX.Count - 1 || SplineControl[i]>0)
                            newPoints.Add(new CCPoint((float)splineControlPointsX[j], (float)splineControlPointsY[j]));
                        // then (if it is not the end point)
                        if (j != splineControlPointsX.Count-1)
                        {
                            // get the x-distance to the next control point
                            double dx = splineControlPointsX[j + 1] - splineControlPointsX[j];
                            // divide it into "segments" many parts of equal length, go there and add points (based on the spline) in between
                            // don't add the end point (as that will be done in the next i-loop iteration)
                            for(int k=1; k<segments; k++)
                            {
                                double segmentEndX = splineControlPointsX[j] + ((dx * k) / segments);
                                double segmentEndY = spline.Interpolate(segmentEndX);
                                newPoints.Add(new CCPoint((float)segmentEndX, (float)segmentEndY));
                            }
                        }
                    }
                    */
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
    }
}
