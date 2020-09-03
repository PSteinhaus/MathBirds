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
            TailColor = new CCColor4B(1.0f, 0.4f, 0.4f, 1.0f);  // I don't use "SetTailColor" here on purpose to create a color change from head to tail
        }
    }
}
