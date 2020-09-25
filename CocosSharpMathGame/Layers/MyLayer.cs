using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using Xamarin.Forms.Xaml;

namespace CocosSharpMathGame
{
    /// <summary>
    /// A general purpose layer class that adds some functionality to CCLayerColor
    /// </summary>
    public abstract class MyLayer : CCLayerColor
    {
        private protected Scroller Scroller { get; set; } = new Scroller();
        private protected CCEventListenerTouchAllAtOnce FirstTouchListener { get; set; }
        public MyLayer(CCColor4B color, bool countTouches=false) : base(color)
        {
            Schedule();
            Scroller.MoveFunction = (movePoint) => { CameraPosition -= movePoint; UpdateCamera(); };
            if (countTouches)
            {
                TouchCount = 0; // for CountTouches
                FirstTouchListener = new CCEventListenerTouchAllAtOnce();
                FirstTouchListener.OnTouchesBegan = OnTouchesBeganDoAccounting;
                FirstTouchListener.OnTouchesMoved = OnTouchesMovedDoAccounting;
                FirstTouchListener.OnTouchesEnded = OnTouchesEndedDoAccounting;
                FirstTouchListener.OnTouchesCancelled = OnTouchesEndedDoAccounting;
                AddEventListener(FirstTouchListener, int.MinValue); // intercept everything
            }
        }
        internal MyLayer TouchCountSource { get; set; } = null;
        public bool CountsTouches { get { return TouchCount != -25; } }
        private int touchCount = -25; // -25 means this layer does not count touches;
        public int TouchCount
        { 
            get
            {
                if (TouchCountSource != null)
                    return TouchCountSource.TouchCount;
                else
                    return touchCount;
            }
            private set
            {
                touchCount = value;
            }
        }
        private protected void OnTouchesBeganDoAccounting(List<CCTouch> touches, CCEvent touchEvent)
        {
            foreach (var touch in touches)
            {
                if (!ActiveTouches.ContainsKey(touch))
                {
                    Console.WriteLine("TOUCH ADDED, ID: " + touch.Id);
                    ActiveTouches.Add(touch, touch.Location);
                }
            }
            if (touches.Count > 0)
            {
                TouchCount += touches.Count;
                // intercept all additional touches (don't allow a second touch)
                if (TouchCount > 1)
                {
                    touchEvent.StopPropogation();
                }
            }
        }

        private protected void OnTouchesMovedDoAccounting(List<CCTouch> touches, CCEvent touchEvent)
        {
            foreach (var touch in touches)
            {
                if (ActiveTouches.ContainsKey(touch))
                {
                    ActiveTouches[touch] = touch.Location;
                    Console.WriteLine("touch "+touch.Id+" location updated: " + touch.Location);
                }
            }
        }

        private protected void OnTouchesEndedDoAccounting(List<CCTouch> touches, CCEvent touchEvent)
        {
            foreach (var touch in touches)
            {
                ActiveTouches.Remove(touch);
                Console.WriteLine("TOUCH REMOVED, ID: " + touch.Id);
                Console.WriteLine("ActiveTouches.Count: " + ActiveTouches.Count);
            }
            if (touches.Count > 0)
            {
                TouchCount -= touches.Count;
                if (TouchCount < 0) TouchCount = 0;
                touchEvent.IsStopped = false; // workaround for a bug that is created by the swallowing of a touchMoved-event in UIElement(Node)
                // intercept the event if there are touches remaining (i.e. only the last release will be the "real" release)
                if (TouchCount > 0)
                {
                    touchEvent.StopPropogation();
                }
                Console.WriteLine("TouchCount: " + TouchCount);
            }
        }

        internal abstract void Clear();

