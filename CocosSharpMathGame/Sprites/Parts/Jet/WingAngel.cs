using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class WingAngel : Part
    {
        public WingAngel() : base("wingAngel.png")
        {
            SetHealthAndMaxHealth(10);
            // set your types
            Types = new Type[] { Type.SINGLE_WING };
            NormalAnchorPoint = new CCPoint(0.5f, 1 / ContentSize.Height);

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(20f);

            // give it a maneuver ability
            ManeuverAbility = new ManeuverAbility(0, 0, 0, (float)Math.Pow(10, 5) * 3.75f);
        }
    }
}
