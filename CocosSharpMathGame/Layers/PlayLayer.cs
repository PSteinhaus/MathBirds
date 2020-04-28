using System;
using System.Collections.Generic;
using CocosSharp;
using Microsoft.Xna.Framework;

namespace CocosSharpMathGame
{
    public class PlayLayer : CCLayerColor
    {
        internal enum GameState
        {
            PLANNING, EXECUTING_ORDERS
        }
        internal GameState State { get; private set; } = GameState.PLANNING;
        private ExecuteOrdersButton ExecuteOrdersButton = new ExecuteOrdersButton();
        private MathSprite mathSprite1 = new MathSprite("(a+b)*((x))");
        private CCDrawNode drawNode = new CCDrawNode();
        private List<Aircraft> Aircrafts { get; set; } = new List<Aircraft>();
        private TestAircraft testAircraft;
        public PlayLayer() : base(CCColor4B.Black)
        {
            // for now place some MathSprites as a test
            //AddChild(mathSprite1);
            //AddChild(mathSprite2);
            //AddChild(mathSprite3);
            
            // a DrawNode is always useful for debugging
            AddChild(drawNode);
            drawNode.ZOrder = 0;
            mathSprite1.ZOrder = 1;

            // add a touch listener
            var touchListener = new CCEventListenerTouchAllAtOnce();
            touchListener.OnTouchesBegan = OnTouchesBegan;
            touchListener.OnTouchesEnded = OnTouchesEnded;
            AddEventListener(touchListener, this);
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

        internal void ExecuteOrders()
        {
            State = GameState.EXECUTING_ORDERS;
        }
        internal void StartPlanningPhase()
        {
            State = GameState.PLANNING;
            // prepare the aircrafts
            foreach (var aircraft in Aircrafts)
                aircraft.PrepareForPlanningPhase();
            // make the ExecuteOrderButton visible again
            ExecuteOrdersButton.Visible = true;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            switch(State)
            {
                case GameState.PLANNING:
                    break;
                case GameState.EXECUTING_ORDERS:
                    {
                        Console.WriteLine("EXECUTING ORDERS; dt: " + dt);
                        // go through all aircrafts and let them execute their orders
                        bool executionFinished = true;  // check if they are done
                        foreach (var aircraft in Aircrafts)
                        {
                            bool finished = aircraft.ExecuteOrders(dt);
                            if (!finished) executionFinished = false;
                        }
                        // if all aircrafts have finished executing their orders now start the planning phase
                        if (executionFinished)
                            StartPlanningPhase();
                    }
                    break;
            }
        }

        protected override void AddedToScene()
        {
            Schedule();
            base.AddedToScene();    // MAGIC
            var bounds = VisibleBoundsWorldspace;

            ExecuteOrdersButton.Position = new CCPoint(bounds.MinX+ExecuteOrdersButton.ScaledContentSize.Width, bounds.MaxY- ExecuteOrdersButton.ScaledContentSize.Height);
            AddChild(ExecuteOrdersButton);

            testAircraft = new TestAircraft();
            AddAircraft(testAircraft);
            testAircraft.MoveBy(bounds.Size.Width/2, bounds.Size.Height / 4);
            testAircraft.RotateBy(-90f);
            testAircraft.PrepareForPlanningPhase();
            //Console.WriteLine("ZOrder Parent before: " + testAircraft.ZOrder);
            //Console.WriteLine("ZOrder before: " + testAircraft.wings.ZOrder);
            //testAircraft.ZOrder = 0;
            //testAircraft.wings.ZOrder = -1;
            //Console.WriteLine("ZOrder Parent now: " + testAircraft.ZOrder);
            //Console.WriteLine("ZOrder now: " + testAircraft.wings.ZOrder);
            drawNode.DrawRect(testAircraft.BoundingBoxTransformedToWorld, CCColor4B.Green);
            drawNode.DrawSolidCircle( bounds.Center, 50, CCColor4B.Red);
            drawNode.DrawSolidCircle(testAircraft.Position, 60, CCColor4B.Blue);
            Console.WriteLine("Bounds: "+testAircraft.BoundingBoxTransformedToWorld);
            Console.WriteLine("Aircraft Position: " + testAircraft.Position);

            //var maneuverDrawNode = testAircraft.ManeuverPolygon.CreateDrawNode();
            //AddChild(maneuverDrawNode);

            // CREATE AND DRAW A POLYGON (AS A TEST)
            var polygonPoints = new CCPoint[]
            { new CCPoint(200, 40), new CCPoint(130, 180), new CCPoint(160, 200), new CCPoint(200, 210), new CCPoint(240, 200), new CCPoint(270, 180) };
            var polygon = new Polygon(polygonPoints);
            var polyDrawNode = polygon.CreateDrawNode(CCColor4B.Transparent, 2f, CCColor4B.White);
            AddChild(polyDrawNode);
            // NOW A MORE POINTY ONE
            polygonPoints = new CCPoint[]
            { new CCPoint(200, 40), new CCPoint(100, 180), new CCPoint(200, 300), new CCPoint(300, 180) };
            polygon = new Polygon(polygonPoints);
            polygon.MoveBy(300, 0);
            polyDrawNode = polygon.CreateDrawNode(CCColor4B.Transparent, 2f, CCColor4B.White);
            AddChild(polyDrawNode);
            // AND NOW ONE WITH SPLINES
            int[] splineControl = new int[] { -1, -1, 20, 0 };
            var splinePolygon = new PolygonWithSplines(polygonPoints, splineControl);
            splinePolygon.MoveBy(300, 0);
            polyDrawNode = splinePolygon.CreateDrawNode(CCColor4B.Transparent, 2f, CCColor4B.White);
            AddChild(polyDrawNode);
            splinePolygon.Transform(0, 400, 200f);
            polyDrawNode = splinePolygon.CreateDrawNode(CCColor4B.Transparent, 2f, CCColor4B.White);
            AddChild(polyDrawNode);

            //var headSprite = new FlightPathHead();
            //headSprite.Position = bounds.Center;
            //headSprite.PositionY += 200;
            //AddChild(headSprite);

            var center = bounds.Center;
            var point1 = new CCPoint(center.X, center.Y + bounds.Size.Height / 3);
            //var point2 = center;
            //var point3 = new CCPoint(center.X, center.Y - bounds.Size.Height / 3);

            //mathSprite1.Position = point1;
            //mathSprite2.Position = point2;
            //mathSprite3.Position = point3;

            //float desiredWidth = 800;
            //mathSprite1.FitToWidth(desiredWidth);
            //mathSprite2.FitToWidth(desiredWidth);
            //mathSprite3.FitToWidth(desiredWidth);

            // create a DrawNode to check the boundaries
            //drawNode.DrawRect(mathSprite1.BoundingBoxTransformedToParent, CCColor4B.AliceBlue);
            //drawNode.DrawRect(mathSprite2.BoundingBoxTransformedToParent, CCColor4B.Green);
            //drawNode.DrawRect(mathSprite3.BoundingBoxTransformedToParent, CCColor4B.Red);

            //drawNode.DrawSolidCircle(mathSprite1.Position, mathSprite1.ContentSize.Width / 2, CCColor4B.Gray);
            //drawNode.DrawSolidCircle(mathSprite2.Position, mathSprite2.ContentSize.Width / 2, CCColor4B.LightGray);
            //drawNode.DrawSolidCircle(mathSprite3.Position, mathSprite3.ContentSize.Width / 2, CCColor4B.Black);
            StartPlanningPhase();
            
        }

        void OnTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (touches.Count > 0)
            {
                var touch = touches[0];
                var startLoc = touch.StartLocation;
                Console.WriteLine(startLoc);
                if (testAircraft.BoundingBoxTransformedToWorld.ContainsPoint(startLoc))
                    drawNode.Visible = false;
                if (testAircraft.ManeuverPolygon.ContainsPoint(startLoc))
                    testAircraft.IsManeuverPolygonDrawn = false;
            }
        }

        void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (touches.Count > 0)
            {
                drawNode.Visible = true;
                testAircraft.IsManeuverPolygonDrawn = true;
                Console.WriteLine("Released: "+touches[0].Location.ToString());
            }
        }
    }
}

