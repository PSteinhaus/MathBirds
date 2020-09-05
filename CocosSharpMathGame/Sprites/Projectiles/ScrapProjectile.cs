using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;
using CSharpMath;

namespace CocosSharpMathGame
{
    internal class ScrapProjectile : Projectile
    {
        internal ScrapProjectile()
        {
            Velocity = 650f;
            LifeTime = 3.4f;
            Damage = 1f;
            SetTailColor(new CCColor4B(1f, 1f, 1f, 0.75f));
        }
    }
}
