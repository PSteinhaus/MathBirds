using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal abstract class Projectile : GameObjectNode, ICollidible, ICloneable
    {
        public CollisionType CollisionType { get; set; } = new CollisionTypeLine(CCPoint.Zero, CCPoint.Zero);
        internal CCDrawNode TailNode { get; private protected set; } = new CCDrawNode();
        internal float Velocity { get; set; }
        internal float LifeTime { get; set; }
        internal float TimeAlive { get; set; } = 0;
        internal float TailLifeTime = 1f;
        internal float TailWidth { get; set; } = 3f;
        internal CCColor4B TailColor { get; set; }
        internal CCColor4B TailEndColor { get; set; }
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
        
        internal Projectile(CCPoint position, float CCrotation, float velocity, Team team)
        {
            Position = position;
            Velocity = velocity;
            MyRotation = CCrotation;
            Constants.CCDegreesToDxDy(MyRotation, out float dx, out float dy);
            Dx = dx * velocity; Dy = velocity * dy;
            MyTeam = team;
            TailNode.BlendFunc = CCBlendFunc.NonPremultiplied;
            SetTailColor(CCColor4B.White);
        }

        internal void SetTailColor(CCColor4B color)
        {
            TailColor = color;
            TailEndColor = new CCColor4B(color.R, color.G, color.B, color.A / 2);
        }

        internal bool IsAlive()
        {
            return TimeAlive < LifeTime;
        }

        internal void Advance(float dt)
        {
            TimeAlive += dt;
            var oldPos = Position;
            PositionX += Dx * dt;
            PositionY += Dy * dt;
            ((CollisionTypeLine)CollisionType).StartPoint = oldPos;
            ((CollisionTypeLine)CollisionType).EndPoint = Position;
            // draw the tail
            DrawTail();
            if (IsAlive())
            {
                // check for collision
                foreach (var aircraft in ((PlayLayer)Parent).Aircrafts)
                {
                    if (aircraft.Team != MyTeam)
                    {
                        if (Collisions.CollideBoundingBoxLine(aircraft, (CollisionTypeLine)CollisionType))
                        {
                            CollideWithAircraft(aircraft);
                        }
                    }
                }
            }
            else
            {
                // remove if the tail is no longer visible
                if (TimeAlive - LifeTime > TailLifeTime)
                    ((PlayLayer)Parent).RemoveProjectile(this);    
            }
        }

        internal void Die()
        {
            LifeTime = TimeAlive;
        }

        internal void DrawTail()
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
            TailNode.Clear();
            if (tMidTail != 0)
                TailNode.DrawLine(Position, midPoint, TailWidth, TailColor);
            if (tEndTail != 0)
                TailNode.DrawLine(midPoint, endPoint, TailWidth, TailEndColor);
        }

        protected override void AddedToScene()
        {
            base.AddedToScene();
            // add your tail
            Parent.AddChild(TailNode);
        }

        internal virtual void PrepareForRemoval()
        {
            // remove your tail
            Parent.RemoveChild(TailNode);
        }

        public abstract object Clone();

        internal abstract void CollideWithAircraft(Aircraft aircraft);
    }
}
