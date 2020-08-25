using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class RudderJet : Part
    {
        public RudderJet() : base("rudderJet.png")
        {
            SetHealthAndMaxHealth(20);
            // set your types
            Types = new Type[] { Type.RUDDER };
            NormalAnchorPoint = new CCPoint(5f/ContentSize.Width, 0f);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(8f);

            // specify the collision type
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // give the rudder ManeuverAbility
            ManeuverAbility = new ManeuverAbility(0,0, (float)Math.Pow(10, 5) * 2.5f, (float)Math.Pow(10, 5) * 10f);
        }
    }
}
