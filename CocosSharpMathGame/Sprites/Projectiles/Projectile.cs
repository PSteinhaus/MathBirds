using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal abstract class Projectile : GameObjectNode, ICollidible, ICloneable, IAdvancable, IDrawNodeUser
    {
        public CollisionType CollisionType { get; set; }
        internal float Velocity { get; private protected set; }
        internal float LifeTime { get; set; }
        internal float TimeAlive { get; set; } = 0;
        internal float TailLifeTime = 1f;
        internal float TailWidth { get; set; } = 3f;
        internal CCColor4B TailColor { get; set; }
        internal CCColor4B TailEndColor { get; set; }
        internal float Damage { get; set; }
        internal float Dx { get; private protected set; }
        internal float Dy { get; private protected set; }
        internal float Reach
        {
            get
            {
                return Velocity * LifeTime;
            }
        }
        internal Team MyTeam { get; set; }
        
        internal Projectile()
        {
            Init();
            SetTailColor(CCColor4B.White);
        }

        internal void SetRotation(float CCrotation, bool updateDxDy=true)
        {
            MyRotation = CCrotation;
            if (updateDxDy) UpdateDxDy();
        }

        internal void SetVelocity(float velocity, bool updateDxDy=true)
        {
            Velocity = velocity;
            if (updateDxDy) UpdateDxDy();
        }

        private protected void UpdateDxDy()
        {
            Constants.CCDegreesToDxDy(MyRotation, out float dx, out float dy);
            Dx = dx * Velocity; Dy = Velocity * dy;
        }

        internal void SetTailColor(CCColor4B color)
        {
            TailColor = color;
            byte alpha = (byte) (color.A - color.A / 4);
            TailEndColor = new CCColor4B(color.R, color.G, color.B, alpha);
        }

        internal bool IsAlive()
        {
            return TimeAlive < LifeTime;
        }

        public bool Advance(float dt)
        {
            TimeAlive += dt;
            if (IsAlive())
            {
                var oldPos = Position;
                PositionX += Dx * dt;
                PositionY += Dy * dt;
                ((CollisionTypeLine)CollisionType).StartPoint = oldPos;
                ((CollisionTypeLine)CollisionType).EndPoint = Position;
                // check for collision
                foreach (var aircraft in (MyTeam == Team.PlayerTeam ? ((PlayLayer)Parent).Aircrafts : ((PlayLayer)Parent).PlayerAircrafts))
                {
                    if (aircraft.Team.IsEnemy(MyTeam))
                    {
                        if (Collisions.CollideBoundingBoxLine(aircraft, (CollisionTypeLine)CollisionType))
                        {
                            CollideWithAircraft(aircraft);
                        }
                    }
                }
            }
            return CanBeRemoved();
        }

        internal bool CanBeRemoved()
        {
            return TimeAlive - LifeTime > TailLifeTime;
        }

        internal void Die()
        {
            LifeTime = TimeAlive;
        }

        public void UseDrawNodes(CCDrawNode highNode, CCDrawNode lowNode)
        {
            // calculate how far back the tail needs to go
            float tMidTail = TailLifeTime / 2;
            float tEndTail = TailLifeTime;
            if (TimeAlive - tMidTail < 0)
            {
                tMidTail = TimeAlive;
                tEndTail = TimeAlive;
            }
            else if (TimeAlive - tEndTail < 0)
                tEndTail = TimeAlive;
            if (!IsAlive())
            {
                float diff = TimeAlive - LifeTime;
                tMidTail -= diff;
                tEndTail -= diff;
                if (tMidTail < 0) tMidTail = 0;
                if (tEndTail < 0) tEndTail = 0;
            }
            CCPoint midPoint = new CCPoint(PositionX - Dx * tMidTail, PositionY - Dy * tMidTail);
            CCPoint endPoint = new CCPoint(PositionX - Dx * tEndTail, PositionY - Dy * tEndTail);
            if (tMidTail != 0)
                lowNode.DrawLine(Position, midPoint, TailWidth, TailColor);
            if (tEndTail != 0)
                lowNode.DrawLine(midPoint, endPoint, TailWidth, TailEndColor);
            //TailNode.DrawSolidCircle(Position, 10f, CCColor4B.Yellow);
        }

        private void Init()
        {
            CollisionType = new CollisionTypeLine(CCPoint.Zero, CCPoint.Zero);
        }

        public object Clone()   // make virtual in the future if necessary
        {
            object clone = MemberwiseClone();
            ((Projectile)clone).Init();
            return clone;
        }

        private protected void StandardCollisionBehaviour(Part part, CCPoint collisionPos)
        {
            // damage the part
            part.TakeDamage(Damage);
            // let the part react visually
            part.ReactToHit(Damage, collisionPos);
            // end your life
            Die();
        }

        internal void CollideWithAircraft(Aircraft aircraft)
        {
            // collect all parts that you could collide with right now and then choose the one that is lowest in the mount hierarchy
            List<Part> collidingParts = new List<Part>();
            // check if you really collide with it (at this point you only know that you collided with its bounding box)
            foreach (var part in aircraft.TotalParts)
            {
                if (part.MyState != Part.State.DESTROYED && Collisions.Collide(this, part))
                {
                    collidingParts.Add(part);
                }
            }
            Part lowestPart = null;
            int maxMountParents = -1;
            foreach (var part in collidingParts)
            {
                int mountParents = part.NumOfMountParents();
                if (mountParents > maxMountParents)
                {
                    lowestPart = part;
                    maxMountParents = mountParents;
                }
            }
            if (lowestPart != null)
            {
                CCPoint collisionPos = Collisions.CollisionPosLinePoly((CollisionTypeLine)CollisionType, lowestPart);
                StandardCollisionBehaviour(lowestPart, collisionPos);
            }
        }
    }
}
