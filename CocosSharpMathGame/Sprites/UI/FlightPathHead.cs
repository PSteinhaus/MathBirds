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
        internal FlightPathHead() : base("flightPathHead.png")
        {
            //Scale = 1 / Constants.STANDARD_SCALE;
            Scale = 8f;
            // add a touch listener
            MakeClickable(OnTouchesBegan, OnTouchesMoved, IsCircleButton: true);
        }

        internal void MoveTo(float x, float y)
        {
            PositionX = x;
            PositionY = y;
        }

        internal void OnTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {
            touchEvent.StopPropogation();
            if (touches.Count > 0)
            {
                var touch = touches[0];
                // TODO: show alternative orders (super-speed, shield, ...)
            }
        }

        internal void OnTouchesMoved(List<CCTouch> touches, CCEvent touchEvent)
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
