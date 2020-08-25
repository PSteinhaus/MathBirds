using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class RudderBigBomber : Part
    {
        public RudderBigBomber() : base("rudderBigBomber.png")
        {
            SetHealthAndMaxHealth(17);
            // set your types
            Types = new Type[] { Type.RUDDER };
            NormalAnchorPoint = new CCPoint(0.5f, 0f);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(20f);

            // specify the collision type
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // give the rudder ManeuverAbility
            ManeuverAbility = new ManeuverAbility(0,0, (float)Math.Pow(10, 5) * 10f, (float)Math.Pow(10, 5) * 15f);
        }
    }
}
