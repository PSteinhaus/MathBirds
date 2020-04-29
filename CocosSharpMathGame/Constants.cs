using CocosSharp;
using System;

namespace CocosSharpMathGame
{
    public static class Constants
    {
        public const int COCOS_WORLD_WIDTH = 1080;
        public const int COCOS_WORLD_HEIGHT = 1920;
        public const float STANDARD_SCALE = 8;
        public const float TURN_DURATION = 3f;  // executing orders takes 3.5 seconds

        public static float DegreesToRadians(float degrees)
        {
            return (degrees / 180f) * (float)Math.PI;
        }
        public static float RadiansToDegrees(float radians)
        {
            return ((radians / (float)Math.PI) * 180f);
        }
        public static float RadiansToCCDegrees(float radians)
        {
            return MathToCCDegrees(RadiansToDegrees(radians));
        }
        public static float CCDegreesToMathRadians(float CCdegrees)
        {
            return DegreesToRadians(CCToMathDegrees(CCdegrees));
        }

        public static void CCDegreesToDxDy(float CCdegrees, out float dx, out float dy)
        {
            var radians = CCDegreesToMathRadians(CCdegrees);
            dx = (float)Math.Cos(radians);
            dy = (float)Math.Sin(radians);
        }
        public static float DxDyToCCDegrees(float dx, float dy)
        {
            var radians = DxDyToRadians(dx, dy);
            return RadiansToCCDegrees(radians);
        }
        public static float CCDegreesToSlope(float degrees)
        {
            var radians = DegreesToRadians(CCToMathDegrees(degrees));
            return (float)(1/Math.Tan(radians));
        }

        public static float DegreesToSlope(float degrees)
        {
            var radians = DegreesToRadians(degrees);
            return (float)(1 / Math.Tan(radians));
        }

        public static float CCToMathDegrees(float degrees)
        {
            // clockwise (CC) vs counter-clockwise rotation (math)
            return -degrees;
        }
        public static float MathToCCDegrees(float degrees)
        {
            // clockwise (CC) vs counter-clockwise rotation (math)
            return -degrees;
        }

        public static float DxDyToRadians(float dx, float dy)
        {
            return (float)Math.Atan2(dy, dx);
        }

        public static float DistanceBetween(CCPoint point1, CCPoint point2)
        {
            return (float)Math.Sqrt( (point2.X - point1.X) * (point2.X - point1.X) +
                               (point2.Y - point1.Y) * (point2.Y - point1.Y) );
        }
    }
}