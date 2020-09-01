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
            Velocity = 2000f;
            LifeTime = 0.9f;
            Damage = 2f;
        }
    }

    internal class TestProjectileShiny : Projectile
    {
        internal TestProjectileShiny()
        {
            Velocity = 2000f;
            LifeTime = 0.8f;
            Damage = 2f;
            SetTailColor(new CCColor4B(1.0f, 0.7f, 0.7f, 1.0f));
        }
    }
}
