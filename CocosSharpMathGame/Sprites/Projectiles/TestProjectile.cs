using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using CSharpMath;

namespace CocosSharpMathGame
{
    internal class TestProjectile : Projectile
    {
        internal TestProjectile()
        {
            Velocity = 1000f;
            LifeTime = 1.5f;
            Damage = 1f;
        }

        internal override void CollideWithAircraft(Aircraft aircraft)
        {
            // collect all parts that you could collide with right now and then choose the one that is lowest in the mount hierarchy
            List<Part> collidingParts = new List<Part>();
            // check if you really collide with it (at this point you only know that you collided with its bounding box)
            foreach (var part in aircraft.TotalParts)
            {
                if (part.MyState != Part.State.DESTROYED && Collisions.Collide(this,part))
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
