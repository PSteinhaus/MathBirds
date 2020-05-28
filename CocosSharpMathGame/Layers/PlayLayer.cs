using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using CocosSharp;
using Microsoft.Xna.Framework;

namespace CocosSharpMathGame
{
    public class PlayLayer : CCLayerColor
    {
        internal CCNode BGNode { get; private protected set; }
        internal enum GameState
        {
            PLANNING, EXECUTING_ORDERS
        }
        internal GameState State { get; private set; } = GameState.PLANNING;
        public GUILayer GUILayer { get; set; }
        private MathSprite mathSprite1 = new MathSprite("(a+b)*((x))");
        private CCDrawNode drawNode = new CCDrawNode();
        internal List<Aircraft> Aircrafts { get; set; } = new List<Aircraft>();
        internal List<Projectile> Projectiles { get; } = new List<Projectile>();
        internal List<IDrawNodeUser> DrawNodeUsers { get; } = new List<IDrawNodeUser>();
        private TestAircraft testAircraft;
        private CCPoint cameraPosition = new CCPoint(0,0);
        private CCPoint currentCameraPosition = new CCPoint(0, 0);
        private CCPoint CameraPosition
        {
            get
            {
                return cameraPosition;
            }
            set
            {
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
                cameraSize = value;
                if (cameraSize.Width > MaxCameraWidth)
                    cameraSize = new CCSize(MaxCameraWidth, MaxCameraHeight);
            }
        }
        private CCPoint ShakeAmount { get; set; }
        private CCPoint ScreenShakeVec { get; set; }
        internal CCDrawNode HighDrawNode { get; set; }
        internal CCDrawNode LowDrawNode { get; set; }
        public PlayLayer() : base(CCColor4B.Black)
        {
            GUILayer = new GUILayer(this);
            BGNode = new CCNode();
            AddChild(BGNode);
            BGNode.VertexZ = Constants.VERTEX_Z_GROUND;
            BGNode.AddChild(drawNode);
            const float bgsize = 30000f;
            var bgColor = new CCColor4B(55, 55, 55);
            for (int i=-40; i<40; i++)
            {
                drawNode.DrawLine(new CCPoint(i * bgsize/40, -bgsize), new CCPoint(i * bgsize/40, bgsize), 20f, bgColor);
                drawNode.DrawLine(new CCPoint(-bgsize, i * bgsize/40), new CCPoint(bgsize, i * bgsize/40), 20f, bgColor);
            }
            BGNode.ZOrder = (int)Constants.VERTEX_Z_GROUND;
            BGNode.Rotation = 10f;

            HighDrawNode = new CCDrawNode();
            HighDrawNode.BlendFunc = CCBlendFunc.NonPremultiplied;
            AddChild(HighDrawNode, zOrder: 1000);
            LowDrawNode = new CCDrawNode();
            LowDrawNode.BlendFunc = CCBlendFunc.NonPremultiplied;
            AddChild(LowDrawNode, zOrder: -1000);
            // for now place some MathSprites as a test
            //AddChild(mathSprite1);
            //AddChild(mathSprite2);
            //AddChild(mathSprite3);

            // a DrawNode is always useful for debugging
            //AddChild(drawNode);
            //drawNode.ZOrder = 0;
            mathSprite1.ZOrder = 1;

            // add a touch listener
            var touchListener = new CCEventListenerTouchAllAtOnce();
            touchListener.OnTouchesBegan = OnTouchesBegan;
            touchListener.OnTouchesMoved = OnTouchesMoved;
            touchListener.OnTouchesEnded = OnTouchesEnded;
            AddEventListener(touchListener, this);

            // add a mouse listener
            var mouseListener = new CCEventListenerMouse();
            mouseListener.OnMouseScroll = OnMouseScroll;
            AddEventListener(mouseListener, this);
        }

