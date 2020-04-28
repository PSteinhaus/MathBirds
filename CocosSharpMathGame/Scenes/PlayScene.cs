using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
/*
namespace CocosSharpMathGame
{
    public class PlayScene : CCScene
    {
        internal PlayLayer PlayLayer { get; private set; } = new PlayLayer();
        internal GUILayer GUILayer { get; private set; } = new GUILayer();
        internal enum GameState
        {
            PLANNING, EXECUTING_ORDERS
        }
        internal GameState State { get; private set; } = GameState.PLANNING;
        private List<Aircraft> Aircrafts { get; set; } = new List<Aircraft>();
        private TestAircraft testAircraft;
        public PlayScene(CCGameView gameView) : base(gameView)
        {
            AddChild(PlayLayer);
            AddChild(GUILayer);
        }
        public PlayScene(CCScene scene) : base(scene)
        {
            AddChild(PlayLayer);
            AddChild(GUILayer);
        }

        protected override void AddedToScene()
        {
            base.AddedToScene();
            Schedule();
            var bounds = VisibleBoundsWorldspace;
            testAircraft = new TestAircraft();
            AddAircraft(testAircraft);
            testAircraft.MoveBy(bounds.Size.Width / 2, bounds.Size.Height / 4);
            testAircraft.RotateBy(-90f);
            testAircraft.PrepareForPlanningPhase();
            StartPlanningPhase();
        }

        internal void AddAircraft(Aircraft aircraft)
        {
            Aircrafts.Add(aircraft);
            PlayLayer.AddChild(aircraft);
        }
        internal void RemoveAircraft(Aircraft aircraft)
        {
            Aircrafts.Remove(aircraft);
            aircraft.PrepareForRemoval();
            PlayLayer.RemoveChild(aircraft);
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
    }
}
*/