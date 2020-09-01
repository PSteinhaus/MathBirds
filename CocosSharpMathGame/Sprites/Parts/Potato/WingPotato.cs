using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class WingPotato : Part
    {
        public WingPotato(bool shiny = false) : base("wingPotato" + (shiny ? "Shiny" : "") + ".png")
        {
            SetHealthAndMaxHealth(3);
            // set your types
            Types = new Type[] { Type.SINGLE_WING };
            NormalAnchorPoint = new CCPoint(0.6f, 2 / ContentSize.Height);

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(12f);

            // give it a maneuver ability!
            ManeuverAbility = new ManeuverAbility((float)Math.Pow(10, 5) * 0.75f, (float)Math.Pow(10, 5) * 2.0f, (float)Math.Pow(10, 5) * 0.45f, (float)Math.Pow(10, 5) * 0.75f);
            ManeuverAbility.CloudTailNode = null;
        }
    }

    internal class WingPotatoShiny : Part
    {
        public WingPotatoShiny() : base("wingPotatoShiny.png")
        {
            SetHealthAndMaxHealth(3);
            // set your types
            Types = new Type[] { Type.SINGLE_WING };
            NormalAnchorPoint = new CCPoint(0.6f, 2 / ContentSize.Height);

            // SHINY: add mount points for two weapons
            var weaponMount1 = new PartMount(this, new CCPoint(2f, (ContentSize.Height / 2) + 1), Type.GUN);
            var weaponMount2 = new PartMount(this, new CCPoint(2f, (ContentSize.Height / 2) - 1), Type.GUN);
            weaponMount1.MaxTurningAngle = 90f;
            weaponMount2.MaxTurningAngle = 90f;

            PartMounts = new PartMount[] { weaponMount1, weaponMount2 };

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(15f);

            // give it a maneuver ability!
            ManeuverAbility = new ManeuverAbility((float)Math.Pow(10, 5) * 0.95f, (float)Math.Pow(10, 5) * 3.0f, (float)Math.Pow(10, 5) * 0.45f, (float)Math.Pow(10, 5) * 0.75f);
            ManeuverAbility.CloudTailNode = null;
        }
    }
}