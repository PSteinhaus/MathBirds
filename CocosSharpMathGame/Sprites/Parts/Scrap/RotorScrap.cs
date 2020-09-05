using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class RotorScrap : Part
    {
        public RotorScrap() : base("rotorScrap.png")
        {
            SetHealthAndMaxHealth(2);
            // set your types
            Types = new Type[] { Type.ROTOR };
            NormalAnchorPoint = new CCPoint(3 / ContentSize.Width, 0.5f);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(10f);

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // give the rotor ManeuverAbility
            ManeuverAbility = new ManeuverAbility((float)Math.Pow(10, 5) * 0.75f, (float)Math.Pow(10, 5) * 4.0f);
            ManeuverAbility.CloudTailNode.CloudDelay *= 1.5f;
            ManeuverAbility.CloudTailNode.CloudLifeTime *= 1.5f;
            ManeuverAbility.CloudTailNode.CloudColor = CCColor4B.Gray;
        }
    }
}
