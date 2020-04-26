using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    abstract internal class UIElement : CCSprite
    {
        static protected CCSpriteSheet spriteSheet = new CCSpriteSheet("ui.plist");
        internal UIElement(string textureName) : base(spriteSheet.Frames.Find(_ => _.TextureFilename.Equals(textureName)))
        {

        }

        internal void MakeClickable(Action<List<CCTouch>,CCEvent> onTouchesBegan, Action<List<CCTouch>, CCEvent> onTouchesMoved = null, Action<List<CCTouch>, CCEvent> onTouchesEnded=null, Action<List<CCTouch>, CCEvent> onTouchesCancelled = null, bool touchMustEndOnIt=true, bool IsCircleButton=false)
        {
            Func<CCTouch, bool> touchStartedOnIt = null;
            Func<CCTouch, bool> touchIsOnIt = null;
            if (IsCircleButton)
            {
                touchStartedOnIt = (CCTouch touch) =>
                 {
                     return touch.StartLocation.IsNear(BoundingBoxTransformedToWorld.Center, ScaledContentSize.Width / 2);
                 };
                touchIsOnIt = (CCTouch touch) =>
                {
                    return touch.Location.IsNear(BoundingBoxTransformedToWorld.Center, ScaledContentSize.Width / 2);
                };
            }
            else
            {
                touchStartedOnIt = (CCTouch touch) =>
                {
                    return BoundingBoxTransformedToWorld.ContainsPoint(touch.StartLocation);
                };
                touchIsOnIt = (CCTouch touch) =>
                {
                    return BoundingBoxTransformedToWorld.ContainsPoint(touch.Location);
                };
            }
            // add a touch listener
            var touchListener = new CCEventListenerTouchAllAtOnce();
            touchListener.OnTouchesBegan = (arg1, arg2) => { if (touchStartedOnIt(arg1[0])) onTouchesBegan(arg1, arg2); };
            touchListener.OnTouchesMoved = (arg1, arg2) => { if (touchStartedOnIt(arg1[0])) onTouchesMoved(arg1, arg2); };
            touchListener.OnTouchesEnded = (arg1, arg2) => { if (touchMustEndOnIt ? touchIsOnIt(arg1[0]):touchStartedOnIt(arg1[0])) onTouchesEnded(arg1, arg2); };
            touchListener.OnTouchesCancelled = (arg1, arg2) => { if (touchStartedOnIt(arg1[0])) onTouchesCancelled(arg1, arg2); };
            AddEventListener(touchListener, this);
        }
    }
}
