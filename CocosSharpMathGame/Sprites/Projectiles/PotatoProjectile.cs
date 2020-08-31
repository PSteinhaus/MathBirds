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
            Velocity = 1000f;
            LifeTime = 1.15f;
            Damage = 4f;
            TailWidth = 5f;
        }
    }
}
