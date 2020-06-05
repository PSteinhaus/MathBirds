using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using Xamarin.Forms.Xaml;

namespace CocosSharpMathGame
{
    /// <summary>
    /// A general purpose layer class that adds some functionality to CCLayerColor
    /// </summary>
    public class MyLayer : CCLayerColor
    {
        private protected Scroller Scroller { get; set; } = new Scroller();
        public MyLayer(CCColor4B color) : base(color)
        {
            Schedule();
            Scroller.MoveFunction = (movePoint) => { CameraPosition -= movePoint; UpdateCamera(); };
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

        internal void UpdateCamera()
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

        private protected void OnTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {
            switch (touches.Count)
            {
                case 1:
                    {
                        // stop all scrolling
                        if (Scroller != null)
                            Scroller.OnTouchesBegan(touches, touchEvent);
                    }
                    break;
                default:
                    break;
            }
        }

        private protected void OnTouchesMovedMoveAndZoom(List<CCTouch> touches, CCEvent touchEvent)
        {
            switch (touches.Count)
            {
                case 1:
                    {
                        // move the camera
                        if (Scroller != null)
                            Scroller.OnTouchesMoved(touches, touchEvent);
                    }
                    break;
                case 2:
                    {
                        // check for zoom
                        var touch1 = touches[0];
                        var touch2 = touches[1];
                        float zoomFactor = MyTouchExtensions.GetZoom(touch1, touch2);
                        if (!float.IsNaN(zoomFactor))
                        {
                            var oldCameraSize = new CCSize(CameraSize.Width, CameraSize.Height);
                            CameraSize = new CCSize(oldCameraSize.Width * zoomFactor, oldCameraSize.Height * zoomFactor);
                            float dw = CameraSize.Width - oldCameraSize.Width;
                            float dh = CameraSize.Height - oldCameraSize.Height;
                            CCPoint touchCenter = new CCPoint((touch1.Location.X + touch2.Location.X) / 2, (touch1.Location.Y + touch2.Location.Y) / 2);
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
            switch (touches.Count)
            {
                case 1:
                    {
                        // start inert scrolling
                        if (Scroller != null)
                            Scroller.OnTouchesEnded(touches, touchEvent);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
