using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class TestBody : Part
    {
        internal TestBody() : base("testBody.png")
        {
            AnchorPoint = CCPoint.AnchorMiddle;
            //Position = new CCPoint((ContentSize.Width / 2), (ContentSize.Height / 2));
            // set your types
            Types = new Type[] { Type.BODY };

            // add a mount point for wings at your center
            var wingsMount = new PartMount(this, new CCPoint((ContentSize.Width / 2), (ContentSize.Height / 2)), Type.WINGS);
            // add a mount point for an engine at the end point
            var engineMount = new PartMount(this, new CCPoint(0, ContentSize.Height / 2), Type.ENGINE);
            PartMounts = new PartMount[] { wingsMount, engineMount };

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // specify the mass points
            MassPoints = new MassPoint[] { new MassPoint(ContentSize.Width / 5, ContentSize.Height / 2, 100), new MassPoint(ContentSize.Width - (ContentSize.Width / 5), ContentSize.Height / 2, 90) };
        }
    }
}
