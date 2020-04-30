using System;
using CocosSharp;

namespace CocosSharpMathGame
{
    public static class MyTouchExtensions
    {
        /// <summary>
        /// If the two touches have a common center that they move toward or away from, this function returns the relative change of distance between them.
        /// Else it returns <c>float.NaN</c>.
        /// </summary>
        /// <param name="touch1"></param>
        /// <param name="touch2"></param>
        /// <returns>how the distance between the two touches changed</returns>
        public static float GetZoom(CCTouch touch1, CCTouch touch2)
        {
            // get the middlepoint and check if they both move towards it
            CCPoint midPoint = new CCPoint((touch1.Location.X + touch2.Location.X) / 2, (touch1.Location.Y + touch2.Location.Y) / 2);
            if ((CCPoint.Distance(touch1.Location, midPoint) < CCPoint.Distance(touch1.PreviousLocation, midPoint) &&
                 CCPoint.Distance(touch2.Location, midPoint) < CCPoint.Distance(touch2.PreviousLocation, midPoint)) ||
                (CCPoint.Distance(touch1.Location, midPoint) > CCPoint.Distance(touch1.PreviousLocation, midPoint) &&
                 CCPoint.Distance(touch2.Location, midPoint) > CCPoint.Distance(touch2.PreviousLocation, midPoint)))
            {
                return CCPoint.Distance(touch1.PreviousLocation, touch2.PreviousLocation) / CCPoint.Distance(touch1.Location, touch2.Location);
            }
            else
                return float.NaN;
        }
    }
}