        protected override void AddedToScene()
        {
            
            base.AddedToScene();    // MAGIC
            Schedule();
            var bounds = VisibleBoundsWorldspace;
            testAircraft = new TestAircraft();
            var playerTeam = new Team();
            testAircraft.Team = playerTeam;
            testAircraft.ControlledByPlayer = true;
            AddAircraft(testAircraft);
            testAircraft.MoveBy(bounds.Size.Width / 2, bounds.Size.Height / 4);
            testAircraft.RotateBy(-90f);
            
            // add two other planes from different teams
            var secondAircraft = new TestAircraft(withWeapon: true);
            var secondTeam = new Team();
            secondAircraft.Team = secondTeam;
            secondAircraft.ChangeColor(CCColor3B.Red);
            var ai1 = new StandardAI();
            secondAircraft.AI = ai1;
            AddAircraft(secondAircraft);
            secondAircraft.MoveBy(bounds.Size.Width / 5, bounds.Size.Height * 1.3f);
            secondAircraft.RotateBy(60f);

            /*
            var thirdAircraft = new TestAircraft();
            thirdAircraft.Team = secondTeam;
            thirdAircraft.ChangeColor(CCColor3B.Red);
            var ai2 = new StandardAI();
            thirdAircraft.AI = ai2;
            AddAircraft(thirdAircraft);
            thirdAircraft.MoveBy(bounds.Size.Width * 1.2f, bounds.Size.Height * 0.1f);
            thirdAircraft.RotateBy(-20f);
            */

            StartPlanningPhase();

            //ExecuteOrdersButton.Position = new CCPoint(bounds.MinX+ExecuteOrdersButton.ScaledContentSize.Width, bounds.MaxY- ExecuteOrdersButton.ScaledContentSize.Height);
            //AddChild(ExecuteOrdersButton);

            //Console.WriteLine("ZOrder Parent before: " + testAircraft.ZOrder);
            //Console.WriteLine("ZOrder before: " + testAircraft.wings.ZOrder);
            //testAircraft.ZOrder = 0;
            //testAircraft.wings.ZOrder = -1;
            //Console.WriteLine("ZOrder Parent now: " + testAircraft.ZOrder);
            //Console.WriteLine("ZOrder now: " + testAircraft.wings.ZOrder);
            drawNode.DrawRect(testAircraft.BoundingBoxTransformedToWorld, CCColor4B.Green);
            //drawNode.DrawSolidCircle( bounds.Center, 50, CCColor4B.Red);
            //drawNode.DrawSolidCircle(testAircraft.Position, 60, CCColor4B.Blue);
            //Console.WriteLine("Bounds: "+testAircraft.BoundingBoxTransformedToWorld);
            //Console.WriteLine("Aircraft Position: " + testAircraft.Position);

            UpdateCamera();
        }

        internal void UpdateCamera()
        {
            Camera = new CCCamera(new CCRect(cameraPosition.X+ScreenShakeVec.X, cameraPosition.Y+ScreenShakeVec.Y, CameraSize.Width, CameraSize.Height));
            Camera.NearAndFarPerspectiveClipping = new CCNearAndFarClipping(1f, 1000000f);
        }
        internal void AddAircraft(Aircraft aircraft)
        {
            Aircrafts.Add(aircraft);
            AddChild(aircraft);
        }
        internal void RemoveAircraft(Aircraft aircraft)
        {
            Aircrafts.Remove(aircraft);
            aircraft.PrepareForRemoval();
            RemoveChild(aircraft);
        }

        internal void AddProjectile(Projectile projectile)
        {
            Projectiles.Add(projectile);
            DrawNodeUsers.Add(projectile);
            AddChild(projectile);
        }
        internal void RemoveProjectile(Projectile projectile)
        {
            Projectiles.Remove(projectile);
            DrawNodeUsers.Remove(projectile);
            RemoveChild(projectile);
        }

        internal void AddScreenShake(float shakeX, float shakeY)
        {
            ShakeAmount += new CCPoint(shakeX, shakeY);
        }
        private float timeSinceLastShake = 30f;
        private CCPoint currentShakePoint = CCPoint.Zero;
        private CCPoint nextShakePoint;
        private void ShakeScreen(float dt)
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

        private float TimeLeftExecutingOrders { get; set; }

