using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class TestProjectile : Projectile
    {
        internal TestProjectile()
        {
            Velocity = 1000f;
            LifeTime = 2f;
            Damage = 1f;
        }

        internal override void CollideWithAircraft(Aircraft aircraft)
        {
            // check if you really collide with it (at this point you only know that you collided with its bounding box)
            foreach (var part in aircraft.TotalParts)
            {
                if (part.MyState != Part.State.DESTROYED && Collisions.Collide(this,part))
                {
                    // end your life
                    Console.WriteLine("COLLIDED");
                    part.TakeDamage(Damage);
                    Die();
                    break;
                }
            }
        }
    }
}