        /// <summary>
        /// Since layer (and scene) opacity is broken this implements a workaround for a creating a fading transition.
        /// </summary>
        /// <param name="guiLayer1"></param>
        /// <param name="guiLayer2"></param>
        public static void TransitionFadingFromTo(MyLayer guiLayer1, MyLayer guiLayer2, MyLayer layer1, MyLayer layer2, float duration)
        {
            var fadeNode = new CCDrawNode();
            var bigRect = new CCRect(-100000000, -100000000, 1000000000, 1000000000);
            fadeNode.BlendFunc = CCBlendFunc.NonPremultiplied;
            fadeNode.DrawRect(bigRect, CCColor4B.Transparent);
            guiLayer1.AddChild(fadeNode, int.MaxValue);
            void FadeIn(float prog, float du)
            {
                fadeNode.Clear(); fadeNode.DrawRect(bigRect, new CCColor4B(0f, 0f, 0f, prog));
            }
            void FadeOut(float prog, float du)
            {
                fadeNode.Clear(); fadeNode.DrawRect(bigRect, new CCColor4B(0f, 0f, 0f, 1 - prog));
            }
            fadeNode.AddAction(new CCSequence(new CCCallFiniteTimeFunc(duration / 2, FadeIn),
                                              new CCCallFunc(() =>
                                              {
#if ANDROID
                                                  var director = layer1.Director;
                                                    var scene = director.RunningScene;
                                                    var gv = scene.GameView;

                                                    fadeNode.RemoveFromParent();
                                                    // the following are attempts at fixing the memory leak (which all mostly failed)
                                                    // created by the fact that layers are not released for some reason
                                                    layer1.RemoveAllListeners();
                                                    guiLayer1.RemoveAllListeners();
                                                    layer1.RemoveEventListeners();
                                                    guiLayer1.RemoveEventListeners();
                                                    layer1.RemoveFromParent();
                                                    guiLayer1.RemoveFromParent();
                                                    layer1.Clear();
                                                    guiLayer1.Clear();
                                                    layer1.Cleanup();
                                                    guiLayer1.Cleanup();
                                                    layer1.Dispose();
                                                    guiLayer1.Dispose();
                                                    scene.RemoveAllListeners();
                                                    scene.RemoveAllChildren();
                                                    scene.StopAllActions();
                                                    scene.UnscheduleAll();
                                                    scene.RemoveFromParent();
                                                    scene.Cleanup();
                                                    scene.Dispose();

                                                    var scene2 = new CCScene(gv);
                                                    scene2.AddLayer(guiLayer2);
                                                    scene2.AddLayer(layer2, int.MinValue);
                                                    guiLayer2.AddChild(fadeNode, int.MaxValue);
                                                    director.ResetSceneStack();
                                                    director.ReplaceScene(scene2);
                                                    fadeNode.AddAction(new CCSequence(new CCCallFiniteTimeFunc(duration / 2, FadeOut),
                                                                        new CCRemoveSelf()));
#else
                                                  // this part is how it needs to be done in order to work on the old CocosSharp version
                                                  // where GameView.Get fails (i.e. the DX-version)
                                                  layer1.RemoveAllListeners();
                                                  guiLayer1.RemoveAllListeners();
                                                  var parent = layer1.Parent;
                                                  layer1.RemoveFromParent();
                                                  guiLayer1.RemoveFromParent();
                                                  parent.AddChild(guiLayer2);
                                                  parent.AddChild(layer2, int.MinValue);
                                                  fadeNode.RemoveFromParent();
                                                  guiLayer2.AddChild(fadeNode, int.MaxValue);
                                                  fadeNode.AddAction(new CCSequence(new CCCallFiniteTimeFunc(duration / 2, FadeOut),
                                                                      new CCRemoveSelf()));
#endif
                                              })));
        
                                              
        }

        private CCPoint ShakeAmount { get; set; }
        private protected CCPoint ScreenShakeVec { get; private set; } = CCPoint.Zero;
        internal CCRect CameraSpace { get; private protected set; } = new CCRect(float.NegativeInfinity, float.NegativeInfinity, float.PositiveInfinity, float.PositiveInfinity);
        private protected CCPoint cameraPosition = new CCPoint(0, 0);
        internal CCPoint CameraPosition
        {
            get
            {
                return cameraPosition;
            }
            set
            {
                // make sure the camera is still contained in the camera space after the change
                if (value.X < CameraSpace.MinX) value.X = CameraSpace.MinX;
                if (value.Y < CameraSpace.MinY) value.Y = CameraSpace.MinY;
                CCPoint upperRight = value + (CCPoint)CameraSize;
                if (upperRight.X > CameraSpace.MaxX) value.X = CameraSpace.MaxX - CameraSize.Width;
                if (upperRight.Y > CameraSpace.MaxY) value.Y = CameraSpace.MaxY - CameraSize.Height;
                cameraPosition = value;
            }
        }
        private CCSize cameraSize = new CCSize(Constants.COCOS_WORLD_WIDTH, Constants.COCOS_WORLD_HEIGHT);
        internal CCSize CameraSize
        {
            get
            {
                return cameraSize;
            }
            set
            {
                if (value.Width > MaxCameraWidth)
                    value = new CCSize(MaxCameraWidth, value.Height);
                if (value.Height > MaxCameraHeight)
                    value = new CCSize(value.Width, MaxCameraHeight);
                // make sure the camera is still contained in the camera space after the change
                CCPoint upperRight = CameraPosition + (CCPoint)value;
                if (upperRight.X > CameraSpace.MaxX)
                {
                    value.Width = CameraSpace.MaxX - CameraPosition.X;
                    value.Height = value.Width * cameraSize.Height / cameraSize.Width;
                }
                if (upperRight.Y > CameraSpace.MaxY)
                {
                    value.Height = CameraSpace.MaxY - CameraPosition.Y;
                    value.Width = value.Height * cameraSize.Width / cameraSize.Height;
                }
                cameraSize = value;
            }
        }
        private protected float MaxCameraWidth = Constants.COCOS_WORLD_WIDTH * 8;
        private protected float MaxCameraHeight = Constants.COCOS_WORLD_HEIGHT * 8;