        internal void ExecuteOrders()
        {
            State = GameState.EXECUTING_ORDERS;
            TimeLeftExecutingOrders = Constants.TURN_DURATION;
        }
        internal void StartPlanningPhase()
        {
            State = GameState.PLANNING;
            // prepare the aircrafts
            foreach (var aircraft in Aircrafts)
                aircraft.PrepareForPlanningPhase();
            // make the ExecuteOrderButton visible again
            GUILayer.ExecuteOrdersButton.Visible = true;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            switch (State)
            {
                case GameState.PLANNING:
                    break;
                case GameState.EXECUTING_ORDERS:
                    {
                        TimeLeftExecutingOrders -= dt;
                        // DEBUG: Console.WriteLine("EXECUTING ORDERS; dt: " + dt);
                        // go through all aircrafts and let them execute their orders
                        List<Aircraft> aircraftToBeRemoved = new List<Aircraft>();
                        foreach (var aircraft in Aircrafts)
                        {
                            aircraft.ExecuteOrders(dt);
                            if (aircraft.ToBeRemoved)
                                aircraftToBeRemoved.Add(aircraft);
                        }
                        // remove aircrafts that have to be removed
                        foreach (var aircraft in aircraftToBeRemoved)
                            RemoveAircraft(aircraft);
                        // go through all projectiles and let them advance
                        // check whether a projectile needs to be removed
                        List<Projectile> projectilesToBeRemoved = new List<Projectile>();
                        foreach (var projectile in Projectiles)
                        {
                           projectile.Advance(dt);
                            if (projectile.CanBeRemoved()) projectilesToBeRemoved.Add(projectile);
                        }
                        foreach (var projectile in projectilesToBeRemoved)
                            RemoveProjectile(projectile);
                        UpdateDrawNodes();
                        if (TimeLeftExecutingOrders <= 0)
                            StartPlanningPhase();
                    }
                    break;
            }
            // shake the screen
            ShakeScreen(dt);
        }
        /// <summary>
        /// Draw everything that is supposed to be drawn by the DrawNode
        /// </summary>
        private void UpdateDrawNodes()
        {
            HighDrawNode.Clear();
            LowDrawNode.Clear();
            // draw the projectiles
            foreach (var drawNodeUser in DrawNodeUsers)
                drawNodeUser.UseDrawNodes(HighDrawNode, LowDrawNode);
            // draw everything directly related to the aircrafts
            foreach (var aircraft in Aircrafts)
                aircraft.UseDrawNodes(HighDrawNode, LowDrawNode);
        }

        void OnTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (touches.Count > 0)
            {
                //var touch = touches[0];
                //var startLoc = touch.StartLocation;
                //Console.WriteLine(startLoc);
                //if (testAircraft.BoundingBoxTransformedToWorld.ContainsPoint(startLoc))
                //    drawNode.Visible = false;
                //if (testAircraft.ManeuverPolygon.ContainsPoint(startLoc))
                //   testAircraft.IsManeuverPolygonDrawn = false;
            }
        }

        void OnTouchesMoved(List<CCTouch> touches, CCEvent touchEvent)
        {
            switch (touches.Count)
            {
                case 1:
                    {
                        var touch = touches[0];
                        var xDif = touch.Location.X - touch.PreviousLocation.X;
                        var yDif = touch.Location.Y - touch.PreviousLocation.Y;
                        CameraPosition = new CCPoint(CameraPosition.X - xDif, CameraPosition.Y - yDif);
                        UpdateCamera();
                        //var touch = touches[0];
                        //var startLoc = touch.StartLocation;
                        //Console.WriteLine(startLoc);
                        //if (testAircraft.BoundingBoxTransformedToWorld.ContainsPoint(startLoc))
                        //    drawNode.Visible = false;
                        //if (testAircraft.ManeuverPolygon.ContainsPoint(startLoc))
                        //   testAircraft.IsManeuverPolygonDrawn = false;
                    }
                    break;
                case 2:
                    {
                        // check for zoom
                        var touch1 = touches[0];
                        var touch2 = touches[1];
                        float zoomFactor = 1.5f * MyTouchExtensions.GetZoom(touch1, touch2);
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

        void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (touches.Count > 0)
            {
                //drawNode.Visible = true;
                //testAircraft.IsManeuverPolygonDrawn = true;
                //Console.WriteLine("Released: "+touches[0].Location.ToString());
            }
        }

        private float MaxCameraWidth = Constants.COCOS_WORLD_WIDTH * 8;
        private float MaxCameraHeight = Constants.COCOS_WORLD_HEIGHT * 8;

        private void OnMouseScroll(CCEventMouse mouseEvent)
        {
            // also enable zooming with mouse
            var oldCameraSize = new CCSize(CameraSize.Width, CameraSize.Height);
            var zoomFactor = mouseEvent.ScrollY > 0 ? mouseEvent.ScrollY / 100 : - 1/(mouseEvent.ScrollY / 100);
            CameraSize = new CCSize(oldCameraSize.Width * zoomFactor, oldCameraSize.Height * zoomFactor);
            float dw = CameraSize.Width - oldCameraSize.Width;
            float dh = CameraSize.Height - oldCameraSize.Height;
            //CCPoint scrollCenter = mouseEvent.Cursor;
            //float relativeX = (scrollCenter.X - CameraPosition.X) / oldCameraSize.Width;
            //float relativeY = (scrollCenter.Y - CameraPosition.Y) / oldCameraSize.Height;
            CameraPosition = new CCPoint(CameraPosition.X - dw * 0.5f, CameraPosition.Y - dh * 0.5f);
            UpdateCamera();
        }
    }
}

