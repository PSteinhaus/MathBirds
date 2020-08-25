using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using TestMySpline;

namespace CocosSharpMathGame
{
    /// <summary>
    /// a DrawNode that is supposed to always stay at (0,0);
    /// it can calculate, return and draw a flight path;
    /// </summary>
    internal class FlightPathNode : CCDrawNode
    {
        private const int POINTS_PER_PATH = 100;
        /// <summary>
        /// Divide the POINTS_PER_PATH by this number and only draw so many lines.
        /// </summary>
        private const int DRAWING_QUOTIENT = 8;
        internal CCColor4B LineColor { get; set; } = CCColor4B.White;
        internal float LineWidth { get; set; } = 3f;
        internal CCPoint[] Path { get; private set; }
        internal float PathLength { get; private set; } = 0f;
        /// <summary>
        /// models advanchement on the path as a quasi index
        /// this value / 1 is the index of the Path point most recently passed
        /// this value % 1 is where you are between the most recent point and the next point in percent
        /// </summary>
        internal float AdvancementAsQuasiIndex { get; private set; } = 0f;
        internal float SlopeAtEnd
        {
            get
            {
                var lastPoint = Path[Path.Length - 1];
                var secondToLastPoint = Path[Path.Length - 2];
                return (lastPoint.Y - secondToLastPoint.Y) / (lastPoint.X - secondToLastPoint.X);
            }
        }
        internal float SlopeAtStart
        {
            get
            {
                var firstPoint = Path[0];
                var secondPoint = Path[1];
                return (secondPoint.Y - firstPoint.Y) / (secondPoint.X - firstPoint.X);
            }
        }
        internal CCPoint StartPoint
        {
            get { return Path[0]; }
        }
        internal CCPoint EndPoint
        {
            get { return Path[Path.Length-1]; }
        }
        internal void CalculatePath(CCPoint startPosition, float startSlopeDx, float startSlopeDy, CCPoint endPosition, float endSlopeDx = float.NaN, float endSlopeDy = float.NaN)
        {
            List<CCPoint> pathPoints = new List<CCPoint>();
            // fixes for numerical problems
            if (startSlopeDx < 0.0001 && 0 < startSlopeDx) startSlopeDx = 0.001f;
            if (startSlopeDx > -0.0001 && 0 > startSlopeDx) startSlopeDx = -0.001f;
            if (startSlopeDy < 0.0001 && 0 < startSlopeDy) startSlopeDy = 0.001f;
            if (startSlopeDy > -0.0001 && 0 > startSlopeDy) startSlopeDy = -0.001f;
            if (startSlopeDx == 0) startSlopeDx = 0.001f;
            if (startSlopeDy == 0) startSlopeDy = 0.001f;

            // ALTERNATIVE METHOD:
            // create a path that is a circular arc
            // for this 1. find the rotational center
            // 2. rotate the start point towards the end point around the rotational center, step by step, and add these points to the path

            // SPECIAL CASE: the path is a straight line
            CCPoint normalizedVectorStartEnd = CCPoint.Normalize(endPosition - startPosition);
            const float DELTA = 0.01f;
            if (CCPoint.Distance(normalizedVectorStartEnd, new CCPoint(startSlopeDx, startSlopeDy)) < DELTA)
            {
                /*
                var xValues = new float[] { startPosition.X, (endPosition.X + startPosition.X) / 2, endPosition.X };
                var yValues = new float[] { startPosition.Y, (endPosition.Y + startPosition.Y) / 2, endPosition.Y };
                CubicSpline.FitParametric(xValues, yValues, POINTS_PER_PATH, out float[] pathX, out float[] pathY);
                for (int i=0; i<pathX.Length; i++)
                {
                    pathPoints.Add(new CCPoint(pathX[i], pathY[i]));
                }
                */
                pathPoints.Add(startPosition);
                pathPoints.Add(endPosition);
            }
            else
            {
                // 1.1 solve yStart = -1/(dy/dx) * xStart + n  for n (line through start perpendicular to the start direction)
                // 1.2 get the midpoint between start and end
                // 1.3 solve yMid = -1/((endY-startY)/(endX-startX)) * xMid + n2  for n2 (line through midpoint perpendicular to the line from start to end)
                // 1.4 solve -1/(dy/dx) * x + n = -1/((endY-startY)/(endX-startX)) * x + n2  for x (intersection of the previous two lines, aka "the rotational center") 

                // 1.1
                // yStart = - dx/dy * xStart + n
                // yStart + dx/dy * xStart = n
                float n = startPosition.Y + (startSlopeDx / startSlopeDy) * startPosition.X;
                // 1.2
                CCPoint midPoint = (endPosition + startPosition) / 2;
                // 1.3
                // yMid + ((endX-startX)/(endY-startY)) * xMid = n2
                float n2 = midPoint.Y + ((endPosition.X - startPosition.X) / (endPosition.Y - startPosition.Y)) * midPoint.X;
                // 1.4
                // - dx/dy * x + n = - ((endX-startX)/(endY-startY)) * x + n2
                // - dx/dy * x + ((endX-startX)/(endY-startY)) * x = n2 - n
                // x = (n2 - n) / ((dx/dy) - ((endX-startX)/(endY-startY)))
                float xRotCenter = (n2 - n) / (((endPosition.X - startPosition.X) / (endPosition.Y - startPosition.Y)) - (startSlopeDx / startSlopeDy));
                float yRotCenter = -(startSlopeDx / startSlopeDy) * xRotCenter + n;
                CCPoint rotationPoint = new CCPoint(xRotCenter, yRotCenter);

                // 2.1 find out whether to rotate left or right
                // for that rotate the start-direction-vector by 90° and by -90° and check which rotated vector is closer to the rotation point 
                CCPoint rotatedLeft = CCPoint.RotateByAngle(new CCPoint(startSlopeDx, startSlopeDy), CCPoint.Zero, (float)Math.PI / 2) + startPosition;
                CCPoint rotatedRight = CCPoint.RotateByAngle(new CCPoint(startSlopeDx, startSlopeDy), CCPoint.Zero, -(float)Math.PI / 2) + startPosition;
                float angleSign;
                if (CCPoint.Distance(rotatedLeft, rotationPoint) < CCPoint.Distance(rotatedRight, rotationPoint))
                {
                    // the rotation point is on your left, so rotate to the left
                    angleSign = 1;
                }
                else
                {
                    // the rotation point is on your right, so rotate to the right
                    angleSign = -1;
                }

                // ALTERNATE PATH COMPUTATION:
                // compute the angle between the vectors starting at the rotational center and ending in a) the startpoint b) the endpoint.
                // The path points are then generated by rotating the startpoint (using that point for each rotation) and increasing the angle
                // of rotation until it reaches the computed angle between the vectors.
                // The number of steps is dependent on the length of the line, calculated from the radius * the angle.
                float radius = CCPoint.Distance(rotationPoint, startPosition);
                CCPoint normalizedVectorRotStart = CCPoint.Normalize(startPosition - rotationPoint);
                CCPoint normalizedVectorRotEnd   = CCPoint.Normalize(endPosition - rotationPoint);
                float angleStart = Constants.DxDyToRadians(normalizedVectorRotStart.X, normalizedVectorRotStart.Y);
                float angleEnd   = Constants.DxDyToRadians(normalizedVectorRotEnd.X, normalizedVectorRotEnd.Y);
                // make sure angles are positive
                if (angleStart < 0) angleStart = (float)(2.0 * Math.PI) + angleStart;
                if (angleEnd < 0) angleEnd = (float)(2.0 * Math.PI) + angleEnd;
                // normalize the angles
                float angleShift = (float)(2.0 * Math.PI) - angleStart;
                angleEnd = (angleEnd + angleShift) % (float)(2.0 * Math.PI);
                float angleDestination = angleSign == 1 ? angleEnd : (float)(2.0*Math.PI) - angleEnd;
                //int steps = (int)(radius * angleDestination);
                //if (steps < 200) steps = 200;
                int steps = 250;
                for (int i=0; i < steps; i++)
                {
                    CCPoint pathPoint = CCPoint.RotateByAngle(startPosition, rotationPoint, angleSign * angleDestination * ((float)i / (float)steps));
                    pathPoints.Add(pathPoint);
                }
                /*
                CCPoint pathPoint = startPosition;
                pathPoints.Add(pathPoint);
                float radius = CCPoint.Distance(rotationPoint, startPosition);
                float dAlpha = 3f / radius;
                float STOP_DISTANCE = ((radius * (float)Math.PI) / ((float)Math.PI / dAlpha)) * 2;
                while (CCPoint.Distance(pathPoint, endPosition) > STOP_DISTANCE)
                {
                    // rotate the path point a little towards the end position
                    pathPoint = CCPoint.RotateByAngle(pathPoint, rotationPoint, dAlpha * angleSign);
                    pathPoints.Add(pathPoint);
                }
                */
                pathPoints.Add(endPosition);
            }

            /*
            // calculate a spline
            // as the first point of the input-path add a new point
            // this point realises the start slope
            float firstX = startPosition.X - startSlopeDx;
            float firstY = startPosition.Y - startSlopeDy;
            // also create another point as the third point
            // it makes sure that the plane HAS TO MOVE a little bit in a somewhat straight way first
            float secondX = startPosition.X + 1 * startSlopeDx;
            float secondY = startPosition.Y + 1 * startSlopeDy;
            float thirdX = startPosition.X + 10 * startSlopeDx;
            float thirdY = startPosition.Y + 10 * startSlopeDy;

            // now calculate a special midpoint; it strongly defines the curvature of the path
            // start with the midpoint between start and end
            CCPoint midpoint = new CCPoint((endPosition.X + startPosition.X) / 2, (endPosition.Y + startPosition.Y) / 2);
            // now we need the perpendicular line going through that point (midpoint.Y = (-1/m)*midpoint.X + np) (mp = -1/m)
            float m = (endPosition.Y - startPosition.Y) / (endPosition.X - startPosition.X);
            float mp = -1 / m;
            float np = midpoint.Y - midpoint.X * mp;
            // now get the line extending from the starting point with the startSlope (startPosition.Y = startSlope*startPosition.X + ns)
            float ns = startPosition.Y - (startSlopeDy / startSlopeDx) * startPosition.X;
            // next find the intersection point that these lines form (startSlope*x + ns = mp*x + np)
            // x*(startSlope - mp) = np - ns;
            float x = (np - ns) / ((startSlopeDy / startSlopeDx) - mp);
            float y = mp * x + np;
            // finally, as the special curvature point calculate the midpoint between the start-end-midpoint and intersection point
            float curvaturePointX = midpoint.X + ((x - midpoint.X) / 3f);
            float curvaturePointY = midpoint.Y + ((y - midpoint.Y) / 3f);
            // ADDITIONAL PROCESS FOR REFINING THIS FURTHER:
            // first get the curvature point as a vector relative to the midpoint
            CCPoint curveVector = new CCPoint(curvaturePointX - midpoint.X, curvaturePointY - midpoint.Y);
            // if it's not (0,0) (i.e. if there is any curvature at all)
            float curveFactor = 0;
            float halfDistance = CCPoint.Distance(startPosition, midpoint);
            float magicDistanceFactor = halfDistance / 900f < 1 ? halfDistance / 900f : 1;
            if (!curveVector.Equals(CCPoint.Zero))
            {
                // normalize it
                curveVector = CCPoint.Normalize(curveVector);
                // now we need to calculate the factor by which it is to be scaled
                // for that we calculate the scalar product of the normalized direction vector of the starting slope and the normalized direction vector from start to end point
                float scalarProduct = CCPoint.Dot(new CCPoint(startSlopeDx, startSlopeDy), CCPoint.Normalize(new CCPoint(endPosition.X - startPosition.X, endPosition.Y - startPosition.Y)));
                // the larger this product, the less curvature
                curveFactor = 1 - scalarProduct;
                //Console.WriteLine("CurveVector: " + curveVector);
                //Console.WriteLine("CurveFactor: " + curveFactor);
                //Console.WriteLine("Distance: " + CCPoint.Distance(startPosition, midpoint));
                // now calculate the curvature point
                curvaturePointX = midpoint.X + curveVector.X * curveFactor * (1.3f-0.8f* magicDistanceFactor) * halfDistance * (curveFactor > 1 ? -1 : 1);
                curvaturePointY = midpoint.Y + curveVector.Y * curveFactor * (1.3f-0.8f* magicDistanceFactor) * halfDistance * (curveFactor > 1 ? -1 : 1);
                //Console.WriteLine("Midpoint: " + midpoint);
                //Console.WriteLine("CurvaturePoint: " + curvaturePointX + "," + curvaturePointY);
            }
            float[] xValues, yValues;
            magicDistanceFactor = halfDistance / 900f;
            if (curveFactor/magicDistanceFactor > 0.55f)
            {
                xValues = new float[] { startPosition.X, secondX, thirdX, curvaturePointX, endPosition.X };
                yValues = new float[] { startPosition.Y, secondY, thirdY, curvaturePointY, endPosition.Y };
            }
            else
            {
                xValues = new float[] { startPosition.X, secondX, thirdX, endPosition.X };
                yValues = new float[] { startPosition.Y, secondY, thirdY, endPosition.Y };
            }
            //var xValues = new float[] { startPosition.X, curvaturePointX, endPosition.X };
            //var yValues = new float[] { startPosition.Y, curvaturePointY, endPosition.Y };
            CubicSpline.FitParametric(xValues, yValues, POINTS_PER_PATH/4, out float[] pathX1, out float[] pathY1);// startSlopeDx, startSlopeDy, endSlopeDx, endSlopeDy);
            // get the point before the endpoint to adjust the curvature
            float xBeforeEnd = pathX1[pathX1.Length - 2];
            float yBeforeEnd = pathY1[pathY1.Length - 2];
            if (curveFactor/magicDistanceFactor > 0.55f)
            {
                xValues = new float[] { startPosition.X, secondX, thirdX, curvaturePointX, xBeforeEnd, endPosition.X };
                yValues = new float[] { startPosition.Y, secondY, thirdY, curvaturePointY, yBeforeEnd, endPosition.Y };
            }
            else
            {
                xValues = new float[] { startPosition.X, secondX, thirdX, xBeforeEnd, endPosition.X };
                yValues = new float[] { startPosition.Y, secondY, thirdY, yBeforeEnd, endPosition.Y };
            }
            CubicSpline.FitParametric(xValues, yValues, POINTS_PER_PATH, out float[] pathX, out float[] pathY);
            var newPath = new CCPoint[pathX.Length];

            // for the output skip the first point (start slope point)
            // and replace it with the start point
            newPath[0] = startPosition;
            for (int i = 1; i < pathX.Length; i++)
            {
                newPath[i] = new CCPoint(pathX[i], pathY[i]);
            }
            Path = newPath;
            */

            Path = pathPoints.ToArray();
            // draw it properly
            Clear();
            // don't draw all the points, a portion of it is enough
            int k = 0;
            for (; k < Path.Length - DRAWING_QUOTIENT; k += DRAWING_QUOTIENT)
                DrawLine(Path[k], Path[k + DRAWING_QUOTIENT], LineWidth, LineColor, CCLineCap.Round);
            // make sure the last point is hit
            if (k != Path.Length - 1 - DRAWING_QUOTIENT)
                DrawLine(Path[k], Path[Path.Length - 1], LineWidth, LineColor, CCLineCap.Round);
            // calculate and update the PathLength
            var pathLength = 0f;
            for (int i=0; i<Path.Length-1; i++)
            {
                pathLength += Constants.DistanceBetween(Path[i], Path[i + 1]);
            }
            PathLength = pathLength;
            // reset the advancement to 0
            AdvancementAsQuasiIndex = 0f;
        }

