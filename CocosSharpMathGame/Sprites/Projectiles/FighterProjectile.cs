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
            LifeTime = 1.0f;
            Damage = 3.5f;
        }
    }
}
