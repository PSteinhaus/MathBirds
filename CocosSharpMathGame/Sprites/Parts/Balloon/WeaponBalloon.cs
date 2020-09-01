using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class WeaponBalloon : Part
    {
        public WeaponBalloon() : base("weaponBalloon.png")
        {
                SetHealthAndMaxHealth(15);
                // set your types
                Types = new Type[] { Type.GUN };
                NormalAnchorPoint = new CCPoint(3.5f / ContentSize.Width, 0.5f);

                // specify the mass points
                MassPoints = CreateDiamondMassPoints(12f);

                // specify the collision polygon
                CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

                // give the gun a WeaponAbility
                WeaponAbility = WeaponAbility.CreateBalloonWeapon(this);
        }
    }

    internal class WeaponBalloonShiny : Part
    {
        public WeaponBalloonShiny() : base("weaponBalloonShiny.png")
        {
                SetHealthAndMaxHealth(12);
                // set your types
                Types = new Type[] { Type.GUN };
                NormalAnchorPoint = new CCPoint(3.5f / ContentSize.Width, 0.5f);

                // specify the mass points
                MassPoints = CreateDiamondMassPoints(20f);

                // specify the collision polygon
                CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

                // give the gun a WeaponAbility
                WeaponAbility = WeaponAbility.CreateBalloonWeaponShiny(this);
        }
    }
}
