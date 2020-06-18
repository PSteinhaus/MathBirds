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
        internal bool IsCircleButton { get; private set; }
        protected bool Pressed { get; set; } = false;
        internal bool Pressable { get; set; } = true;
        internal float RadiusFactor { get; set; } = 0.5f;
        internal UIElement(string textureName) : base(spriteSheet.Frames.Find(_ => _.TextureFilename.Equals(textureName)))
        {

        }

        internal void MakeClickable(Action<List<CCTouch>,CCEvent> onTouchesBegan, Action<List<CCTouch>, CCEvent> onTouchesMoved = null, Action<List<CCTouch>, CCEvent> onTouchesEnded=null, Action<List<CCTouch>, CCEvent> onTouchesCancelled = null, bool touchMustEndOnIt=true, bool IsCircleButton=false, bool swallowTouch=true)
        {
            this.IsCircleButton = IsCircleButton;
            Func<CCTouch, bool> touchStartedOnIt = null;
            Func<CCTouch, bool> touchIsOnIt = null;
            if (IsCircleButton)
            {
                touchStartedOnIt = TouchStartedOnItCircle;
                touchIsOnIt = TouchIsOnItCircle;
            }
            else
            {
                touchStartedOnIt = TouchStartedOnItBox;
                touchIsOnIt = TouchIsOnItBox;
            }
            // add a touch listener
            var touchListener = new CCEventListenerTouchAllAtOnce();
            touchListener.OnTouchesBegan = (arg1, arg2) =>                                      { if (Pressable && MyVisible && touchStartedOnIt(arg1[0]))                           { if (swallowTouch) arg2.StopPropogation(); Pressed = true;  onTouchesBegan(arg1, arg2); } };
            if (onTouchesMoved!=null) touchListener.OnTouchesMoved = (arg1, arg2) =>            { if (MyVisible && Pressed)                                                          { if (false)/*DEBUG: if a touch-moved-event is also a touch-ended-event then the event would not reach onTouchesEnded*/ arg2.StopPropogation(); onTouchesMoved(arg1, arg2); } };
            else touchListener.OnTouchesMoved = (arg1, arg2) =>                                 { if (MyVisible && Pressed)                                                          { if (false)/**/    arg2.StopPropogation(); } };
            if (onTouchesEnded != null) touchListener.OnTouchesEnded = (arg1, arg2) =>          { if (MyVisible && (touchMustEndOnIt ? touchIsOnIt(arg1[0]) : true) && Pressed)      { if (swallowTouch) arg2.StopPropogation(); Pressed = false; onTouchesEnded(arg1, arg2); } };
            else touchListener.OnTouchesEnded = (arg1, arg2) =>                                 { if (MyVisible && (touchMustEndOnIt ? touchIsOnIt(arg1[0]) : true) && Pressed)      { if (swallowTouch) arg2.StopPropogation(); Pressed = false; } };
            if (onTouchesCancelled != null) touchListener.OnTouchesCancelled = (arg1, arg2) =>  { if (MyVisible && (touchMustEndOnIt ? touchIsOnIt(arg1[0]) : true) && Pressed)      { if (swallowTouch) arg2.StopPropogation(); Pressed = false; onTouchesCancelled(arg1, arg2); } };
            else touchListener.OnTouchesCancelled = (arg1, arg2) =>                             { if (MyVisible && (touchMustEndOnIt ? touchIsOnIt(arg1[0]) : true) && Pressed)      { if (swallowTouch) arg2.StopPropogation(); Pressed = false; } };
            AddEventListener(touchListener, this);
        }

        internal bool TouchStartedOnIt(CCTouch touch)
        {
            return IsCircleButton ? TouchStartedOnItCircle(touch) : TouchStartedOnItBox(touch);
        }
        internal bool TouchIsOnIt(CCTouch touch)
        {
            return IsCircleButton ? TouchIsOnItCircle(touch) : TouchIsOnItBox(touch);
        }
        internal bool TouchStartedOnItCircle(CCTouch touch)
        {
            return touch.StartLocation.IsNear(BoundingBoxTransformedToWorld.Center, ScaledContentSize.Width * RadiusFactor);
        }
        internal bool TouchIsOnItCircle(CCTouch touch)
        {
            return touch.Location.IsNear(BoundingBoxTransformedToWorld.Center, ScaledContentSize.Width * RadiusFactor);
        }
        internal bool TouchStartedOnItBox(CCTouch touch)
        {
            return BoundingBoxTransformedToWorld.ContainsPoint(touch.StartLocation);
        }
        internal bool TouchIsOnItBox(CCTouch touch)
        {
            return BoundingBoxTransformedToWorld.ContainsPoint(touch.Location);
        }
    }
}
