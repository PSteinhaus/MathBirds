using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Symbolics;
using Symbolism.Trigonometric;

namespace CocosSharpMathGame
{
    /// <summary>
    /// Describes how a part can aim and shoot.
    /// </summary>
    internal class WeaponAbility
    {
        internal Part MyPart { get; private protected set; }
        /// <summary>
        /// In CCDegrees
        /// </summary>
        internal float AttentionAngle { get; set; }
        internal float AttentionRange { get; set; }
        /// <summary>
        /// In CCDegrees
        /// </summary>
        internal float MaxTurningAngle { get; set; }
        internal float ShootingRange { get; set; }
        internal float SpreadAngle { get; set; } // doesn't do anything yet
        internal bool FireAtWill { get; set; } = true;
        /// <summary>
        /// Measured in shots per second
        /// </summary>
        internal float RateOfFire
        { 
            get
            {
                return 1 / ShootDelay;
            }
        }
        /// <summary>
        /// in seconds
        /// </summary>
        internal float ShootDelay { get; set; }
        /// <summary>
        /// in seconds
        /// </summary>
        internal float CooldownUntilNextShot { get; set; } = 0;
        internal float TurningAnglePerSecond { get; set; }
        /// <summary>
        /// projectile to be cloned when a new shot is fired
        /// </summary>
        internal Projectile ProjectileBlueprint { get; private protected set; }
        /// <summary>
        /// how far a shot can fly
        /// </summary>
        internal float Reach
        {
            get
            {
                return ProjectileBlueprint.Reach;
            }
        }
        /// <summary>
        /// the part (belonging to an aircraft) that this weapon currently aims for
        /// </summary>
        internal Part TargetPart { get; set; }
        internal Aircraft TargetAircraft
        {
            get
            {
                return TargetPart.Parent as Aircraft;
            }
        }

        /// <summary>
        /// called by the Aircraft owning the part owning this WeaponAbility each frame
        /// </summary>
        /// <param name="dt">time since the previous frame</param>
        internal void ExecuteOrders(float dt)
        {
            // cool down
            CooldownUntilNextShot -= dt;
            if (CooldownUntilNextShot < 0) CooldownUntilNextShot = 0;
            // if you have a target check if it is still in range
            if (TargetPart != null)
            {
                CCPoint vectorMyPartTarget = TargetPart.PositionWorldspace - MyPart.PositionWorldspace;
                if (   TargetPart.MyState == Part.State.DESTROYED || TargetAircraft.MyState == Aircraft.State.SHOT_DOWN
                    || CCPoint.Distance(MyPart.PositionWorldspace, TargetPart.PositionWorldspace) > AttentionRange
                    || Constants.AbsAngleDifferenceDeg(MyPart.TotalRotation - MyPart.RotationFromNull, Constants.DxDyToCCDegrees(vectorMyPartTarget.X, vectorMyPartTarget.Y)) > AttentionAngle)
                    TargetPart = null;
            }
            if (TargetPart == null)     // if you currently do not aim at anything search for a target
            {
                // collect aircrafts that are near enough to have parts which could be targets
                // go through the parts of all of these planes and collect those that are in the attention angle
                PartsInRange(out List<Part> partsInRange, out List<float> anglesFromTo, out List<float> distances);
                // try to choose a part that is in reach
                // choose the part that is closest anglewise
                // but prioritize aircraft bodies:
                //  this means that you should only change target from a body to another part if the part you would choose instead (because it's closer)
                //  belongs to another plane
                float minAngle = float.PositiveInfinity;
                for (int i=0; i<partsInRange.Count(); i++)
                {
                    if (distances[i] <= ShootingRange)  // first only try parts that are already in reach
                    {
                        float absAngle = (float)Math.Abs(anglesFromTo[i]);
                        var part = partsInRange[i];
                        if (absAngle < minAngle &&
                            (TargetPart == null || !(part.Parent == TargetPart.Parent && TargetPart == TargetAircraft.Body)))  // don't switch from a body to a different part of the same aircraft
                        {
                            TargetPart = part;
                            minAngle = absAngle;
                        }
                    }
                }
                if (TargetPart == null) // if you found no target this way check the rest
                {
                    minAngle = float.PositiveInfinity;
                    for (int i = 0; i < partsInRange.Count(); i++)
                    {
                        float absAngle = (float)Math.Abs(anglesFromTo[i]);
                        var part = partsInRange[i];
                        bool isBody = part == part.Aircraft.Body;
                        // now also try parts that are not already in reach
                        if (absAngle < minAngle &&
                            (TargetPart == null || !(part.Parent == TargetPart.Parent && TargetPart == TargetAircraft.Body)))  // don't switch from a body to a different part of the same aircraft
                        {
                            TargetPart = part;
                            minAngle = absAngle;
                        }
                    }
                }
            }
            // calculate the perfect point to aim for in order to hit the target
            if (TargetPart != null)
            {
                float angleToAimFor = AngleToAimFor();
                float angleToTurnTo = angleToAimFor;
                float angleTurn = Constants.AngleFromToDeg(MyPart.NullRotation, angleToTurnTo);
                if (angleTurn > MaxTurningAngle)
                    angleToTurnTo = MyPart.NullRotation + MaxTurningAngle;
                else if (angleTurn < -MaxTurningAngle)
                    angleToTurnTo = MyPart.NullRotation - MaxTurningAngle;
                MyPart.RotateTowards(angleToTurnTo, TurningAnglePerSecond * dt);
                // if you're now close enough to the perfect angle (and in range) start shooting
                if (CanShoot()
                    && CCPoint.Distance(MyPart.PositionWorldspace, TargetPart.PositionWorldspace) <= ShootingRange
                    && (Constants.AbsAngleDifferenceDeg(angleToAimFor, MyPart.MyRotation) <= 5f || WouldHit()))
                {
                    TryShoot();
                }
                    
            }
            // and if you have no target try to get back to NullRotation
            else
            {
                MyPart.RotateTowards(MyPart.NullRotation, TurningAnglePerSecond * dt);
            }
        }

