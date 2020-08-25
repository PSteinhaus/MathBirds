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
            LifeTime = 1.0f;
            Damage = 2f;
        }
    }
}
