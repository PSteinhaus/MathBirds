using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class TestRudder : Part
    {
        public TestRudder() : base("testRudder.png")
        {
            SetHealthAndMaxHealth(4);
            // set your types
            Types = new Type[] { Type.RUDDER };
            NormalAnchorPoint = new CCPoint(0.5f, 0f);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(5f);

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // give the rotor ManeuverAbility
            ManeuverAbility = new ManeuverAbility(0,0, (float)Math.Pow(10, 5) * 1f, (float)Math.Pow(10, 5) * 3.0f);
        }
    }

    internal class TestRudderShiny : Part
    {
        public TestRudderShiny() : base("rudderTestShiny.png")
        {
            SetHealthAndMaxHealth(7);
            // set your types
            Types = new Type[] { Type.RUDDER };
            NormalAnchorPoint = new CCPoint(0.5f, 0f);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(4f);

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // give the rotor ManeuverAbility
            ManeuverAbility = new ManeuverAbility(0, 0, (float)Math.Pow(10, 5) * 4f, (float)Math.Pow(10, 5) * 5.0f);
        }
    }
}