        internal virtual void UpdateCamera()
        {
            Camera = new CCCamera(new CCRect(cameraPosition.X + ScreenShakeVec.X, cameraPosition.Y + ScreenShakeVec.Y, CameraSize.Width, CameraSize.Height));
            Camera.NearAndFarPerspectiveClipping = new CCNearAndFarClipping(1f, 1000000f);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            // shake the screen
            ShakeScreen(dt);
            // scroll on using inertia
            if (Scroller != null)
                Scroller.Update(dt);
        }

        internal void AddScreenShake(float shakeX, float shakeY)
        {
            ShakeAmount += new CCPoint(shakeX, shakeY);
        }
        private protected float timeSinceLastShake = 30f;
        private protected CCPoint currentShakePoint = CCPoint.Zero;
        private protected CCPoint nextShakePoint;
        private protected void ShakeScreen(float dt)
        {
            const float shakeDelay = 0.032625f;
            const float reductionFactor = 0.8f;
            const float reductionFactorCutoff = 80f;
            const float cutoffLength = 50f;
            timeSinceLastShake += dt;
            if (ShakeAmount != CCPoint.Zero)
            {
                // check if it's time for a new shake point
                if (timeSinceLastShake >= shakeDelay)
                {
                    var rng = new Random();
                    currentShakePoint = ScreenShakeVec;
                    int sign1 = rng.Next(0, 2) == 1 ? 1 : -1;
                    int sign2 = rng.Next(0, 2) == 1 ? 1 : -1;
                    nextShakePoint = new CCPoint(sign1 * (float)rng.NextDouble() * ShakeAmount.X, sign2 * (float)rng.NextDouble() * ShakeAmount.Y);
                    timeSinceLastShake = timeSinceLastShake % shakeDelay;
                }
                // calculate the current shake
                // the actual shake point is somewhere between the current and the next shake point
                ScreenShakeVec = currentShakePoint + (nextShakePoint - currentShakePoint) * timeSinceLastShake / shakeDelay;
                // reduce the shake
                float reduction;
                var length = ShakeAmount.Length;
                if (length < cutoffLength)
                    reduction = dt * reductionFactorCutoff;
                else
                    reduction = dt * length * reductionFactor;
                if (ShakeAmount.X > ShakeAmount.Y)
                    ShakeAmount -= new CCPoint(reduction, ShakeAmount.Y / ShakeAmount.X * reduction);
                else
                    ShakeAmount -= new CCPoint(ShakeAmount.X / ShakeAmount.Y * reduction, reduction);
                if (ShakeAmount.X < 0) ShakeAmount = new CCPoint(0, ShakeAmount.Y);
                if (ShakeAmount.Y < 0) ShakeAmount = new CCPoint(ShakeAmount.X, 0);
                UpdateCamera();
            }
            else if (ScreenShakeVec != CCPoint.Zero)
            {
                ScreenShakeVec = CCPoint.Zero;
                UpdateCamera();
            }
        }

