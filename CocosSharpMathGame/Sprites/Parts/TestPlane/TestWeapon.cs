using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class TestWeapon : Part
    {
        public TestWeapon() : base("testWeapon.png")
        {
            SetHealthAndMaxHealth(8);
            // set your types
            Types = new Type[] { Type.GUN };
            NormalAnchorPoint = new CCPoint(1/ContentSize.Width, 0.5f);

            // specify the mass points
            MassPoints = new MassPoint[] { new MassPoint(ContentSize.Width / 2, ContentSize.Height / 2, 30) };

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // give the gun a WeaponAbility
            WeaponAbility = WeaponAbility.CreateTestWeapon(this);
        }
    }

    internal class TestWeaponShiny : Part
    {
        public TestWeaponShiny() : base("weaponTestShiny.png") // for now, as there is no gun sprite yet
        {
            SetHealthAndMaxHealth(8);
            // set your types
            Types = new Type[] { Type.GUN };
            NormalAnchorPoint = new CCPoint(1 / ContentSize.Width, 0.5f);

            // specify the mass points
            MassPoints = new MassPoint[] { new MassPoint(ContentSize.Width / 2, ContentSize.Height / 2, 30) };

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // give the gun a WeaponAbility
            WeaponAbility = WeaponAbility.CreateTestWeaponShiny(this);
        }
    }
}
