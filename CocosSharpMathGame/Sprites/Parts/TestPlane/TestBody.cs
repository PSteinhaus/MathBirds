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
        public TestBody() : base("testBody.png")
        {
            SetHealthAndMaxHealth(14);
            NormalAnchorPoint = CCPoint.AnchorMiddle;
            //Position = new CCPoint((ContentSize.Width / 2), (ContentSize.Height / 2));
            // set your types
            Types = new Type[] { Type.BODY };

            // OLD: add a mount point for wings at your center
            // NEW: add mount points for two single wings
            var wingMount1 = new PartMount(this, new CCPoint((ContentSize.Width / 2)+2f, (ContentSize.Height / 2) + 4), Type.SINGLE_WING);
            var wingMount2 = new PartMount(this, new CCPoint((ContentSize.Width / 2)+2f, (ContentSize.Height / 2) - 4), Type.SINGLE_WING);
            var doubleWingMount = new PartMount(this, new CCPoint((ContentSize.Width / 2)+2.5f, (ContentSize.Height / 2)), Type.WINGS);
            doubleWingMount.PossiblyBlockingPartMounts.Add(wingMount1);
            doubleWingMount.PossiblyBlockingPartMounts.Add(wingMount2);
            wingMount1.PossiblyBlockingPartMounts.Add(doubleWingMount);
            wingMount2.PossiblyBlockingPartMounts.Add(doubleWingMount);
            // add mount points for two rudders
            var rudderMount1 = new PartMount(this, new CCPoint(4, (ContentSize.Height / 2) + 1), Type.RUDDER);
            var rudderMount2 = new PartMount(this, new CCPoint(4, (ContentSize.Height / 2) - 1), Type.RUDDER);
            // add a mount for a rotor
            var rotorMount = new PartMount(this, new CCPoint(ContentSize.Width - 5, ContentSize.Height / 2), Type.ROTOR);
            PartMounts = new PartMount[] { wingMount1, wingMount2, doubleWingMount, rudderMount1, rudderMount2, rotorMount };

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(60);
               //new MassPoint[] { new MassPoint(ContentSize.Width / 5, ContentSize.Height / 2, 100), new MassPoint(ContentSize.Width - (ContentSize.Width / 5), ContentSize.Height / 2, 90) };
        }
    }
}
