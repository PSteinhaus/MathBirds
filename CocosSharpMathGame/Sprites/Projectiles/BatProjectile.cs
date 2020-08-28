using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using CSharpMath;

namespace CocosSharpMathGame
{
    internal class BatProjectile : Projectile
    {
        internal BatProjectile()
        {
            Velocity = 1500f;
            LifeTime = 0.65f;
            Damage = 1f;
            TailLifeTime = 0.25f;
            TailWidth = 2f;
        }
    }
}
