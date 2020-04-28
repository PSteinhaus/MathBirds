using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class ExecuteOrdersButton : UIElement
    {
        internal ExecuteOrdersButton() : base("flightPathHead.png")
        {
            Scale = 12f;
            MakeClickable(OnTouchesBegan, onTouchesEnded: OnTouchesEnded, onTouchesCancelled:OnTouchesCancelled, touchMustEndOnIt:false, IsCircleButton: true);
        }

        internal void OnTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {
            touchEvent.StopPropogation();
            if (touches.Count > 0)
            {
                // turn darker when pressed
                Color = CCColor3B.Gray;
            }
        }

        internal void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            touchEvent.StopPropogation();
            if (touches.Count > 0)
            {
                // turn back to original color when released
                Color = CCColor3B.White;
                var touch = touches[0];
                if (TouchIsOnItCircle(touch))
                {
                    Visible = false;
                    // execute orders
                    (Layer as GUILayer).ExecuteOrders();
                }
            }
        }

        internal void OnTouchesCancelled(List<CCTouch> touches, CCEvent touchEvent)
        {
            touchEvent.StopPropogation();
            if (touches.Count > 0)
            {
                // turn back to original color when released
                Color = CCColor3B.White;
            }
        }
    }
}