        /// <summary>
        /// Advance the current position some distance on the flight path.
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="destination">position after the advance</param>
        /// <param name="cCfinalDirection">direction of the segment that the position is on after the advance</param>
        internal void Advance(CCPoint currentPosition, float distance, out CCPoint destination, out float CCfinalDirection)
        {
            int currentIndex = (int) AdvancementAsQuasiIndex;
            float relativeAdvancementToNextPoint = AdvancementAsQuasiIndex % 1;
            if (currentIndex == Path.Length-1)  // if you've already reached the end you're done
            {
                destination = EndPoint;
                CCfinalDirection = DirectionAt(currentIndex);
                return;
            }
            float absoluteAdvancementToNextPoint = relativeAdvancementToNextPoint * CCPoint.Distance(Path[currentIndex], Path[currentIndex+1]);
            float directionToNextPointInRadians = Constants.CCDegreesToMathRadians(DirectionAt(currentIndex));
            float absoluteXAdvancement = absoluteAdvancementToNextPoint * (float)Math.Cos(directionToNextPointInRadians);
            float absoluteYAdvancement = absoluteAdvancementToNextPoint * (float)Math.Sin(directionToNextPointInRadians);
            //CCPoint currentPosition = new CCPoint(Path[currentIndex].X + absoluteXAdvancement, Path[currentIndex].Y + absoluteYAdvancement);
            // try to advance from the current position to the next point on the Path
            float distanceToNextPoint = CCPoint.Distance(currentPosition, Path[currentIndex + 1]);
            while (distanceToNextPoint < distance)
            {
                currentIndex = currentIndex + 1;
                currentPosition = Path[currentIndex];
                distance -= distanceToNextPoint;
                AdvancementAsQuasiIndex = currentIndex;
                if (currentIndex == Path.Length - 1) break;
                distanceToNextPoint = CCPoint.Distance(currentPosition, Path[currentIndex + 1]);
            }
            // if you can't go far enough just move towards the point
            if (distance > 0 && currentIndex != Path.Length - 1)
            {
                // update the direction
                directionToNextPointInRadians = Constants.CCDegreesToMathRadians(DirectionAt(currentIndex));
                // calculate how far you can go
                // relative between the points
                float relativeAdvanche = distance / distanceToNextPoint;
                // in x direction
                float newX = currentPosition.X + distance * (float)Math.Cos(directionToNextPointInRadians);
                // in y direction
                float newY = currentPosition.Y + distance * (float)Math.Sin(directionToNextPointInRadians);
                AdvancementAsQuasiIndex = (float)currentIndex + relativeAdvanche;
                destination = new CCPoint(newX, newY);
                CCfinalDirection = Constants.RadiansToCCDegrees(directionToNextPointInRadians);
            }
            else
            {
                destination = Path[currentIndex];
                CCfinalDirection = DirectionAt(currentIndex);
            }
            //
        }

        /// <summary>
        /// Returns the Direction (in CCDegrees) of the segment
        /// starting at pathIndex and ending at pathIndex +1.
        /// For the Endpoint the same value is returned as for endIndex-1.
        /// </summary>
        /// <param name="pathIndex"></param>
        /// <returns></returns>
        internal float DirectionAt(int pathIndex)
        {
            if (pathIndex == Path.Length - 1) pathIndex--;
            return Constants.DxDyToCCDegrees(Path[pathIndex+1].X - Path[pathIndex].X, Path[pathIndex + 1].Y - Path[pathIndex].Y);
        }
    }
}
