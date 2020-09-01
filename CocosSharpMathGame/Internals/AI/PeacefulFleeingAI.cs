using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// The usual AI for planes.
    /// It simply follows and attacks the next enemy for now.
    /// </summary>
    class PeacefulFleeingAI : AI
    {
        internal const float SCARE_DISTANCE = 800f;
        internal bool Scared { get; set; } = false;
        internal PeacefulFleeingAI() : base()
        {

        }
        internal override void ActInPlanningPhase()
        {
            // first check whether you're hurt and go scared if you are
            if (!Scared && Aircraft.Health < Aircraft.MaxHealth)
                Scared = true;
            // go through all aircrafts and find the closest enemy
            // (for now an enemy is everyone who isn't on your team)
            var myTeam = Aircraft.Team;
            IEnumerable<Aircraft> aircrafts;
            if (myTeam == Team.PlayerTeam)
                aircrafts = Aircraft.ActiveAircraftsInLevel();
            else
                aircrafts = Aircraft.PlayerAircraftsInLevel();
            Aircraft closestAircraft = null;
            foreach (var aircraft in aircrafts)
            {
                if (aircraft.MyState != Aircraft.State.SHOT_DOWN && aircraft != Aircraft && aircraft.Team.IsEnemy(myTeam) && aircraft.IsActive())
                {
                    // check the distance (closer is better)
                    if (closestAircraft == null ||
                        CCPoint.Distance(Aircraft.Position, aircraft.Position) < CCPoint.Distance(Aircraft.Position, closestAircraft.Position))
                        closestAircraft = aircraft;
                }
            }
            // if it is too close run away
            if (closestAircraft != null && (Scared || CCPoint.Distance(Aircraft.Position, closestAircraft.Position) < SCARE_DISTANCE))
            {
                //Console.WriteLine("FLEEING");
                Aircraft.TryToSetFlightPathHeadTo(Aircraft.Position + (Aircraft.Position - closestAircraft.Position)*16);
            }
            // else move randomly
            else
            {
                //Console.WriteLine("Moving randomly");
                Aircraft.TryToSetFlightPathHeadTo(Constants.RandomPointBoxnear(Aircraft.Position, 1000f));
            }
        }
    }
}
