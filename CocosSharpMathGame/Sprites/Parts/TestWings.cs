using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class TestWings : Part
    {
        public TestWings() : base("testWings.png")
        {
            // set your types
            Types = new Type[] { Type.WINGS };
            NormalAnchorPoint = CCPoint.AnchorMiddle;

            // add a mount point for two engines at the center of each wing
            var engineMount1 = new PartMount(this, new CCPoint(ContentSize.Width / 2, (ContentSize.Height / 2) + (ContentSize.Height / 4)), Type.ENGINE);
            var engineMount2 = new PartMount(this, new CCPoint(ContentSize.Width / 2, (ContentSize.Height / 2) - (ContentSize.Height / 4)), Type.ENGINE);
            // add mount points for two guns a bit further out
            var gunMount1 = new PartMount(this, new CCPoint(ContentSize.Width / 2, (ContentSize.Height - (ContentSize.Height / 6))), Type.GUN);
            var gunMount2 = new PartMount(this, new CCPoint(ContentSize.Width / 2,                        ContentSize.Height / 6), Type.GUN);
            gunMount1.NullRotation = -20f;
            gunMount1.Dz = 1;
            gunMount2.NullRotation =  20f;
            gunMount2.Dz = 1;
            PartMounts = new PartMount[] { engineMount1, engineMount2, gunMount1, gunMount2 };

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // specify the mass points
            MassPoints = new MassPoint[] { new MassPoint((ContentSize.Width / 2)-1, ContentSize.Height, 30), new MassPoint((ContentSize.Width / 2), (ContentSize.Height/2)+(ContentSize.Height/6), 50), new MassPoint((ContentSize.Width / 2), (ContentSize.Height / 2) - (ContentSize.Height / 6), 50), new MassPoint((ContentSize.Width / 2) - 1, 0, 30) };
        }
    }
}
