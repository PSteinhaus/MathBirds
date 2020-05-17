using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal abstract class Projectile : GameObjectSprite, ICollidible, ICloneable
    {
        public CollisionType CollisionType { get; set; } = new CollisionTypeLine(CCPoint.Zero, CCPoint.Zero);
        internal float Velocity { get; set; }
        internal float LifeTime { get; set; }
        internal float Reach
        {
            get
            {
                return Velocity * LifeTime;
            }
        }
        internal Team MyTeam { get; set; }
        
        internal Projectile(CCSpriteFrame spriteFrame, CCPoint position, float CCrotation) : base(spriteFrame)
        {
            Position = position;
            MyRotation = CCrotation;
        }

        internal void Advance(float dt)
        {
            var oldPos = Position;
            Constants.CCDegreesToDxDy(MyRotation, out float dx, out float dy);
            PositionX += dx * Velocity * dt;
            PositionY += dy * Velocity * dt;
            ((CollisionTypeLine)CollisionType).StartPoint = oldPos;
            ((CollisionTypeLine)CollisionType).EndPoint = Position;
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

        internal virtual void PrepareForRemoval()
        {
        }

        public abstract object Clone();

        internal abstract void CollideWithAircraft(Aircraft aircraft);
    }
}
