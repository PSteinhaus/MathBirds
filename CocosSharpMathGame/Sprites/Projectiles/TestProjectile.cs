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
        }

        internal override void CollideWithAircraft(Aircraft aircraft)
        {
            // end your life
            Console.WriteLine("COLLIDED");
            Die();
        }
    }
}
