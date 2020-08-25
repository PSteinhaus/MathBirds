using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class WingBat : Part
    {
        public WingBat() : base("wingBat.png")
        {
            SetHealthAndMaxHealth(9);
            // set your types
            Types = new Type[] { Type.SINGLE_WING };
            NormalAnchorPoint = CCPoint.AnchorMiddleBottom;

            // add a mount point for a gun at the center of the wing
            /*
            var gunMount = new PartMount(this, new CCPoint(ContentSize.Width / 2, (ContentSize.Height / 2)), Type.GUN);
            gunMount.NullRotation = -5f;
            gunMount.Dz = 1;
            PartMounts = new PartMount[] { gunMount };
            */

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(12f);
        }
    }
}
