using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class RotorBalloon : Part
    {
        public RotorBalloon() : base("rotorBalloon.png")
        {
                SetHealthAndMaxHealth(2);
                // set your types
                Types = new Type[] { Type.ROTOR };
                NormalAnchorPoint = new CCPoint(0.35f, 0.5f);

                // specify the mass points
                MassPoints = CreateDiamondMassPoints(8f);

                // specify the collision polygon
                CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

                // give the rotor ManeuverAbility
                ManeuverAbility = new ManeuverAbility((float)Math.Pow(10, 5) * 0.5f, (float)Math.Pow(10, 5) * 1.0f);
                ManeuverAbility.CloudTailNode.CloudDelay = 0.35f;
                ManeuverAbility.CloudTailNode.CloudLifeTime = 1.0f;
        }
    }

    internal class RotorBalloonShiny : Part
    {
        public RotorBalloonShiny() : base("rotorBalloonShiny.png")
        {
                SetHealthAndMaxHealth(10);
                // set your types
                Types = new Type[] { Type.ROTOR };
                NormalAnchorPoint = new CCPoint(0.35f, 0.5f);

                // specify the mass points
                MassPoints = CreateDiamondMassPoints(12f);

                // specify the collision polygon
                CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

                // give the rotor ManeuverAbility
                ManeuverAbility = new ManeuverAbility((float)Math.Pow(10, 5) * 0.5f, (float)Math.Pow(10, 5) * 2.0f);
                ManeuverAbility.CloudTailNode.CloudDelay = 0.35f;
                ManeuverAbility.CloudTailNode.CloudLifeTime = 1.0f;
        }
    }
}
