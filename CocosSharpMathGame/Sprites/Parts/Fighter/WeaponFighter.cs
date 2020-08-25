using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class WeaponFighter : Part
    {
        public WeaponFighter() : base("weaponFighter.png")
        {
            SetHealthAndMaxHealth(16);
            // set your types
            Types = new Type[] { Type.GUN };
            NormalAnchorPoint = CCPoint.AnchorMiddle;

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(12f);

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // give the gun a WeaponAbility
            WeaponAbility = WeaponAbility.CreateFighterWeapon(this);
        }
    }
}
