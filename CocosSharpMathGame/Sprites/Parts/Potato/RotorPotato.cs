using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class RotorPotato : Part
    {
        public RotorPotato() : base("rotorPotato.png")
        {
            SetHealthAndMaxHealth(5);
            // set your types
            Types = new Type[] { Type.ROTOR };
            NormalAnchorPoint = new CCPoint(3 / ContentSize.Width, 0.5f);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(8f);

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // give the rotor ManeuverAbility
            ManeuverAbility = new ManeuverAbility((float)Math.Pow(10, 5) * 0.75f, (float)Math.Pow(10, 5) * 2.0f);
            ManeuverAbility.CloudTailNode.CloudDelay = 0.5f;
            ManeuverAbility.CloudTailNode.CloudLifeTime = 1.5f;
        }
    }

    internal class RotorPotatoShiny : Part
    {
        public RotorPotatoShiny() : base("rotorPotatoShiny.png")
        {
            SetHealthAndMaxHealth(8);
            // set your types
            Types = new Type[] { Type.ROTOR };
            NormalAnchorPoint = new CCPoint(3 / ContentSize.Width, 0.5f);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(9f);

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // give the rotor ManeuverAbility
            ManeuverAbility = new ManeuverAbility((float)Math.Pow(10, 5) * 0.25f, (float)Math.Pow(10, 5) * 3.0f);
            ManeuverAbility.CloudTailNode.CloudDelay = 0.5f;
            ManeuverAbility.CloudTailNode.CloudLifeTime = 1.5f;
        }
    }
}
