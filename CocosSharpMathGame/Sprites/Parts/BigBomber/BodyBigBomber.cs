using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosSharpMathGame
{
    internal class BodyBigBomber : Part
    {
        public BodyBigBomber() : base("bodyBigBomber.png")
        {
            SetHealthAndMaxHealth(50);
            NormalAnchorPoint = CCPoint.AnchorMiddle;
            // set your types
            Types = new Type[] { Type.BODY };

            // NEW: add mount points for two single wings
            var wingMount1 = new PartMount(this, new CCPoint((ContentSize.Width * 0.62f), (ContentSize.Height / 2) + 4), Type.SINGLE_WING);
            var wingMount2 = new PartMount(this, new CCPoint((ContentSize.Width * 0.62f), (ContentSize.Height / 2) - 4), Type.SINGLE_WING);
            // add mount points for two rudders
            var rudderMount1 = new PartMount(this, new CCPoint(11f, (ContentSize.Height / 2) + 2), Type.RUDDER);
            var rudderMount2 = new PartMount(this, new CCPoint(11f, (ContentSize.Height / 2) - 2), Type.RUDDER);
            // add a mount for a weapon
            var gunMount = new PartMount(this, new CCPoint(31, ContentSize.Height / 2), Type.GUN);
            gunMount.MaxTurningAngle = 180f;

            PartMounts = new PartMount[] { wingMount1, wingMount2, rudderMount1, rudderMount2, gunMount };

            // specify the collision polygon
            CollisionType = Collisions.CreateDiamondCollisionPolygon(this);

            // specify the mass points
            MassPoints = CreateDiamondMassPoints(140);
        }
    }
}
