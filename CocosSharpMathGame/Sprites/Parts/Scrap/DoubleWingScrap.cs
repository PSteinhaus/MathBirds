using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class DoubleWingScrap : Part
    {
        public DoubleWingScrap() : base("doubleWingScrap.png")
        {
            SetHealthAndMaxHealth(22);
            // set your types
            Types = new Type[] { Type.WINGS };
            NormalAnchorPoint = CCPoint.AnchorMiddle;

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // give the wings a special ManeuverAbility (allowing to glide forward even after the rotor has been hit)
            ManeuverAbility = new ManeuverAbility((float)Math.Pow(10, 5) * 4.25f, (float)Math.Pow(10, 5) * 4.3f);
            ManeuverAbility.CloudTailNode = null;

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(60);
        }
    }
}
