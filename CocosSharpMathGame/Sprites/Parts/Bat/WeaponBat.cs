using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class WeaponBat : Part
    {
        public WeaponBat() : base("weaponBat.png")
        {
            SetHealthAndMaxHealth(5);
            // set your types
            Types = new Type[] { Type.GUN };
            NormalAnchorPoint = new CCPoint(2f/ContentSize.Width, 0.5f);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(6f);

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // give the gun a WeaponAbility
            WeaponAbility = WeaponAbility.CreateBatWeapon(this);
        }
    }
}