        internal class SingleTouchEventArgs : EventArgs
        {
            internal SingleTouchEventArgs(CCTouch touch) : base()
            {
                Touch = touch;
            }
            internal CCTouch Touch { get; private set; }
        }
        internal event EventHandler<SingleTouchEventArgs> DoubleTapEvent;
        private DateTime TimeLastTap { get; set; } = DateTime.MinValue;
        internal float DoubleTapInterval { get; set; } = 0.25f;
        internal bool Pressed { get; set; } = false;
        private protected Dictionary<CCTouch, CCPoint> ActiveTouches { get; set; } = new Dictionary<CCTouch, CCPoint>();
        private protected void OnTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {
            Pressed = true;
            switch (touches.Count)
            {
                case 1:
                    {
                        var now = DateTime.Now;
                        if ((now - TimeLastTap).TotalSeconds < DoubleTapInterval)
                            DoubleTapEvent?.Invoke(this, new SingleTouchEventArgs(touches[0]));
                        // stop all scrolling
                        if (Scroller != null)
                            Scroller.OnTouchesBegan(touches, touchEvent);
                        TimeLastTap = now;
                    }
                    break;
                default:
                    break;
            }
        }

        private protected void OnTouchesMovedMoveAndZoom(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (!Pressed) return;
            Console.WriteLine("touches.Count: " + touches.Count);
            Console.WriteLine("TouchCount: " + TouchCount);
            switch (touches.Count)
            {
                case 1:
                    {
                        if (TouchCount == 2)
                            goto zoom;
                        // move the camera
                        if (Scroller != null)
                            Scroller.OnTouchesMoved(touches, touchEvent);
                    }
                    break;
                case 2:
                zoom:
                    {
                        // check for zoom
                        float zoomFactor = 1f;
                        CCTouch touch1 = touches[0];
                        CCTouch touch2 = null;
                        CCPoint touch1Loc = touch1.Location;
                        CCPoint touch2Loc = CCPoint.Zero;
                        if (touches.Count == 2)
                        { 
                            touch2 = touches[1];
                            touch2Loc = touch2.Location;
                            zoomFactor = MyTouchExtensions.GetZoom(touch1, touch2);
                        }
                        else
                        {
                            foreach (var touch in ActiveTouches.Keys)
                                if (touch != touch1)
                                    touch2Loc = ActiveTouches[touch];
                            zoomFactor = MyTouchExtensions.GetZoomOneTouchMoving(touch1, touch2Loc);
                        }
                        if (!float.IsNaN(zoomFactor))
                        {
                            var oldCameraSize = new CCSize(CameraSize.Width, CameraSize.Height);
                            CameraSize = new CCSize(oldCameraSize.Width * zoomFactor, oldCameraSize.Height * zoomFactor);
                            float dw = CameraSize.Width - oldCameraSize.Width;
                            float dh = CameraSize.Height - oldCameraSize.Height;
                            CCPoint touchCenter = new CCPoint((touch1Loc.X + touch2Loc.X) / 2, (touch1Loc.Y + touch2Loc.Y) / 2);
                            float relativeX = (touchCenter.X - CameraPosition.X) / oldCameraSize.Width;
                            float relativeY = (touchCenter.Y - CameraPosition.Y) / oldCameraSize.Height;
                            CameraPosition = new CCPoint(CameraPosition.X - dw * relativeX, CameraPosition.Y - dh * relativeY);
                            UpdateCamera();
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private protected void OnMouseScrollZoom(CCEventMouse mouseEvent)
        {
            // also enable zooming with mouse
            var oldCameraSize = new CCSize(CameraSize.Width, CameraSize.Height);
            var zoomFactor = mouseEvent.ScrollY > 0 ? mouseEvent.ScrollY / 100 : -1 / (mouseEvent.ScrollY / 100);
            CameraSize = new CCSize(oldCameraSize.Width * zoomFactor, oldCameraSize.Height * zoomFactor);
            float dw = CameraSize.Width - oldCameraSize.Width;
            float dh = CameraSize.Height - oldCameraSize.Height;
            CameraPosition = new CCPoint(CameraPosition.X - dw * 0.5f, CameraPosition.Y - dh * 0.5f);
            UpdateCamera();
        }

        private protected void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (!Pressed) return;
            // start inert scrolling
            if (Scroller != null)
                Scroller.OnTouchesEnded(touches, touchEvent);
            switch (touches.Count)
            {
                case 1:
                    {
                        
                    }
                    break;
                default:
                    break;
            }
            Pressed = false;
        }

        public override void RemoveChild(CCNode child, bool cleanup = true)
        {
            if (child is IGameObject g)
                g.PrepareForRemoval();
            base.RemoveChild(child, cleanup);
        }
    }
}
