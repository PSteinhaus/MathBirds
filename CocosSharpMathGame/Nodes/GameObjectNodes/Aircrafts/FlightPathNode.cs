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
        private const int POINTS_PER_PATH = 400;
        internal CCColor4B LineColor { get; set; } = CCColor4B.White;
        internal float LineWidth { get; set; } = 4f;
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
        internal void CalculatePath(CCPoint startPosition, float startSlopeDx, float startSlopeDy, CCPoint endPosition, float endSlopeDx=float.NaN, float endSlopeDy=float.NaN)
        {
            // calculate a spline
            // as the first point of the input-path add a new point
            // this point realises the start slope
            float firstX = startPosition.X - startSlopeDx;
            float firstY = startPosition.Y - startSlopeDy;
            // also create another point as the third point
            // it makes sure that the plane HAS TO MOVE a little bit in a somewhat straight way first
            float thirdX = startPosition.X + startSlopeDx;
            float thirdY = startPosition.Y + startSlopeDy;
            var xValues = new float[] { firstX, startPosition.X, thirdX, endPosition.X };
            var yValues = new float[] { firstY, startPosition.Y, thirdY, endPosition.Y };
            CubicSpline.FitParametric(xValues, yValues, POINTS_PER_PATH, out float[] pathX, out float[] pathY, startSlopeDx, startSlopeDy, endSlopeDx, endSlopeDy);
            var newPath = new CCPoint[pathX.Length];
            // for the output skip the first point (start slope point)
            // and replace it with the start point
            newPath[0] = startPosition;
            for (int i=1; i<pathX.Length; i++)
            {
                newPath[i] = new CCPoint(pathX[i], pathY[i]);
            }
            Path = newPath;
            // draw it properly
            Clear();
            for (int i=0; i<Path.Length-1; i++)
            {
                DrawLine(Path[i], Path[i + 1], LineWidth, LineColor, CCLineCap.Round);
            }
            // calculate and update the PathLength
            var pathLength = 0f;
            for (int i=0; i<Path.Length-1; i++)
            {
                pathLength += Constants.DistanceBetween(Path[i], Path[i + 1]); //Path[i].DistanceSquared(ref Path[i + 1]);
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
        internal void Advance(float distance, out CCPoint destination, out float CCfinalDirection)
        {
            int currentIndex = (int) AdvancementAsQuasiIndex;
            float relativeAdvancementToNextPoint = AdvancementAsQuasiIndex % 1;
            if (currentIndex == Path.Length-1)  // if you've already reached the end you're done
            {
                destination = EndPoint;
                CCfinalDirection = DirectionAt(currentIndex);
                return;
            }
            //float absoluteAdvancementToNextPoint = relativeAdvancementToNextPoint * Path[currentIndex].DistanceSquared(ref Path[currentIndex + 1]);
            float absoluteAdvancementToNextPoint = relativeAdvancementToNextPoint * Constants.DistanceBetween(Path[currentIndex], Path[currentIndex+1]);
            float directionToNextPointInRadians = Constants.CCDegreesToMathRadians(DirectionAt(currentIndex));
            float absoluteXAdvancement = absoluteAdvancementToNextPoint * (float)Math.Cos(directionToNextPointInRadians);
            float absoluteYAdvancement = absoluteAdvancementToNextPoint * (float)Math.Sin(directionToNextPointInRadians);
            CCPoint currentPosition = new CCPoint(Path[currentIndex].X + absoluteXAdvancement, Path[currentIndex].Y + absoluteYAdvancement);
            // try to advance from the current position to the next point on the Path
            float distanceToNextPoint = Constants.DistanceBetween(currentPosition, Path[currentIndex + 1]); //DistanceSquared(ref Path[currentIndex + 1]);
            while (distanceToNextPoint < distance)
            {
                currentIndex = currentIndex + 1;
                currentPosition = Path[currentIndex];
                distance -= distanceToNextPoint;
                AdvancementAsQuasiIndex = currentIndex;
                if (currentIndex == Path.Length - 1) break;
                distanceToNextPoint = Constants.DistanceBetween(currentPosition, Path[currentIndex + 1]);//currentPosition.DistanceSquared(ref Path[currentIndex + 1]);
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
                AdvancementAsQuasiIndex += relativeAdvanche;
                destination = new CCPoint(newX, newY);
                CCfinalDirection = Constants.RadiansToCCDegrees(directionToNextPointInRadians);
            }
            else
            {
                destination = Path[currentIndex];
                CCfinalDirection = DirectionAt(currentIndex);
            }
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
