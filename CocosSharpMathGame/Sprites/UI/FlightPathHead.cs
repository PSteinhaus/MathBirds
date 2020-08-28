using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// The FlightPathHead is positioned at the end of a flight path.
    /// It can be moved (for example by touching and dragging it) to change the flight path.
    /// </summary>
    internal class FlightPathHead : UIElement
    {
        internal const float MAX_DISTANCE_FROM_CENTER = 10000;
        internal FlightPathHead() : base("flightPathHead.png")
        {
            Scale = Constants.STANDARD_SCALE * 1.25f;
            RadiusFactor = 1.5f;  // make the button a bit easier to hit
            // add a touch listener
            MakeClickable(IsCircleButton: true);
        }

        internal void MoveTo(float x, float y)
        {
            PositionX = x;
            PositionY = y;
        }

        private protected override void OnTouchesBeganUI(List<CCTouch> touches, CCEvent touchEvent)
        {
            touchEvent.StopPropogation();
            if (touches.Count > 0)
            {
                var touch = touches[0];
                // TODO: show alternative orders (super-speed, shield, ...)
            }
        }
        /// <summary>
        /// Returns the geometrical center of mass of all the flight path heads of the player
        /// </summary>
        /// <returns></returns>
        internal CCPoint PlayerHeadCenter()
        {
            var center = CCPoint.Zero;
            if (Layer is PlayLayer pl)
            {
                foreach (var aircraft in pl.PlayerAircrafts)
                {
                    center += aircraft.FlightPathHeadPos;
                }
                center /= pl.PlayerAircrafts.Count;
            }
            return center;
        }

        internal void EnsureProximityToOtherPlayerHeads()
        {
            bool testing = true;
            while (testing)
            {
                // additionally calc the geometrical center of mass (of all player flightPathHeads)
                CCPoint headCenter = PlayerHeadCenter();
                // and make sure that you're close enough to it
                CCPoint vecToCenter = headCenter - Position;
                float delta = vecToCenter.Length - MAX_DISTANCE_FROM_CENTER;
                if (delta > 0)
                {
                    Position += CCPoint.Normalize(vecToCenter) * (delta + 5f);
                }
                else
                    testing = false;
            }
        }

        private protected override void OnTouchesMovedUI(List<CCTouch> touches, CCEvent touchEvent)
        {
            touchEvent.StopPropogation();
            if (touches.Count > 0)
            {
                var touch = touches[0];
                // move to the position that is allowed and closest to the touch (the closest point that is still inside the ManeuverPolygon)
                MoveHeadToClosestPointInsideManeuverPolygon(touch.Location);
            }
        }

        private void MoveHeadToClosestPointInsideManeuverPolygon(CCPoint point)
        {
            (Parent as FlightPathControlNode).MoveHeadToClosestPointInsideManeuverPolygon(point);
        }
    }
}
