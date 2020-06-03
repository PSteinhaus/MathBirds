using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    abstract internal class UIElement : GameObjectSprite
    {
        static internal CCSpriteSheet spriteSheet = new CCSpriteSheet("ui.plist");
        protected bool Pressed { get; set; } = false;
        internal UIElement(string textureName) : base(spriteSheet.Frames.Find(_ => _.TextureFilename.Equals(textureName)))
        {

        }

        internal void MakeClickable(Action<List<CCTouch>,CCEvent> onTouchesBegan, Action<List<CCTouch>, CCEvent> onTouchesMoved = null, Action<List<CCTouch>, CCEvent> onTouchesEnded=null, Action<List<CCTouch>, CCEvent> onTouchesCancelled = null, bool touchMustEndOnIt=true, bool IsCircleButton=false)
        {
            Func<CCTouch, bool> touchStartedOnIt = null;
            Func<CCTouch, bool> touchIsOnIt = null;
            if (IsCircleButton)
            {
                touchStartedOnIt = TouchStartedOnItCircle;
                touchIsOnIt = TouchIsOnItCircle;
            }
            else
            {
                touchStartedOnIt = TouchStartedOnIt;
                touchIsOnIt = TouchIsOnIt;
            }
            // add a touch listener
            var touchListener = new CCEventListenerTouchAllAtOnce();
            touchListener.OnTouchesBegan = (arg1, arg2) =>                                     { if (MyVisible && touchStartedOnIt(arg1[0]))                              { arg2.StopPropogation(); Pressed = true;  onTouchesBegan(arg1, arg2); } };
            if(onTouchesMoved!=null) touchListener.OnTouchesMoved = (arg1, arg2) =>    { if (MyVisible && Pressed)                                                        { arg2.StopPropogation(); onTouchesMoved(arg1, arg2); } };
            if (onTouchesEnded != null) touchListener.OnTouchesEnded = (arg1, arg2) => { if (MyVisible && touchMustEndOnIt ? touchIsOnIt(arg1[0]) : true && Pressed)      { arg2.StopPropogation(); Pressed = false; onTouchesEnded(arg1, arg2); } };
            else touchListener.OnTouchesEnded = (arg1, arg2) =>                        { if (MyVisible && Pressed)                                                        { arg2.StopPropogation(); Pressed = false; } };
            if (onTouchesCancelled != null) touchListener.OnTouchesCancelled = (arg1, arg2) => { if (MyVisible && Pressed)                                                { arg2.StopPropogation(); Pressed = false; onTouchesCancelled(arg1, arg2); } };
            else touchListener.OnTouchesCancelled = (arg1, arg2) =>                    { if (MyVisible && Pressed)                                                        { arg2.StopPropogation(); Pressed = false; } };
            AddEventListener(touchListener, this);
        }

        internal bool TouchStartedOnIt(CCTouch touch)
        {
            return BoundingBoxTransformedToWorld.ContainsPoint(touch.StartLocation);
        }
        internal bool TouchIsOnIt(CCTouch touch)
        {
            return BoundingBoxTransformedToWorld.ContainsPoint(touch.Location);
        }
        internal bool TouchStartedOnItCircle(CCTouch touch)
        {
            return touch.StartLocation.IsNear(BoundingBoxTransformedToWorld.Center, ScaledContentSize.Width * 0.75f);
        }
        internal bool TouchIsOnItCircle(CCTouch touch)
        {
            return touch.Location.IsNear(BoundingBoxTransformedToWorld.Center, ScaledContentSize.Width * 0.75f);
        }
    }
}
