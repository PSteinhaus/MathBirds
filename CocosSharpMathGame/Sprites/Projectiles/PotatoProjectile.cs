using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using CSharpMath;

namespace CocosSharpMathGame
{
    internal class PotatoProjectile : Projectile
    {
        internal PotatoProjectile()
        {
            Velocity = 1600f;
            LifeTime = 1.55f;
            Damage = 3.0f;
            TailWidth = 5f;
        }
    }
}
