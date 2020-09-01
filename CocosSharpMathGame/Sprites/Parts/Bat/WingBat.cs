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

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(12f);
        }
    }

    internal class WingBatShiny : Part
    {
        public WingBatShiny() : base("wingBatShiny.png")
        {
            SetHealthAndMaxHealth(18);
            // set your types
            Types = new Type[] { Type.SINGLE_WING };
            NormalAnchorPoint = CCPoint.AnchorMiddleBottom;

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(6f);
        }
    }
}
