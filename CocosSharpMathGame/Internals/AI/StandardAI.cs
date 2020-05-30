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
        internal override void ActInPlanningPhase(IEnumerable<Aircraft> aircrafts)
        {
            // first go through all aircrafts and find the closest enemy
            // (for now an enemy is everyone who isn't on your team
            var myTeam = Aircraft.Team;
            Aircraft closestAircraft = null;
            foreach (var aircraft in aircrafts)
            {
                if (aircraft != Aircraft && aircraft.Team != myTeam && aircraft.MyState != Aircraft.State.SHOT_DOWN)
                {
                    // check the distance (closer is better)
                    if (closestAircraft == null ||
                        CCPoint.Distance(Aircraft.Position, aircraft.Position) < CCPoint.Distance(Aircraft.Position, closestAircraft.Position))
                        closestAircraft = aircraft;
                }
            }
            // set path towards where he might be at the end of the next turn
            if (closestAircraft != null)
                Aircraft.TryToSetFlightPathHeadTo(closestAircraft.Position + (closestAircraft.VelocityVector * Constants.TURN_DURATION));
        }
    }
}
