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
    class StandardAI : AI
    {
        internal StandardAI() : base()
        {

        }
        internal override void ActInPlanningPhase()
        {
            // first go through all aircrafts and find the closest enemy
            // (for now an enemy is everyone who isn't on your team)
            var myTeam = Aircraft.Team;
            IEnumerable<Aircraft> aircrafts;
            if (myTeam == Team.PlayerTeam)
                aircrafts = Aircraft.AircraftsInLevel();
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
            // set path towards where he might be at the end of the next turn
            if (closestAircraft != null)
            {
                Console.WriteLine("Attacking!");
                Aircraft.TryToSetFlightPathHeadTo(closestAircraft.Position + (closestAircraft.VelocityVector * Constants.TURN_DURATION));
            }
        }
    }
}
