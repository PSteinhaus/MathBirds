using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class WeaponBigBomber : Part
    {
        public WeaponBigBomber() : base("weaponBigBomber.png")
        {
            SetHealthAndMaxHealth(16);
            // set your types
            Types = new Type[] { Type.GUN };
            NormalAnchorPoint = new CCPoint(1.5f/ContentSize.Width, 0.5f);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(25f);

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // give the gun a WeaponAbility
            WeaponAbility = WeaponAbility.CreateBigBomberWeapon(this);
        }
    }

    internal class WeaponBigBomberShiny : Part
    {
        public WeaponBigBomberShiny() : base("weaponBigBomberShiny.png")
        {
            SetHealthAndMaxHealth(26);
            // set your types
            Types = new Type[] { Type.GUN };
            NormalAnchorPoint = new CCPoint(1.5f / ContentSize.Width, 0.5f);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(35f);

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // give the gun a WeaponAbility
            WeaponAbility = WeaponAbility.CreateBigBomberWeaponShiny(this);
        }
    }
}
