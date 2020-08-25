using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class WingBigBomber : Part
    {
        public WingBigBomber() : base("wingBigBomber.png")
        {
            SetHealthAndMaxHealth(40);
            // set your types
            Types = new Type[] { Type.SINGLE_WING };
            NormalAnchorPoint = new CCPoint(0.5f, 0);

            // add mount points for 2 rotors
            var rotorMount1 = new PartMount(this, new CCPoint(ContentSize.Width - 3f, 8.5f), Type.ROTOR);
            var rotorMount2 = new PartMount(this, new CCPoint(ContentSize.Width - 3f, 21.5f), Type.ROTOR);

            PartMounts = new PartMount[] { rotorMount1, rotorMount1 };

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(120f);
        }
    }
}
