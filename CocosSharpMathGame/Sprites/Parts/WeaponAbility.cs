using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.Random;
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
        internal float ToleratedError { get; set; } = 2f;
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
        internal float UpdateTargetDelay { get; set; } = 0.5f;
        internal float CooldownUntilNextTargetUpdate { get; set; }

        /// <summary>
        /// called by the Aircraft owning the part owning this WeaponAbility each frame
        /// </summary>
        /// <param name="dt">time since the previous frame</param>
        internal void ExecuteOrders(float dt)
        {
            // cool down
            CooldownUntilNextShot -= dt;
            CooldownUntilNextTargetUpdate -= dt;
            if (CooldownUntilNextShot < 0) CooldownUntilNextShot = 0;
            // if you have a target check if it is still in range
            if (TargetPart != null)
            {
                    CCPoint vectorMyPartTarget = TargetPart.PositionWorldspace - MyPart.PositionWorldspace;
                    if (TargetPart.MyState == Part.State.DESTROYED || TargetAircraft.MyState == Aircraft.State.SHOT_DOWN
                        || CooldownUntilNextTargetUpdate <= 0
                        || CCPoint.Distance(MyPart.PositionWorldspace, TargetPart.PositionWorldspace) > AttentionRange
                        || Constants.AbsAngleDifferenceDeg(MyPart.TotalRotation - MyPart.RotationFromNull, Constants.DxDyToCCDegrees(vectorMyPartTarget.X, vectorMyPartTarget.Y)) > AttentionAngle)
                        TargetPart = null;
            }
            if (TargetPart == null && CooldownUntilNextTargetUpdate <= 0)     // if you currently do not aim at anything search for a target
            {
                CooldownUntilNextTargetUpdate = UpdateTargetDelay;
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
                        if (absAngle < minAngle) //&&
                            //(TargetPart == null || !(part.Parent == TargetPart.Parent && TargetPart == TargetAircraft.Body)))  // don't switch from a body to a different part of the same aircraft
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
                        // now also try parts that are not already in reach
                        if (absAngle < minAngle) //&&
                            //(TargetPart == null || !(part.Parent == TargetPart.Parent && TargetPart == TargetAircraft.Body)))  // don't switch from a body to a different part of the same aircraft
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
                // make sure you don't rotate further than your MountPoint allows
                if (angleTurn > MyPart.MountPoint.MaxTurningAngle)
                    angleToTurnTo = MyPart.NullRotation + MyPart.MountPoint.MaxTurningAngle;
                else if (angleTurn < -MyPart.MountPoint.MaxTurningAngle)
                    angleToTurnTo = MyPart.NullRotation - MyPart.MountPoint.MaxTurningAngle;
                // make sure you don't rotate further than this weapons MaxTurningAngle allows
                if (MaxTurningAngle < MyPart.MountPoint.MaxTurningAngle)
                {
                    if (angleTurn > MaxTurningAngle)
                        angleToTurnTo = MyPart.NullRotation + MaxTurningAngle;
                    else if (angleTurn < -MaxTurningAngle)
                        angleToTurnTo = MyPart.NullRotation - MaxTurningAngle;
                }
                MyPart.RotateTowards(angleToTurnTo, TurningAnglePerSecond * dt);
                // if you're now close enough to the perfect angle (and in range) start shooting
                if (CanShoot()
                    && CCPoint.Distance(MyPart.PositionWorldspace, TargetPart.PositionWorldspace) <= ShootingRange
                    && (Constants.AbsAngleDifferenceDeg(angleToAimFor, MyPart.MyRotation) <= ToleratedError || WouldHit()))
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
            foreach (var aircraft in (MyPart.Aircraft.Team == Team.PlayerTeam ? ((PlayLayer)MyPart.Layer).ActiveAircrafts : ((PlayLayer)MyPart.Layer).PlayerAircrafts))   // a bit dirty since technically NPC allies to the player could exist (someday), which would not be contained in PlayerAircrafts, but which would still (maybe) be part of the player team
            {
                // check if it is considered an enemy
                if (!MyPart.Aircraft.Team.IsEnemy(aircraft.Team) || aircraft.MyState.Equals(Aircraft.State.SHOT_DOWN))
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
        /// Returns the (relative) rotation angle (for MyRotation) indicating how far into which direction the weapon
        /// should be turned to be able to hit the target perfectly.
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

            // ITERATION USING THE BISECTION METHOD
            double epsilon = 1;
            double angle = 0;
            // define the interval
            double startAngle = Constants.DxDyToRadians(-TargetToMyPart.X, -TargetToMyPart.Y);
            double endAngle = 0;
            float totalBulletVelocity = ProjectileBlueprint.Velocity + AircraftVelocityBoost();
            double deltaStart = TargetToMyPart.X - TargetToMyPart.Y / Math.Tan(startAngle) + (TargetToMyPart.Y * TargetAircraft.VelocityVector.Length) / (Math.Sin(startAngle) * totalBulletVelocity);
            for (int i = 0; i < 6; i++) // iterate 6 times at max
            {
                angle = (startAngle + endAngle) / 2;
                double delta = TargetToMyPart.X - TargetToMyPart.Y / Math.Tan(angle) + (TargetToMyPart.Y * TargetAircraft.VelocityVector.Length) / (Math.Sin(angle) * totalBulletVelocity);
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
            return CooldownUntilNextShot <= 0 && MyPart.Aircraft.SelectedPower != PowerUp.PowerType.SHIELD;
        }

        internal void TryShoot()
        {
            if (CanShoot())
            {
                CooldownUntilNextShot = ShootDelay;
                Projectile newProjectile = (Projectile)ProjectileBlueprint.Clone();

                // add an (additive) error to the shot angle
                var rng = new Random();
                float spreadError = (float)rng.NextDouble() * SpreadAngle * (rng.NextBoolean() ? 1 : -1);

                newProjectile.SetRotation(MyPart.TotalRotation + spreadError, updateDxDy:false);
                newProjectile.SetVelocity(newProjectile.Velocity + AircraftVelocityBoost());
                newProjectile.Position = MyPart.PositionWorldspace;
                newProjectile.MyTeam = MyPart.Aircraft.Team;
                ((PlayLayer)MyPart.Layer).AddProjectile(newProjectile);
            }
            
        }

        private float AircraftVelocityBoost()
        {
            return MyPart.Aircraft.VelocityVector.Length * (float)Math.Cos(Constants.CCDegreesToMathRadians(Constants.AngleFromToDeg(MyPart.TotalRotation, MyPart.Aircraft.TotalRotation)));
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
            testWeapon.SpreadAngle = 1f;
            testWeapon.ShootDelay = 0.5f;
            testWeapon.MaxTurningAngle = 15f;
            testWeapon.TurningAnglePerSecond = 20f;
            testWeapon.CalcAttentionAngle();
            return testWeapon;
        }

        internal static WeaponAbility CreateBalloonWeapon(Part weaponBalloon)
        {
            var weapon = new WeaponAbility(weaponBalloon);
            weapon.ProjectileBlueprint = new BalloonProjectile();
            weapon.CalcBaseValuesFromProjectile();
            weapon.SpreadAngle = 5f;
            weapon.ShootDelay = 2f;
            weapon.MaxTurningAngle = 180f;
            weapon.TurningAnglePerSecond = 25f;
            weapon.CalcAttentionAngle();
            return weapon;
        }

        internal static WeaponAbility CreateBatWeapon(Part weaponBat)
        {
            var weapon = new WeaponAbility(weaponBat);
            weapon.ProjectileBlueprint = new BatProjectile();
            weapon.CalcBaseValuesFromProjectile();
            weapon.SpreadAngle = 14f;
            weapon.ShootDelay = 0.25f;
            weapon.MaxTurningAngle = 180f;
            weapon.TurningAnglePerSecond = 90f;
            weapon.CalcAttentionAngle();
            return weapon;
        }

        internal static WeaponAbility CreatePotatoWeapon(Part weaponPotato)
        {
            var weapon = new WeaponAbility(weaponPotato);
            weapon.ProjectileBlueprint = new PotatoProjectile();
            weapon.CalcBaseValuesFromProjectile();
            weapon.SpreadAngle = 2.5f;
            weapon.ShootDelay = 1.5f;
            weapon.MaxTurningAngle = 35f;
            weapon.TurningAnglePerSecond = 55f;
            weapon.CalcAttentionAngle();
            return weapon;
        }

        internal static WeaponAbility CreateBigBomberWeapon(Part weaponBigBomber)
        {
            var weapon = new WeaponAbility(weaponBigBomber);
            weapon.ProjectileBlueprint = new BigBomberProjectile();
            weapon.CalcBaseValuesFromProjectile();
            weapon.SpreadAngle = 15.5f;
            weapon.ShootDelay = 0.35f;
            weapon.MaxTurningAngle = 180f;
            weapon.TurningAnglePerSecond = 35f;
            weapon.CalcAttentionAngle();
            return weapon;
        }

        internal static WeaponAbility CreateFighterWeapon(Part weaponFighter)
        {
            var weapon = new WeaponAbility(weaponFighter);
            weapon.ProjectileBlueprint = new FighterProjectile();
            weapon.CalcBaseValuesFromProjectile();
            weapon.SpreadAngle = 0.5f;
            weapon.ShootDelay = 0.5f;
            weapon.MaxTurningAngle = 10f;
            weapon.TurningAnglePerSecond = 60f;
            weapon.CalcAttentionAngle();
            return weapon;
        }

        internal static WeaponAbility CreateJetWeapon(Part wingJet)
        {
            var weapon = new WeaponAbility(wingJet);
            weapon.ProjectileBlueprint = new FighterProjectile();
            weapon.CalcBaseValuesFromProjectile();
            weapon.SpreadAngle = 0f;
            weapon.ShootDelay = 1.5f;
            weapon.MaxTurningAngle = 0f;
            weapon.TurningAnglePerSecond = 0f;
            weapon.AttentionAngle = 45f;
            weapon.ToleratedError = 7f;
            return weapon;
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
