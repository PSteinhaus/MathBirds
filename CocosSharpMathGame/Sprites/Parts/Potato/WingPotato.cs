using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class WingPotato : Part
    {
        public WingPotato() : base("wingPotato.png")
        {
            SetHealthAndMaxHealth(3);
            // set your types
            Types = new Type[] { Type.SINGLE_WING };
            NormalAnchorPoint = new CCPoint(0.6f, 2 / ContentSize.Height);

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(12f);

            // give it a maneuver ability!
            ManeuverAbility = new ManeuverAbility((float)Math.Pow(10, 5) * 0.75f, (float)Math.Pow(10, 5) * 2.0f, (float)Math.Pow(10, 5) * 0.45f, (float)Math.Pow(10, 5) * 0.75f);
            ManeuverAbility.CloudTailNode = null;
        }
    }
}
