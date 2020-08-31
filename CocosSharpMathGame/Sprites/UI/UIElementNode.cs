using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    abstract internal class UIElementNode : GameObjectNode
    {
        internal bool IsCircleButton { get; set; } = false;
        internal bool SwallowTouch { get; set; } = true;
        internal bool TouchMustEndOnIt { get; set; }
        //private  bool pressable = false;
        internal bool Pressable { get; set; } = true;// { return pressable; } set { pressable = value; Console.WriteLine("pressable: " + pressable); } }
        internal bool Pressed { get; set; } = false;
        internal float RadiusFactor { get; set; } = 0.5f;
        internal void MakeClickable(bool touchMustEndOnIt = true, bool IsCircleButton = false, bool swallowTouch = true, int priority = -25)
        {
            this.IsCircleButton = IsCircleButton;
            SwallowTouch = swallowTouch;
            TouchMustEndOnIt = touchMustEndOnIt;
            // add a touch listener
            var touchListener = new CCEventListenerTouchAllAtOnce();
            touchListener.OnTouchesBegan = OnTouchesBegan;
            touchListener.OnTouchesMoved = OnTouchesMoved;
            touchListener.OnTouchesEnded = OnTouchesEnded;
            touchListener.OnTouchesCancelled = OnTouchesEnded;
            if (priority == -25)
                AddEventListener(touchListener, this);
            else
                AddEventListener(touchListener, priority);
        }

        internal void OnTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (Pressable && MyVisible && TouchStartedOnIt(touches[0]))
            {
                if (SwallowTouch)
                {
                    //Console.WriteLine("SWALLOWED: "+touches[0].Location);
                    touchEvent.StopPropogation();
                }
                Pressed = true;
                OnTouchesBeganUI(touches, touchEvent);
            }
        }
        /// <summary>
        /// Override this to do work when clicked
        /// </summary>
        /// <param name="touches"></param>
        /// <param name="touchEvent"></param>
        private protected virtual void OnTouchesBeganUI(List<CCTouch> touches, CCEvent touchEvent)
        {
           
        }

        private protected void OnTouchesMoved(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (MyVisible && Pressed)
            {
                if (SwallowTouch) touchEvent.StopPropogation();
                OnTouchesMovedUI(touches, touchEvent);
            }
        }
        /// <summary>
        /// Override this to do work when pressed and the touch moves
        /// </summary>
        /// <param name="touches"></param>
        /// <param name="touchEvent"></param>
        private protected virtual void OnTouchesMovedUI(List<CCTouch> touches, CCEvent touchEvent)
        {

        }

        private protected void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (MyVisible && (TouchMustEndOnIt ? TouchIsOnIt(touches[0]) : true) && Pressed)
            {
                if (SwallowTouch) touchEvent.StopPropogation();
                OnTouchesEndedUI(touches, touchEvent);
            }
            Pressed = false;
        }
        /// <summary>
        /// Override this to do work when pressed and released
        /// </summary>
        /// <param name="touches"></param>
        /// <param name="touchEvent"></param>
        private protected virtual void OnTouchesEndedUI(List<CCTouch> touches, CCEvent touchEvent)
        {

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
