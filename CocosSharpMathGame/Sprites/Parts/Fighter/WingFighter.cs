using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class WingFighter : Part
    {
        public WingFighter() : base("wingFighter.png")
        {
            SetHealthAndMaxHealth(18);
            // set your types
            Types = new Type[] { Type.SINGLE_WING };
            NormalAnchorPoint = new CCPoint(0.5f, 1 / ContentSize.Height);

            // add a mount point for a gun
            var gunMount = new PartMount(this, new CCPoint(ContentSize.Width / 2, ContentSize.Height / 2), Type.GUN);
            gunMount.Dz = -1;
            PartMounts = new PartMount[] { gunMount };

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(18f);
        }
    }
}
