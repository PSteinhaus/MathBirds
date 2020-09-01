using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using CSharpMath;

namespace CocosSharpMathGame
{
    internal class BigBomberProjectile : Projectile
    {
        internal BigBomberProjectile()
        {
            Velocity = 3000f;
            LifeTime = 0.70f;
            Damage = 3.5f;
        }
    }

    internal class BigBomberProjectileShiny : Projectile
    {
        internal BigBomberProjectileShiny()
        {
            Velocity = 4000f;
            LifeTime = 0.70f;
            Damage = 13.5f;
            TailWidth = 5f;
            SetTailColor(new CCColor4B(0.7f, 1f, 0.7f, 1f));
        }
    }
}
