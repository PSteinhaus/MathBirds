using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using CSharpMath;

namespace CocosSharpMathGame
{
    internal class BalloonProjectile : Projectile
    {
        internal BalloonProjectile()
        {
            Velocity = 2200f;
            LifeTime = 1.25f;
            Damage = 10f;
        }
    }
}
