using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    /// <summary>
    /// A squadron coordinates a group of aircrafts to fly in formation on a common path.
    /// </summary>
    internal class Squadron
    {
        /// <summary>
        /// whether the squadron currently flies in formation
        /// </summary>
        internal bool InFormation { get; private protected set; } = true;
        /// <summary>
        /// a dictionary holding all aircrafts of this squadron and their formation positions relative to the leader (for a squadron aiming at the angle of 0°)
        /// </summary>
        internal Dictionary<Aircraft, CCPoint> AircraftsWithRelPositions { get; private protected set; } = new Dictionary<Aircraft, CCPoint>();
        private Aircraft leaderField;
        internal Aircraft Leader { get { return leaderField; } set { leaderField = value; if (leaderField != null) WayPoint = leaderField.Position; } }
        /// <summary>
        /// Returns the leaders position as an approximation of the squadrons position
        /// </summary>
        internal CCPoint Position { get { return Leader != null ? Leader.Position : CCPoint.Zero; } }
        private CCPoint wayPoint;
        internal CCPoint WayPoint { get { return wayPoint; } private protected set { wayPoint = value; Console.WriteLine("WayPoint: " + wayPoint); } }
        /// <summary>
        /// where the leader is supposed to go this turn
        /// </summary>
        internal CCPoint LeaderWayPoint { get; private protected set; }
        /// <summary>
        /// how far the leader moves per second
        /// </summary>
        internal float Velocity { get; set; } = float.PositiveInfinity;
        /// <summary>
        /// minimum distance that the squadron has to keep to the world center
        /// </summary>
        internal float MinR { get; set; }
        /// <summary>
        /// maximum distance that the squadron can keep to the world center
        /// </summary>
        internal float MaxR { get; set; }
        /// <summary>
        /// the distance between two (randomly generated) waypoints
        /// </summary>
        internal float WayPointDistance { get; set; } = 3000f;
        /// <summary>
        /// if any player aircraft moves in closer than this it will pull the aggro (i.e. cause the squadron to break formation and attack)
        /// </summary>
        internal float AggroRange { get; set; } = 800f;

        internal void RemoveAircraft(Aircraft aircraft)
        {
            AircraftsWithRelPositions.Remove(aircraft);
            aircraft.Squadron = null;
            if (aircraft == Leader) // the old leader is gone, select a new one (only pro forma, since the formation should be broken by this point)
                Leader = AircraftsWithRelPositions.Any() ? AircraftsWithRelPositions.Keys.First() : null;
        }

        internal void AddAircraft(Aircraft aircraft, CCPoint formationPos)
        {
            aircraft.Team = Team.EnemyTeam; // since squadrons are only for enemies
            aircraft.Squadron = this;
            AircraftsWithRelPositions.Add(aircraft, formationPos);
            if (Leader == null)
                Leader = aircraft;
            // check if you have to adapt the velocity
            float v = aircraft.MaxVelocity * 0.65f;
            if (v < Velocity)
                Velocity = v;
        }

        internal void GenerateWayPoint()
        {
            var rng = new Random();
            // roll a position somewhere in your proximity, and possibly reroll until you found one that is in your allowed band (between MinR and MaxR)
            do
            {
                var vector = new CCPoint(WayPointDistance, 0);
                WayPoint = Leader.Position + CCPoint.RotateByAngle(vector, CCPoint.Zero, (float)rng.NextDouble() * (float)Math.PI * 2);
            } while (WayPoint.Length < MinR || WayPoint.Length > MaxR);
        }

        internal void PrepareForPlanningPhase(PlayLayer pl)
        {
            // first check the aggro
            // i.e. if an enemy (i.e. a player aircraft) is close enough break formation
            // also if one of the aircrafts is damaged break formation too
            if (InFormation)
            {
                foreach (var aircraft in pl.PlayerAircrafts)
                    if (CCPoint.Distance(aircraft.Position, Position) < AggroRange)
                    {
                        Console.WriteLine("TRIGGERED - DISTANCE");
                        InFormation = false;
                        break;
                    }
                foreach (var aircraft in AircraftsWithRelPositions.Keys)
                    if (aircraft.Health < aircraft.MaxHealth)
                    {
                        Console.WriteLine("TRIGGERED - HP");
                        InFormation = false;
                        break;
                    }
            }

            foreach (var aircraft in AircraftsWithRelPositions.Keys)
                aircraft.PrepareForPlanningPhase();

            if (!InFormation) return;   // squadrons only control their units while in formation

            Console.WriteLine("FLYING IN FORMATION");

            const float dt = Constants.TURN_DURATION;
            // if you're too close to the current Waypoint roll a new one
            var diff = WayPoint - Leader.Position;
            float advanceDistance = Velocity * dt * 4;
            if (diff.Length < advanceDistance)
            {
                GenerateWayPoint();
                diff = WayPoint - Leader.Position;
            }
            LeaderWayPoint = Leader.Position + CCPoint.Normalize(diff) * advanceDistance;
            // rotate all formation positions correctly around the leader and tell everyone to go to their positions
            float angle = Constants.DxDyToRadians(diff.X, diff.Y);
            foreach (var entry in AircraftsWithRelPositions)
            {
                var formationPoint = CCPoint.RotateByAngle(entry.Value, CCPoint.Zero, angle);
                var yourWaypoint = LeaderWayPoint + formationPoint;
                entry.Key.TryToSetFlightPathHeadTo(yourWaypoint);
            }
        }

        internal bool IsActive(PlayLayer pl)
        {
            return pl.PosIsActive(Position);
        }
    }
}