        internal void PartsInRange(out List<Part> partsInRange, out List<float> anglesFromTo, out List<float> distances)
        {
            // collect aircrafts that are near enough to have parts which could be targets
            List<Aircraft> aircraftsInRange = new List<Aircraft>();
            foreach (var aircraft in ((PlayLayer)MyPart.Layer).Aircrafts)
            {
                // check if it is considered an enemy
                if (!((Aircraft)MyPart.Parent).Team.IsEnemy(aircraft.Team) || aircraft.MyState.Equals(Aircraft.State.SHOT_DOWN))
                    continue;
                // check if in attention arc
                if (Collisions.CollideArcBoundingBox(MyPart.PositionWorldspace, AttentionRange, MyPart.TotalNullRotation, AttentionAngle, aircraft.BoundingBoxTransformedToWorld))
                    aircraftsInRange.Add(aircraft);
            }
            partsInRange = new List<Part>();
            anglesFromTo = new List<float>();
            distances = new List<float>();
            foreach (var aircraft in aircraftsInRange)
            {
                foreach (var part in aircraft.TotalParts)
                {
                    if (part.MyState == Part.State.DESTROYED) continue;
                    // using the position as criterium is a bit dirty and could be improved on by using the bounding box instead (at the cost of performance)
                    CCPoint vectorMyPartPart = part.PositionWorldspace - MyPart.PositionWorldspace;
                    float distance = vectorMyPartPart.Length;
                    if (distance <= AttentionRange)
                    {
                        var angleFromTo = Constants.AngleFromToDeg(MyPart.TotalRotation - MyPart.RotationFromNull, Constants.DxDyToCCDegrees(vectorMyPartPart.X, vectorMyPartPart.Y));
                        if ((float)Math.Abs(angleFromTo) <= AttentionAngle)
                        {
                            partsInRange.Add(part);
                            anglesFromTo.Add(Constants.AngleFromToDeg(MyPart.TotalRotation, Constants.DxDyToCCDegrees(vectorMyPartPart.X, vectorMyPartPart.Y)));
                            distances.Add(distance);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the angle (for MyRotation) that the weapon should be turned to to be able to hit the target perfectly
        /// </summary>
        /// <returns></returns>
        internal float AngleToAimFor()
        {
            // as there is no exact solution the solution is here approched by iteration
            CCPoint TargetToMyPart = MyPart.PositionWorldspace - TargetPart.PositionWorldspace;
            // rotate the vector, so that the movement direction of the target is identical to the x axis
            float transformationRotation = -Constants.DxDyToRadians(TargetAircraft.VelocityVector.X, TargetAircraft.VelocityVector.Y);
            TargetToMyPart = CCPoint.RotateByAngle(TargetToMyPart, CCPoint.Zero, transformationRotation);
            // now the cordinate system is simpler and we can compute the difference between
            // a) the intersection of the flight path and the bullet path
            // and b) the point where the target actually is going to be by then
            // of course we want to minimize this error, for this we will iterate

            // ALTERNATIVE ITERATION MATHOD USING THE BISECTION METHOD
            double epsilon = 1;
            double angle = 0;
            // define the interval
            double startAngle = Constants.DxDyToRadians(-TargetToMyPart.X, -TargetToMyPart.Y);
            double endAngle = 0;
            double deltaStart = TargetToMyPart.X - TargetToMyPart.Y / Math.Tan(startAngle) + (TargetToMyPart.Y * TargetAircraft.VelocityVector.Length) / (Math.Sin(startAngle) * ProjectileBlueprint.Velocity);
            for (int i = 0; i < 6; i++) // iterate 6 times at max
            {
                angle = (startAngle + endAngle) / 2;
                double delta = TargetToMyPart.X - TargetToMyPart.Y / Math.Tan(angle) + (TargetToMyPart.Y * TargetAircraft.VelocityVector.Length) / (Math.Sin(angle) * ProjectileBlueprint.Velocity);
                if (Math.Abs(delta) < epsilon)  // you're close enough so break already
                    break;
                // delta is negative when the bullet hits behind the part -> angle needs to be closer to 0
                if (Math.Sign(deltaStart) == Math.Sign(delta))
                {
                    startAngle = angle;
                    deltaStart = delta;
                }
                else
                {
                    endAngle = angle;
                }
            }
            // subtract the transformation rotation to get the total angle
            float totalAngle = Constants.RadiansToCCDegrees((float)angle - transformationRotation);
            // for the final angle transform the total angle to a relative angle
            return Constants.AngleFromToDeg(((IGameObject)MyPart.Parent).TotalRotation, totalAngle);
        }

        internal bool CanShoot()
        {
            return CooldownUntilNextShot <= 0;
        }

        internal void TryShoot()
        {
            if (CanShoot())
            {
                CooldownUntilNextShot = ShootDelay;
                Projectile newProjectile = (Projectile)ProjectileBlueprint.Clone();
                Console.WriteLine("TotalRot: " + MyPart.TotalRotation);
                newProjectile.SetRotation(MyPart.TotalRotation);
                newProjectile.Position = MyPart.PositionWorldspace;
                newProjectile.MyTeam = MyPart.Aircraft.Team;
                ((PlayLayer)MyPart.Layer).AddProjectile(newProjectile);
            }
            
        }

        /// <summary>
        /// Check whether a bullet shot now would hit any parts of the TargetAircraft (if the TargetAircraft wouldn't move).
        /// </summary>
        /// <param name="partsInRange"></param>
        /// <returns>whether the bullet would hit</returns>
        internal bool WouldHit()
        {
            Constants.CCDegreesToDxDy(MyPart.TotalRotation, out float dx, out float dy);
            CollisionTypeLine cTypeReachLine = new CollisionTypeLine(MyPart.PositionWorldspace, MyPart.PositionWorldspace + new CCPoint(dx * Reach, dy * Reach));
            foreach (Part part in TargetAircraft.TotalParts)
            {
                if (Collisions.CollidePolygonLine(part, ((CollisionTypePolygon)part.CollisionType), cTypeReachLine))
                    return true;
            }
            return false;
        }
        internal WeaponAbility(Part myPart)
        {
            MyPart = myPart;
        }

        /// <summary>
        /// Create a test weapon ability. This could be solved by subclassing too, but there is no need to create a new class for now.
        /// </summary>
        /// <param name="myPart"></param>
        /// <returns></returns>
        internal static WeaponAbility CreateTestWeapon(Part myPart)
        {
            var testWeapon = new WeaponAbility(myPart);
            testWeapon.ProjectileBlueprint = new TestProjectile();
            testWeapon.CalcBaseValuesFromProjectile();
            testWeapon.ShootDelay = 0.25f;
            testWeapon.MaxTurningAngle = 45f;
            testWeapon.TurningAnglePerSecond = testWeapon.MaxTurningAngle;
            testWeapon.CalcAttentionAngle();
            return testWeapon;
        }

        /// <summary>
        /// Often it's simpler to just create your base values based on the projectile you want to shoot. 
        /// </summary>
        private protected void CalcBaseValuesFromProjectile()
        {
            ShootingRange = ProjectileBlueprint.Reach;
            AttentionRange = ShootingRange * 1.5f;
        }

        /// <summary>
        /// calculate the AttentionAngle based on the MaxTurningAngle and the turning rate
        /// </summary>
        internal void CalcAttentionAngle()
        {
            AttentionAngle = MaxTurningAngle + MaxTurningAngle * MaxTurningAngle / (TurningAnglePerSecond / 4);
            if (AttentionAngle > 180f) AttentionAngle = 180f;
        }
    }
}
