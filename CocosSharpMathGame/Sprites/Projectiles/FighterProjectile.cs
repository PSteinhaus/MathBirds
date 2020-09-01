using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using CSharpMath;

namespace CocosSharpMathGame
{
    internal class FighterProjectile : Projectile
    {
        internal FighterProjectile()
        {
            Velocity = 2500f;
            LifeTime = 0.6f;
            Damage = 3.5f;
        }
    }

    internal class FighterProjectileShiny : Projectile
    {
        internal FighterProjectileShiny()
        {
            Velocity = 2500f;
            LifeTime = 0.5f;
            Damage = 4f;
            SetTailColor(CCColor4B.Red);
        }
    }
}
