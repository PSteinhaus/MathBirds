using CocosSharp;
using CSharpMath.Atom.Atoms;
using MathNet.Numerics.Random;
using SkiaSharp;
using System;

namespace CocosSharpMathGame
{
    /// <summary>
    /// A class of various (math) helper functions.
    /// </summary>
    public static class Constants
    {
        public const int COCOS_WORLD_WIDTH = 1080;
        public const int COCOS_WORLD_HEIGHT = 1920;
        public const float STANDARD_SCALE = 4;
        public const float TURN_DURATION = 3f;  // executing orders takes this long
        public const float PI = (float)Math.PI;
        public const float VERTEX_Z_GROUND = -16000f;
        public const string SAVE_NAME = "save.sav";
        public enum OS { WINDOWS, ANDROID, IOS };
        public static OS oS;    // holds a value indicating which platfrom the game is running on

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

        /// <summary>
        /// Returns the angle as returned from Atan2 (in interval [-Pi,Pi])
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <returns></returns>
        public static float DxDyToRadians(float dx, float dy)
        {
            return (float)Math.Atan2(dy, dx);
        }

        public static float DistanceBetween(CCPoint point1, CCPoint point2)
        {
            return (float)Math.Sqrt( (point2.X - point1.X) * (point2.X - point1.X) +
                               (point2.Y - point1.Y) * (point2.Y - point1.Y) );
        }

        public static CCPoint[] CCRectPoints(CCRect ccRect)
        {
            var points = new CCPoint[4];
            points[0] = ccRect.LowerLeft;
            points[1] = new CCPoint(ccRect.MinX, ccRect.MaxY);
            points[2] = ccRect.UpperRight;
            points[3] = new CCPoint(ccRect.MaxX, ccRect.MinY);
            return points;
        }

        public static float SpecialMod(float a, float n)
        {
            return (a % n + n) % n;
        }

        public static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static float AngleFromTo(float sourceA, float targetA)
        {
            var a = targetA - sourceA;
            return SpecialMod(a + PI, 2*PI) - PI;
        }

        public static float AngleFromToDeg(float sourceA, float targetA)
        {
            var a = targetA - sourceA;
            return SpecialMod(a + 180f, 360f) - 180f;
        }

        public static float AbsAngleDifference(float angle1, float angle2)
        {
            float difference1 = Math.Abs((angle1 - angle2) % (2*PI));
            float difference2 = 2*PI - difference1;
            return Math.Min(difference1, difference2);
        }

        public static float AbsAngleDifferenceDeg(float angle1, float angle2)
        {
            float difference1 = Math.Abs((angle1 - angle2) % 360f);
            float difference2 = 360f - difference1;
            return Math.Min(difference1, difference2);
        }

        /// <summary>
        /// WARNING: Do not call repeatetly in short intervals as the rng is seeded from the system clock!!! (use the overloaded version with "rng" parameter instead)
        /// </summary>
        /// <param name="point"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static CCPoint RandomPointNear(CCPoint point, float range)
        {
            var rng = new Random();
            CCPoint randomPoint = new CCPoint((float)rng.NextDouble() * range, 0);
            return point + CCPoint.RotateByAngle(randomPoint, CCPoint.Zero, (float)rng.NextDouble() * PI * 2);
        }

        public static CCPoint RandomPointNear(CCPoint point, float range, Random rng)
        {
            CCPoint randomPoint = new CCPoint((float)rng.NextDouble() * range, 0);
            return point + CCPoint.RotateByAngle(randomPoint, CCPoint.Zero, (float)rng.NextDouble() * PI * 2);
        }

        public static CCPoint RandomPointBoxnear(CCPoint point, float range)
        {
            var rng = new Random();
            return point + new CCPoint(rng.NextBoolean() ? 1 : -1 * (float)rng.NextDouble() * range, rng.NextBoolean() ? 1 : -1 * (float)rng.NextDouble() * range);
        }

        public static CCPoint RandomPointBoxnear(CCPoint point, float range, Random rng)
        {
            return point + new CCPoint((rng.NextBoolean() ? 1 : -1) * (float)rng.NextDouble() * range, (rng.NextBoolean() ? 1 : -1) * (float)rng.NextDouble() * range);
        }

        public static CCColor4B MoveHue(CCColor4B colorIn, float hueChange)
        {
            byte clamp(float v) //define a function to bound and round the input float value to 0-255
            {
                if (v < 0)
                    return 0;
                if (v > 255)
                    return 255;
                return (byte)v;
            }
            (new SKColor(colorIn.R, colorIn.G, colorIn.B)).ToHsl(out float h, out float s, out float l);
            var colorOut = SKColor.FromHsl(h + hueChange, s, l, colorIn.A);
            return new CCColor4B(colorOut.Red, colorOut.Green, colorOut.Blue, colorOut.Alpha);
        }
    }
}