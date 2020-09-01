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
}
