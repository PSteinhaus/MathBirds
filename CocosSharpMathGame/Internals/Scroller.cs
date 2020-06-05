using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// a module that can be used to give scrolling behavior to a CCNode
    /// </summary>
    internal class Scroller
    {
        private CCPoint ScrollVelocity { get; set; }
        private float ScrollTime { get; set; }
        private float TotalScrollTime { get; set; }
        internal Action<CCPoint> MoveFunction { get; set; }
        internal Func<CCTouch, CCPoint> VelocityVectorFunction { get; set; }
        internal Func<CCTouch, float> TotalScrollTimeFunction { get; set; }
        internal Func<float, CCPoint, float, float, CCPoint> TimeToScrollMoveFunction { get; set; }
        internal Scroller()
        {
            VelocityVectorFunction = DefaultVelocityVectorFunction;
            TotalScrollTimeFunction = DefaultTotalScrollTimeFunction;
            TimeToScrollMoveFunction = DefaultTimeToScrollMoveFunction;
        }

        public void Update(float dt)
        {
            // scroll on using inertia
            ScrollUsingInertia(dt);
        }

        internal void ScrollUsingInertia(float dt)
        {
            if (ScrollVelocity != CCPoint.Zero)
            {
                //Console.WriteLine("REALLY SCROLLING: " + ScrollVelocity);
                MoveFunction(TimeToScrollMoveFunction(dt, ScrollVelocity, ScrollTime, TotalScrollTime));
                ScrollTime += dt;
                if (ScrollTime > TotalScrollTime)
                    ScrollVelocity = CCPoint.Zero;
            }
        }

        internal void OnTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {
            switch (touches.Count)
            {
                case 1:
                    {
                        // stop all scrolling
                        ScrollVelocity = CCPoint.Zero;
                    }
                    break;
                default:
                    break;
            }
        }
        internal void OnTouchesMoved(List<CCTouch> touches, CCEvent touchEvent)
        {
            switch (touches.Count)
            {
                case 1:
                    {
                        var touch = touches[0];
                        MoveFunction(touch.Delta);
                    }
                    break;
                default:
                    break;
            }
        }

        internal void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            switch (touches.Count)
            {
                case 1:
                    {
                        // start inert scrolling
                        var touch = touches[0];
                        CCPoint DiffOnScreen = touch.LocationOnScreen - touch.PreviousLocationOnScreen;
                        ScrollVelocity = VelocityVectorFunction(touch);
                        ScrollTime = 0;
                        TotalScrollTime = TotalScrollTimeFunction(touch);
                    }
                    break;
                default:
                    break;
            }
        }

        internal CCPoint DefaultVelocityVectorFunction(CCTouch touch)
        {
            CCPoint scrollVelocity = new CCPoint(touch.Delta.X, touch.Delta.Y);
            CCPoint DiffOnScreen = touch.LocationOnScreen - touch.PreviousLocationOnScreen;
            scrollVelocity *= (float)Math.Pow(DiffOnScreen.Length, 0.2);
            scrollVelocity *= 16;
            return scrollVelocity;
        }

        internal float DefaultTotalScrollTimeFunction(CCTouch touch)
        {
            CCPoint DiffOnScreen = touch.LocationOnScreen - touch.PreviousLocationOnScreen;
            return (float)Math.Pow(DiffOnScreen.Length, 0.4) / 8;
        }
        internal CCPoint DefaultTimeToScrollMoveFunction(float dt, CCPoint scrollVelocity, float scrollTime, float totalScrollTime)
        {
            return scrollVelocity * (1 - (float)Math.Pow(scrollTime / totalScrollTime, 1)) * dt;
        }
    }
}
