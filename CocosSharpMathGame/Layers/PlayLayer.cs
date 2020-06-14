using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using CocosSharp;
using Microsoft.Xna.Framework;

namespace CocosSharpMathGame
{
    public class PlayLayer : MyLayer
    {
        internal CCNode BGNode { get; private protected set; }
        internal enum GameState
        {
            PLANNING, EXECUTING_ORDERS
        }
        internal GameState State { get; private set; } = GameState.PLANNING;
        public GUILayer GUILayer { get; set; }
        private CCDrawNode drawNode = new CCDrawNode();
        internal List<Aircraft> Aircrafts { get; set; } = new List<Aircraft>();
        internal List<Projectile> Projectiles { get; } = new List<Projectile>();
        internal List<IDrawNodeUser> DrawNodeUsers { get; } = new List<IDrawNodeUser>();
        private TestAircraft testAircraft;
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
            var bgColor = new CCColor4B(28, 28, 28);
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
            //mathSprite1.ZOrder = 1;

            // add a touch listener
            var touchListener = new CCEventListenerTouchAllAtOnce();
            touchListener.OnTouchesBegan = OnTouchesBegan;
            touchListener.OnTouchesMoved = OnTouchesMovedMoveAndZoom;
            touchListener.OnTouchesEnded = OnTouchesEnded;
            AddEventListener(touchListener, this);

            // add a mouse listener
            var mouseListener = new CCEventListenerMouse();
            mouseListener.OnMouseScroll = OnMouseScrollZoom;
            AddEventListener(mouseListener, this);
        }

        protected override void AddedToScene()
        {
            
            base.AddedToScene();
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
            var color = new CCColor3B(160, 160, 160);
            secondAircraft.ChangeColor(color);
            var ai1 = new StandardAI();
            secondAircraft.AI = ai1;
            AddAircraft(secondAircraft);
            secondAircraft.MoveBy(bounds.Size.Width / 5, bounds.Size.Height * 1.3f);
            secondAircraft.RotateBy(-108f);

            // add two other planes from different teams
            var thirdAircraft = new TestAircraft(withWeapon: true);
            thirdAircraft.Team = secondTeam;
            thirdAircraft.ChangeColor(color);
            thirdAircraft.AI = new StandardAI();
            AddAircraft(thirdAircraft);
            thirdAircraft.MoveBy(bounds.Size.Width / 3, bounds.Size.Height * 1.4f);
            thirdAircraft.RotateBy(-100f);

            var fourthAircraft = new TestAircraft();
            var thirdTeam = new Team();
            fourthAircraft.Team = thirdTeam;
            var colorTeam3 = new CCColor3B(200, 200, 200);
            fourthAircraft.ChangeColor(colorTeam3);
            fourthAircraft.AI = new StandardAI();
            AddAircraft(fourthAircraft);
            fourthAircraft.MoveBy(bounds.Size.Width * 1.2f, bounds.Size.Height * 0.1f);
            fourthAircraft.RotateBy(-10f);

            var fifthAircraft = new TestAircraft();
            fifthAircraft.Team = thirdTeam;
            fifthAircraft.ChangeColor(colorTeam3);
            fifthAircraft.AI = new StandardAI();
            AddAircraft(fifthAircraft);
            fifthAircraft.MoveBy(bounds.Size.Width * 1.3f, bounds.Size.Height * 0.2f);
            fifthAircraft.RotateBy(-20f);

            StartPlanningPhase();

            //ExecuteOrdersButton.Position = new CCPoint(bounds.MinX+ExecuteOrdersButton.ScaledContentSize.Width, bounds.MaxY- ExecuteOrdersButton.ScaledContentSize.Height);
            //AddChild(ExecuteOrdersButton);

            //Console.WriteLine("ZOrder Parent before: " + testAircraft.ZOrder);
            //Console.WriteLine("ZOrder before: " + testAircraft.wings.ZOrder);
            //testAircraft.ZOrder = 0;
            //testAircraft.wings.ZOrder = -1;
            //Console.WriteLine("ZOrder Parent now: " + testAircraft.ZOrder);
            //Console.WriteLine("ZOrder now: " + testAircraft.wings.ZOrder);
            //drawNode.DrawRect(testAircraft.BoundingBoxTransformedToWorld, CCColor4B.Green);
            //drawNode.DrawSolidCircle( bounds.Center, 50, CCColor4B.Red);
            //drawNode.DrawSolidCircle(testAircraft.Position, 60, CCColor4B.Blue);
            //Console.WriteLine("Bounds: "+testAircraft.BoundingBoxTransformedToWorld);
            //Console.WriteLine("Aircraft Position: " + testAircraft.Position);

            UpdateCamera();
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
            // shake the screen (now managed by MyLayer)
            //ShakeScreen(dt);
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

        new void OnTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {
            base.OnTouchesBegan(touches, touchEvent);
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

        new private protected void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            base.OnTouchesEnded(touches, touchEvent);
            if (touches.Count > 0)
            {
                //drawNode.Visible = true;
                //testAircraft.IsManeuverPolygonDrawn = true;
                //Console.WriteLine("Released: "+touches[0].Location.ToString());
            }
        }
    }
}

