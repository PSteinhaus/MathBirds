using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using CSharpMath;

namespace CocosSharpMathGame
{
    internal class JetProjectile : Projectile
    {
        internal JetProjectile()
        {
            Velocity = 3600f;
            LifeTime = 0.55f;
            Damage = 10f;
        }
    }
}
