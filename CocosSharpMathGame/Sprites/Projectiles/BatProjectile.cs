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
            Damage = 2f;
            TailLifeTime = 0.35f;
            TailWidth = 2f;
        }
    }

    internal class BatProjectileShiny : Projectile
    {
        internal BatProjectileShiny()
        {
            Velocity = 2500f;
            LifeTime = 0.65f;
            Damage = 2f;
            TailLifeTime = 0.35f;
            TailWidth = 2f;
            SetTailColor(CCColor4B.Magenta);
        }
    }
}
