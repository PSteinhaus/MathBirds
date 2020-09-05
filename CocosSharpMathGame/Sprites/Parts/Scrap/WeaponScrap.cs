using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class WeaponScrap : Part
    {
        public WeaponScrap() : base("weaponScrap.png")
        {
            SetHealthAndMaxHealth(4);
            // set your types
            Types = new Type[] { Type.GUN };
            NormalAnchorPoint = new CCPoint(1.5f/ContentSize.Width, 0.5f);

            // specify the mass points
            MassPoints = new MassPoint[] { new MassPoint(ContentSize.Width / 2, ContentSize.Height / 2, 12) };

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // give the gun a WeaponAbility
            WeaponAbility = WeaponAbility.CreateScrapWeapon(this);
        }
    }
}
