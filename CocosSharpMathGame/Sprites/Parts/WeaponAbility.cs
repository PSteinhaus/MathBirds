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
        internal float AttentionAngle { get; set; }
        internal float AttentionRange { get; set; }
        internal float ShootingAngle { get; set; }
        internal float ShootingRange { get; set; }
        internal float SpreadAngle { get; set; }
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
        internal void Update(float dt)
        {
            // cool down
            CooldownUntilNextShot -= dt;
            if (CooldownUntilNextShot < 0) CooldownUntilNextShot = 0;
            // if you have a target check if it is still in range
            if (TargetPart != null)
            {
                CCPoint vectorMyPartTarget = TargetPart.PositionWorldspace - MyPart.PositionWorldspace;
                if (CCPoint.Distance(MyPart.PositionWorldspace, TargetPart.PositionWorldspace) > AttentionRange
                    || Constants.AbsAngleDifferenceDeg(MyPart.TotalRotation - MyPart.RotationFromNull, Constants.DxDyToCCDegrees(vectorMyPartTarget.X, vectorMyPartTarget.Y)) > AttentionAngle)
                    TargetPart = null;
            }
            if (TargetPart == null)     // if you currently do not aim at anything search for a target
            {
                // collect aircrafts that are near enough to have parts which could be targets
                List<Aircraft> aircraftsInRange = new List<Aircraft>();
                foreach (var aircraft in ((PlayLayer)MyPart.Layer).Aircrafts)
                {
                    // check if it is considered an enemy
                    if (!((Aircraft)MyPart.Parent).Team.IsEnemy(aircraft.Team))
                        continue;
                    // check if in attention range
                    // for that first check if the circle defined by your part position and the attention radius collides with the bounding box of the aircraft
                    if (Collisions.CollideBoundingBoxCircle(aircraft.BoundingBoxTransformedToWorld, MyPart.PositionWorldspace, AttentionRange))
                        aircraftsInRange.Add(aircraft);
                }
                // go through the parts of all of these planes and collect those that are in the attention angle
                List<Part> partsInRange = new List<Part>();
                List<float> anglesFromTo = new List<float>();
                List<float> distances = new List<float>();
                foreach (var aircraft in aircraftsInRange)
                {
                    foreach (var part in aircraft.TotalParts)
                    {
                        CCPoint vectorMyPartPart = part.PositionWorldspace - MyPart.PositionWorldspace;
                        float distance = vectorMyPartPart.Length;
                        var angleFromTo = Constants.AngleFromToDeg(MyPart.TotalRotation - MyPart.RotationFromNull, Constants.DxDyToCCDegrees(vectorMyPartPart.X, vectorMyPartPart.Y));
                        if (distance <= AttentionRange
                            && (float)Math.Abs(angleFromTo) <= AttentionAngle)
                        {
                            partsInRange.Add(part);
                            anglesFromTo.Add(Constants.AngleFromToDeg(MyPart.TotalRotation, Constants.DxDyToCCDegrees(vectorMyPartPart.X, vectorMyPartPart.Y)));
                            distances.Add(distance);
                        }
                    }
                }
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
                MyPart.RotateTowards(angleToAimFor, TurningAnglePerSecond);
                // if you're now close enough to the perfect angle start shooting
                if (Constants.AbsAngleDifferenceDeg(angleToAimFor, MyPart.MyRotation) <= 5f)
                    TryShoot();
            }
            // and if you have no target try to get back to NullRotation
            else
            {
                MyPart.RotateTowards(MyPart.NullRotation, TurningAnglePerSecond);
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
            float transformationRotation = -Constants.DxDyToRadians(TargetPart.Aircraft.VelocityVector.X, TargetPart.Aircraft.VelocityVector.X);
            CCPoint.RotateByAngle(TargetToMyPart, CCPoint.Zero, transformationRotation);
            // now the cordinate system is simpler and we can compute the difference between
            // a) the intersection of the flight path and the bullet path
            // and b) the point where the target actually is going to be by then
            // of course we want to minimize this error, for this we will iterate
            double angle = Constants.DxDyToRadians(-TargetToMyPart.X, -TargetToMyPart.Y);
            double step = 0.2;
            double delta;
            int signDelta = -1;
            int angleSign = Math.Sign(angle);
            for (int i=0; i<10; i++) // iterate 10 times
            {
                delta = TargetToMyPart.X - TargetToMyPart.Y / Math.Tan(angle) + (TargetToMyPart.Y * TargetAircraft.VelocityVector.Length) / (Math.Sin(angle) * ProjectileBlueprint.Velocity);
                // delta is negative when the bullet hits behind the part -> angle needs to be closer to 0
                if (signDelta != Math.Sign(delta)) // each time the sign changes decrease the step size
                {
                    step /= 2;
                    signDelta = Math.Sign(delta);
                }
                angle -= (delta < 0 ? angleSign : -angleSign) * step;
            }
            // subtract the transformation rotation to get the total angle
            float totalAngle = Constants.RadiansToCCDegrees((float)angle - transformationRotation);
            // for the final angle transform the total angle to a relative angle
            return Constants.AngleFromToDeg(((IGameObject)MyPart.Parent).TotalRotation, totalAngle);
        }

        internal void TryShoot()
        {
            if (CooldownUntilNextShot <= 0)
            {
                CooldownUntilNextShot = ShootDelay;
                Projectile newProjectile = (Projectile)ProjectileBlueprint.Clone();
                newProjectile.Position = MyPart.PositionWorldspace;
                newProjectile.MyRotation = MyPart.TotalRotation;
                ((PlayLayer)MyPart.Layer).AddProjectile(newProjectile);
            }
            
        }

        internal WeaponAbility(Part myPart)
        {
            MyPart = myPart;
        }
    }
}
