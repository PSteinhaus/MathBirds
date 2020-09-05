using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class RotorBigBomber : Part
    {
        public RotorBigBomber() : base("rotorBigBomber.png")
        {
            SetHealthAndMaxHealth(9);
            // set your types
            Types = new Type[] { Type.ROTOR };
            NormalAnchorPoint = new CCPoint(3 / ContentSize.Width, 0.5f);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(9f);

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // give the rotor ManeuverAbility
            ManeuverAbility = new ManeuverAbility((float)Math.Pow(10, 5) * 3.0f, (float)Math.Pow(10, 5) * 10.0f);
            ManeuverAbility.CloudTailNode.CloudDelay *= 2;
            ManeuverAbility.CloudTailNode.CloudLifeTime /= 2;
            ManeuverAbility.CloudTailNode.ReferenceSize = 24f;
        }
    }

    internal class RotorBigBomberShiny : Part
    {
        public RotorBigBomberShiny() : base("rotorBigBomberShiny.png")
        {
            SetHealthAndMaxHealth(11);
            // set your types
            Types = new Type[] { Type.ROTOR };
            NormalAnchorPoint = new CCPoint(3 / ContentSize.Width, 0.5f);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(11f);

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // give the rotor ManeuverAbility
            ManeuverAbility = new ManeuverAbility((float)Math.Pow(10, 5) * 3.0f, (float)Math.Pow(10, 5) * 11.0f);
            ManeuverAbility.CloudTailNode.CloudDelay *= 2;
            ManeuverAbility.CloudTailNode.CloudLifeTime /= 2;
            ManeuverAbility.CloudTailNode.ReferenceSize = 24f;
        }
    }
}
