using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class RotorBat : Part
    {
        public RotorBat() : base("rotorBat.png")
        {
                SetHealthAndMaxHealth(6);
                // set your types
                Types = new Type[] { Type.ROTOR };
                NormalAnchorPoint = new CCPoint(2 / ContentSize.Width, 0.5f);

                // specify the mass points
                MassPoints = CreateDiamondMassPoints(8f);

                // specify the collision polygon
                CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

                // give the rotor ManeuverAbility
                ManeuverAbility = new ManeuverAbility((float)Math.Pow(10, 5) * 0.75f, (float)Math.Pow(10, 5) * 3.0f);
                ManeuverAbility.CloudTailNode.CloudDelay = 0.2f;
                ManeuverAbility.CloudTailNode.CloudLifeTime = 1.25f;
        }
    }

    internal class RotorBatShiny : Part
    {
        public RotorBatShiny() : base("rotorBatShiny.png")
        {
                SetHealthAndMaxHealth(8);
                // set your types
                Types = new Type[] { Type.ROTOR };
                NormalAnchorPoint = new CCPoint(1 / ContentSize.Width, 0.5f);

                // specify the mass points
                MassPoints = CreateDiamondMassPoints(4f);

                // specify the collision polygon
                CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

                // give the rotor ManeuverAbility
                ManeuverAbility = new ManeuverAbility((float)Math.Pow(10, 5) * 0.35f, (float)Math.Pow(10, 5) * 3.5f);
                ManeuverAbility.CloudTailNode.CloudDelay = 0.2f;
                ManeuverAbility.CloudTailNode.CloudLifeTime = 1.25f;
                ManeuverAbility.CloudTailNode.CloudColor = new CCColor4B(1f, 0.8f, 1f, 1f);
        }
    }
}
