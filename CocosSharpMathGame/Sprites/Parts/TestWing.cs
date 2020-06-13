using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class TestWing : Part
    {
        internal TestWing() : base("testWing.png")
        {
            // set your types
            Types = new Type[] { Type.SINGLE_WING };
            NormalAnchorPoint = CCPoint.AnchorMiddleBottom;

            // add a mount point for two engines at the center of the wing
            var engineMount = new PartMount(this, new CCPoint(ContentSize.Width / 2, (ContentSize.Height / 2)), Type.ENGINE);
            // add a mount points for a gun a bit further out
            var gunMount = new PartMount(this, new CCPoint(ContentSize.Width / 2, (ContentSize.Height - (ContentSize.Height / 4))), Type.GUN);
            gunMount.NullRotation = -20f;
            gunMount.Dz = 1;
            PartMounts = new PartMount[] { engineMount, gunMount };

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // specify the mass points
            MassPoints = new MassPoint[] { new MassPoint((ContentSize.Width / 2) - 1, ContentSize.Height, 30), new MassPoint((ContentSize.Width / 2), (ContentSize.Height / 2) + (ContentSize.Height / 6), 50), new MassPoint((ContentSize.Width / 2), (ContentSize.Height / 2) - (ContentSize.Height / 6), 50), new MassPoint((ContentSize.Width / 2) - 1, 0, 30) };
        }
    }
}
