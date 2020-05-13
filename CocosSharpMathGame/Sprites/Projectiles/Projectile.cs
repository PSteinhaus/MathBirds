using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal abstract class Projectile : GameObjectSprite
    {
        internal float Velocity { get; set; }
        
        internal Projectile(CCSpriteFrame spriteFrame) : base(spriteFrame)
        {

        }
    }
}
