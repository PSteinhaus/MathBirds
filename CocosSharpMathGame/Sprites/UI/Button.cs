using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal abstract class Button : UIElement
    {
        internal Button(string textureName, bool isCircleButton) : base(textureName)
        {
            MakeClickable(OnTouchesBegan, onTouchesEnded: OnTouchesEnded, onTouchesCancelled: OnTouchesCancelled, touchMustEndOnIt: false, IsCircleButton: isCircleButton);
        }

        internal virtual void OnTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {
            touchEvent.StopPropogation();
            if (touches.Count > 0)
            {
                // turn darker when pressed
                Color = CCColor3B.DarkGray;
            }
        }

        internal virtual void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            touchEvent.StopPropogation();
            if (touches.Count > 0)
            {
                // turn back to original color when released
                Color = CCColor3B.White;
                var touch = touches[0];
                if (TouchIsOnIt(touch))
                {
                    ButtonEnded(touch);
                }
            }
        }

        internal virtual void OnTouchesCancelled(List<CCTouch> touches, CCEvent touchEvent)
        {
            touchEvent.StopPropogation();
            if (touches.Count > 0)
            {
                // turn back to original color when released
                Color = CCColor3B.White;
            }
        }

        private protected abstract void ButtonEnded(CCTouch touch);
    }
}
