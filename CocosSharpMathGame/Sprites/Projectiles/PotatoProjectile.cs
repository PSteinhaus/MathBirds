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
            Velocity = 1200f;
            LifeTime = 1.35f;
            Damage = 2.5f;
            TailWidth = 5f;
        }
    }
}
