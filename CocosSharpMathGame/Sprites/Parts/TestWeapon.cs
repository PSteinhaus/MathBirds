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
        internal TestWeapon() : base("testEngine.png") // for now, as there is no gun sprite yet
        {
            // set your types
            Types = new Type[] { Type.GUN };
            AnchorPoint = CCPoint.AnchorMiddle;

            // specify the mass points
            MassPoints = new MassPoint[] { new MassPoint(ContentSize.Width / 2, ContentSize.Height / 2, 30) };

            // specify the collision polygon
            CollisionType = new CollisionTypePolygon(new Polygon(DiamondCollisionPoints()));

            // give the gun a WeaponAbility
            WeaponAbility = WeaponAbility.CreateTestWeapon(this);
        }
    }
}
