using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class RotorFighter : Part
    {
        public RotorFighter() : base("rotorFighter.png")
        {
            SetHealthAndMaxHealth(13);
            // set your types
            Types = new Type[] { Type.ROTOR };
            NormalAnchorPoint = new CCPoint(1 / ContentSize.Width, 0.5f);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(6f);

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // give the rotor ManeuverAbility
            ManeuverAbility = new ManeuverAbility((float)Math.Pow(10, 5) * 5.75f, (float)Math.Pow(10, 5) * 18.0f);
            ManeuverAbility.CloudTailNode.CloudDelay *= 1.5f;
            ManeuverAbility.CloudTailNode.CloudLifeTime *= 0.75f;
            ManeuverAbility.CloudTailNode.ReferenceSize = 14f;
        }
    }

    internal class RotorFighterShiny : Part
    {
        public RotorFighterShiny() : base("rotorFighterShiny.png")
        {
            SetHealthAndMaxHealth(18);
            // set your types
            Types = new Type[] { Type.ROTOR };
            NormalAnchorPoint = new CCPoint(1 / ContentSize.Width, 0.5f);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(6f);

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // give the rotor ManeuverAbility
            ManeuverAbility = new ManeuverAbility((float)Math.Pow(10, 5) * 5.75f, (float)Math.Pow(10, 5) * 20.0f);
            ManeuverAbility.CloudTailNode.CloudDelay *= 1.5f;
            ManeuverAbility.CloudTailNode.CloudLifeTime *= 0.75f;
            ManeuverAbility.CloudTailNode.ReferenceSize = 14f;
        }
    }
}
