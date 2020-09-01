using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class WeaponPotato : Part
    {
        public WeaponPotato() : base("weaponPotato.png")
        {
            SetHealthAndMaxHealth(3);
            // set your types
            Types = new Type[] { Type.GUN };
            NormalAnchorPoint = new CCPoint(2f/ContentSize.Width, 0.5f);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(6f);

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // give the gun a WeaponAbility
            WeaponAbility = WeaponAbility.CreatePotatoWeapon(this);
        }
    }

    internal class WeaponPotatoShiny : Part
    {
        public WeaponPotatoShiny() : base("weaponPotatoShiny.png")
        {
            SetHealthAndMaxHealth(6);
            // set your types
            Types = new Type[] { Type.GUN };
            NormalAnchorPoint = new CCPoint(2f / ContentSize.Width, 0.5f);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(7f);

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // give the gun a WeaponAbility
            WeaponAbility = WeaponAbility.CreatePotatoWeaponShiny(this);
        }
    }
}
